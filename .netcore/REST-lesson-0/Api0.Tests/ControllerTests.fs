module ControllerTests

open System

open Xunit

open Controllers

[<Fact>]
let ``Test Guid`` () =
  try
    let (Guid id) = "test"
    failwith "Expected this to fail"
  with _ -> ()
  let valid = Guid.NewGuid()
  let (Guid id) = string valid
  Assert.Equal(id, valid)

[<Fact>]
let ``Test handleExn`` () = async {
  let e = exn "bam"
  let part = handleExn false e
  ()
}