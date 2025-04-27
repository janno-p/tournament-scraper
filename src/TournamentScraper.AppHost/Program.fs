open System.IO
open Aspire.Hosting
open Aspire.Hosting.ApplicationModel

[<EntryPoint>]
let main argv =
    let builder = DistributedApplication.CreateBuilder(argv)
    
    let pgUsername = builder.AddParameter("pg-username", "postgres")
    let pgPassword = builder.AddParameter("pg-password", "postgres")

    let postgres =
        builder.AddPostgres("postgres", pgUsername, pgPassword, 56635)
            .WithLifetime(ContainerLifetime.Persistent)

    let db = postgres.AddDatabase("db1")

    let solutionDir = Path.Combine(Path.GetDirectoryName(Projects.TournamentScraper_AppHost().ProjectPath), "..")
    
    let migrations =
        builder.AddExecutable("db-migrations", "dotnet", solutionDir, "evolve", "migrate", "postgresql", "-c", "Host=localhost;Port=56635;Username=postgres;Password=postgres;Database=db1", "-l", "migrations")
            .WithReference(db)
            .WaitFor(db)

    builder.AddProject<Projects.TournamentScraper>("web-app")
        .WithHttpsEndpoint(8085)
        .WithReference(db)
        .WaitFor(db)
        .WaitForCompletion(migrations)
        |> ignore

    builder.AddExecutable("tailwindcss", "npm", solutionDir, "run", "csswatch") |> ignore

    builder.Build()
        .Run()
    0
