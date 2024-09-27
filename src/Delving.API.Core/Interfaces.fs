[<AutoOpen>]
module Delving.API.Core.Interfaces

open Microsoft.Extensions.Logging
open System

open Data.CommV.Primitives

open Delving.API.Core.Library

type ILineEquipmentBackupStore =
    abstract GetSampleCustomerEquipmentAsync : unit -> Async<CustomerLineEquipment list>
    abstract GetHouseDirectionsAsync : AccountNumber -> Guid -> Async<HouseDirections option>

type IServices =
    abstract LoggerFactory : ILoggerFactory
    abstract LineEquipmentBackupStore : ILineEquipmentBackupStore
