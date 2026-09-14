# 把这一版发到 Steam 创意工坊（更新已有条目，item id 从工作区的 mod_id.txt 读）。
#
# 用法（在工程根目录）：
#   powershell -ExecutionPolicy Bypass -File tools\upload-workshop.ps1
#   powershell -ExecutionPolicy Bypass -File tools\upload-workshop.ps1 -Notes "修了 xxx"
#
# 做的事：dotnet build -> ExportPck -> 把 json/dll/pck 拷进工坊工作区 -> 调官方上传器。
# 上传器需要 Steam 客户端在运行；首次创建条目的步骤见开发笔记「发布」一节。

param(
    [string]$Notes = ""
)

$ErrorActionPreference = "Stop"

# 这台机器上的路径（换机器时改这里，或者把环境变量设好）
$WorkspaceDir = "D:\塔2mod制作\workshop\Aristocrat"
$UploaderExe = "D:\塔2mod制作\_tools\ModUploader\bin\ModUploader.exe"
$ModsDir = "D:\Steam\steamapps\common\Slay the Spire 2\mods\Aristocrat"

$ProjectDir = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path $WorkspaceDir)) { throw "找不到工坊工作区：$WorkspaceDir" }
if (-not (Test-Path $UploaderExe)) { throw "找不到上传器：$UploaderExe" }

Write-Host "== 1/4 构建 ==" -ForegroundColor Cyan
Push-Location $ProjectDir
& dotnet build -v q
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "dotnet build 失败" }

Write-Host "== 2/4 导出 pck ==" -ForegroundColor Cyan
& dotnet build -t:ExportPck -v q
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "ExportPck 失败" }
Pop-Location

Write-Host "== 3/4 同步到工作区 ==" -ForegroundColor Cyan
foreach ($file in @("Aristocrat.json", "Aristocrat.dll", "Aristocrat.pck")) {
    Copy-Item (Join-Path $ModsDir $file) (Join-Path $WorkspaceDir "content") -Force
}

# 可选：把这次更新的说明写进 workshop.json 的 changeNote
if ($Notes -ne "") {
    $jsonPath = Join-Path $WorkspaceDir "workshop.json"
    $json = Get-Content $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $json.changeNote = $Notes
    # 用不带 BOM 的 UTF-8 写回（Set-Content 在 Windows PowerShell 5.1 下会塞 BOM）
    [System.IO.File]::WriteAllText($jsonPath, ($json | ConvertTo-Json -Depth 6), (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "   changeNote = $Notes"
}

Write-Host "== 4/4 上传到创意工坊 ==" -ForegroundColor Cyan
Push-Location (Split-Path -Parent $UploaderExe)   # 上传器要读同目录的 steam_appid.txt
& $UploaderExe upload -w $WorkspaceDir
$code = $LASTEXITCODE
Pop-Location

if ($code -ne 0) { throw "上传失败（退出码 $code）" }
Write-Host "完成。" -ForegroundColor Green
