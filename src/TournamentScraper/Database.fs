module TournamentScraper.Database

open Dapper
open Dapper.FSharp.PostgreSQL
open System
open System.Data.Common
open System.Collections.Generic

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

module Schema =
    type TournamentRow = {
        id: Guid
        name: string
        url: string
        start_date: DateTime
        end_date: DateTime
    }

    let tournaments = table'<TournamentRow> "tournaments"
