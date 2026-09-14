# 对比"Steam 下载下来的工坊副本"和"本地构建产物"是否一致。
#
# 为什么需要：Steam 客户端那边的内容是**缓存**的，作者自己订阅自己的条目时，
# 经常出现"工坊页面显示 7.3MB / Updated 刚刚，但本地 workshop\content 里还是老版本"的情况
# （Steam 复用了订阅时的那份旧 manifest 缓存）。这时候游戏会同时扫到
# mods\Aristocrat（最新）和 workshop\content\<id>（旧的），很容易以为"没推上去"。
#
# 用法（在工程根目录）：
#   powershell -ExecutionPolicy Bypass -File tools\check-workshop-copy.ps1
#   powershell -ExecutionPolicy Bypass -File tools\check-workshop-copy.ps1 -WorkshopDir "D:\...\3801464066"

param(
    [string]$WorkshopDir = "D:\steam\steamapps\workshop\content\2868840\3801464066",
    [string]$ModsDir = "D:\Steam\steamapps\common\Slay the Spire 2\mods\Aristocrat"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $WorkshopDir)) { throw "找不到工坊副本目录：$WorkshopDir（说明 Steam 还没下载这条订阅）" }
if (-not (Test-Path $ModsDir)) { throw "找不到本地构建目录：$ModsDir" }

function Hash($path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }

$files = @("Aristocrat.json", "Aristocrat.dll", "Aristocrat.pck")
$allMatch = $true

Write-Host "工坊副本: $WorkshopDir"
Write-Host "本地构建: $ModsDir"
Write-Host ""

foreach ($name in $files) {
    $w = Join-Path $WorkshopDir $name
    $l = Join-Path $ModsDir $name
    $wExists = Test-Path $w
    $lExists = Test-Path $l

    if (-not $wExists -or -not $lExists) {
        Write-Host ("{0,-18} 缺失：工坊={1} 本地={2}" -f $name, $wExists, $lExists) -ForegroundColor Yellow
        $allMatch = $false
        continue
    }

    $wLen = (Get-Item $w).Length
    $lLen = (Get-Item $l).Length
    $wHash = Hash $w
    $lHash = Hash $l
    $same = ($wLen -eq $lLen) -and ($wHash -eq $lHash)
    if (-not $same) { $allMatch = $false }

    $color = if ($same) { "Green" } else { "Red" }
    Write-Host ("{0,-18} 工坊 {1,10} 字节 / 本地 {2,10} 字节  ->  {3}" -f $name, $wLen, $lLen, $(if ($same) { "一致" } else { "不一致" })) -ForegroundColor $color
    if (-not $same) {
        Write-Host ("   工坊 sha256 = {0}" -f $wHash) -ForegroundColor DarkGray
        Write-Host ("   本地 sha256 = {0}" -f $lHash) -ForegroundColor DarkGray
    }
}

# 顺带把两边的 manifest 版本号打出来（版本没改的话游戏内看不出来，容易误判）
foreach ($pair in @(@("工坊", $WorkshopDir), @("本地", $ModsDir))) {
    $jsonPath = Join-Path $pair[1] "Aristocrat.json"
    if (Test-Path $jsonPath) {
        $version = (Get-Content $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json).version
        Write-Host ("{0} manifest version = {1}" -f $pair[0], $version)
    }
}

Write-Host ""
if ($allMatch) {
    Write-Host "结论：一致，工坊副本就是本地这一版。" -ForegroundColor Green
} else {
    Write-Host "结论：不一致。Steam 那份是缓存/旧版，让它重新下载：" -ForegroundColor Yellow
    Write-Host "  · Steam → 创意工坊页 → 取消订阅，再订阅一次；或"
    Write-Host "  · 删掉目录 $WorkshopDir 后重启 Steam/游戏，Steam 会重新下载。"
}
