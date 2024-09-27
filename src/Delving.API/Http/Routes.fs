module Delving.API.Http.Routes

open Giraffe
open Microsoft.AspNetCore.Http
open System
open System.Net

open Data.CommV.Primitives

open Delving.API.Http
open ErrorHandlers
open Helpers

let private notFoundHandler : HttpFunc -> HttpContext -> HttpFuncResult =
    "Path Not Found" |> text |> RequestErrors.notFound

let webApp staticToken : HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [
            routex "(/?)" >=> text "Delving API - V1"
            subRouteCi
                "/api/v1"
                (choose
                    [
                        staticBearer staticToken
                        >=> choose
                                [
                                    routeCix @"\/sample(\/?)"
                                    >=> GET
                                    >=> SearchHandlers.handleGetSampleCustomerEquipmentAsync
                                    subRouteCif
                                        "/directions/%i/%s"
                                        (fun (account, impId) ->
                                            match account |> decimal |> AccountNumber.TryMake with
                                            | None ->
                                                account |> InvalidHouseIdInUrl |> handleError HttpStatusCode.BadRequest
                                            | Some hid ->
                                                match impId |> Guid.TryParse with
                                                | false, _ ->
                                                    impId
                                                    |> InvalidImportIdInUrl
                                                    |> handleError HttpStatusCode.BadRequest
                                                | true, iid ->
                                                    GET >=> SearchHandlers.handleGetHouseDirectionsAsync hid iid
                                        )
                                ]
                    ])
            notFoundHandler
        ]
