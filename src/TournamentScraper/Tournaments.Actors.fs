module TournamentScraper.Tournaments.Actors

open System
open Akka.Configuration
open Akka.FSharp
open Helpers
open Microsoft.Extensions.DependencyInjection
open TournamentScraper
open TournamentScraper.Database

type TournamentsActorMessages =
    | ReloadTournaments
    | ReloadTournament of Guid

let private tournamentsSystem =
    System.create "TournamentsSystem" <| ConfigurationFactory.Default()

let createTournamentsActor (applicationServices: IServiceProvider) =
    spawn tournamentsSystem "TournamentsActor"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                match message with
                | ReloadTournaments ->
                    runActorTask (fun () -> task {
                        use scope = applicationServices.CreateScope()
                        let! connection = scope.ServiceProvider.GetRequiredService<SqlConnectionFactory>().GetOpenConnectionAsync()
                        printfn "Reloading tournaments ..."
                        use! playwright = Microsoft.Playwright.Playwright.CreateAsync()
                        let! page = Playwright.openNewPage playwright
                        let! count = Playwright.updateTournaments connection page
                        printfn $"Total of {count} tournaments updated"
                        Sse.enqueue (Sse.TournamentsLoaded View.reloadTournamentsButton)
                    })
                | ReloadTournament key ->
                    runActorTask (fun () -> task {
                        use scope = applicationServices.CreateScope()
                        let! connection = scope.ServiceProvider.GetRequiredService<SqlConnectionFactory>().GetOpenConnectionAsync()
                        use! playwright = Microsoft.Playwright.Playwright.CreateAsync()
                        let! page = Playwright.openNewPage playwright
                        printfn $"Reloading tournament details ..."
                        do! Playwright.TournamentEvents.load key false connection page
                        Sse.enqueue (Sse.TournamentLoaded (key, View.reloadTournamentButton key))
                    })
                return! loop()
            }
        loop()
