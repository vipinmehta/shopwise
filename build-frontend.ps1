# Builds the Angular storefront for production and copies it into backend\wwwroot,
# so the API site serves the store itself. Run this, commit backend\wwwroot, then deploy.
$root = $PSScriptRoot
$env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [Environment]::GetEnvironmentVariable("Path", "User")
$env:NG_CLI_ANALYTICS = "false"

Push-Location "$root\frontend"
try {
    if (-not (Test-Path node_modules)) { npm ci }
    npx ng build
    if ($LASTEXITCODE -ne 0) { throw "ng build failed" }
} finally { Pop-Location }

$dist = "$root\frontend\dist\ecommerce-web\browser"
$target = "$root\backend\wwwroot"
if (Test-Path $target) { Remove-Item $target -Recurse -Force }
New-Item -ItemType Directory -Force $target | Out-Null
Copy-Item "$dist\*" $target -Recurse
Write-Host "Copied storefront to $target"
