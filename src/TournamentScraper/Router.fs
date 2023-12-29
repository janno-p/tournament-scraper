﻿module Router

open Microsoft.AspNetCore.Http.HttpResults
open Saturn
open Giraffe.Core

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
}

let appRouter = router {
    forward "" browserRouter
}