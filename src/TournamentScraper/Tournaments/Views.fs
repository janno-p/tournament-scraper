namespace TournamentScraper.Tournaments

open Giraffe.ViewEngine

module View =
    let index (tournaments: Tournament list) : XmlNode =
        ul [] (tournaments |> List.map (fun x -> li [] [rawText x.Name]))
