module Dapper

open System
open System.Data
open System.Data.Common
open Microsoft.AspNetCore.Http
open Microsoft.Data.Sqlite
open Microsoft.Extensions.DependencyInjection
open Saturn

type SqlConnectionFactory(connectionString: string) =
    let mutable connection: DbConnection option = None
    member _.GetOpenConnectionAsync() =
        task {
            match connection with
            | Some(c) when c.State = ConnectionState.Open ->
                return c
            | _ ->
                let c: DbConnection = new SqliteConnection(connectionString)
                do! c.OpenAsync()
                connection <- Some(c)
                return c
        }
    interface IDisposable with
        member _.Dispose() =
            connection |> Option.iter (fun c ->
                if c.State = ConnectionState.Open then c.Dispose()
            )

type ApplicationBuilder with
    [<CustomOperation("use_dapper")>]
    member this.UseDapper(state: ApplicationState, connectionString: string) =
        let service (s: IServiceCollection) =
            s.AddScoped<SqlConnectionFactory>(fun _ -> new SqlConnectionFactory(connectionString))
        
        { state with
            ServicesConfig = service::state.ServicesConfig 
        }

let getConnection (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<SqlConnectionFactory>().GetOpenConnectionAsync()
