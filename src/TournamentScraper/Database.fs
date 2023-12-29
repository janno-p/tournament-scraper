module Database

open System
open System.Data
open Dapper
open System.Data.Common
open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Microsoft.Data.Sqlite
open Microsoft.Extensions.DependencyInjection
open Saturn

let inline (=>) k v = k, box v

let execute (connection: #DbConnection) (sql: string) data =
    task {
        try
            let! res = connection.ExecuteAsync(sql, data)
            return Ok res
        with
        | ex -> return Error ex
    }

let query (connection: #DbConnection) (sql: string) (parameters: IDictionary<string, obj> option) =
    task {
        try
            let! res =
                match parameters with
                | Some p -> connection.QueryAsync<'T>(sql, p)
                | None -> connection.QueryAsync<'T>(sql)
            return Ok res
        with
        | ex -> return Error ex
    }

let querySingle (connection: #DbConnection) (sql: string) (parameters: IDictionary<string, obj> option) =
    task {
        try
            let! res =
                match parameters with
                | Some p -> connection.QuerySingleOrDefaultAsync<'T>(sql, p)
                | None -> connection.QuerySingleOrDefaultAsync<'T>(sql)
            return
                if isNull (box res) then Ok None
                else Ok (Some res)
        with
        | ex -> return Error ex
    }

type SqlConnectionFactory(connectionString: string) =
    let mutable connection: DbConnection option = None
    member _.GetOpenConnectionAsync() =
        task {
            match connection with
            | Some(c) when c.State = ConnectionState.Open ->
                return c
            | _ ->
                let c: DbConnection = new SqliteConnection(connectionString)
                do! c.OpenAsync()
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
