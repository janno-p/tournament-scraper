module Playwright

open Database
open System.Data.Common
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Playwright
open System
open System.Threading.Tasks

let baseUri = Uri("https://www.tournamentsoftware.com/")
let organizerId = Guid.Parse("cb9d3127-7f10-422c-9dba-eae804799642")

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

let private getPage (url: Uri) (getBrowser: Task<IBrowser>) =
    task {
        let! browser = getBrowser
        
        printfn $"Navigating to \"{url}\""
        
        let! page = browser.NewPageAsync()
        let! response = page.GotoAsync(url.ToString())
        
        if not response.Ok then
            failwith "We couldn't navigate to that page"

        return page
    }

let private acceptCookies (getPage: Task<IPage>) =
    task {
        let! page = getPage
        
        printfn "Accepting cookies ..."
        
        let! acceptButton = page.QuerySelectorAsync("button.js-accept-basic")
        do! acceptButton.ClickAsync()

        return page
    }

type private TournamentInfo = {
    Date: string
    Name: string
    Url: string
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
                    let! url = nm.Locator("a").GetAttributeAsync("href")
                    return Some(Tournament { Date = date; Name = name; Url = url })
                }
            | _ -> Task.FromResult(None)
    }

let private getTournaments (getPage: Task<IPage>) =
    task {
        let! page = getPage

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

type TournamentData = {
    Id: Guid
    Url: string
    Name: string
    StartDate: int64
    EndDate: int64
}

module DateOnly =
    let toUnixTime (d: DateOnly) =
        DateTimeOffset(d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds()

module private TournamentData =
    let fromRow (year: uint) (value: TournamentInfo) =
        let range = value.Date.Split("to") |> Array.map (fun x -> DateOnly.ParseExact($"{x.Trim()}/{year}", "MM/dd/yyyy"))
        {
            Id = Guid.Parse((QueryHelpers.ParseQuery(Uri(baseUri, value.Url).Query)["id"]).ToString())
            Url = value.Url
            Name = value.Name
            StartDate = range[0] |> DateOnly.toUnixTime
            EndDate = range[1] |> DateOnly.toUnixTime
        }

let updateTournament (connection: #DbConnection) (data: TournamentData) =
    task {
        let! result =
            execute connection
                """
                insert into tournaments (id, url, name, start_date, end_date) values (@id, @url, @name, @start, @end)
                on conflict (id) do update set url=@url, name=@name, start_date=@start, end_date=@end
                """
                (dict [
                    "id" => data.Id
                    "url" => data.Url
                    "name" => data.Name
                    "start" => data.StartDate
                    "end" => data.EndDate
                ])
        result |> Result.map (fun _ -> ()) |> Result.defaultWith (fun err -> eprintfn $"{err.Message}")
    }

let updateTournaments (connection: #DbConnection) =
    task {
        use! playwright = Playwright.CreateAsync()
        let! tournaments =
            playwright
            |> getBrowser Edge
            |> getPage (Uri(baseUri, $"find.aspx?a=7&q={organizerId:D}"))
            |> acceptCookies
            |> getTournaments
        let folder (acc: uint option * ResizeArray<TournamentData>) (row: TournamentRow) =
            let year, lst = acc
            match row, year with
            | Year year, _ -> (Some year, lst)
            | Tournament info, Some year ->
                lst.Add(TournamentData.fromRow year info)
                (Some year, lst)
            | _ -> acc    
        let tournaments = tournaments |> Array.fold folder (None, ResizeArray<_>()) |> snd |> Seq.toArray
        for t in tournaments do
            do! updateTournament connection t
        return tournaments.Length
    }
