module Scraper

open System
open Akka.Configuration
open Akka.Dispatch
open Akka.FSharp
open Dapper
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Saturn.Application
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let reloadTournamentsButton =
    button [_hxPost "/tournaments/reload"; _hxSwap "outerHTML"] [rawText "Reload tournaments"]

type ScraperActorMessages =
    | ReloadTournaments

let system = System.create "ScraperSystem" <| ConfigurationFactory.Default()

let createScraper (applicationServices: IServiceProvider) =
    spawn system "ScraperActor"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                match message with
                | ReloadTournaments ->
                    ActorTaskScheduler.RunTask(System.Func<System.Threading.Tasks.Task>(fun () -> task {
                        use scope = applicationServices.CreateScope()
                        let! connection = scope.ServiceProvider.GetRequiredService<SqlConnectionFactory>().GetOpenConnectionAsync()
                        printfn "Reloading tournaments ..."
                        let! count = Playwright.updateTournaments connection
                        printfn $"Total of {count} tournaments updated"
                        Sse.enqueue (Sse.TournamentsLoaded reloadTournamentsButton)
                    }))
                return! loop()
            }
        loop()

type ApplicationBuilder with
    [<CustomOperation("use_scraper")>]
    member this.UseScraper(state: ApplicationState) =
        let service (s: IServiceCollection) = s.AddSingleton<Akka.Actor.IActorRef>(createScraper)
        { state with
            ServicesConfig = service::state.ServicesConfig 
        }

let getScraper (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<Akka.Actor.IActorRef>()
