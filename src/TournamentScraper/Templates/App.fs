module App

open Giraffe.ViewEngine

let layout (content: XmlNode list) =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1"]
            title [] [encodedText "Hello SaturnServer"]
            link [_rel "stylesheet"; _type "text/css"; _href "/style.css"]
            script [_src "/htmx@1.9.10.min.js"] []
            script [_src "/_hyperscript@0.9.12.min.js"] []
        ]
        body [] [
            yield nav [_class "navbar is-fixed-top has-shadow"] [
                div [_class "navbar-brand"] [
                    a [_class "navbar-item"; _href "/"] [
                        img [_src "https://avatars0.githubusercontent.com/u/35305523?s=200"; _width "28"; _height "28"]
                    ]
                    div [_class "navbar-burger burger"; attr "data-target" "navMenu"] [
                        span [] []
                        span [] []
                        span [] []
                    ]
                ]
                div [_class "navbar-menu"; _id "navMenu"] [
                    div [_class "navbar-start"] [
                        a [_class "navbar-item"; _href "https://github.com/SaturnFramework/Saturn/blob/master/README.md"] [rawText "Getting started"]
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
