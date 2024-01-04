module TournamentScraper.Tournaments.Controllers

open Actors
open Helpers
open System
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn

let private tournaments =
    fun ctx ->
        task {
            let! tournaments = Model.find ctx
            let view = View.index tournaments
            return! Controller.renderHtml ctx (App.layout [view])
        }

let private reload : HttpHandler =
    fun next ctx ->
        ReloadTournaments >>> ctx
        (next, ctx) ||> (tryHtmx (htmlView View.loadingButton) >=> Successful.ACCEPTED ()) 

let private reloadTournament (key: Guid) : HttpHandler =
    fun next ctx ->
        ReloadTournament key >>> ctx
        (next, ctx) ||> (tryHtmx (View.loadingTournamentButton key |> htmlView) >=> Successful.ACCEPTED ())

let private tournament (ctx: HttpContext) (key: Guid) =
    task {
        let! tournament = Model.get ctx key
        let view = tournament |> Option.map View.show |> Option.defaultWith (fun _ -> NotFound.layout)
        return! Controller.renderHtml ctx (App.layout [view])
    }

let private TournamentController =
    controller {
        index tournaments
        show tournament
    }

let tournamentRoutes = router {
    post "/reload" reload
    postf "/%O/reload" reloadTournament
    forward "" TournamentController
}
