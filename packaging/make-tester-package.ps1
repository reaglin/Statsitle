<#
.SYNOPSIS
    Builds the zip that goes to test users.

.DESCRIPTION
    Publishes Statistle.App self-contained as one exe (the tester needs no .NET runtime), puts it in
    a folder with the sample projects, a README and a desktop-shortcut script, and zips the
    folder to artifacts\Statistle-<version>-win-x64.zip. Unzip anywhere and run Statistle.exe: nothing
    is installed, no admin rights are needed, and the user's files go to Documents\Statistle.

    This is the pre-Store distribution. The Store package is packaging\pack.ps1 (MSIX), which
    needs the Partner Center publisher identity and signs the package; an unsigned sideloaded
    MSIX would make every tester install a certificate, so the zip is used until then.

.EXAMPLE
    ./packaging/make-tester-package.ps1
    Builds artifacts\Statistle-0.5.1-win-x64.zip.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version,
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'artifacts' }

if (-not $Version) {
    $props = Join-Path $repoRoot 'Directory.Build.props'
    $Version = ([xml](Get-Content $props)).Project.PropertyGroup.StatistleVersion |
        Where-Object { $_ } | Select-Object -First 1
    if (-not $Version) { throw "StatistleVersion not found in $props." }
}

$name = "Statistle-$Version-$Runtime"
$stage = Join-Path $OutputDirectory "tester\$name"
$publish = Join-Path $OutputDirectory 'tester\publish'
$zip = Join-Path $OutputDirectory "$name.zip"

foreach ($dir in @($stage, $publish)) {
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
    New-Item -ItemType Directory -Path $dir | Out-Null
}

# --- publish -------------------------------------------------------------------------------
Write-Host "Publishing $name (self-contained, single file)..."
& dotnet publish (Join-Path $repoRoot 'src\Statistle.App\Statistle.App.csproj') `
    -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true -p:DebugType=none `
    -o $publish --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)." }

$exe = Get-ChildItem $publish -Filter '*.exe' | Select-Object -First 1
if (-not $exe) { throw "No exe in $publish." }
Copy-Item $exe.FullName (Join-Path $stage 'Statistle.exe')

# --- samples, README, shortcut script -------------------------------------------------------
$samples = Join-Path $stage 'samples'
New-Item -ItemType Directory -Path $samples | Out-Null
Copy-Item (Join-Path $repoRoot 'samples\*.statistle') $samples

$readme = Get-Content (Join-Path $PSScriptRoot 'tester\README.txt') -Raw
$readme = $readme.Replace('{VERSION}', $Version).Replace('{DATE}', (Get-Date -Format 'yyyy-MM-dd'))
Set-Content (Join-Path $stage 'README.txt') $readme -Encoding UTF8

Copy-Item (Join-Path $PSScriptRoot 'tester\Create desktop shortcut.cmd') $stage

# --- zip -----------------------------------------------------------------------------------
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path $stage -DestinationPath $zip -CompressionLevel Optimal

$size = [math]::Round((Get-Item $zip).Length / 1MB, 1)
Write-Host "Tester package: $zip ($size MB)"
Write-Host "Contents: Statistle.exe, README.txt, Create desktop shortcut.cmd, samples\"
