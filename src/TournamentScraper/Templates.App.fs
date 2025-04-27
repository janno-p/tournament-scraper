module TournamentScraper.Templates.App

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let activeMenuLink (href: string) (text: string) =
    li() {
        a(href = href, class' = "block py-2 px-3 text-white bg-blue-700 rounded md:bg-transparent md:text-blue-700 md:p-0 dark:text-white md:dark:text-blue-500", ariaCurrent = "page") { text }
    }

let menuLink (ctx: HttpContext) href text =
    if ctx.Request.Path.StartsWithSegments (PathString href) then activeMenuLink href text else
    li() {
        a(href = href, class' = "block py-2 px-3 text-gray-900 rounded hover:bg-gray-100 md:hover:bg-transparent md:border-0 md:hover:text-blue-700 md:p-0 dark:text-white md:dark:hover:text-blue-500 dark:hover:bg-gray-700 dark:hover:text-white md:dark:hover:bg-transparent") { text }
    }

type svg() as this =
    inherit RegularNode("svg")
    do this.attr("xmlns", "http://www.w3.org/2000/svg") |> ignore
    member this.fill
        with set (value: string | null) = this.attr("fill", value) |> ignore
    member this.viewBox
        with set (value: string | null) = this.attr("viewBox", value) |> ignore

type path() =
    inherit RegularNode("path")
    member this.stroke
        with set (value: string | null) = this.attr("stroke", value) |> ignore
    member this.strokeLinecap
        with set (value: string | null) = this.attr("stroke-linecap", value) |> ignore
    member this.strokeLinejoin
        with set (value: string | null) = this.attr("stroke-linejoin", value) |> ignore
    member this.strokeWidth
        with set (value: string | null) = this.attr("stroke-width", value) |> ignore
    member this.d
        with set (value: string | null) = this.attr("d", value) |> ignore

let layout (content: HtmlElement) (ctx: HttpContext) =
    html(class' = "has-navbar-fixed-top") {
        head() {
            meta(charset = "utf-8")
            meta(name = "viewport", content = "width=device-width, initial-scale=1")
            title() { "Tournament Scraper" }
            link(rel = "stylesheet", type' = "text/css", href = "/style.css")
            script(src = "/htmx.js") { }
            script(src = "/htmx-sse.js") { }
            script(defer = true, src = "/alpinejs.js") { }
        }
        body() {
            nav(class' = "bg-white border-gray-200 dark:bg-gray-900 drop-shadow-md mb-4") {
                div(class' = "max-w-screen-xl flex flex-wrap items-center justify-between mx-auto p-4 py-6") {
                    a(class' = "flex items-center space-x-3 rtl:space-x-reverse", href = "/") {
                        img(src = "https://avatars0.githubusercontent.com/u/35305523?s=200", class' = "h-8")
                        span(class' = "self-center text-2xl font-semibold whitespace-nowrap dark:text-white") { "Tournament Scraper" }
                    }
                    button(type' = "button", class' = "inline-flex items-center p-2 w-10 h-10 justify-center text-sm text-gray-500 rounded-lg md:hidden hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-gray-200 dark:text-gray-400 dark:hover:bg-gray-700 dark:focus:ring-gray-600", ariaControls = "navbar-default", ariaExpanded = false).data("collapse-toggle", "navbar-default") {
                        span(class' = "sr-only") { "Open main menu" }
                        svg(class' = "w-5 h-5", ariaHidden = true, fill = "none", viewBox = "0 0 17 14") {
                            path(stroke ="currentColor", strokeLinecap = "round", strokeLinejoin = "round", strokeWidth = "2", d = "M1 1h15M1 7h15M1 13h15") { }
                        }
                    }
                    div(class' = "hidden w-full md:block md:w-auto", id = "navbar-default") {
                        ul(class' = "font-medium flex flex-col p-4 md:p-0 mt-4 border border-gray-100 rounded-lg bg-gray-50 md:flex-row md:space-x-8 rtl:space-x-reverse md:mt-0 md:border-0 md:bg-white dark:bg-gray-800 md:dark:bg-gray-900 dark:border-gray-700") {
                            menuLink ctx "/" "Home"
                            menuLink ctx "/tournaments" "Tournaments"
                            menuLink ctx "/rankings" "Rankings"
                        }
                    }
                }
            }
            content
            footer(class' = "footer is-fixed-bottom") {
                div(class' = "container") {
                    div(class' = "content has-text-centered") {
                        p() {
                            "Powered by "
                            a(href = "https://github.com/SaturnFramework/Saturn") { "Saturn" }
                            " - F# MVC framework created by "
                            a(href = "http://lambdafactory.io") { "λFactory" }
                        }
                    }
                }
            }
        }
    }
