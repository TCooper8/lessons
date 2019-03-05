module Util

open System
open System.Web

open Npgsql

let connectionStringFromUrl (url:Uri): string =
  let userInfo = url.UserInfo.Split ':'
  let builder = NpgsqlConnectionStringBuilder()

  builder.Host <- url.Host
  builder.Port <- url.Port
  builder.Username <- userInfo.[0]
  builder.Password <- userInfo.[1]
  builder.Database <- url.LocalPath.TrimStart '/'
  
  let query = HttpUtility.ParseQueryString url.Query

  match query.Get "ssl-mode" with
  | null -> ()
  | "disable" -> builder.SslMode <- SslMode.Disable
  | "prefer" -> builder.SslMode <- SslMode.Prefer

  printfn "connection string = %s" (string builder)

  string builder