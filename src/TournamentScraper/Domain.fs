module TournamentScraper.Domain

open System

type CachedPage = {
    Uri: Uri
    TournamentId: Guid
    Html: string
    Updated: DateTimeOffset
}

type Tournament = {
    Id: Guid
    Name: string
    Uri: Uri
    StartDate: DateOnly
    EndDate: DateOnly
}

type EventType =
    | MenDouble
    | MenSingle
    | MixedDouble
    | WomenDouble
    | WomenSingle

type EventLeague =
    | Masters
    | First
    | Second
    | Third
    | Fourth

type TournamentEvent = {
    Id: Guid
    TournamentId: Guid
    Uri: Uri
    Name: string
    EventType: EventType option
    EventTypeGuess: EventType option
    EventLeague: EventLeague option
    EventLeagueGuess: EventLeague option
}