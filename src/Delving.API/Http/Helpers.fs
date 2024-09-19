module Delving.API.Http.Helpers

open Giraffe
open Giraffe.FormatExpressions
open Microsoft.AspNetCore.Http
open System

///<summary>
///Filters an incoming HTTP request based on a part of the request path (case sensitive).
///If the sub route matches the incoming HTTP request then the arguments from the <see cref="Microsoft.FSharp.Core.PrintfFormat"/> will be automatically resolved and passed into the supplied routeHandler.
///
///Supported format chars
///
///%b: bool
///%c: char
///%s: string
///%i: int
///%d: int64
///%f: float/double
///%O: Guid
///
///Subsequent routing handlers inside the given handler function should omit the already validated path.
///</summary>
///<param name="path">A format string representing the expected request sub path.</param>
///<param name="routeHandler">A function which accepts a tuple 'T of the parsed arguments and returns a <see cref="HttpHandler"/> function which will subsequently deal with the request.</param>
///<returns>A Giraffe <see cref="HttpHandler"/> function which can be composed into a bigger web application.</returns>
let subRouteCif (path : PrintfFormat<_, _, _, _, 'T>) (routeHandler : 'T -> HttpHandler) : HttpHandler =
    validateFormat path
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let paramCount = (path.Value.Split '/').Length
        let subPathParts = (SubRouting.getNextPartOfPath ctx).Split '/'
        if paramCount > subPathParts.Length then
            skipPipeline
        else
            let subPath =
                subPathParts
                |> Array.take paramCount
                |> Array.fold
                    (fun state elem ->
                        if String.IsNullOrEmpty elem then
                            state
                        else
                            sprintf "%s/%s" state elem
                    )
                    ""
            tryMatchInput path MatchOptions.IgnoreCaseExact subPath
            |> function
                | None -> skipPipeline
                | Some args -> SubRouting.routeWithPartialPath subPath (routeHandler args) next ctx
