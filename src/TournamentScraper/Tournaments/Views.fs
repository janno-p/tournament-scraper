namespace TournamentScraper.Tournaments

open Giraffe.ViewEngine

module View =
    let index (tournaments: Tournament list) : XmlNode =
        ul [_class "border mt-4"] (tournaments |> List.map (fun x -> li [] [rawText x.Name]))
