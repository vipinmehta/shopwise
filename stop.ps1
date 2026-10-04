# Stops the processes listening on the MongoDB, API and Angular ports.
foreach ($port in 4200, 5211, 27017) {
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
}
Write-Host "Stopped."
