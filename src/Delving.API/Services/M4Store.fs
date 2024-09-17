module Delving.API.Services.M4Store

open Data.M4

open Delving.API.Core

type DefaultM4Store (m4Client : IM4Client) =
    interface IM4Store with
        member _.PLACEHOLDER () : Async<unit> = async { return () }
