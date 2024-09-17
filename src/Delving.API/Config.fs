[<AutoOpen>]
module Delving.API.Config

open System

open Data.CommV
open Data.M4
open Data.M4.Database
open Data.M4.WebServices
open MTA.AspNetCore.Mvc.CommV.WebServices


[<CLIMutable>]
type LineEquipmentBackupDbConfig =
    {
        ConnectionString : string
        LogQueries : bool
    }

[<CLIMutable>]
type ConnectionsConfig =
    {
        CommVDb : CommVDbContextConfig
        CommVWebServices : CommVWebServicesClientOptions
        LineEquipmentBackupDb : LineEquipmentBackupDbConfig
        M4WebServices : M4WebServiceClientConfig
        M4Db : M4DbConfig
    }

[<CLIMutable>]
type DelvingApiConfig =
    {
        AuthenticationToken : string
        ErrorUriPrefix : string
    }
    member this.ErrorUriPrefixUri =
        new Uri(this.ErrorUriPrefix, UriKind.RelativeOrAbsolute)

[<CLIMutable>]
type RootConfig =
    {
        Connections : ConnectionsConfig
        DelvingApi : DelvingApiConfig
    }

[<RequireQualifiedAccess>]
module RootConfig =
    let toCommVDbConfig (config : RootConfig) : CommVDbContextConfig = config.Connections.CommVDb
    let toCommVWebServicesConfig (config : RootConfig) : CommVWebServicesClientOptions =
        config.Connections.CommVWebServices
    let toDelvingApiConfig (root : RootConfig) : DelvingApiConfig = root.DelvingApi
    let toLineEquipmentBackupDbContextConfig (root : RootConfig) : LineEquipmentBackupDbConfig =
        root.Connections.LineEquipmentBackupDb
    let toM4ClientConfig (config : RootConfig) : M4ClientConfig =
        M4ClientConfig(M4WebServicesConfig = config.Connections.M4WebServices, M4DbConfig = config.Connections.M4Db)
    let toM4DbConfig (config : RootConfig) : M4DbConfig = config.Connections.M4Db
