module ProfileController

open Suave
open Suave.Successful
open Suave.Operators

open Controllers
open ProfileModel

let get (service:IProfileService) id: WebPart =
  fun http -> async {
    let (Guid id) = id
    let! profile = service.Get id
    return!
      OK (jsonResult profile)
      >=> Writers.setMimeType "application/json"
      <| http
  }