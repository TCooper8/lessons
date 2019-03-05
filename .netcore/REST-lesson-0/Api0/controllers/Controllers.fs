module Controllers

open System

open Newtonsoft.Json
open Suave

open ProfileModel

type InvalidGuid () =
  inherit Exception "Invalid GUID"

let jsonResult o =
  JsonConvert.SerializeObject o

let (|Guid|) (str:string) =
  match Guid.TryParse str with
  | false, _ -> raise (InvalidGuid())
  | true, id -> id

let handleExn forwardInternal: exn -> WebPart = function
  | :? InvalidGuid as e ->
    RequestErrors.BAD_REQUEST e.Message
  | :? ProfileNotFound as e ->
    RequestErrors.NOT_FOUND e.Message
  | e ->
    let id = Guid.NewGuid()
    let head = sprintf "InternalError %A" id
    let msg = sprintf "%s: %s" head e.Message
    printfn "[Controller] %s" msg
    ServerErrors.INTERNAL_ERROR (if forwardInternal then msg else head)