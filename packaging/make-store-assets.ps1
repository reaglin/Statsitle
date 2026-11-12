<#
.SYNOPSIS
    Regenerates every Statistics+ icon, tile and Store listing image.

.DESCRIPTION
    Two visual layers, chosen by size — the same rule SMADA uses:

      * The LOGO carries everything shown large: the 310x310 tile, the splash screen, and the
        Partner Center listing images. It is drawn here: the name "Statistics+" above the banded
        bell, on the navy of the old Statistle logo (resources/images/Statistle_logo2048x2048.jpg,
        kept for the 1.x history).
      * The MARK carries anything small — the 44x44, 71x71 and 150x150 tiles, the taskbar target
        sizes, the .statistle file icon and Statistle.ico. It is the banded bell with a white "+"
        at its shoulder: the "+" of the suite's names (Gamify+, Author+, ...) is in every icon.

    Every package asset is written under WACK's 204800-byte limit for logo images: 256 colours
    first, then 128, 64, 32 until it fits. On flat artwork like this that is invisible — the
    1240 px tile at 32 colours is indistinguishable from 24-bit. The Partner Center uploads are
    not capped and stay 24-bit.

    Outputs:
      packaging/Assets/                MSIX tiles - unqualified (scale-100) plus scale-* variants
      resources/images/store/          Partner Center upload images
      src/Statistle.App/Statistle.ico  the .exe / installer icon (skip with -SkipIco)

    Requires ImageMagick 7 (magick.exe) on PATH.

.EXAMPLE
    ./packaging/make-store-assets.ps1
#>
[CmdletBinding()]
param(
    [switch]$SkipIco
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

$assetsDir = Join-Path $repoRoot 'packaging/Assets'
$storeDir  = Join-Path $repoRoot 'resources/images/store'
$workDir   = Join-Path ([System.IO.Path]::GetTempPath()) 'statistle-store-assets'

foreach ($d in @($assetsDir, $storeDir, $workDir)) { New-Item -ItemType Directory -Force $d | Out-Null }

function Invoke-Magick {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)
    & magick @Arguments
    if ($LASTEXITCODE -ne 0) { throw "magick failed: $($Arguments -join ' ')" }
}

# --- palette, read out of the logo -----------------------------------------------------------
$navy   = '#142E4F'   # the logo's ground
$cyan   = '#01A9E6'   # the central band
$green  = '#07C05A'
$orange = '#E38706'
$white  = '#FFFFFF'
# magick strips backslashes out of -font paths, so keep it forward-slashed.
$font   = $env:WINDIR.Replace('\', '/') + '/Fonts/segoeuib.ttf'

# WACK fails a package whose logo images exceed this; see Write-Capped.
$MaxAssetBytes = 204800

# --- the mark ---------------------------------------------------------------------------------
# The logo's curve, redrawn at 2048 so every emitted size is a downscale. The bands are painted
# by masking vertical colour stripes with the filled bell, which is far simpler than expressing
# three slices of one curve as three MVG paths.
$bell = 'M 190,1650 C 690,1638 712,470 1024,470 C 1336,470 1358,1638 1858,1650'

$bellMask = Join-Path $workDir 'bell-mask.png'
$stripes  = Join-Path $workDir 'stripes.png'
$bands    = Join-Path $workDir 'bands.png'
$mark     = Join-Path $workDir 'mark-2048.png'
$glyph    = Join-Path $workDir 'glyph-2048.png'
$plusMark = Join-Path $workDir 'plus-mark-2048.png'

Invoke-Magick -size 2048x2048 'xc:none' -fill $white -stroke none -draw "path '$bell Z'" $bellMask
Invoke-Magick -size 2048x2048 "xc:$orange" `
    -fill $green -draw 'rectangle 540,0 1508,2048' `
    -fill $cyan  -draw 'rectangle 790,0 1258,2048' $stripes
Invoke-Magick $stripes $bellMask -alpha off -compose CopyOpacity -composite $bands

# The outline and the axis, drawn over the bands: once on the navy ground for the mark, once on
# transparency for compositing onto the wide tile.
$outline = @(
    '-draw', "stroke-linecap round fill none stroke $white stroke-width 58 path '$bell'",
    '-draw', "stroke-linecap round stroke $white stroke-width 36 line 150,1670 1898,1670"
)
# The "+" sits in the empty corner above the bell's right shoulder, clear of the outline.
$plus = @('-fill', $white, '-stroke', 'none',
          '-draw', 'roundrectangle 1380,500 1880,640 24,24',
          '-draw', 'roundrectangle 1560,320 1700,820 24,24')
Invoke-Magick -size 2048x2048 "xc:$navy" $bands -compose over -composite @outline @plus -strip $mark
Invoke-Magick -size 2048x2048 'xc:none'  $bands -compose over -composite @outline -strip $glyph

# --- the logo ---------------------------------------------------------------------------------
# The name above the bell, as the old Statistle logo had it. The name carries the "+", so the
# bell here is the plain glyph.
$artFull = Join-Path $workDir 'art-full.png'
$word    = Join-Path $workDir 'wordmark.png'
Invoke-Magick -background none -fill $white -font $font -size 1640x420 -gravity center `
    'caption:Statistics+' -strip $word
Invoke-Magick -size 2048x2048 "xc:$navy" `
    $word -gravity north -geometry '+0+190' -compose over -composite `
    '(' $glyph -filter Lanczos -resize 1420x1420 ')' -gravity south -geometry '+0+80' -composite `
    -strip $artFull

# --- emitters ---------------------------------------------------------------------------------
function Write-Capped {
    # Write Src at Width x Height, quantised only as far as it must be to clear the byte cap.
    # 256 colours covers almost everything here; the 1240 px tile needs 32, which on flat
    # artwork is indistinguishable from 24-bit.
    param([string]$Src, [int]$Width, [int]$Height, [string]$Dest, [string]$Mode = 'Fit')

    foreach ($colors in 256, 128, 64, 32) {
        $cmd = if ($Mode -eq 'Pad') {
            @('-size', "${Width}x${Height}", "xc:$navy",
              '(', $Src, '-filter', 'Lanczos', '-resize', "${Width}x${Height}", ')',
              '-gravity', 'center', '-composite')
        } else {
            @($Src, '-filter', 'Lanczos', '-resize', "${Width}x${Height}^",
              '-gravity', 'center', '-extent', "${Width}x${Height}")
        }
        Invoke-Magick @cmd -colors $colors -strip $Dest
        if ((Get-Item $Dest).Length -le $MaxAssetBytes) { return }
    }
    Write-Warning ("{0} is {1} bytes, over WACK's {2} limit even at 32 colours." -f `
        (Split-Path -Leaf $Dest), (Get-Item $Dest).Length, $MaxAssetBytes)
}

function Write-Plain {
    # Uncapped, full colour - for the Partner Center uploads, which are not in the package.
    param([string]$Src, [int]$Width, [int]$Height, [string]$Dest, [string]$Mode = 'Fit')
    if ($Mode -eq 'Pad') {
        Invoke-Magick -size "${Width}x${Height}" "xc:$navy" `
            '(' $Src -filter Lanczos -resize "${Width}x${Height}" ')' `
            -gravity center -composite -strip $Dest
    } else {
        Invoke-Magick $Src -filter Lanczos -resize "${Width}x${Height}^" `
            -gravity center -extent "${Width}x${Height}" -strip $Dest
    }
}

$scales = @(
    @{ Suffix = '.scale-100'; Factor = 1.00 },
    @{ Suffix = '.scale-125'; Factor = 1.25 },
    @{ Suffix = '.scale-150'; Factor = 1.50 },
    @{ Suffix = '.scale-200'; Factor = 2.00 },
    @{ Suffix = '.scale-400'; Factor = 4.00 }
)

function Write-TileSet {
    param([string]$Name, [int]$Width, [int]$Height, [string]$Src, [string]$Mode = 'Fit')
    foreach ($s in $scales) {
        $w = [int][math]::Round($Width  * $s.Factor)
        $h = [int][math]::Round($Height * $s.Factor)
        $dest = Join-Path $assetsDir "$Name$($s.Suffix).png"
        Write-Capped $Src $w $h $dest $Mode
        if ($s.Suffix -eq '.scale-100') { Copy-Item $dest (Join-Path $assetsDir "$Name.png") -Force }
    }
    Write-Host "  $Name" -ForegroundColor DarkGray
}

function Write-TargetSizes {
    param([string]$Name, [string]$Src)
    foreach ($t in 16, 24, 32, 48, 256) {
        Write-Capped $Src $t $t (Join-Path $assetsDir "$Name.targetsize-$t.png")
        Write-Capped $Src $t $t (Join-Path $assetsDir ("$Name.targetsize-$t" + "_altform-unplated.png"))
    }
}

function Write-WideTile {
    # The glyph beside the name, on flat navy. The logo is not used here: cropped to 310x150 it
    # keeps either the wordmark or the curve, never both.
    # The name is set as caption: so it fits the column whatever the scale.
    param([int]$Width, [int]$Height, [string]$Dest)
    $margin = [int]($Width * 0.05)
    $artW   = [int]($Height * 0.70)
    $textX  = $margin + $artW + [int]($Width * 0.04)
    $textW  = $Width - $textX - $margin
    $name   = Join-Path $workDir "wide-name-$Width.png"
    Invoke-Magick -background none -fill $white -font $font `
        -size "${textW}x$([int]($Height * 0.24))" -gravity west 'caption:Statistics+' -strip $name
    Invoke-Magick -size "${Width}x${Height}" "xc:$navy" `
        '(' $glyph -filter Lanczos -resize "${artW}x${artW}" ')' `
        -gravity west -geometry "+$margin+0" -compose over -composite `
        $name -gravity west -geometry "+$textX+0" -composite `
        -colors 256 -strip $Dest
}

function Write-Hero {
    # The logo left, a tagline right. The logo already carries the name, so it is not repeated
    # beside it. The copy is rendered as caption: layers so magick fits the point size to the
    # column - hard-coded sizes overflow as soon as the wording or the canvas changes.
    param([int]$Width, [int]$Height, [string]$Dest)
    $margin = [int]($Width * 0.05)
    # The logo is square and the canvas is 2:1, so a logo sized to the full height leaves the
    # copy in a column too narrow to set it at a readable size - caption: fits to width here.
    $artW   = [int]($Height * 0.66)
    $textX  = $margin + $artW + [int]($Width * 0.05)
    $textW  = $Width - $textX - $margin

    $sub = Join-Path $workDir "hero-sub-$Width.png"
    $tag = Join-Path $workDir "hero-tag-$Width.png"
    Invoke-Magick -background none -fill $white -font $font `
        -size "${textW}x$([int]($Height * 0.115))" -gravity west `
        'caption:Learn statistics for engineers' -strip $sub
    Invoke-Magick -background none -fill '#9FC4E4' -font $font `
        -size "${textW}x$([int]($Height * 0.058))" -gravity west `
        'caption:Lessons, exercises, and the Python and R for every result' -strip $tag

    Invoke-Magick -size "${Width}x${Height}" "xc:$navy" `
        '(' $artFull -filter Lanczos -resize "${artW}x${artW}" ')' `
        -gravity west -geometry "+$margin+0" -compose over -composite `
        -gravity northwest `
        $sub -geometry "+$textX+$([int]($Height * 0.34))" -composite `
        $tag -geometry "+$textX+$([int]($Height * 0.53))" -composite `
        -strip $Dest
    Write-Host "  $(Split-Path -Leaf $Dest)" -ForegroundColor DarkGray
}

# --- MSIX tiles --------------------------------------------------------------------------------
Write-Host 'MSIX tiles -> packaging/Assets' -ForegroundColor Cyan
Write-TileSet 'Square44x44Logo'   44  44  $mark
Write-TileSet 'Square71x71Logo'   71  71  $mark
Write-TileSet 'Square150x150Logo' 150 150 $mark
Write-TileSet 'StoreLogo'         50  50  $mark
Write-TileSet 'StatistleFile'     44  44  $mark
Write-TileSet 'Square310x310Logo' 310 310 $artFull
# The manifest declares the splash background as this navy, so the logo sits on its own colour.
Write-TileSet 'SplashScreen'      620 300 $artFull 'Pad'
Write-TargetSizes 'Square44x44Logo' $mark
Write-TargetSizes 'StatistleFile'   $mark

foreach ($s in $scales) {
    Write-WideTile ([int][math]::Round(310 * $s.Factor)) ([int][math]::Round(150 * $s.Factor)) `
        (Join-Path $assetsDir "Wide310x150Logo$($s.Suffix).png")
}
Copy-Item (Join-Path $assetsDir 'Wide310x150Logo.scale-100.png') (Join-Path $assetsDir 'Wide310x150Logo.png') -Force
Write-Host '  Wide310x150Logo' -ForegroundColor DarkGray

# --- Partner Center listing images -------------------------------------------------------------
Write-Host 'Store listing -> resources/images/store' -ForegroundColor Cyan
Write-Plain $mark    300  300  (Join-Path $storeDir 'StoreLogo-300x300.png')
# The program's icon on SoftwarePlus.ai (site/images/icons/statistics-plus.png) is this one too.
Write-Plain $mark    360  360  (Join-Path $storeDir 'SiteIcon-360x360.png')
Write-Plain $artFull 1080 1080 (Join-Path $storeDir 'BoxArt-1080x1080.png')
Write-Plain $artFull 720  1080 (Join-Path $storeDir 'PosterArt-720x1080.png') 'Pad'
Write-Hero 2400 1200 (Join-Path $storeDir 'SuperHeroArt-2400x1200.png')
Write-Hero 1920 1080 (Join-Path $storeDir 'HeroImage-1920x1080.png')

# --- the .exe icon -----------------------------------------------------------------------------
if (-not $SkipIco) {
    Write-Host 'Application icon -> src/Statistle.App/Statistle.ico' -ForegroundColor Cyan
    Invoke-Magick $mark -filter Lanczos `
        -define 'icon:auto-resize=256,128,64,48,40,32,24,20,16' `
        (Join-Path $repoRoot 'src/Statistle.App/Statistle.ico')
}

# --- contact sheet -----------------------------------------------------------------------------
$sheet = Join-Path $storeDir 'asset-contact-sheet.png'
Push-Location $assetsDir
try {
    & magick montage `
        'Square44x44Logo.targetsize-16.png' 'Square44x44Logo.targetsize-32.png' `
        'Square44x44Logo.png' 'Square71x71Logo.png' 'Square150x150Logo.png' `
        'Square310x310Logo.png' 'Wide310x150Logo.png' 'SplashScreen.png' `
        -tile 4x2 -geometry '+10+10<' -background '#909090' $sheet
    if ($LASTEXITCODE -ne 0) { throw 'montage failed.' }
}
finally { Pop-Location }

$pngs = Get-ChildItem $assetsDir -Filter *.png
$over = @($pngs | Where-Object Length -gt $MaxAssetBytes)
Write-Host ''
Write-Host ("Assets: {0} files, {1:N1} MB, {2} over the {3}-byte cap" -f `
    $pngs.Count, (($pngs | Measure-Object Length -Sum).Sum / 1MB), $over.Count, $MaxAssetBytes) `
    -ForegroundColor Green
Write-Host "Contact sheet: $sheet" -ForegroundColor Green
