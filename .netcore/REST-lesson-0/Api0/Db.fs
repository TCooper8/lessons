module Db

open Npgsql

let exec connectionString mapping query = async {
  let sql, args = QueryBuilder.toSql query

  use conn = new NpgsqlConnection(connectionString)
  use cmd = conn.CreateCommand()
  cmd.CommandText <- sql

  
  args
  |> Seq.iteri (fun i v ->
    cmd.Parameters.Add(NpgsqlParameter(string i, v)) |> ignore
  )

  do! conn.OpenAsync() |> Async.AwaitTask
  return! mapping cmd
}