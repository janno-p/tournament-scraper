module TournamentScraper.Tournaments.Controllers

open Akka.FSharp
open Giraffe
open Giraffe.Htmx
open Saturn
open Scraper

let private tournaments =
    fun ctx ->
        task {
            let! tournaments = Model.find ctx
            let view = View.index tournaments
            return! Controller.renderHtml ctx (App.layout [view])
        }

let private reload: Core.HttpHandler =
    fun next ctx ->
        (getScraper ctx) <! ReloadTournaments
        if ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh then htmlView View.loadingButton next ctx
        else Successful.ACCEPTED () next ctx

let private TournamentController =
    controller {
        index tournaments
    }

let tournamentRoutes = router {
    post "/reload" reload
    forward "" TournamentController
}
