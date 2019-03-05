module ProfileModel

open System

type ProfileNotFound() =
  inherit Exception "Profile not found"

type Profile = {
  entity: Guid
  screenName: string
}

type IProfileService =
  abstract Get: Guid -> Profile Async
  abstract Create: Profile -> unit Async
