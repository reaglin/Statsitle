<#
.SYNOPSIS
    Builds the Inno Setup installer for local testers.

.DESCRIPTION
    Publishes Statistle.App self-contained as one exe (the tester needs no .NET runtime), stages
    it with the sample projects and the README, and compiles packaging\installer\Statistle.iss
    with Inno Setup 6 into artifacts\Statistle-<version>-setup.exe.

    The installer is per-user by default: no administrator rights, program in
    %LOCALAPPDATA%\Programs\Statistle, Start-menu entry, optional desktop icon, optional
    .statistle file association, and an uninstaller. An administrator can choose "all users"
    in the wizard. The user's files live in Documents\Statistle and are never touched.

    This is the pre-Store distribution alongside the zip (make-tester-package.ps1). The Store
    package is pack.ps1 (MSIX). Inno Setup 6 must be installed
    (https://jrsoftware.org/isinfo.php); the script looks for ISCC.exe in the usual place or on
    the PATH, or takes -Iscc.

.EXAMPLE
    ./packaging/make-installer.ps1
    Builds artifacts\Statistle-0.5.2-setup.exe.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version,
    [string]$OutputDirectory,
    [string]$Iscc
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

if (-not $Iscc) {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    )
    $Iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $Iscc) { $Iscc = (Get-Command ISCC.exe -ErrorAction SilentlyContinue).Source }
    if (-not $Iscc) { throw 'Inno Setup 6 (ISCC.exe) was not found. Install it from https://jrsoftware.org/isinfo.php or pass -Iscc.' }
}

$stage = Join-Path $OutputDirectory 'installer\stage'
$publish = Join-Path $OutputDirectory 'installer\publish'

foreach ($dir in @($stage, $publish)) {
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
    New-Item -ItemType Directory -Path $dir | Out-Null
}

# --- publish -------------------------------------------------------------------------------
Write-Host "Publishing Statistle $Version ($Runtime, self-contained, single file)..."
& dotnet publish (Join-Path $repoRoot 'src\Statistle.App\Statistle.App.csproj') `
    -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true -p:DebugType=none `
    -o $publish --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)." }

$exe = Get-ChildItem $publish -Filter '*.exe' | Select-Object -First 1
if (-not $exe) { throw "No exe in $publish." }
Copy-Item $exe.FullName (Join-Path $stage 'Statistle.exe')

# --- samples and README ---------------------------------------------------------------------
$samples = Join-Path $stage 'samples'
New-Item -ItemType Directory -Path $samples | Out-Null
Copy-Item (Join-Path $repoRoot 'samples\*.statistle') $samples

$readme = Get-Content (Join-Path $PSScriptRoot 'tester\README.txt') -Raw
$readme = $readme.Replace('{VERSION}', $Version).Replace('{DATE}', (Get-Date -Format 'yyyy-MM-dd'))
Set-Content (Join-Path $stage 'README.txt') $readme -Encoding UTF8

# --- compile --------------------------------------------------------------------------------
$script = Join-Path $PSScriptRoot 'installer\Statistle.iss'
Write-Host "Compiling $script with $Iscc..."
& $Iscc /Qp "/DAppVersion=$Version" "/DStageDir=$stage" "/DOutputDir=$OutputDirectory" $script
if ($LASTEXITCODE -ne 0) { throw "ISCC failed ($LASTEXITCODE)." }

$setup = Join-Path $OutputDirectory "Statistle-$Version-setup.exe"
if (-not (Test-Path $setup)) { throw "Expected $setup was not produced." }
$size = [math]::Round((Get-Item $setup).Length / 1MB, 1)
Write-Host "Installer: $setup ($size MB)"
Write-Host 'Per-user by default (no admin); silent install: Statistle-<ver>-setup.exe /VERYSILENT /CURRENTUSER'
