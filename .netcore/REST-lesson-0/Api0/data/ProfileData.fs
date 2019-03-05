module ProfileData

open System

open ProfileModel
open QueryBuilder

open Npgsql

type ProfileService (connectionString) =
  let profiles = ref Map.empty
  interface IProfileService with
    member __.Get id =
      Select(
          [ "entity"
            "screen_name"
          ],
          [ From(Table "profiles", None)
            Where(Eq ("entity", Val id))
          ]
        )
      |> Db.exec connectionString (fun cmd -> async {
        use! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
        if not <| reader.Read() then
          return raise <| ProfileNotFound()
        else
          return
            { entity = reader.GetGuid (reader.GetOrdinal "entity")
              screenName = reader.GetString (reader.GetOrdinal "screen_name")
            }
      })

    member __.Create profile =
      Insert(
        Table "profiles",
        [ "entity", Val profile.entity
          "screen_name", Val profile.screenName
        ],
        None
      )
      |> Db.exec connectionString (fun cmd -> async {
        let! _ = cmd.ExecuteNonQueryAsync() |> Async.AwaitTask
        return ()
      })