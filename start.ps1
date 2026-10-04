# Starts MongoDB (portable), the .NET API and the Angular dev server as hidden background processes.
# Usage: powershell -ExecutionPolicy Bypass -File .\start.ps1      (stop with .\stop.ps1)
$root = $PSScriptRoot
$env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [Environment]::GetEnvironmentVariable("Path", "User")
$env:NG_CLI_ANALYTICS = "false"
$env:ASPNETCORE_ENVIRONMENT = "Development"

New-Item -ItemType Directory -Force "$root\data\db", "$root\data\log" | Out-Null

$mongod = Get-ChildItem "$root\tools\mongodb" -Recurse -Filter mongod.exe -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $mongod) { Write-Error "mongod.exe not found under tools\mongodb. See README."; exit 1 }

if (-not (Get-NetTCPConnection -LocalPort 27017 -State Listen -ErrorAction SilentlyContinue)) {
    Start-Process $mongod.FullName -ArgumentList '--dbpath', "$root\data\db", '--bind_ip', '127.0.0.1', '--port', '27017', '--logpath', "$root\data\log\mongod.log" -WindowStyle Hidden
    Start-Sleep -Seconds 5
}

if (-not (Get-NetTCPConnection -LocalPort 5211 -State Listen -ErrorAction SilentlyContinue)) {
    Start-Process dotnet -ArgumentList 'run', '--no-launch-profile', '--urls', 'http://localhost:5211' -WorkingDirectory "$root\backend" -WindowStyle Hidden `
        -RedirectStandardOutput "$root\data\log\api.out.log" -RedirectStandardError "$root\data\log\api.err.log"
}

if (-not (Get-NetTCPConnection -LocalPort 4200 -State Listen -ErrorAction SilentlyContinue)) {
    Start-Process "$env:ProgramFiles\nodejs\node.exe" -ArgumentList 'node_modules/@angular/cli/bin/ng.js', 'serve', '--port', '4200' -WorkingDirectory "$root\frontend" -WindowStyle Hidden `
        -RedirectStandardOutput "$root\data\log\web.out.log" -RedirectStandardError "$root\data\log\web.err.log"
}

Write-Host "Starting... give it ~30s, then open:"
Write-Host "  Store : http://localhost:4200"
Write-Host "  API   : http://localhost:5211/swagger"
Write-Host "  Admin : admin@example.com / Admin@123"
