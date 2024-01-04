module Router

open Saturn
open Giraffe
open TournamentScraper.Tournaments.Controllers
open Sse

let browser = pipeline {
    plug (mustAccept ["text/html"])
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = router {
    get "/" (fun next ctx -> htmlView (Index.layout ctx) next ctx)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
}

let browserRouter = router {
    not_found_handler (fun next ctx -> htmlView (NotFound.layout ctx) next ctx)
    pipe_through browser
    forward "" defaultView
    forward "/tournaments" tournamentRoutes
}

let appRouter = router {
    forward "" browserRouter
    forward "/sse" sseRouter
}
