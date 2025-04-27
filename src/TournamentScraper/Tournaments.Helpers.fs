module TournamentScraper.Tournaments.Helpers

open Akka.Dispatch
open Akka.FSharp
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker
open Oxpecker.Htmx
open System.Threading.Tasks

let isHtmx (ctx: HttpContext) =
    ctx.TryGetHeaderValue HxRequestHeader.Request |> Option.map bool.Parse |> Option.defaultValue false

let isHtmxRefresh (ctx: HttpContext) =
    isHtmx ctx && ctx.TryGetHeaderValue HxRequestHeader.HistoryRestoreRequest |> Option.map bool.Parse |> Option.defaultValue false

let private getTournamentsActor (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<Akka.Actor.IActorRef>()

let (>>>) (msg: 'Msg) (ctx: HttpContext) =
    getTournamentsActor ctx <! msg

let tryHtmx (htmxHandler: EndpointHandler) : EndpointMiddleware =
    fun next ctx ->
        if isHtmx ctx && not (isHtmxRefresh ctx) then htmxHandler ctx
        else next ctx

let runActorTask (func: unit -> Task) =
    ActorTaskScheduler.RunTask(func)
