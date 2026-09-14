# 对比"Steam 下载下来的工坊副本"和"我们最后上传的那份"是否一致。
#
# 为什么需要：Steam 是按**改动说明里关联的游戏版本**决定给玩家下哪一版的。
# 如果第一版上传时给改动说明关联了游戏版本、之后的补丁没关联，Steam 检测完游戏版本会一直
# 拿最早那份关联过的构建 —— 工坊页面上的大小/更新时间是新的，下下来的却是旧内容，
# 看起来就像"没推上去"。解决办法：在工坊页面把旧改动说明的关联删掉，给最新那条加上关联。
#
# 用法（在工程根目录）：
#   powershell -ExecutionPolicy Bypass -File tools\check-workshop-copy.ps1
#   powershell -ExecutionPolicy Bypass -File tools\check-workshop-copy.ps1 -LocalDir "D:\Steam\steamapps\common\Slay the Spire 2\mods\Aristocrat"

param(
    [string]$WorkshopDir = "D:\steam\steamapps\workshop\content\2868840\3801464066",
    # 默认跟"刚上传的那份"比；也可以指到游戏 mods 目录
    [string]$LocalDir = "D:\塔2mod制作\workshop\Aristocrat\content"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $WorkshopDir)) { throw "找不到工坊副本目录：$WorkshopDir（说明 Steam 还没把这条订阅下载到本地）" }
if (-not (Test-Path $LocalDir)) { throw "找不到本地对比目录：$LocalDir" }

function Hash($path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }

$files = @("Aristocrat.json", "Aristocrat.dll", "Aristocrat.pck")
$allMatch = $true

Write-Host "工坊副本: $WorkshopDir"
Write-Host "本地对比: $LocalDir"
Write-Host ""

foreach ($name in $files) {
    $w = Join-Path $WorkshopDir $name
    $l = Join-Path $LocalDir $name
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
foreach ($pair in @(@("工坊", $WorkshopDir), @("本地", $LocalDir))) {
    $jsonPath = Join-Path $pair[1] "Aristocrat.json"
    if (Test-Path $jsonPath) {
        $version = (Get-Content $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json).version
        Write-Host ("{0} manifest version = {1}" -f $pair[0], $version)
    }
}

Write-Host ""
if ($allMatch) {
    Write-Host "结论：一致，玩家下载到的就是这一版。" -ForegroundColor Green
} else {
    Write-Host "结论：不一致。先在工坊页面把旧改动说明的「关联游戏版本」删掉、给最新那条加上，然后：" -ForegroundColor Yellow
    Write-Host "  · Steam → 该条目 → 取消订阅，再订阅一次；或"
    Write-Host "  · 删掉目录 $WorkshopDir 后重启 Steam/游戏，让它重新下载。"
}
