namespace TournamentScraper.Tournaments

open System
open Microsoft.AspNetCore.Http
open Npgsql.FSharp
open TournamentScraper
open TournamentScraper.Domain

module Tournament =
    let ofRowReader (read: RowReader) : Tournament =
        {
            Id = read.uuid "id"
            Uri = Uri(read.string "url")
            Name = read.string "name"
            StartDate = read.dateOnly "start_date"
            EndDate = read.dateOnly "end_date"
        }

module EventType =
    let ofString = function
        | "MD" -> MenDouble
        | "MS" -> MenSingle
        | "WD" -> WomenDouble
        | "WS" -> WomenSingle
        | "XD" -> MixedDouble
        | value -> failwith $"Invalid event_type value of {value}"

module EventLeague =
    let ofString = function
        | "MASTERS" -> Masters
        | "FIRST" -> First
        | "SECOND" -> Second
        | "THIRD" -> Third
        | "FOURTH" -> Fourth
        | value -> failwith $"Invalid event_league value of {value}"

module TournamentEvent =
    let ofRowReader (read: RowReader) : TournamentEvent =
        {
            Id = read.uuid "id"
            TournamentId = read.uuid "tournament_id"
            Uri = Uri(read.string "url")
            Name = read.string "name"
            EventType = read.stringOrNone "event_type" |> Option.map EventType.ofString
            EventTypeGuess = read.stringOrNone "event_type_guess" |> Option.map EventType.ofString
            EventLeague = read.stringOrNone "event_league" |> Option.map EventLeague.ofString
            EventLeagueGuess = read.stringOrNone "event_league_guess" |> Option.map EventLeague.ofString
        }

[<RequireQualifiedAccess>]
module Model =
    let find (ctx: HttpContext) = task {
        let! connection = Database.getConnection ctx
        let! tournaments =
            connection
            |> Sql.existingConnection
            |> Sql.query "select id, url, name, start_date, end_date from tournaments where start_date > @start order by start_date desc"
            |> Sql.parameters [ "@start", Sql.date (DateOnly(2023, 8, 1)) ]
            |> Sql.executeAsync Tournament.ofRowReader
        return tournaments
    }

    let get (ctx: HttpContext) (key: Guid) = task {
        let! connection = Database.getConnection ctx
        let! tournaments =
            connection
            |> Sql.existingConnection
            |> Sql.query "select id, url, name, start_date, end_date from tournaments where id = @id"
            |> Sql.parameters [ "@id", Sql.uuid key ]
            |> Sql.executeAsync Tournament.ofRowReader
        return tournaments |> Seq.tryExactlyOne
    }

    let getEvents (ctx: HttpContext) (key: Guid) = task {
        let! connection = Database.getConnection ctx
        let! tournamentEvents =
            connection
            |> Sql.existingConnection
            |> Sql.query
                """
                select id, tournament_id, url, name, event_type, event_type_guess, event_league, event_league_guess
                from tournament_events
                where tournament_id = @tournament_id
                """
            |> Sql.parameters [ "@tournament_id", Sql.uuid key ]
            |> Sql.executeAsync TournamentEvent.ofRowReader
        return tournamentEvents
    }
