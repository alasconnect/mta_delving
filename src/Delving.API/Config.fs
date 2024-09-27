[<AutoOpen>]
module Delving.API.Config

open System

[<CLIMutable>]
type LineEquipmentBackupDbConfig =
    {
        ConnectionString : string
        LogQueries : bool
    }

[<CLIMutable>]
type ConnectionsConfig =
    {
        LineEquipmentBackupDb : LineEquipmentBackupDbConfig
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
    let toDelvingApiConfig (root : RootConfig) : DelvingApiConfig = root.DelvingApi
    let toLineEquipmentBackupDbContextConfig (root : RootConfig) : LineEquipmentBackupDbConfig =
        root.Connections.LineEquipmentBackupDb
