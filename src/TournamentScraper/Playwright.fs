module TournamentScraper.Playwright

open System
open System.Threading.Tasks
open AngleSharp.Text
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Playwright
open Npgsql
open Npgsql.FSharp
open TournamentScraper.Domain

let baseUri = Uri("https://www.tournamentsoftware.com/")
let organizerId = Guid.Parse("cb9d3127-7f10-422c-9dba-eae804799642")

let toAbsoluteUri href =
    if Uri.IsWellFormedUriString(href, UriKind.Absolute) then Uri(href) else Uri(baseUri, href)

type private Browser =
    | Chromium
    | Chrome
    | Edge
    | Firefox
    | Webkit
    member this.AsString =
        match this with
        | Chromium -> "Chromium"
        | Chrome -> "Chrome"
        | Edge -> "Edge"
        | Firefox -> "Firefox"
        | Webkit -> "Webkit"

let private getBrowser (kind: Browser) (playwright: IPlaywright) =
    task {
        printfn $"Browsing with {kind.AsString}"

        return!
            match kind with
            | Chromium ->
                playwright.Chromium.LaunchAsync()
            | Chrome ->
                let opts = BrowserTypeLaunchOptions()
                opts.Channel <- "chrome"
                playwright.Chromium.LaunchAsync(opts)
            | Edge ->
                let opts = BrowserTypeLaunchOptions()
                opts.Channel <- "msedge"
                opts.Headless <- false
                playwright.Chromium.LaunchAsync(opts)
            | Firefox ->
                playwright.Firefox.LaunchAsync()
            | Webkit ->
                playwright.Webkit.LaunchAsync()
    }

let private gotoPage (url: Uri) (page: IPage) =
    task {
        printfn $"Navigating to \"{url}\""
        let! response = page.GotoAsync(url.ToString())
        if not response.Ok then
            failwith "We couldn't navigate to that page"
        ()
    } :> Task

let private getPage (url: Uri) (browser: IBrowser) = task {
    let! page = browser.NewPageAsync()
    do! gotoPage url page
    return page
}

let private acceptCookies (page: IPage) =
    task {
        printfn "Accepting cookies ..."
        let! acceptButton = page.QuerySelectorAsync("button.js-accept-basic")
        do! acceptButton.ClickAsync()
    } :> Task

let private consent (page: IPage) =
    task {
        printfn "Accepting privacy policies ..."
        let acceptButton = page.FrameLocator("body div iframe").Locator(".btn.green")
        do! acceptButton.ClickAsync()
    } :> Task

type private TournamentInfo = {
    Date: string
    Name: string
    Uri: Uri
}

type private TournamentRow =
    | Year of uint
    | Tournament of TournamentInfo

let private parseTournamentRowType (rowLocator: ILocator) =
    task {
        let! years = rowLocator.Locator("th").AllAsync()
        let years = years |> Seq.toArray

        let! rows = rowLocator.Locator("td").AllAsync()
        let rows = rows |> Seq.toArray

        return!
            match years, rows with
            | [| year |], _ ->
                task {
                    let! textContent = year.TextContentAsync()
                    return Some(Year(UInt32.Parse(textContent)))
                }
            | _, [| dt; nm |] ->
                task {
                    let! date = dt.TextContentAsync()
                    let! name = nm.TextContentAsync()
                    let! href = nm.Locator("a").GetAttributeAsync("href")
                    return Some(Tournament { Date = date; Name = name; Uri = toAbsoluteUri href })
                }
            | _ -> Task.FromResult(None)
    }

let private getTournaments (page: IPage) =
    task {
        printfn "Parsing tournaments list ..."

        let content = page.Locator("div#content").Filter(LocatorFilterOptions(Has = page.Locator("h2.content-title").Filter(LocatorFilterOptions(HasText = "Tournaments of"))))
        do! content.WaitForAsync()
        
        let! tournaments = content.Locator("table > tbody > tr").AllAsync()

        let! tournamentRows =
            tournaments
            |> Seq.toArray
            |> Array.Parallel.map parseTournamentRowType
            |> Task.WhenAll

        return tournamentRows |> Array.choose id
    }

module private TournamentData =
    let fromRow (year: uint) (value: TournamentInfo) : Tournament =
        let range = value.Date.Split("to") |> Array.map (fun x -> DateOnly.ParseExact($"{x.Trim()}/{year}", "MM/dd/yyyy"))
        {
            Id = Guid.Parse((QueryHelpers.ParseQuery(value.Uri.Query)["id"]).ToString())
            Uri = value.Uri
            Name = value.Name
            StartDate = range[0]
            EndDate = range[1]
        }

let toTask (task: Task<_>) : Task =
    task :> Task

let updateTournament (tournament: Tournament) =
    Sql.existingConnection
    >> Sql.query
        """
        insert into tournaments (id, url, name, start_date, end_date)
        values (@id, @url, @name, @startDate, @endDate)
        on conflict (id)
        do update set
            url = excluded.url,
            name = excluded.name,
            start_date = excluded.start_date,
            end_date = excluded.end_date;
        """
    >> Sql.parameters
        [
            "@id", Sql.uuid tournament.Id
            "@url", Sql.string (tournament.Uri.ToString())
            "@name", Sql.string tournament.Name
            "@startDate", Sql.date tournament.StartDate
            "@endDate", Sql.date tournament.EndDate
        ]
    >> Sql.executeNonQueryAsync
    >> toTask

let updateTournaments (connection: NpgsqlConnection) (page: IPage) =
    task {
        do! gotoPage (Uri(baseUri, $"find.aspx?a=7&q={organizerId:D}")) page
        let! tournaments = getTournaments page
        let folder (acc: uint option * ResizeArray<Tournament>) (row: TournamentRow) =
            let year, lst = acc
            match row, year with
            | Year year, _ -> (Some year, lst)
            | Tournament info, Some year ->
                lst.Add(TournamentData.fromRow year info)
                (Some year, lst)
            | _ -> acc    
        let tournaments = tournaments |> Array.fold folder (None, ResizeArray<_>()) |> snd |> Seq.toArray
        for tournament in tournaments do
            do! connection |> updateTournament tournament
        return tournaments.Length
    }

let openNewPage (playwright: IPlaywright) = task {
    let! browser = playwright |> getBrowser Edge
    let! page = browser |> getPage baseUri
    do! page |> acceptCookies
    do! page |> consent
    return page
}

module CachedPage =
    let ofRowReader (read: RowReader) : CachedPage =
        {
            Uri = Uri(read.string "url")
            TournamentId = read.uuid "tournament_id"
            Html = read.text "html"
            Updated = read.datetimeOffset "updated"
        }

let loadPage (tournamentId: Guid) (uri: Uri) (connection: NpgsqlConnection) (skipCache: bool) (page: IPage) = task {
    let! cachedPage =
        if skipCache then Task.FromResult(None) else task {
            let! cachedHtml =
                connection
                |> Sql.existingConnection
                |> Sql.query "select html from cached_pages where url = @url"
                |> Sql.parameters [ "@url", Sql.string (uri.ToString()) ]
                |> Sql.executeAsync (fun read -> read.string "html")
            return cachedHtml |> Seq.tryExactlyOne
        }
    let! html =
        match cachedPage with
        | None ->
            task {
                do! gotoPage uri page
                let! html = page.ContentAsync()
                let cachedPage: CachedPage = {
                    Uri = uri
                    TournamentId = tournamentId
                    Html = html
                    Updated = DateTimeOffset.UtcNow
                }
                let! _ =
                    connection
                    |> Sql.existingConnection
                    |> Sql.query
                        """
                        insert into cached_pages (url, tournament_id, html, updated)
                        values (@url, @tournament_id, @html, @updated)
                        on conflict (url)
                        do update set
                            tournament_id = excluded.tournament_id,
                            html = excluded.html,
                            updated = excluded.updated;
                        """
                    |> Sql.parameters
                        [
                            "@url", Sql.string (cachedPage.Uri.ToString())
                            "@tournament_id", Sql.uuid cachedPage.TournamentId
                            "@html", Sql.text cachedPage.Html
                            "@updated", Sql.timestamptz cachedPage.Updated
                        ]
                    |> Sql.executeNonQueryAsync
                return html
            }
        | Some html -> Task.FromResult(html)
    return html
}

module TournamentEvents =
    open AngleSharp

    let private eventsUri (tournamentId: Guid) =
        Uri(baseUri, $"/sport/events.aspx?id={tournamentId:D}")

    type EventLink = {
        Name: string
        Href: string
    }
    
    module EventType =
        let toSql = function
            | MenDouble -> "MD"
            | MenSingle -> "MS"
            | MixedDouble -> "XD"
            | WomenDouble -> "WD"
            | WomenSingle -> "WS"

    module EventLeague =
        let toSql = function
            | Masters -> "MASTERS"
            | First -> "FIRST"
            | Second -> "SECOND"
            | Third -> "THIRD"
            | Fourth -> "FOURTH"

    module Sql =
        let eventType = EventType.toSql >> Sql.string
        let eventTypeOrNone = Option.map EventType.toSql >> Sql.stringOrNone
        let eventLeague = EventLeague.toSql >> Sql.string
        let eventLeagueOrNone = Option.map EventLeague.toSql >> Sql.stringOrNone

    let initTournamentEvent (tournamentEvent: TournamentEvent) =
        Sql.existingConnection
        >> Sql.query
            """
            insert into tournament_events (id, tournament_id, url, name, event_type_guess, event_league_guess)
            values (@id, @tournament_id, @url, @name, cast(@event_type_guess as event_type), cast(@event_league_guess as event_league))
            on conflict (url)
            do update set
                name = excluded.name,
                event_type_guess = excluded.event_type_guess,
                event_league_guess = excluded.event_league_guess;
            """
        >> Sql.parameters
            [
                "@id", Sql.uuid tournamentEvent.Id
                "@tournament_id", Sql.uuid tournamentEvent.TournamentId
                "@url", Sql.string (tournamentEvent.Uri.ToString())
                "@name", Sql.string tournamentEvent.Name
                "@event_type_guess", Sql.eventTypeOrNone tournamentEvent.EventTypeGuess
                "@event_league_guess", Sql.eventLeagueOrNone tournamentEvent.EventLeagueGuess
            ]
        >> Sql.executeNonQueryAsync
        >> toTask

    let parseTournamentEvent tournamentId (eventLink: EventLink) : TournamentEvent =
        let guessType =
            match eventLink.Name.SplitSpaces() with
            | x when x.Contains("MD") || x.Contains("MP") -> Some(MenDouble)
            | x when x.Contains("MS") -> Some(MenSingle)
            | x when x.Contains("WD") || x.Contains("NP") -> Some(WomenDouble)
            | x when x.Contains("WS") -> Some(WomenSingle)
            | x when x.Contains("XD") || x.Contains("SP") -> Some(MixedDouble)
            | _ -> None
        let guessLeague =
            match eventLink.Name.SplitSpaces() with
            | x when x.Contains("Meistriliiga", StringComparison.InvariantCultureIgnoreCase) -> Some(Masters)
            | x when x.Contains("Esiliiga", StringComparison.InvariantCultureIgnoreCase)|| x.Contains("1.") -> Some(First)
            | x when x.Contains("2.") -> Some(Second)
            | x when x.Contains("3.") -> Some(Third)
            | x when x.Contains("4.") -> Some(Fourth)
            | _ -> None
        {
            Id = Guid.CreateVersion7()
            TournamentId = tournamentId
            Uri = toAbsoluteUri eventLink.Href
            Name = eventLink.Name
            EventType = None
            EventTypeGuess = guessType
            EventLeague = None
            EventLeagueGuess = guessLeague
        }

    let load tournamentId skipCache (connection: NpgsqlConnection) (page: IPage) = task {
        let! html = loadPage tournamentId (eventsUri tournamentId) connection skipCache page
        let context = BrowsingContext.New()
        let! document = context.OpenAsync(fun req -> req.Content(html) |> ignore)
        let tournamentEvents =
            document.QuerySelectorAll("table.admintournamentevents tbody tr td a")
            |> Seq.map (fun el -> { Name = el.InnerHtml; Href = el.GetAttribute("href") })
            |> Seq.map (parseTournamentEvent tournamentId)
            |> Seq.toList
        for e in tournamentEvents do
            do! connection |> initTournamentEvent e
    }
