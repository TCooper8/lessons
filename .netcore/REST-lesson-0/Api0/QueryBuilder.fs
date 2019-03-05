module QueryBuilder

open System

type Value =
  | Val of obj
  | Ref of string

type Condition =
  | Eq of key:string * value:Value

type Sql =
  | Table of string

type Query =
  | Returning of key:string
  | Insert of sql:Sql * pairs:(string * Value) list * Query option
  | Select of fields:string list * Query list
  | From of sql:Sql * refName:string option
  | Join of Sql:Sql * refName:string * on:Condition
  | Where of Condition
  | And of Condition
  | Or of Condition

let profiles = Table "profiles"

let user =
  Select(
    [ "entity"
      "screen_name"
    ],
    [ From(profiles, None)
      Where (Eq ("entity", Val "testing"))
    ]
  )

let conditionToSql count args condition =
  let (|Value|) = function
    | Ref name -> name, None
    | Val var ->
      let i = !count
      incr count
      sprintf ":%i" i, Some var

  let condSql, arg =
    match condition with
    | Eq (key, Value(value, arg)) -> sprintf "%s = %s" key value, arg

  condSql
  , count
  , match arg with | None -> args | Some arg -> arg :: args

let rec toSqlLoop (sql, count, args) = function
  | Returning key ->
    sprintf "%s RETURNING %s"
      <| sql
      <| key
      , count
      , args

  | Insert (Table table, fields, tail) ->
    let keys, values, args =
      fields
      |> Seq.fold(fun (keys, values, args) (key, value) ->
        match value with
        | Val arg ->
          let i = !count
          incr count
          key::keys, (i :> obj)::values, arg::args
        | Ref value ->
          key::keys, (value :> obj)::values, args
      ) ([], [], args)

    match tail with
    | None -> 
      sprintf "%s INSERT into %s (%s) VALUES (%s)"
        <| sql
        <| table
        <| String.Join(",", keys)
        <| String.Join(",", values)
      , count
      , args

    | Some tail ->
      let tailSql, count, args = toSqlLoop (sql, count, args) tail
      sprintf "%s INSERT into %s (%s) VALUES (%s) %s"
        <| sql
        <| table
        <| String.Join(",", keys)
        <| String.Join(",", values)
        <| tailSql
      , count
      , args

  | Select (fields, clauses) ->
    let clausesSql, count, args =
      clauses
      |> List.fold toSqlLoop (sql, count, args)
    sprintf "%s SELECT %s %s"
      <| sql
      <| String.Join(", ", fields) 
      <| clausesSql
    , count
    , args

  | From (Table table, refName) ->
    sprintf "%s FROM %s %s"
      <| sql
      <| table
      <| defaultArg refName ""
    , count
    , args

  | Join (Table table, refName, condition) ->
    let clause, count, args = conditionToSql count args condition
    sprintf "%s JOIN %s %s ON %s"
      <| sql
      <| table
      <| refName
      <| clause
    , count
    , args

  | Where condition ->
    let clause, count, args = conditionToSql count args condition
    sprintf "%s WHERE %s"
      <| sql
      <| clause
    , count
    , args

  | And condition ->
    let clause, count, args = conditionToSql count args condition
    sprintf "%s AND %s"
      <| sql
      <| clause
    , count
    , args

  | Or condition ->
    let clause, count, args = conditionToSql count args condition
    sprintf "%s OR %s"
      <| sql
      <| clause
    , count
    , args

let toSql query =
  let count = ref 0
  toSqlLoop ("", count, []) query
  |> fun (sql, _, args) -> sql, args