namespace Delving.API.Http

open System
open System.Net

open Giraffe

type ErrorResponse =
    | UnknownError of string
    | MissingSearchTerm
    | EmptySearchTerm
    | InvalidPhoneNumberInUrl of string
    | InvalidUsernameInUrl of string
    | InvalidCustomerNameInUrl of string
    | M4DataError of string

type ErrorResponseBody =
    {
        Type : Uri
        Title : string
        Detail : string
        Status : HttpStatusCode
    }

[<RequireQualifiedAccess>]
module ErrorResponse =
    let toUriRef (error : ErrorResponse) : string =
        match error with
        | UnknownError _ -> "errors/UnknownError/"
        | MissingSearchTerm -> "errors/MissingSearchTerm/"
        | EmptySearchTerm -> "errors/EmptySearchTerm/"
        | InvalidPhoneNumberInUrl _ -> "errors/InvalidPhoneNumberInUrl/"
        | InvalidUsernameInUrl _ -> "errors/InvalidUsernameInUrl/"
        | InvalidCustomerNameInUrl _ -> "errors/InvalidCustomerNameInUrl/"
        | M4DataError _ -> "errors/M4DataError/"
    let toUri (error : ErrorResponse) : Uri =
        let ref = toUriRef error
        new Uri(ref, UriKind.Relative)
    let toTitle (error : ErrorResponse) : string =
        match error with
        | UnknownError _ -> "Unknown error occurred."
        | MissingSearchTerm -> "Missing search term."
        | EmptySearchTerm -> "Empty search term provided."
        | InvalidPhoneNumberInUrl _ -> "Invalid phone number in URL."
        | InvalidUsernameInUrl _ -> "Invalid username in URL."
        | InvalidCustomerNameInUrl _ -> "Invalid customer name in URL."
        | M4DataError _ -> "An error occurred while processing data from M4."
    let toDetail (error : ErrorResponse) : string =
        match error with
        | UnknownError msg -> msg
        | MissingSearchTerm -> "Query parameter 'term' is required."
        | EmptySearchTerm -> "Search term cannot be empty."
        | InvalidPhoneNumberInUrl pn -> sprintf "Value '%s' is not a valid phone number." pn
        | InvalidUsernameInUrl un -> sprintf "Value '%s' is not a valid username." un
        | InvalidCustomerNameInUrl cn -> sprintf "Value '%s' is not a valid account number." cn
        | M4DataError msg -> msg
    let toErrorResponseBody
        (errorUriPrefix : Uri)
        (status : HttpStatusCode)
        (error : ErrorResponse)
        : ErrorResponseBody =
        {
            Type = new Uri(errorUriPrefix, toUri error)
            Title = toTitle error
            Detail = toDetail error
            Status = status
        }
    let toHttpHandler (statusCode : HttpStatusCode) (body : ErrorResponseBody) =
        (statusCode |> int |> setStatusCode) >=> (body |> json)
