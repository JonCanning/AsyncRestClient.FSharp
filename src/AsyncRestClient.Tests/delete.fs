namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Delete'() =
    inherit BaseTest(OK "Hello")

    let result = base.client "/" Delete |> Async.RunSynchronously

    [<Fact>]
    member x.``delete HttpMethod is used``() =
        base.lastHttpMethod |> should equal System.Net.Http.HttpMethod.Delete