namespace Delving.API.Http

open FSharp.Control.TaskBuilder
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Options
open Microsoft.Extensions.Logging
open System
open System.Net

open Data.CommV.Primitives

open Delving.API.Config
open Delving.API.Http
open Delving.API.Json
open Delving.API.Core
open Delving.API.Core.Library

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

type private HandleGetCustomerEquipmentRequest = HandleGetCustomerEquipmentRequest
type private HandleGetHouseDirectionsRequest = HandleGetHouseDirectionsRequest

module SearchHandlers =
    open ErrorHandlers
    let handleGetSampleCustomerEquipmentAsync =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let logger = ctx.GetService<ILogger<HandleGetCustomerEquipmentRequest>>()
                let services = ctx.GetService<IServices>()

                try
                    let! result = findSampleCustomerEquipmentAsync services
                    match result.Length > 0 with
                    | true ->
                        let jsonResult =
                            result |> List.map CustomerLineEquipmentJson.ofDomain |> List.distinct

                        logger.LogInformation("Returning success ({StatusCode})", 200)
                        return! json jsonResult next ctx
                    | false -> return! RequestErrors.notFound (setBody [||]) next ctx
                with ex ->
                    return!
                        handleException logger ex HttpStatusCode.InternalServerError (UnknownError ex.Message) next ctx
            }

    let handleGetHouseDirectionsAsync (account : AccountNumber) (impId : Guid) =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let logger = ctx.GetService<ILogger<HandleGetHouseDirectionsRequest>>()
                let services = ctx.GetService<IServices>()

                try
                    let! result = findHouseDirectionsAsync services account impId

                    match result with
                    | Some hd ->
                        let jsonResult = HouseDirectionsJson.ofDomain hd
                        logger.LogInformation("Returning success ({StatusCode})", 200)
                        return! json jsonResult next ctx
                    | None -> return! RequestErrors.notFound (setBody [||]) next ctx
                with ex ->
                    return!
                        handleException logger ex HttpStatusCode.InternalServerError (UnknownError ex.Message) next ctx
            }
