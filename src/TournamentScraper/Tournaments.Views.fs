namespace TournamentScraper.Tournaments

open System
open Oxpecker.Htmx
open Oxpecker.ViewEngine
open TournamentScraper.Domain
open TournamentScraper.Templates

module View =
    let private tournamentDate (tournament: Tournament) =
        if tournament.StartDate = tournament.EndDate then tournament.StartDate.ToString("dd.MM.yyyy") else
        let s = tournament.StartDate.ToString("dd.MM.yyyy")
        let e = tournament.EndDate.ToString("dd.MM.yyyy")
        $"{s} - {e}"
    
    let private tournamentRow (tournament: Tournament) =
        let date = tournamentDate tournament
        li() {
            Stylebook.appLink(href = $"/tournaments/{tournament.Id:D}") { tournament.Name }
            raw "&nbsp;"
            $"({date})"
        }

    let reloadTournamentsButton =
        let btn = Stylebook.buttonDefault <| Fragment() { "Reload tournaments" }
        btn.hxPost <- "/tournaments/reload"
        btn.hxSwap <- "outerHTML"
        btn

    let reloadTournamentButton (id: Guid) =
        button(hxPost = $"/tournaments/{id:D}/reload", hxSwap = "outerHTML") { "Reload tournament" }
    
    let index (tournaments: Tournament list) =
        MainContent.layout <| Fragment() {
            reloadTournamentsButton
            ul(class' = "border mt-4") { yield! tournaments |> List.map tournamentRow }
        }

    let show (tournament: Tournament) (events: TournamentEvent list) : HtmlElement =
        div() {
            h1() { tournament.Name }
            em() { $"({tournamentDate tournament})" }
            reloadTournamentButton tournament.Id
            ul() {
                for e in events do
                    li() { e.Name }
            }
        }

    let loadingButton =
        div(hxExt = "sse", hxSwap = "outerHTML").attr("sse-connect", "/sse").attr("sse-swap", "TournamentsLoaded") { "Loading ..." }
        
    let loadingTournamentButton (key: Guid) =
        div(hxExt = "sse", hxSwap = "outerHTML").attr("sse-connect", "/sse").attr("sse-swap", $"TournamentLoaded/{key:D}") { "Loading ..." }
