Push-Location -Path "$PSScriptRoot\.."

$env:DOTNET_ASPIRE_CONTAINER_RUNTIME = "podman"

dotnet run --project .\src\TournamentScraper.AppHost\TournamentScraper.AppHost.fsproj

Pop-Location
