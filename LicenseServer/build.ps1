Set-StrictMode -Version Latest
$ErrorActionPreference="Stop"
function Retry{param([scriptblock]$Action,[int]$Attempts=8,[int]$Delay=2)for($i=1;$i -le $Attempts;$i++){try{& $Action;return}catch{if($i -eq $Attempts){throw};Start-Sleep -Seconds $Delay}}}
function RunDotnet([string[]]$a){Retry{ { & dotnet @a } }}
Write-Host "================================"
Write-Host "License Server Build"
Write-Host "================================"
if(-not (Get-Command dotnet -ErrorAction SilentlyContinue)){Write-Error ".NET SDK nicht gefunden";exit 1}
$ver=& dotnet --version
Write-Host ".NET SDK:" $ver
$csproj=Get-ChildItem -Path . -Filter *.csproj -File -ErrorAction SilentlyContinue|Select-Object -First 1
if(-not $csproj){RunDotnet @("new","console","-f","net8.0","--force");$csproj=Get-ChildItem -Filter *.csproj|Select-Object -First 1}
RunDotnet @("restore",$csproj.Name)
RunDotnet @("add",$csproj.Name,"package","Raylib-cs","--version","5.0.0")
RunDotnet @("add",$csproj.Name,"package","System.Text.Json","--version","8.0.0")
RunDotnet @("add",$csproj.Name,"package","DotNetEnv","--version","2.4.0")
RunDotnet @("restore",$csproj.Name)
RunDotnet @("build",$csproj.Name,"--configuration","Release","--no-restore")
$rids=@("win-x64","linux-x64","osx-arm64")
foreach($rid in $rids){
$out=Join-Path "publish" $rid
RunDotnet @("publish",$csproj.Name,"--configuration","Release","--runtime",$rid,"--self-contained","true","-p:PublishSingleFile=true","-p:IncludeNativeLibrariesForSelfExtract=true","--output",$out)
foreach($f in @("config.json","licenses.json")){if(Test-Path $f){Copy-Item $f $out -Force}}
}
Write-Host "Fertig."
Get-ChildItem -Recurse -File publish | Select-Object FullName,Length | Format-Table -AutoSize
