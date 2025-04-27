[<AutoOpen>]
module TournamentScraper.ServiceDefaults.HostingExtensions

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open OpenTelemetry
open OpenTelemetry.Metrics
open OpenTelemetry.Trace

[<Literal>]
let HealthEndpointPath = "/health"

[<Literal>]
let AlivenessEndpointPath = "/alive"

let private addOpenTelemetryExporters (builder: 'TBuilder when 'TBuilder :> IHostApplicationBuilder) =
    if not (String.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])) then
        builder.Services.AddOpenTelemetry().UseOtlpExporter() |> ignore
    ()

let private addDefaultHealthChecks (builder: 'TBuilder when 'TBuilder :> IHostApplicationBuilder) =
    builder.Services
        .AddHealthChecks()
        .AddCheck("self", (fun () -> HealthCheckResult.Healthy()), ["live"])
        |> ignore

let private configureOpenTelemetry (builder: #IHostApplicationBuilder) =
    builder.Logging.AddOpenTelemetry
        (fun logging ->
            logging.IncludeFormattedMessage <- true
            logging.IncludeScopes <- true
        )
        |> ignore
        
    builder.Services
        .AddOpenTelemetry()
        .WithMetrics(
            fun metrics ->
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    |> ignore
            )
        .WithTracing(
            fun tracing ->
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(
                        fun t ->
                            t.Filter <- (fun context ->
                                (not (context.Request.Path.StartsWithSegments(HealthEndpointPath)))
                                && (not (context.Request.Path.StartsWithSegments(AlivenessEndpointPath)))
                            )
                        )
                    .AddHttpClientInstrumentation()
                    |> ignore
            )
        |> ignore

    builder |> addOpenTelemetryExporters

let addServiceDefaults (builder: #IHostApplicationBuilder) =
    builder |> configureOpenTelemetry
    builder |> addDefaultHealthChecks

    builder.Services.AddServiceDiscovery() |> ignore

    builder.Services.ConfigureHttpClientDefaults
        (fun http ->
            http.AddStandardResilienceHandler() |> ignore
            http.AddServiceDiscovery() |> ignore
        )
        |> ignore

let mapDefaultEndpoints (app: WebApplication) =
    if app.Environment.IsDevelopment() then
        app.MapHealthChecks(HealthEndpointPath) |> ignore
        app.MapHealthChecks(AlivenessEndpointPath, HealthCheckOptions(Predicate = _.Tags.Contains("live"))) |> ignore
    ()
