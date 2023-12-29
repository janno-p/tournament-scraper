module TournamentScraper.Scraper

open Akka.Configuration
open Akka.FSharp

type ScraperActorMessages =
    | LoadUrl of string

let system = System.create "ScraperSystem" <| ConfigurationFactory.Default()

let scraper =
    spawn system "ScraperActor"
    <| fun mailbox ->
        let rec loop() =
            actor {
                let! message = mailbox.Receive()
                match message with
                | LoadUrl(url) ->
                    printfn $"Loading url %s{url}"
                    return! loop()
            }
        loop()
