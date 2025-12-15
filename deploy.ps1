Import-Module WebAdministration

# ----------------------------
# CONFIG
# ----------------------------
$siteName = "stage2.abcwarehouse.com"
$publishPath = "C:\NopABC"

# ----------------------------
# RESOLVE APP POOL
# ----------------------------
$appPoolName = (Get-Website $siteName).applicationPool

if ([string]::IsNullOrWhiteSpace($appPoolName)) {
    throw "❌ Could not resolve IIS App Pool for site '$siteName'"
}

Write-Host "✔ Using App Pool: $appPoolName"

# ----------------------------
# BUILD
# ----------------------------
Write-Host "🔨 Cleaning solution"
dotnet clean src/NopCommerce.sln

Write-Host "🔨 Building Debug"
dotnet build src/NopCommerce.sln

Write-Host "🔨 Cleaning Release"
dotnet clean src/NopCommerce.sln -c Release

Write-Host "🔨 Building Release"
dotnet build src/NopCommerce.sln -c Release

# ----------------------------
# STOP IIS
# ----------------------------
Write-Host "⏹ Stopping App Pool"
Stop-WebAppPool -Name $appPoolName
Start-Sleep -Seconds 10

# ----------------------------
# SAFE CLEANUP
# ----------------------------
Write-Host "🧹 Cleaning plugin binaries"

if (Test-Path "$publishPath\Plugins") {
    Get-ChildItem "$publishPath\Plugins" -Directory | ForEach-Object {
        $binPath = Join-Path $_.FullName "bin"
        if (Test-Path $binPath) {
            Remove-Item -Recurse -Force $binPath
        }
    }
}

# ----------------------------
# PUBLISH
# ----------------------------
Write-Host "🚀 Publishing nopCommerce"
dotnet publish `
    -c Release `
    ./src/Presentation/Nop.Web/Nop.Web.csproj `
    --no-restore `
    -o $publishPath

# ----------------------------
# START IIS
# ----------------------------
Write-Host "▶ Starting App Pool"
Start-WebAppPool -Name $appPoolName

Write-Host "✅ Deployment completed successfully"
