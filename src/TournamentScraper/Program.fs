module TournamentScraper.Server

open System
open System.IO
open System.IO.Compression
open System.Threading.Tasks
open Dapper
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Oxpecker
open TournamentScraper.ServiceDefaults
open TournamentScraper.Templates.Errors

let head : EndpointMiddleware =
    fun next ctx ->
        if ctx.Request.Method = "HEAD" then ctx.Request.Method <- "GET"
        next ctx

let requestId : EndpointMiddleware =
    fun next ctx -> task {
        let reqId =
            match ctx.Request.Headers.TryGetValue "x-request-id" with
            | true, v when v[0].Length >= 20 && v[0].Length <= 200 -> v[0]
            | _ -> Guid.NewGuid().ToString("N")
        ctx.Items["RequestId"] <- reqId
        do! setHttpHeader "x-request-id" reqId ctx
        return! next ctx
    }

let endpointPipeline =
    applyBefore (head >> requestId)

let endpoints =
    List.map endpointPipeline Endpoints.app

let notFoundHandler (ctx: HttpContext) =
    let logger = ctx.GetLogger()
    logger.LogWarning("Unhandled 404 error")
    ctx.SetStatusCode StatusCodes.Status404NotFound
    ctx.WriteHtmlView notFound

let errorHandler (ctx: HttpContext) (next: RequestDelegate) =
    task {
        try
            return! next.Invoke(ctx)
        with
        | :? ModelBindException
        | :? RouteParseException as ex ->
            let logger = ctx.GetLogger()
            logger.LogWarning(ex, "Unhandled 400 error")
            ctx.SetStatusCode StatusCodes.Status400BadRequest
            return! ctx.WriteHtmlView(badRequest (string ex))
        | ex ->
            let logger = ctx.GetLogger()
            logger.LogError(ex, "Unhandled 500 error")
            ctx.SetStatusCode StatusCodes.Status500InternalServerError
            return! ctx.WriteHtmlView(internalError ex)
    }
    :> Task

let configureApp (appBuilder: WebApplication) =
    appBuilder.UseResponseCompression() |> ignore
    appBuilder.UseDefaultFiles().UseStaticFiles().UseSession() |> ignore
    appBuilder.UseRouting().Use(errorHandler).UseOxpecker(endpoints).Run(notFoundHandler)
    appBuilder.MapOpenApi() |> ignore

let configureServices (builder: WebApplicationBuilder) =
    addServiceDefaults builder
    
    builder.Services
        .Configure<GzipCompressionProviderOptions>(fun (opts: GzipCompressionProviderOptions) ->
            opts.Level <- CompressionLevel.Optimal
        )
        .AddResponseCompression(fun o ->
            let additionalMime = [|
                "application/x-yaml"
                "image/svg+xml"
                "application/octet-stream"
                "application/x-font-ttf"
                "application/x-font-opentype"
                "application/x-javascript"
                "text/javascript"
            |]
            o.MimeTypes <- Seq.append ResponseCompressionDefaults.MimeTypes additionalMime
        )
        |> ignore
        
    builder.Services.AddSingleton<Akka.Actor.IActorRef>(Tournaments.Actors.createTournamentsActor) |> ignore

    builder.Services.AddRouting().AddOxpecker().AddOpenApi().AddSession().AddDistributedMemoryCache() |> ignore
    builder.Services.AddScoped<SqlConnectionFactory>() |> ignore
    builder.AddNpgsqlDataSource("db1")

[<EntryPoint>]
let main args =
    Dapper.FSharp.PostgreSQL.OptionTypes.register()
    let options = WebApplicationOptions(Args = args, WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "static"))
    let builder = WebApplication.CreateBuilder(options)
    configureServices builder
    let app = builder.Build()
    configureApp app
    app.Run()
    0
