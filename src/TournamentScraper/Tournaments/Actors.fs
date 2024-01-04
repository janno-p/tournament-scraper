module TournamentScraper.Tournaments.Actors

open System
open Akka.Configuration
open Akka.FSharp
open Dapper
open Helpers
open Microsoft.Extensions.DependencyInjection

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
                        let! count = Playwright.updateTournaments connection
                        printfn $"Total of {count} tournaments updated"
                        Sse.enqueue (Sse.TournamentsLoaded View.reloadTournamentsButton)
                    })
                | ReloadTournament key ->
                    runActorTask (fun () -> task {
                        printfn $"Reloading tournament details ..."
                        do! Async.Sleep (TimeSpan.FromSeconds 5.)
                        Sse.enqueue (Sse.TournamentLoaded (key, View.reloadTournamentButton key))
                    })
                return! loop()
            }
        loop()
