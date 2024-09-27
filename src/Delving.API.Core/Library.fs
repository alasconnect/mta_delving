namespace Delving.API.Core.Library

open System

open Data.CommV.Primitives

open Delving.API.Core

[<AutoOpen>]
module Default =
    let findSampleCustomerEquipmentAsync (services : IServices) : Async<CustomerLineEquipment list> =
        async { return! services.LineEquipmentBackupStore.GetSampleCustomerEquipmentAsync() }

    let findHouseDirectionsAsync
        (services : IServices)
        (account : AccountNumber)
        (impId : Guid)
        : Async<HouseDirections option> =
        async { return! services.LineEquipmentBackupStore.GetHouseDirectionsAsync account impId }
