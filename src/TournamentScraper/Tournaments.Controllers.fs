module TournamentScraper.Tournaments.Controllers

open Oxpecker
open System
open TournamentScraper.Templates
open TournamentScraper.Templates.Errors
open TournamentScraper.Tournaments.Actors
open TournamentScraper.Tournaments.Helpers
open type Microsoft.AspNetCore.Http.TypedResults

let private reloadHandler : EndpointHandler =
    fun ctx ->
        ReloadTournaments >>> ctx
        (tryHtmx (htmlView View.loadingButton) >=> %Accepted("")) ctx

let private reloadTournamentHandler (key: Guid) : EndpointHandler =
    fun ctx ->
        ReloadTournament key >>> ctx
        (tryHtmx (View.loadingTournamentButton key |> htmlView) >=> %Accepted("")) ctx

let private tournamentsHandler : EndpointHandler =
    fun ctx ->
        task {
            let! tournaments = Model.find ctx 
            let view = App.layout (View.index tournaments) ctx
            return! htmlView view ctx
        }

let private tournamentHandler (key: Guid) : EndpointHandler =
    fun ctx -> task {
        let! tournament = Model.get ctx key
        let! view =
            match tournament with
            | Some tournament ->
                task {
                    let! events = Model.getEvents ctx key
                    return View.show tournament events
                }
            | None ->
                System.Threading.Tasks.Task.FromResult(notFound)
        do! htmlView (App.layout view ctx) ctx
    }

let endpoints: Endpoint list = [
    POST [
        route "/reload" reloadHandler
        routef "/{%O:guid}/reload" reloadTournamentHandler
    ]
    GET [
        route "/" tournamentsHandler
        routef "/{%O:guid}" tournamentHandler
    ]
]
