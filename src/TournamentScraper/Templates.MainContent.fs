module TournamentScraper.Templates.MainContent

open Oxpecker.ViewEngine

let layout (content: Fragment) =
    main(class' = "max-w-screen-xl mx-auto p-4") { content }
