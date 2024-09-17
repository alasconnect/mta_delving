[<AutoOpen>]
module Delving.API.Core.Interfaces

open Microsoft.Extensions.Logging

type ILEBStore =
    abstract PLACEHOLDER : unit -> Async<unit>

type IM4Store =
    abstract PLACEHOLDER : unit -> Async<unit>

type ICommVStore =
    abstract PLACEHOLDER : unit -> Async<unit>

type IServices =
    abstract LoggerFactory : ILoggerFactory
    abstract LEBStore : ILEBStore
    abstract M4Store : IM4Store
    abstract CommVStore : ICommVStore
