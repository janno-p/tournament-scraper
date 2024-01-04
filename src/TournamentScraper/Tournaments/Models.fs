namespace TournamentScraper.Tournaments

open Database
open Microsoft.AspNetCore.Http
open System

type Tournament = {
    Id: Guid
    Name: string
    StartDate: DateOnly
    EndDate: DateOnly
}

type TournamentRow = {
    id: string
    name: string
    start_date: int64
    end_date: int64
}

module private Tournament =
    let fromRow (r: TournamentRow) =
        {
            Id = Guid.Parse(r.id)
            Name = r.name
            StartDate = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(r.start_date).Date)
            EndDate = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(r.end_date).Date)
        }

[<RequireQualifiedAccess>]
module Model =
    let find (ctx: HttpContext) =
        task {
            let! conn = Dapper.getConnection ctx
            let! result =
                query conn
                    """SELECT id, name, start_date, end_date FROM tournaments WHERE start_date > @start ORDER BY start_date DESC"""
                    (Some (dict [ "start" => DateTimeOffset(DateTime(2023, 8, 1, 0, 0, 0)).ToUnixTimeSeconds() ]))
            return result |> Result.defaultWith (fun err -> eprintfn $"{err.Message}"; Seq.empty) |> Seq.map Tournament.fromRow |> Seq.toList
        }
        
    let get (ctx: HttpContext) (key: Guid) =
        task {
            let! conn = Dapper.getConnection ctx
            let! result =
                querySingle conn
                    """SELECT id, name, start_date, end_date FROM tournaments WHERE id = @key"""
                    (Some (dict [ "key" => key ]))
            return result |> Result.defaultWith (fun err -> eprintfn $"{err.Message}"; None) |> Option.map Tournament.fromRow
        }
