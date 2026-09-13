# 去掉 Godot 导出时硬塞进资源包的 project.binary / uid_cache.bin / global_script_class_cache.cfg。
#
# 原因：游戏用 ProjectSettings.LoadResourcePack(path) 加载 mod 资源包，第二个参数默认 true，
# 也就是包里的工程设置和 UID 索引会覆盖游戏自己的。UID 索引一旦被顶掉，
# 所有靠 UID 引用的贴图和 Spine 资源就都找不到，表现为选人背景和战斗画面全黑。
# 成熟 mod（观者、海克斯拓展包）的资源包里都没有这三个文件，就是这个原因。
#
# 用法：powershell -File repack-pck.ps1 -PckPath <要处理的 .pck>

param([Parameter(Mandatory=$true)][string]$PckPath)

$ErrorActionPreference = 'Stop'
$drop = 'project\.binary|uid_cache\.bin|global_script_class_cache\.cfg'

$b = [System.IO.File]::ReadAllBytes($PckPath)
$fileBase = [int][BitConverter]::ToUInt64($b, 24)
$dirOff   = [int][BitConverter]::ToUInt64($b, 32)
$count    = [BitConverter]::ToUInt32($b, $dirOff)

$p = $dirOff + 4
$entries = @()
for ($i = 0; $i -lt $count; $i++) {
    $len = [BitConverter]::ToUInt32($b, $p); $p += 4
    $name = [System.Text.Encoding]::UTF8.GetString($b, $p, $len); $p += $len
    $p += (4 - ($len % 4)) % 4
    $off = [BitConverter]::ToUInt64($b, $p); $p += 8
    $size = [BitConverter]::ToUInt64($b, $p); $p += 8
    $md5 = $b[$p..($p + 15)]; $p += 16
    $flags = [BitConverter]::ToUInt32($b, $p); $p += 4
    $entries += [pscustomobject]@{ Name = $name; Offset = $off; Size = $size; Md5 = $md5; Flags = $flags }
}

$keep = $entries | Where-Object { $_.Name -notmatch $drop }
if ($keep.Count -eq $entries.Count) {
    Write-Host "资源包无需处理：$PckPath"
    return
}

$ms = New-Object System.IO.MemoryStream
$ms.Write($b, 0, $fileBase)
$pos = $fileBase
$newEntries = @()
foreach ($e in $keep) {
    $pad = (16 - ($pos % 16)) % 16
    if ($pad -gt 0) { $ms.Write((New-Object byte[] $pad), 0, $pad); $pos += $pad }
    $rel = $pos - $fileBase
    $ms.Write($b, [int]($fileBase + $e.Offset), [int]$e.Size)
    $pos += $e.Size
    $newEntries += [pscustomobject]@{ Name = $e.Name; Offset = $rel; Size = $e.Size; Md5 = $e.Md5; Flags = $e.Flags }
}

$dirPos = $pos
$dms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($dms)
$bw.Write([uint32]$newEntries.Count)
foreach ($e in $newEntries) {
    $nb = [System.Text.Encoding]::UTF8.GetBytes($e.Name)
    $bw.Write([uint32]$nb.Length)
    $bw.Write($nb)
    $pad = (4 - ($nb.Length % 4)) % 4
    if ($pad -gt 0) { $bw.Write((New-Object byte[] $pad)) }
    $bw.Write([uint64]$e.Offset)
    $bw.Write([uint64]$e.Size)
    $bw.Write([byte[]]$e.Md5, 0, 16)
    $bw.Write([uint32]$e.Flags)
}
$bw.Flush()
$dirBytes = $dms.ToArray()
$ms.Write($dirBytes, 0, $dirBytes.Length)

$outBytes = $ms.ToArray()
[BitConverter]::GetBytes([uint64]$dirPos).CopyTo($outBytes, 32)
[System.IO.File]::WriteAllBytes($PckPath, $outBytes)

Write-Host "资源包已清理：$($entries.Count - $keep.Count) 个多余文件被移除（$PckPath）"