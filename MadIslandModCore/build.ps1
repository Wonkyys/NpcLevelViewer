$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$gameDir = Resolve-Path "$scriptDir\..\..\.."
$managedDir = "$gameDir\Mad Island_Data\Managed"
$coreDir = "$gameDir\BepInEx\core"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

$outDll = "$scriptDir\MadIslandModCore.dll"

$references = @(
    "/noconfig",
    "/nostdlib+",
    "/reference:$managedDir\mscorlib.dll",
    "/reference:$managedDir\netstandard.dll",
    "/reference:$managedDir\System.dll",
    "/reference:$managedDir\System.Core.dll",
    "/reference:$coreDir\BepInEx.dll",
    "/reference:$coreDir\0Harmony.dll",
    "/reference:$managedDir\UnityEngine.dll",
    "/reference:$managedDir\UnityEngine.CoreModule.dll",
    "/reference:$managedDir\UnityEngine.IMGUIModule.dll",
    "/reference:$managedDir\UnityEngine.InputLegacyModule.dll",
    "/reference:$managedDir\UnityEngine.TextRenderingModule.dll",
    "/reference:$managedDir\UnityEngine.UI.dll",
    "/reference:$managedDir\Assembly-CSharp.dll"
)

$sources = (Get-ChildItem "$scriptDir\src\*.cs" | ForEach-Object { $_.FullName })

$args = @(
    "/target:library",
    "/optimize",
    "/out:$outDll"
) + $references + $sources

Write-Host "Compilando MadIslandModCore.dll..." -ForegroundColor Cyan
& $csc $args

if ($LASTEXITCODE -eq 0 -and (Test-Path $outDll)) {
    $size = (Get-Item $outDll).Length
    Write-Host "EXITO: MadIslandModCore.dll compilado correctamente ($size bytes)" -ForegroundColor Green
} else {
    Write-Host "ERROR: Fallo la compilacion." -ForegroundColor Red
}
