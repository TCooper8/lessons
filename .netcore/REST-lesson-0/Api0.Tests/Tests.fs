module Tests

open System
open System.Text

open Xunit
open Newtonsoft.Json

let enc = Encoding.UTF8

let dbUrl =
  Environment.GetEnvironmentVariable "DATABASE_URL"
  |> Uri
  |> Util.connectionStringFromUrl

[<Fact>]
let ``My test`` () =
  Assert.Equal(ToTest.x 5 6, 11)

module TestProfileController =
  open ProfileController
  open ProfileData
  open ProfileModel

  [<Fact>]
  let ``Test get profile`` () = async {
    let id = Guid.NewGuid()
    let service = ProfileService dbUrl :> IProfileService
    let profile =
      { entity = id
        screenName = "test"
      }
    do! service.Create profile

    let! (Some http) = get service (string id) Suave.Http.HttpContext.empty
    let resp = http.response

    Assert.Equal(resp.status.code, 200)
    let body = resp.content
    match body with
    | Suave.Http.HttpContent.Bytes bytes ->
      let _profile = JsonConvert.DeserializeObject<Profile> (enc.GetString bytes)
      Assert.Equal(_profile, profile)
    | _ -> failwith "Expected bytes content"
  }