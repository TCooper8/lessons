module DataTests

open System

open Xunit

open ProfileData
open ProfileModel

let dbUrl =
  Environment.GetEnvironmentVariable "DATABASE_URL"
  |> Uri
  |> Util.connectionStringFromUrl

let service = ProfileService dbUrl :> IProfileService

[<Fact>]
let ``Test Create`` () = async {
  let entity = Guid.NewGuid()
  let profile =
    { entity = entity
      screenName = "test"
    }
  do! service.Create profile
}

[<Fact>]
let ``Test Get | NotFound`` () = async {
  let id = Guid.NewGuid()
  try
    do! service.Get id |> Async.Ignore
  with
  | :? ProfileNotFound as e -> ()
  | e -> return raise e
}

[<Fact>]
let ``Test Get`` () = async {
  let entity = Guid.NewGuid()
  let profile =
    { entity = entity
      screenName = "test"
    }
  do! service.Create profile

  let! _profile = service.Get entity
  Assert.Equal(profile, _profile)
}