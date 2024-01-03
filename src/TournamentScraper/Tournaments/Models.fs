namespace TournamentScraper.Tournaments

open Microsoft.AspNetCore.Http
open System
open Playwright

type Tournament = {
    Id: Guid
    Name: string
    StartDate: DateOnly
    EndDate: DateOnly
}

type TournamentRow = {
    id: string
    name: string
    start_date: string
    end_date: string
}

module private Tournament =
    let fromRow (r: TournamentRow) =
        {
            Id = Guid.Parse(r.id)
            Name = r.name
            StartDate = DateOnly.FromDateTime(DateTime.Parse(r.start_date))
            EndDate = DateOnly.FromDateTime(DateTime.Parse(r.end_date))
        }

[<RequireQualifiedAccess>]
module Model =
    let find (ctx: HttpContext) =
        task {
            let! conn = Dapper.getConnection ctx
            let! result = Database.query conn """SELECT id, name, start_date, end_date FROM tournaments""" None
            return result |> Result.defaultWith (fun err -> eprintfn $"{err.Message}"; Seq.empty) |> Seq.map Tournament.fromRow |> Seq.toList
        }
