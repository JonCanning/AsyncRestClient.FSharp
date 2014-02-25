namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Get'() =
    inherit BaseTest(OK "Hello")

    let (Some response) = base.client "/" Get |> Async.RunSynchronously

    [<Fact>]
    member x.``response is returned``() = 
        response |> should equal "Hello"

    [<Fact>]
    member x.``get HttpMethod is used``() =
        base.lastHttpMethod |> should equal System.Net.Http.HttpMethod.Get