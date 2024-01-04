module TournamentScraper.Tournaments.Extensions

open Microsoft.Extensions.DependencyInjection
open Saturn.Application
open TournamentScraper.Tournaments

type ApplicationBuilder with
    [<CustomOperation("use_tournaments")>]
    member this.UseScraper(state: ApplicationState) =
        let service (s: IServiceCollection) = s.AddSingleton<Akka.Actor.IActorRef>(Actors.createTournamentsActor)
        { state with
            ServicesConfig = service::state.ServicesConfig 
        }
