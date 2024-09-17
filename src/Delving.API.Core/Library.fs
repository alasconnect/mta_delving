namespace Delving.API.Core.Library

open Delving.API.Core

[<AutoOpen>]
module Default =
    let PLACEHOLDER (services : IServices) : Async<unit> = async { return () }
