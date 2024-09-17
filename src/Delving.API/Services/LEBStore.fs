module Delving.API.Services.LEBStore

open Delving.API.Core
open Delving.API.DataAccess

type DefaultLEBStore (lebDb : LineEquipmentBackupDbContext) =
    interface ILEBStore with
        member _.PLACEHOLDER () : Async<unit> = async { return () }
