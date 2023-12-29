module Router

open Saturn
open Giraffe.Core
open TournamentScraper.Tournaments.Controllers

let browser = pipeline {
    plug (mustAccept ["text/html"])
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = router {
    get "/" (htmlView Index.layout)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
}

let browserRouter = router {
    not_found_handler (htmlView NotFound.layout)
    pipe_through browser
    forward "" defaultView
    forward "/tournaments" TournamentController
}

let appRouter = router {
    forward "" browserRouter
}
