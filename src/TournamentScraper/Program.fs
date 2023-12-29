﻿module Server

open Database
open Saturn
open Config
open Microsoft.Extensions.DependencyInjection

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
    use_config (fun _ -> { connectionString = "DataSource=database.sqlite" })
    service_config (fun s -> s.AddScoped<SqlConnectionFactory>(fun _ -> new SqlConnectionFactory("DataSource=database.sqlite")))
}

[<EntryPoint>]
let main _ =
    printfn $"Working directory - %s{System.IO.Directory.GetCurrentDirectory()}"
    run app
    0
