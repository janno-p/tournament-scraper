module TournamentScraper.Server

open Dapper
open Saturn
open Tournaments.Extensions

let endpointPipe = pipeline {
    plug head
    plug requestId
}

let app = application {
    pipe_through endpointPipe
    error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
    use_router Router.appRouter
    url "http://localhost:8085"
    memory_cache
    use_static "static"
    use_gzip
    use_dapper "DataSource=database.sqlite"
    use_tournaments
}

[<EntryPoint>]
let main _ =
    printfn $"Working directory - %s{System.IO.Directory.GetCurrentDirectory()}"
    run app
    0
