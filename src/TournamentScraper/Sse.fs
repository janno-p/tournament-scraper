module TournamentScraper.Sse

open System.Collections.Concurrent
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Oxpecker
open Oxpecker.ViewEngine
open System

let sendEventResponse event html (ctx: HttpContext) =
    task {
        let html = (Render.toString html).Replace("\n", "")
        do! ctx.Response.WriteAsync($"event: {event}\n")
        do! ctx.Response.WriteAsync($"data: {html}\n\n")
        do! ctx.Response.Body.FlushAsync()
    } :> Task

type SseMessage =
    | TournamentsLoaded of RegularNode
    | TournamentLoaded of Guid * RegularNode

let private queue = ConcurrentQueue<SseMessage>()

let private sse : EndpointHandler =
    fun ctx -> task {
        do! setStatusCode StatusCodes.Status200OK ctx
        do! setHttpHeader "content-type" "text/event-stream" ctx
        do! setHttpHeader "cache-control" "no-cache" ctx
        while not ctx.RequestAborted.IsCancellationRequested do
            do!
                match queue.TryDequeue() with
                | true, TournamentsLoaded node ->
                    printfn $"TournamentsLoaded: {node}"
                    sendEventResponse "TournamentsLoaded" node ctx
                | true, TournamentLoaded (id, node) ->
                    printfn $"TournamentLoaded/{id:D}: {node}"
                    sendEventResponse $"TournamentLoaded/{id:D}" node ctx
                | false, _ ->
                    Task.Delay (TimeSpan.FromSeconds 1.)
        ()
    }

let enqueue msg =
    printfn $"Enqueue: {msg}"
    queue.Enqueue(msg)

let endpoints: Endpoint list = [
    GET [ route "/" sse ]
]
