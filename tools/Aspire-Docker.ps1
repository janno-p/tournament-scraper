Push-Location -Path "$PSScriptRoot\.."

dotnet run --project .\src\TournamentScraper.AppHost\TournamentScraper.AppHost.csproj

Pop-Location
