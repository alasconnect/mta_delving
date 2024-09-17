namespace Delving.API.Http

open FSharp.Control.TaskBuilder
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Options
open Microsoft.Extensions.Logging
open System
open System.Net

open Data.M4.Common

open Delving.API.Config
open Delving.API.Http

[<AutoOpen>]
module Default =
    let (|M4DataException|_|) (exn : Exception) =
        match exn with
        | :? M4DataException as ex -> Some ex
        | :? AggregateException as ex ->
            match ex.InnerException with
            | :? M4DataException as ex' -> Some ex'
            | _ -> None
        | _ -> None

type private HandleError = HandleError

module ErrorHandlers =
    let private getErrorResponseBody (statusCode : HttpStatusCode) (errorResponse : ErrorResponse) (ctx : HttpContext) =
        let config = ctx.GetService<IOptions<DelvingApiConfig>>().Value
        ErrorResponse.toErrorResponseBody config.ErrorUriPrefixUri statusCode errorResponse
    let handleErrorQuietly (statusCode : HttpStatusCode) (errorResponse : ErrorResponse) =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let body = getErrorResponseBody statusCode errorResponse ctx
                return! ErrorResponse.toHttpHandler statusCode body next ctx
            }
    let handleError (statusCode : HttpStatusCode) (errorResponse : ErrorResponse) =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let logger = ctx.GetService<ILogger<HandleError>>()
                let body = getErrorResponseBody statusCode errorResponse ctx
                use _ = logger.BeginScope(body)
                logger.LogError("Encountered error while processing request: {ErrorMessage:l}", body.Detail)
                return! ErrorResponse.toHttpHandler statusCode body next ctx
            }
    let handleException
        (logger : ILogger)
        (ex : Exception)
        (statusCode : HttpStatusCode)
        (errorResponse : ErrorResponse)
        =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let body = getErrorResponseBody statusCode errorResponse ctx
                use _ = logger.BeginScope(body)
                logger.LogError(ex, "Encountered error while processing request: {ErrorMessage:l}", body.Detail)
                return! ErrorResponse.toHttpHandler statusCode body next ctx
            }

type private HandleGetCustomersRequest = HandleGetCustomersRequest

module SearchHandlers =
    open ErrorHandlers
    let handleGetPLACEHOLDERAsync =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let logger = ctx.GetService<ILogger<HandleGetCustomersRequest>>()
                return! handleErrorQuietly HttpStatusCode.BadRequest EmptySearchTerm next ctx
            }
