$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProject = Join-Path $projectRoot "src\Daily_Tracker.Api\Daily_Tracker.Api.csproj"
$infrastructureProject = Join-Path $projectRoot "src\Daily_Tracker.Infrastructure\Daily_Tracker.Infrastructure.csproj"

Write-Host "Applying EF Core migrations..."
& dotnet ef database update --project $infrastructureProject --startup-project $apiProject

if ($LASTEXITCODE -ne 0) {
    throw "Migration apply failed."
}

Write-Host "Migrations applied successfully."
