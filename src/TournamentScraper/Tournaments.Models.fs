namespace TournamentScraper.Tournaments

open Dapper.FSharp.PostgreSQL
open Microsoft.AspNetCore.Http
open System
open TournamentScraper
open TournamentScraper.Database

type Tournament = {
    Id: Guid
    Name: string
    StartDate: DateOnly
    EndDate: DateOnly
}

module private Tournament =
    let fromRow (r: Schema.TournamentRow) =
        {
            Id = r.id
            Name = r.name
            StartDate = DateOnly.FromDateTime r.start_date
            EndDate = DateOnly.FromDateTime r.end_date
        }

[<RequireQualifiedAccess>]
module Model =
    let find (ctx: HttpContext) =
        task {
            let! conn = Dapper.getConnection ctx
            let start = DateTime(2023, 8, 1, 0, 0, 0, DateTimeKind.Utc)
            let! result =
                select {
                    for t in Schema.tournaments do
                    where (t.start_date > start)
                    orderByDescending t.start_date
                } |> conn.SelectAsync<Schema.TournamentRow>
            return result |> Seq.map Tournament.fromRow |> Seq.toList
        }
        
    let get (ctx: HttpContext) (key: Guid) =
        task {
            let! conn = Dapper.getConnection ctx
            let! result =
                select {
                    for t in Schema.tournaments do
                    where (t.id = key)
                } |> conn.SelectAsync<Schema.TournamentRow>
            return result |> Seq.tryExactlyOne |> Option.map Tournament.fromRow
        }
