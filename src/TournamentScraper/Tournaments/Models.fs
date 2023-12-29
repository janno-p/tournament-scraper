namespace TournamentScraper.Tournaments

open Microsoft.AspNetCore.Http
open System

type Tournament = {
    Id: Guid
    Name: string
}

module private Tournament =
    let fromRow (r: {| Id: string; Name: string |}) =
        {
            Id = Guid.Parse(r.Id)
            Name = r.Name
        }

[<RequireQualifiedAccess>]
module Model =
    let find (ctx: HttpContext) =
        task {
            let! conn = Database.getConnection ctx
            let! result = Database.query conn """SELECT * FROM tournaments""" None
            return result |> Result.defaultValue Seq.empty |> Seq.map Tournament.fromRow |> Seq.toList
        }

