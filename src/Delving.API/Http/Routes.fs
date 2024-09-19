module Delving.API.Http.Routes

open Giraffe
open Microsoft.AspNetCore.Http

open Delving.API.Http

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
                        >=> choose [ routeCix @"\/delving(\/?)" >=> GET >=> SearchHandlers.handleGetPLACEHOLDERAsync ]
                    ])
            notFoundHandler
        ]
