module TournamentScraper.Tournaments.Helpers

open Akka.Dispatch
open Akka.FSharp
open Giraffe
open Giraffe.Htmx
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Threading.Tasks

let private getTournamentsActor (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<Akka.Actor.IActorRef>()

let (>>>) (msg: 'Msg) (ctx: HttpContext) =
    (getTournamentsActor ctx) <! msg

let tryHtmx (htmxHandler: HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        if ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh then htmxHandler next ctx
        else next ctx

let runActorTask (func: unit -> Task) =
    ActorTaskScheduler.RunTask(func)
