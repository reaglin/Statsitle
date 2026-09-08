<#
.SYNOPSIS
    Builds a signed MSIX for Statistle.

.DESCRIPTION
    Publishes Statistle.App self-contained, stages it with the manifest and assets, packs it with
    makeappx and signs it with signtool. No Visual Studio and no .wapproj — the same steps run
    on this machine and on a bare windows-latest CI runner.

    For local use the script creates a self-signed development certificate on demand and
    rewrites the manifest Publisher to match it. Store builds pass -Publisher with the identity
    Partner Center issued, and -CertificateThumbprint for the real signing certificate.

.EXAMPLE
    ./packaging/pack.ps1
    Development build: self-signed, output in artifacts/.

.EXAMPLE
    ./packaging/pack.ps1 -SkipSigning
    Unsigned package, for CI where a certificate is not available.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version,
    [string]$Publisher,
    [string]$CertificateThumbprint,
    [string]$DevCertSubject = 'CN=Statistle Development',
    [string]$OutputDirectory,
    [switch]$SkipSigning
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$packagingDir = $PSScriptRoot
$stageDir = Join-Path $repoRoot 'artifacts/stage'
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'artifacts' }

function Find-WindowsKitTool {
    param([Parameter(Mandatory)][string]$Name)

    $binRoot = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
    if (-not (Test-Path $binRoot)) { throw "Windows SDK not found at $binRoot." }

    $tool = Get-ChildItem -Path $binRoot -Filter $Name -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\' } |
        Sort-Object { [version]($_.Directory.Parent.Name) } -Descending |
        Select-Object -First 1

    if (-not $tool) { throw "$Name not found under $binRoot. Install the Windows 10/11 SDK." }
    return $tool.FullName
}

# --- version -------------------------------------------------------------------------------
if (-not $Version) {
    $props = Join-Path $repoRoot 'Directory.Build.props'
    $Version = ([xml](Get-Content $props)).Project.PropertyGroup.StatistleVersion |
        Where-Object { $_ } | Select-Object -First 1
    if (-not $Version) { throw "StatistleVersion not found in $props." }
}

# MSIX versions are four-part and the revision must be 0 for Store submission.
$parts = @($Version.Split('.'))
while ($parts.Count -lt 4) { $parts += '0' }
$packageVersion = ($parts[0..3] -join '.')

Write-Host "Statistle $packageVersion ($Configuration / $Runtime)" -ForegroundColor Cyan

# --- publish -------------------------------------------------------------------------------
$publishDir = Join-Path $repoRoot "artifacts/publish/$Runtime"
Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue

# Self-contained: the MSIX carries the .NET runtime, so a clean machine needs no prerequisite.
dotnet publish (Join-Path $repoRoot 'src/Statistle.App/Statistle.App.csproj') `
    -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=false `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

# --- stage ---------------------------------------------------------------------------------
Remove-Item $stageDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $stageDir | Out-Null

Copy-Item "$publishDir/*" $stageDir -Recurse -Force
Copy-Item (Join-Path $packagingDir 'Assets') $stageDir -Recurse -Force

# .pdb files are not shipped and trip Store certification.
Get-ChildItem $stageDir -Filter *.pdb -Recurse | Remove-Item -Force

# --- certificate ---------------------------------------------------------------------------
# The manifest Publisher must equal the signing certificate subject exactly, or Windows
# refuses the package at install time with a signature/identity mismatch.
$certificate = $null
if (-not $SkipSigning) {
    if ($CertificateThumbprint) {
        $certificate = Get-Item "Cert:\CurrentUser\My\$CertificateThumbprint" -ErrorAction Stop
    }
    else {
        $certificate = Get-ChildItem Cert:\CurrentUser\My |
            Where-Object { $_.Subject -eq $DevCertSubject -and $_.NotAfter -gt (Get-Date) } |
            Select-Object -First 1

        if (-not $certificate) {
            Write-Host "Creating development certificate $DevCertSubject" -ForegroundColor Yellow
            $certificate = New-SelfSignedCertificate `
                -Type Custom -Subject $DevCertSubject `
                -KeyUsage DigitalSignature -FriendlyName 'Statistle development signing' `
                -CertStoreLocation 'Cert:\CurrentUser\My' `
                -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
            Write-Host "  Sideloading requires this certificate trusted under Local Machine\\TrustedPeople." -ForegroundColor Yellow
        }
    }
}

if (-not $Publisher) {
    $Publisher = if ($certificate) { $certificate.Subject } else { $DevCertSubject }
}

# --- manifest ------------------------------------------------------------------------------
$manifest = [xml](Get-Content (Join-Path $packagingDir 'AppxManifest.xml'))
$manifest.Package.Identity.Version = $packageVersion
$manifest.Package.Identity.Publisher = $Publisher
$manifest.Package.Identity.ProcessorArchitecture = $Runtime.Replace('win-', '')
$manifest.Save((Join-Path $stageDir 'AppxManifest.xml'))

# --- pack ----------------------------------------------------------------------------------
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$msix = Join-Path $OutputDirectory "Statistle-$packageVersion-$Runtime.msix"
Remove-Item $msix -Force -ErrorAction SilentlyContinue

$makeappx = Find-WindowsKitTool 'makeappx.exe'
& $makeappx pack /d $stageDir /p $msix /o
if ($LASTEXITCODE -ne 0) { throw 'makeappx failed.' }

# --- sign ----------------------------------------------------------------------------------
if ($SkipSigning) {
    Write-Host "Unsigned package: $msix" -ForegroundColor Yellow
    Write-Host 'It will not install until signed.' -ForegroundColor Yellow
}
else {
    $signtool = Find-WindowsKitTool 'signtool.exe'
    & $signtool sign /fd SHA256 /sha1 $certificate.Thumbprint $msix
    if ($LASTEXITCODE -ne 0) { throw 'signtool failed.' }
    Write-Host "Signed package: $msix" -ForegroundColor Green
}

Write-Output $msix
