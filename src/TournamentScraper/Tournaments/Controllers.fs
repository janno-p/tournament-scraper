module TournamentScraper.Tournaments.Controllers

open Akka.FSharp
open Saturn
open Scraper

let private tournaments =
    fun ctx ->
        task {
            let! tournaments = Model.find ctx
            let view = View.index tournaments
            let scraper = getScraper ctx
            scraper <! ReloadTournaments
            return! Controller.renderHtml ctx (App.layout [view])
        }

let TournamentController =
    controller {
        index tournaments
    }
