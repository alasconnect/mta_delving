namespace Delving.API.Json

open System

open Delving.API.Core.Library

type CustomerLineEquipmentJson =
    {
        CustomerName : string
        AccountNumber : int
        PhoneNumber : string
        StructureId : int
        SubscriberId : string
        ServiceType : string
        EquipmentName : string
        EquipmentCategory : string
        EquipmentType : string
        EquipmentStatus : string
        Clli : string
        IsBonded : bool
        IsLeftIn : bool
        LoopLength : int
        PrePostLoopLength : int
        EquipmentId : int
        LineCardId : int
        ImportId : string
    }

module CustomerLineEquipmentJson =
    let ofDomain (cle : CustomerLineEquipment) : CustomerLineEquipmentJson =
        {
            CustomerName = cle.CustomerName
            AccountNumber = cle.AccountNumber.Value |> int
            PhoneNumber = cle.PhoneNumber.ToSimpleString()
            StructureId = cle.StructureId.Value
            SubscriberId = cle.SubscriberId
            ServiceType = cle.ServiceType
            EquipmentName = cle.EquipmentName
            EquipmentCategory = cle.EquipmentCategory
            EquipmentType = cle.EquipmentType
            EquipmentStatus = cle.EquipmentStatus.ToString()
            Clli = cle.Clli.Value
            IsBonded = cle.IsBonded
            IsLeftIn = cle.IsLeftIn
            LoopLength = cle.LoopLength
            PrePostLoopLength = cle.PrePostLoopLength
            EquipmentId = cle.EquipmentId.Value
            LineCardId = cle.LineCardId.Value
            ImportId = cle.ImportId.ToString()
        }

type HouseDirectionsJson =
    {
        HouseId : int
        StreetNumber : int
        StreetName : string
        City : string
        DrivingDirections : string option
    }

module HouseDirectionsJson =
    let ofDomain (hd : HouseDirections) : HouseDirectionsJson =
        {
            HouseId = hd.HouseId.Value |> int
            StreetNumber = hd.StreetNumber |> int
            StreetName = hd.StreetName
            City = hd.City
            DrivingDirections = hd.DrivingDirections
        }
