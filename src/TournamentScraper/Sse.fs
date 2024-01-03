module Sse

open System.Collections.Concurrent
open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.HtmlElements
open Microsoft.AspNetCore.Http
open Saturn

type SseMessage =
    | TournamentsLoaded of XmlNode

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
                    task {
                        let html = (RenderView.AsString.htmlNode node).Replace("\n", "")
                        do! res.WriteAsync($"event: TournamentsLoaded\ndata: {html}\n\n")
                        do! res.Body.FlushAsync()
                    }
                | _ ->
                    task {
                        do! Async.Sleep (System.TimeSpan.FromSeconds 1.)
                    }
        return! text "" next ctx
    }

let enqueue msg =
    queue.Enqueue(msg)

let sseRouter = router {
    get "" sse
}
