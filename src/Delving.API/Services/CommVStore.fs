module Delving.API.Services.CommVStore

open Data.CommV

open Delving.API.Core

type DefaultCommVStore (commvDb : CommVDbContext) =
    interface ICommVStore with
        member _.PLACEHOLDER () : Async<unit> = async { return () }
