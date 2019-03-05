module Migrations

open Npgsql
open Evolve
open System.Data.Common

let evolve connectionString =
  use conn = new NpgsqlConnection(connectionString)
  let ev = Evolve(conn)
  ev.Locations <- [ "db"; "db/migrations" ]
  ev.IsEraseDisabled <- true

  ev.Migrate()
  printfn "Migrations done"
  ()