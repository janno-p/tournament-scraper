module TournamentScraper.Templates.Errors

open System
open Oxpecker.ViewEngine

let private layout (titleText: string) (bodyHtml: Fragment) : HtmlElement =
    html() {
        head() {
            meta(charset = "utf-8")
            meta(name = "viewport", content = "width=device-width, initial-scale=1")
            title() { titleText }
            script(defer = true, src = "alpinejs.js") { }
            script(src = "htmx.js") { }
            script(src = "htmx-sse.js") { }
            link(href = "style.css", rel = "stylesheet")
        }
        body() {
            h1() { titleText }
            bodyHtml
            a(href = "/") { "Go back to home page" }
        }
    }

let badRequest (message: string) =
    layout "ERROR #400" <| Fragment() {
        p() { message }
    }

let notFound =
    layout "ERROR #404" <| Fragment()

let internalError (ex: Exception) =
    layout "ERROR #500" <| Fragment() {
        h3() { ex.Message }
        h4() { ex.Source }
        pre() { ex.StackTrace }
    }
