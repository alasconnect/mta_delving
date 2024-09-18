module Delving.API.Services.LineEquipmentBackupStore

open Delving.API.Core
open Delving.API.DataAccess

type DefaultLineEquipmentBackupStore (lebDb : LineEquipmentBackupDbContext) =
    interface ILineEquipmentBackupStore with
        member _.PLACEHOLDER () : Async<unit> = async { return () }
