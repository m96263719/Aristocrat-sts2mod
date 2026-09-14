# 把卡图四角的透明区域补满。
#
# 为什么需要：塔1 的卡图是 250x190、带透明圆角/斜角（塔1 的卡框会把它们盖住），
# 而塔2 的能力牌卡框是直角矩形，透明的地方就会露出卡框底色，看着像"卡图缺了一块"。
#
# 做法：逐行把最左/最右的不透明像素向两侧铺满；整行透明的用上方最近的内容行补；
# 顶部/底部剩下的空行用最近的内容行补。原图会被覆盖（先备份就自己拷一份）。
#
# 用法（在工程根目录）：
#   powershell -ExecutionPolicy Bypass -File tools\fill-card-art-corners.ps1

param(
    [string]$CardDir = ""
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

if ($CardDir -eq "") {
    $CardDir = Join-Path (Split-Path -Parent $PSScriptRoot) "Aristocrat\images\cards"
}
if (-not (Test-Path $CardDir)) { throw "找不到卡图目录：$CardDir" }

$alphaThreshold = 8
$files = Get-ChildItem $CardDir -Filter *.png
Write-Host "处理 $($files.Count) 张卡图：$CardDir" -ForegroundColor Cyan

foreach ($file in $files) {
    $src = [System.Drawing.Bitmap]::new($file.FullName)
    $w = [int]$src.Width
    $h = [int]$src.Height
    $dst = [System.Drawing.Bitmap]::new($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

    $lastRow = $null
    for ($y = 0; $y -lt $h; $y++) {
        $left = -1
        $right = -1
        for ($x = 0; $x -lt $w; $x++) {
            if ($src.GetPixel($x, $y).A -gt $alphaThreshold) {
                if ($left -lt 0) { $left = $x }
                $right = $x
            }
        }

        if ($left -lt 0) {
            if ($null -ne $lastRow) {
                for ($x = 0; $x -lt $w; $x++) { $dst.SetPixel($x, $y, $lastRow[$x]) }
            }
            continue
        }

        $leftColor = $src.GetPixel($left, $y)
        $rightColor = $src.GetPixel($right, $y)
        for ($x = 0; $x -lt $w; $x++) {
            if ($x -lt $left) { $dst.SetPixel($x, $y, $leftColor) }
            elseif ($x -gt $right) { $dst.SetPixel($x, $y, $rightColor) }
            else { $dst.SetPixel($x, $y, $src.GetPixel($x, $y)) }
        }

        $row = New-Object 'System.Drawing.Color[]' $w
        for ($x = 0; $x -lt $w; $x++) { $row[$x] = $dst.GetPixel($x, $y) }
        $lastRow = $row
    }

    # 顶部/底部可能还有整行透明：用最近的内容行补
    $firstContent = -1
    for ($y = 0; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) { if ($dst.GetPixel($x, $y).A -gt $alphaThreshold) { $firstContent = $y; break } }
        if ($firstContent -ge 0) { break }
    }
    if ($firstContent -gt 0) {
        for ($y = 0; $y -lt $firstContent; $y++) { for ($x = 0; $x -lt $w; $x++) { $dst.SetPixel($x, $y, $dst.GetPixel($x, $firstContent)) } }
    }

    $lastContent = -1
    for ($y = $h - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $w; $x++) { if ($dst.GetPixel($x, $y).A -gt $alphaThreshold) { $lastContent = $y; break } }
        if ($lastContent -ge 0) { break }
    }
    if ($lastContent -ge 0 -and $lastContent -lt $h - 1) {
        for ($y = $lastContent + 1; $y -lt $h; $y++) { for ($x = 0; $x -lt $w; $x++) { $dst.SetPixel($x, $y, $dst.GetPixel($x, $lastContent)) } }
    }

    $src.Dispose()
    $dst.Save("$($file.FullName).tmp.png", [System.Drawing.Imaging.ImageFormat]::Png)
    $dst.Dispose()
}

foreach ($file in $files) {
    Move-Item -LiteralPath "$($file.FullName).tmp.png" -Destination $file.FullName -Force
}

Write-Host "完成：$($files.Count) 张卡图的四角已补满。" -ForegroundColor Green
