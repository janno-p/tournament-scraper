module TournamentScraper.Endpoints

open Microsoft.AspNetCore.Http
open Microsoft.Net.Http.Headers
open Oxpecker
open TournamentScraper.Templates

let mustAccept (mimeTypes: string list) : EndpointMiddleware =
    let acceptedMimeTypes: MediaTypeHeaderValue list =
        mimeTypes |> List.map MediaTypeHeaderValue.Parse
    fun next ctx ->
        ctx.Request.GetTypedHeaders().Accept
        |> Seq.exists (fun h -> acceptedMimeTypes |> List.exists _.IsSubsetOf(h))
        |> function
            | true -> next ctx
            | false -> System.Threading.Tasks.Task.CompletedTask

let putSecureBrowserHeaders : EndpointMiddleware =
    fun next ctx -> task {
        do! setHttpHeader "x-frame-options" "SAMEORIGIN" ctx 
        do! setHttpHeader "x-xss-protection" "1; mode=block" ctx
        do! setHttpHeader "x-content-type-options" "nosniff" ctx
        do! setHttpHeader "x-download-options" "noopen" ctx
        do! setHttpHeader "x-permitted-cross-domain-policies" "none" ctx
        return! next ctx
    }

let fetchSession : EndpointMiddleware =
    fun next ctx ->
        task {
              do! ctx.Session.LoadAsync()
              return! next ctx
        }

let setHeader key value : EndpointMiddleware =
    fun next ctx -> task {
        do! setHttpHeader key value ctx
        return! next ctx
    }

let browserPipeline =
    mustAccept ["text/html"]
    >> putSecureBrowserHeaders
    >> fetchSession
    >> setHeader "x-pipeline-type" "Browser"
    |> applyBefore
    |> List.map

let indexHandler : EndpointHandler =
    fun ctx -> htmlView (Index.layout ctx) ctx

let app: Endpoint list = [
    subRoute "/sse" Sse.endpoints
    subRoute "/" (browserPipeline [
        GET [
            route "/" indexHandler
            route "/index.html" (redirectTo "/" false)
            route "/default.html" (redirectTo "/" false)
        ]
        subRoute "/tournaments" Tournaments.Controllers.endpoints
    ])
]
