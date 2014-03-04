namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Get() as this =
    inherit BaseTest(OK "Hello")

    let (Some response) = this.client |> get "/" |> Async.RunSynchronously

    [<Fact>]
    member x.``response is returned``() = 
        response.text |> should equal "Hello"

    [<Fact>]
    member x.``get HttpMethod is used``() =
        this.httpMethod |> should equal System.Net.Http.HttpMethod.Get