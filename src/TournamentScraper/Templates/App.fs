module App

open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http

let activeMenuLink href text =
    li [] [
        a [_href href; _class "block py-2 px-3 text-white bg-blue-700 rounded md:bg-transparent md:text-blue-700 md:p-0 dark:text-white md:dark:text-blue-500"; attr "aria-current" "page"] [encodedText text]
    ]

let menuLink (ctx: HttpContext) href text =
    if ctx.Request.Path.StartsWithSegments (PathString href) then activeMenuLink href text else
    li [] [
        a [_href href; _class "block py-2 px-3 text-gray-900 rounded hover:bg-gray-100 md:hover:bg-transparent md:border-0 md:hover:text-blue-700 md:p-0 dark:text-white md:dark:hover:text-blue-500 dark:hover:bg-gray-700 dark:hover:text-white md:dark:hover:bg-transparent"] [encodedText text]
    ]

let layout (content: XmlNode list) (ctx: HttpContext) =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1"]
            title [] [encodedText "Tournament Scraper"]
            link [_rel "stylesheet"; _type "text/css"; _href "/style.css"]
            script [_src "/htmx@1.9.10.min.js"] []
            script [_src "/htmx-sse@1.9.10.js"] []
            script [_src "/_hyperscript@0.9.12.min.js"] []
        ]
        body [] [
            yield nav [_class "bg-white border-gray-200 dark:bg-gray-900 drop-shadow-md mb-4"] [
                div [_class "max-w-screen-xl flex flex-wrap items-center justify-between mx-auto p-4 py-6"] [
                    a [_class "flex items-center space-x-3 rtl:space-x-reverse"; _href "/"] [
                        img [_src "https://avatars0.githubusercontent.com/u/35305523?s=200"; _class "h-8"]
                        span [_class "self-center text-2xl font-semibold whitespace-nowrap dark:text-white"] [encodedText "Tournament Scraper"]
                    ]
                    button [_data "collapse-toggle" "navbar-default"; _type "button"; _class "inline-flex items-center p-2 w-10 h-10 justify-center text-sm text-gray-500 rounded-lg md:hidden hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-gray-200 dark:text-gray-400 dark:hover:bg-gray-700 dark:focus:ring-gray-600"; attr "aria-controls" "navbar-default"; attr "aria-expanded" "false"] [
                        span [_class "sr-only"] [encodedText "Open main menu"]
                        tag "svg" [_class "w-5 h-5"; attr "aria-hidden" "true"; attr "xmlns" "http://www.w3.org/2000/svg"; attr "fill" "none"; attr "viewBox" "0 0 17 14"] [
                            tag "path" [attr "stroke" "currentColor"; attr "stroke-linecap" "round"; attr "stroke-linejoin" "round"; attr "stroke-width" "2"; attr "d" "M1 1h15M1 7h15M1 13h15"] []
                        ]
                    ]
                    div [_class "hidden w-full md:block md:w-auto"; _id "navbar-default"] [
                        ul [_class "font-medium flex flex-col p-4 md:p-0 mt-4 border border-gray-100 rounded-lg bg-gray-50 md:flex-row md:space-x-8 rtl:space-x-reverse md:mt-0 md:border-0 md:bg-white dark:bg-gray-800 md:dark:bg-gray-900 dark:border-gray-700"] [
                            menuLink ctx "/" "Home"
                            menuLink ctx "/tournaments" "Tournaments"
                            menuLink ctx "/rankings" "Rankings"
                        ]
                    ]
                ]
            ]
            yield! content
            yield footer [_class "footer is-fixed-bottom"] [
                div [_class "container"] [
                    div [_class "content has-text-centered"] [
                        p [] [
                            rawText "Powered by "
                            a [_href "https://github.com/SaturnFramework/Saturn"] [rawText "Saturn"]
                            rawText " - F# MVC framework created by "
                            a [_href "http://lambdafactory.io"] [rawText "λFactory"]
                        ]
                    ]
                ]
            ]
        ]
    ]
