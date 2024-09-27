module Delving.API.Services.Default

open Microsoft.Extensions.Logging

open Delving.API.Core

type DefaultServices (loggerFactory : ILoggerFactory, lebStore : ILineEquipmentBackupStore) =
    interface IServices with
        member _.LoggerFactory = loggerFactory
        member _.LineEquipmentBackupStore = lebStore
