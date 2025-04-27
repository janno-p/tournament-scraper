module TournamentScraper.Templates.Index

open Oxpecker.ViewEngine
open TournamentScraper.Templates

let index =
    Fragment() {
        section(class' = "hero is-primary") {
            div(class' = "hero-body") {
                div(class' = "container") {
                    div(class' = "columns is-vcentered") {
                        div(class' = "column") {
                            p(class' = "title") { "Welcome to Saturn!" }
                            p(class' = "subtitle") { "Opinionated, web development framework for F# which implements the server-side, functional MVC pattern" }
                        }
                    }
                }
            }
        }
        section(class' = "section") {
            h1(class' = "title") { "Resources" }
            div(class' = "tile is-ancestor") {
                div(class' = "tile is-parent is-4") {
                    article(class' = "tile is-child notification is-primary box") {
                        a(class' = "title", href = "https://saturnframework.org/tutorials/how-to-start.html") { "Guides (WIP)" }
                    }
                }
                div(class' = "tile is-parent is-4") {
                    article(class' = "tile is-child notification is-info box") {
                        a(class' = "title", href = "https://saturnframework.org/explanations/overview.html") { "Documentation (WIP)" }
                    }
                }
                div(class' = "tile is-parent is-4") {
                    article(class' = "tile is-child notification is-warning-box") {
                        a(class' = "title", href = "https://github.com/SaturnFramework/Saturn") { "Source" }
                    }
                }
            }
        }
        section(class' = "section") {
            h1(class' = "title") { "Help" }
            div(class' = "tile is-ancestor") {
                div(class' = "tile is-parent is-4") {
                    article(class' = "tile is-child notification is-link box") {
                        a(class' = "title", href = "https://github.com/SaturnFramework/Saturn/issues") { "GitHub issues" }
                    }
                }
                div(class' = "tile is-parent is-4") {
                    article(class' = "tile is-child notification is-danger box") {
                        a(class' = "title", href = "https://gitter.im/SaturnFramework/Saturn") { "Gitter" }
                    }
                }
                div(class' = "tile is-parent is-4") {
                    article(class' = "tile is-child notification is-success box") {
                        a(class' = "title", href = "https://safe-stack.github.io/") { "SAFE Stack" }
                    }
                }
            }
        }
    }

let layout ctx =
    App.layout index ctx
