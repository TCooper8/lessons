module QueryBuilderTests

open Xunit
open QueryBuilder

[<Fact>]
let ``Test basic query`` () =
  let sql, args =
    Select(
      [ "user.id"
        "user.name"
      ],
      [ From(Table "users", Some "user")
        Join(Table "profiles", "profile", Eq("profile.id", Val "testing"))
        Where (Eq ("user.id", Val "test"))
      ]
    )
    |> toSql
  ()