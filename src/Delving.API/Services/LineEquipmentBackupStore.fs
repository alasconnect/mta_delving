module Delving.API.Services.LineEquipmentBackupStore

open Data.CommV.Primitives
open Data.MTA.Primitives
open Data.M4.Primitives
open System

open Delving.API.Core
open Delving.API.Core.Library
open Delving.API.DataAccess
open Delving.API.DataAccess.Schema.``public``

let private toCustomerLineEquipment
    (ad : account_details, i : import, cs : customer_service, l : linecard, e : equipment, s : structure, t : terminal)
    : CustomerLineEquipment =
    {
        ImportId = i.id
        CustomerName =
            match (ad.first_name.Length > 0 && ad.last_name.Length > 0) with
            | true -> ad.first_name + " " + ad.last_name
            | false -> ad.service_name
        AccountNumber = AccountNumber.MakeUnsafe ad.account_number
        PhoneNumber =
            {
                AreaCode = AreaCode.MakeUnsafe ad.area_code
                Exchange = Exchange.MakeUnsafe ad.exchange
                LineNumber = LineNumber.MakeUnsafe ad.line_number
            }
        StructureId = StructureId.MakeUnsafe s.id
        SubscriberId = ad.subscriber_id
        ServiceType = ad.service_type_id
        EquipmentName = e.name
        EquipmentCategory = e.category
        EquipmentType = e.``type``
        EquipmentStatus = AssignmentStatus.MakeUnsafe e.status
        Clli = CentralOfficeName.MakeUnsafe e.central_office_name
        IsBonded = l.is_bonded
        IsLeftIn = l.is_left_in
        LoopLength = t.loop_length
        PrePostLoopLength = t.pre_post_loop_length
        EquipmentId = ElementId.MakeUnsafe e.id
        LineCardId = LinecardId.MakeUnsafe l.id
    }

let private toHouseDirections (ad : account_details, hd : house_directions option) : HouseDirections =
    {
        HouseId = HouseId.MakeUnsafe ad.house_id
        StreetNumber = ad.street_number
        StreetName = ad.street_name
        City = ad.city
        DrivingDirections = hd |> Option.map (fun h -> h.driving_directions)
    }

type DefaultLineEquipmentBackupStore (lebDb : LineEquipmentBackupDbContext) =
    interface ILineEquipmentBackupStore with
        member _.GetSampleCustomerEquipmentAsync () : Async<CustomerLineEquipment list> =
            async {
                try
                    return!
                        lebDb.SelectAsync() {
                            for ad in account_details do
                                join i in import on ((ad.import_id) = (i.id))
                                join cs in customer_service
                                               on
                                               ((i.id, ad.account_number) = (cs.import_id, cs.commv_account_number))
                                join l in linecard on ((i.id, (Some cs.id)) = (l.import_id, l.customer_service_id))
                                join e in equipment on ((i.id, (Some l.id)) = (e.import_id, e.linecard_id))
                                join s in structure on ((i.id, (Some ad.house_id)) = (s.import_id, s.house_id))
                                join t in terminal on ((i.id, e.id) = (t.import_id, t.id))
                                where (i.is_latest && ad.account_number = 282677M)
                                select (ad, i, cs, l, e, s, t) into selected
                                mapList (toCustomerLineEquipment selected)
                        }
                with ex ->
                    raise ex
                    return []
            }
        member _.GetHouseDirectionsAsync (account : AccountNumber) (impId : Guid) : Async<HouseDirections option> =
            async {
                try
                    let act = account.Value

                    let! result =
                        lebDb.SelectAsync() {
                            for ad in account_details do
                                leftJoin hd in house_directions
                                                   on
                                                   ((ad.import_id, ad.house_id) = (hd.Value.import_id, hd.Value.house_id))
                                where (ad.import_id = impId && ad.account_number = act)
                                select (ad, hd) into selected
                                mapList (toHouseDirections selected)
                        }
                    match not (result.IsEmpty) && result.Length = 1 with
                    | true -> return Some result.Head
                    | false -> return None
                with ex ->
                    raise ex
                    return None
            }
