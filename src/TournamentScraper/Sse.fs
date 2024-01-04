module Sse

open System.Collections.Concurrent
open System.Threading.Tasks
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open Saturn
open System

type HttpResponse with
    member this.WriteEventAsync(event: string, node: XmlNode) : Task =
        task {
            let html = (RenderView.AsString.htmlNode node).Replace("\n", "")
            do! this.WriteAsync($"event: {event}\n")
            do! this.WriteAsync($"data: {html}\n\n")
            do! this.Body.FlushAsync()
        }

type SseMessage =
    | TournamentsLoaded of XmlNode
    | TournamentLoaded of Guid * XmlNode

let private queue = ConcurrentQueue<SseMessage>()

let private sse next (ctx: HttpContext) =
    task {
        let res = ctx.Response
        ctx.SetStatusCode 200
        ctx.SetHttpHeader ("Content-Type", "text/event-stream")
        ctx.SetHttpHeader ("Cache-Control", "no-cache")
        while true do
            do!
                match queue.TryDequeue() with
                | true, TournamentsLoaded node ->
                    res.WriteEventAsync("TournamentsLoaded", node)
                | true, TournamentLoaded (id, node) ->
                    res.WriteEventAsync($"TournamentLoaded/{id:D}", node)
                | false, _ ->
                    Task.Delay (TimeSpan.FromSeconds 1.)
        return! text "" next ctx
    }

let enqueue msg =
    queue.Enqueue(msg)

let sseRouter = router {
    get "" sse
}
