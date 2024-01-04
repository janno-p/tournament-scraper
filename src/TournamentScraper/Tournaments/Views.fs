namespace TournamentScraper.Tournaments

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open System

module View =
    let private tournamentDate (tournament: Tournament) =
        if tournament.StartDate = tournament.EndDate then tournament.StartDate.ToString("dd.MM.yyyy") else
        let s = tournament.StartDate.ToString("dd.MM.yyyy")
        let e = tournament.EndDate.ToString("dd.MM.yyyy")
        $"{s} - {e}"
    
    let private tournamentRow (tournament: Tournament) =
        let date = tournamentDate tournament
        li [] [
            a [_href $"/tournaments/{tournament.Id:D}"] [encodedText tournament.Name]
            rawText $"&nbsp;({date})"
        ]
        
    let reloadTournamentsButton =
        button [_hxPost "/tournaments/reload"; _hxSwap "outerHTML"] [encodedText "Reload tournaments"]
        
    let reloadTournamentButton (id: Guid) =
        button [_hxPost $"/tournaments/{id:D}/reload"; _hxSwap "outerHTML"] [encodedText "Reload tournament"]
    
    let index (tournaments: Tournament list) : XmlNode =
        div [] [
            reloadTournamentsButton
            ul [_class "border mt-4"] (tournaments |> List.map tournamentRow)
        ]
        
    let show (tournament: Tournament) : XmlNode =
        div [] [
            h1 [] [encodedText tournament.Name]
            em [] [encodedText $"({tournamentDate tournament})"]
            reloadTournamentButton tournament.Id
        ]
        
    let loadingButton : XmlNode =
        div [_hxExt "sse"; attr "sse-connect" "/sse"; attr "sse-swap" "TournamentsLoaded"; _hxSwap "outerHTML"] [
            rawText "Loading ..."
        ]
        
    let loadingTournamentButton (key: Guid) : XmlNode =
        div [_hxExt "sse"; attr "sse-connect" "/sse"; attr "sse-swap" $"TournamentLoaded/{key:D}"; _hxSwap "outerHTML"] [
            encodedText "Loading ..."
        ]
