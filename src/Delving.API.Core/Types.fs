[<AutoOpen>]
module Delving.API.Core.Library.Types

open System

open Data.CommV.Primitives
open Data.MTA.Primitives
open Data.M4.Primitives

type CustomerLineEquipment =
    {
        ImportId : Guid
        CustomerName : string
        AccountNumber : AccountNumber
        PhoneNumber : PhoneNumber
        StructureId : StructureId
        SubscriberId : string
        ServiceType : string
        EquipmentName : string
        EquipmentCategory : string
        EquipmentType : string
        EquipmentStatus : AssignmentStatus
        Clli : CentralOfficeName
        IsBonded : bool
        IsLeftIn : bool
        LoopLength : int
        PrePostLoopLength : int
        EquipmentId : ElementId
        LineCardId : LinecardId
    }

type HouseDirections =
    {
        HouseId : HouseId
        StreetNumber : decimal
        StreetName : string
        City : string
        DrivingDirections : string option
    }
