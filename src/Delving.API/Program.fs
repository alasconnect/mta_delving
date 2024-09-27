module Delving.API.App

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Microsoft.FSharpLu.Json
open Newtonsoft.Json
open Serilog
open System

open Delving.API.Core

open Delving.API.Config
open Delving.API.DataAccess
open Delving.API.Http.Routes
open Delving.API.Services.Default
open Delving.API.Services.LineEquipmentBackupStore

// ---------------------------------
// Error handler
// ---------------------------------
let errorHandler (ex : Exception) (logger : Microsoft.Extensions.Logging.ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text "An unhandled error occurred."

// ---------------------------------
// Config Helpers
// ---------------------------------

let build (bldr : WebApplicationBuilder) = bldr.Build()

let run (app : WebApplication) =
    try
        app.Run()
    with ex ->
        Serilog.Log.Fatal(ex, "Fatal error occurred.")
        reraise ()

// ---------------------------------
// Config and Main
// ---------------------------------

let configureLogging (bldr : ILoggingBuilder) = bldr.AddConsole().AddDebug() |> ignore

let configureJsonOptions =
    JsonSerializerSettings()
    |> fun conf ->
        conf.Converters.Add(CompactUnionJsonConverter(true))
        conf

let configureSerilog (context : HostBuilderContext) (services : IServiceProvider) (config : LoggerConfiguration) =
    config.ReadFrom
        .Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
    |> ignore

let configureCors (bldr : CorsPolicyBuilder) =
    bldr.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() |> ignore

let addSerilog (bldr : WebApplicationBuilder) : WebApplicationBuilder =
    configureSerilog |> bldr.Host.UseSerilog |> ignore
    bldr

let addGiraffe (bldr : WebApplicationBuilder) =
    bldr.Services.AddGiraffe() |> ignore
    bldr

let addCors (bldr : WebApplicationBuilder) =
    bldr.Services.AddCors() |> ignore
    bldr

let addServices (bldr : WebApplicationBuilder) =
    bldr.Host.UseDefaultServiceProvider(fun ctx ops ->
        if ctx.HostingEnvironment.IsDevelopment() then
            ops.ValidateOnBuild <- true
            ops.ValidateScopes <- true
    )
    |> ignore

    bldr.Services
        .Configure<RootConfig>(bldr.Configuration)
        .Configure(RootConfig.toDelvingApiConfig)
        .Configure(RootConfig.toLineEquipmentBackupDbContextConfig)
        .AddScoped<LineEquipmentBackupDbContext>()
        .AddScoped<ILineEquipmentBackupStore, DefaultLineEquipmentBackupStore>()
        .AddScoped<IServices, DefaultServices>()
    |> ignore

    bldr


let useGiraffe (app : WebApplication) =
    let config = app.Services.GetService<IOptions<DelvingApiConfig>>().Value
    let staticToken = config.AuthenticationToken
    let env = app.Services.GetService<IWebHostEnvironment>()
    app.UseGiraffe(webApp staticToken)
    if not (env.IsDevelopment()) then
        app.UseGiraffeErrorHandler(errorHandler) |> ignore
    app

let useCors (app : WebApplication) =
    app.UseCors(configureCors) |> ignore
    app

let useDevEnv (app : WebApplication) =
    let env = app.Services.GetService<IWebHostEnvironment>()
    if env.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore
    app

let useSerilogRequestLogging (app : WebApplication) =
    app.UseSerilogRequestLogging() |> ignore
    app

let bootstrapLogger () =
    Log.Logger <- LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger()
    Serilog.Debugging.SelfLog.Enable(fun msg ->
        // Print the error to the console so we can see it in the worst case.
        printfn "An error occurred while logging a message: %s" msg
        // Try to log to Serilog, but if this fails, it could log the failure in an infinite loop.
        Log.Logger
            .ForContext("Alerting", {| Alert = true ; Severity = "Low" |}, true)
            .Error("An error occurred while logging a message: {ErrorMessage:l}", msg)
    )

[<EntryPoint>]
let main args =
    do bootstrapLogger ()
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true

    try
        try
            WebApplication.CreateBuilder(args)
            |> addSerilog
            |> addCors
            |> addGiraffe
            |> addServices
            |> build
            |> useDevEnv
            |> useCors
            |> useSerilogRequestLogging
            |> useGiraffe
            |> run
            0
        with ex ->
            Serilog.Log.Fatal(ex, "Fatal error occurred: {ErrorMessage:l}", ex.Message)
            1
    finally
        // Close and flush the log before the application exists to ensure all log messages are written to their sinks.
        Log.CloseAndFlush()
