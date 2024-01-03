namespace TournamentScraper.Tournaments

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

module View =
    let index (tournaments: Tournament list) : XmlNode =
        div [] [
            Scraper.reloadTournamentsButton
            ul [_class "border mt-4"] (tournaments |> List.map (fun x -> li [] [rawText x.Name]))
        ]
        
    let loadingButton : XmlNode =
        div [_hxExt "sse"; attr "sse-connect" "/sse"; attr "sse-swap" "TournamentsLoaded"; _hxSwap "outerHTML"] [
            rawText "Loading ..."
        ]
