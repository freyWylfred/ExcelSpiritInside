param(
	[Parameter(Mandatory = $true)] [string] $Png,
	[Parameter(Mandatory = $true)] [string] $Ico
)

Add-Type -AssemblyName System.Drawing

$sizes = 16, 24, 32, 48, 64, 128, 256
$src = [System.Drawing.Image]::FromFile($Png)

$pngStreams = @()
foreach ($s in $sizes) {
	$bmp = New-Object System.Drawing.Bitmap $s, $s
	$g = [System.Drawing.Graphics]::FromImage($bmp)
	$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
	$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
	$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
	$g.DrawImage($src, 0, 0, $s, $s)
	$g.Dispose()

	$ms = New-Object System.IO.MemoryStream
	$bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
	$bmp.Dispose()
	$pngStreams += , $ms.ToArray()
	$ms.Dispose()
}
$src.Dispose()

$fs = [System.IO.File]::Create($Ico)
$bw = New-Object System.IO.BinaryWriter($fs)

# ICONDIR
$bw.Write([UInt16]0)                    # reserved
$bw.Write([UInt16]1)                    # type = icon
$bw.Write([UInt16]$pngStreams.Count)    # count

$offset = 6 + (16 * $pngStreams.Count)
for ($i = 0; $i -lt $pngStreams.Count; $i++) {
	$s = $sizes[$i]
	$data = $pngStreams[$i]
	$dim = if ($s -ge 256) { 0 } else { $s }
	$bw.Write([Byte]$dim)   # width
	$bw.Write([Byte]$dim)   # height
	$bw.Write([Byte]0)      # colors
	$bw.Write([Byte]0)      # reserved
	$bw.Write([UInt16]1)    # planes
	$bw.Write([UInt16]32)   # bpp
	$bw.Write([UInt32]$data.Length)
	$bw.Write([UInt32]$offset)
	$offset += $data.Length
}
foreach ($data in $pngStreams) {
	$bw.Write($data)
}
$bw.Flush()
$bw.Close()
$fs.Close()
Write-Host "ICO written: $Ico"
