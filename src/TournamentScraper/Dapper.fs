module TournamentScraper.Dapper

open System
open System.Data
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Npgsql

type SqlConnectionFactory(dataSource: NpgsqlDataSource) =
    let mutable connection: NpgsqlConnection option = None
    member _.GetOpenConnectionAsync() =
        task {
            match connection with
            | Some(c) when c.State = ConnectionState.Open ->
                return c
            | _ ->
                let! c = dataSource.OpenConnectionAsync()
                connection <- Some(c)
                return c
        }
    interface IDisposable with
        member _.Dispose() =
            connection |> Option.iter (fun c ->
                if c.State = ConnectionState.Open then c.Dispose()
            )

let getConnection (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<SqlConnectionFactory>().GetOpenConnectionAsync()
