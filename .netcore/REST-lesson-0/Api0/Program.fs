open System

open Suave
open Suave.Filters
open Suave.Operators

open ProfileModel
open Controllers

type Services = {
  verification: string -> bool
  profiles: IProfileService
}

let notFound = RequestErrors.NOT_FOUND "Not found"

let guarded verification next: WebPart =
  fun http -> async {
    let auth = Headers.getFirstHeader "authorization" http
    match auth with
    | None ->
      return!
        RequestErrors.UNAUTHORIZED "Authorization required"
        >=> Writers.setHeader "link" "/login"
        <| http

    | Some auth ->
      if not <| verification auth then
        return! RequestErrors.UNAUTHORIZED "Unauthorized" http
      else
        return! next http
  }

let app services: WebPart =
  choose
    [ 
      guarded
        <| services.verification
        <| choose
          [ GET >=> pathScan "/profiles/%s" (ProfileController.get services.profiles)
          ]
      GET >=> path "/health" >=> Successful.OK "Running"
      notFound
    ]

let router forwardInternal app: WebPart =
  fun http -> async {
    printfn "[Router] %A %A %A" (http.clientIp false []) http.request.method http.request.url
    let! res = app http |> Async.Catch
    match res with
    | Choice1Of2 res -> return res
    | Choice2Of2 e ->
      return! handleExn true e http
  }

[<EntryPoint>]
let main argv =
  let port =
    Environment.GetEnvironmentVariable "PORT"
    |> uint16

  let appEnv =
    Environment.GetEnvironmentVariable "APP_ENV"
    |> fun s -> if String.IsNullOrWhiteSpace s then "" else s

  let dbUrl =
    Environment.GetEnvironmentVariable "DATABASE_URL"
    |> Uri
    |> Util.connectionStringFromUrl

  do Migrations.evolve dbUrl

  let forwardInternal =
    match appEnv.ToLower() with
    | ""
    | "dev"
    | "development" -> true
    | _ -> false

  let services =
    { profiles = ProfileData.ProfileService dbUrl
      verification = fun _ -> true
    }

  let conf =
    { defaultConfig with
        bindings =
          [ HttpBinding.create HTTP System.Net.IPAddress.Any port
          ]
    }
  startWebServer conf (
    router forwardInternal (
      app services
    )
  )
  0