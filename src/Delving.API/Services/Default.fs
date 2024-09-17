module Delving.API.Services.Default

open Microsoft.Extensions.Logging

open Delving.API.Core

type DefaultServices
    (loggerFactory : ILoggerFactory, commVStore : ICommVStore, m4Store : IM4Store, lebStore : ILEBStore) =
    interface IServices with
        member _.LoggerFactory = loggerFactory
        member _.CommVStore = commVStore
        member _.M4Store = m4Store
        member _.LEBStore = lebStore
