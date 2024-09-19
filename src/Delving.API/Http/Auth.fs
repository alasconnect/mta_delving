namespace Delving.API.Http

open System

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open FSharp.Control.TaskBuilder
open Giraffe

// Logging context
type private Auth = Auth

[<AutoOpen>]
module Auth =
    let private staticBearerFailure ctx =
        (setStatusCode 401 >=> setHttpHeader "WWW-Authenticate" "Bearer") earlyReturn ctx

    let staticBearer reqToken =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let logger = ctx.GetService<ILogger<Auth>>()
                use _ = logger.BeginScope("Authenticating with Static Bearer Token.")
                logger.LogDebug("Authentication requested.")

                let authHeader = string ctx.Request.Headers.["Authorization"]
                if String.IsNullOrEmpty(authHeader) then
                    logger.LogDebug("Authentication failed: Authorization header missing or empty.")
                    return! staticBearerFailure ctx
                else if authHeader.StartsWith("Bearer") |> not then
                    logger.LogDebug("Authentication failed: Authorization header does not contain 'Bearer' token.")
                    return! staticBearerFailure ctx
                else
                    let token = authHeader.Substring("Bearer ".Length).Trim()
                    if token = reqToken then
                        logger.LogDebug("Authentication succeeded.")
                        return! next ctx
                    else
                        logger.LogDebug("Authentication failed.")
                        return! staticBearerFailure ctx
            }
