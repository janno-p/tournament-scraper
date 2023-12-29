module TournamentScraper.Tournaments.Controllers

open Akka.FSharp
open Saturn
open TournamentScraper.Scraper

let private tournaments =
    fun ctx ->
        task {
            let! tournaments = Model.find ctx
            let view = View.index tournaments
            scraper <! LoadUrl "https://test"
            return! Controller.renderHtml ctx view
        }

let TournamentController =
    controller {
        index tournaments
    }
