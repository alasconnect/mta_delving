[<AutoOpen>]
module Delving.API.DataAccess.Types

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Npgsql
open SqlHydra.Query
open System

open Delving.API.Config
open Delving.API.DataAccess.Schema

type LineEquipmentBackupDbContext (provider : IServiceProvider, config : IOptions<LineEquipmentBackupDbConfig>) =
    let mutable disposed = false
    let config = config.Value
    let connection = new NpgsqlConnection(config.ConnectionString)
    let compiler = SqlKata.Compilers.PostgresCompiler()
    let queryCtx =
        let ctx = new QueryContext(connection, compiler)
        if config.LogQueries then
            let logger = provider.GetService<ILogger<LineEquipmentBackupDbConfig>>()
            if box logger <> null then
                ctx.Logger <- fun q -> logger.LogDebug("Executing query: \r\n{SqlQuery}", q.ToString())
        ctx
    do connection.Open()
    member _.Connection = connection
    member _.QueryCtx = queryCtx
    member _.SelectAsync () =
        selectAsync HydraReader.Read (Shared queryCtx)
    member _.InsertAsync () = insertAsync (Shared queryCtx)
    member _.UpdateAsync () = updateAsync (Shared queryCtx)
    member _.DeleteAsync () = deleteAsync (Shared queryCtx)
    member _.Dispose (disposing : bool) =
        if not disposed then
            if disposing then
                connection.Dispose()
                let ctx = queryCtx :> IDisposable
                ctx.Dispose()
            disposed <- true
    member this.Dispose () = this.Dispose(true)
    interface IDisposable with
        member this.Dispose () = this.Dispose()
