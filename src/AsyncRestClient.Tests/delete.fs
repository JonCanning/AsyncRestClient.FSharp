namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Delete() as this =
    inherit BaseTest(OK "Hello")

    let result = this.client |> delete "/" |> Async.RunSynchronously

    [<Fact>]
    member x.``delete HttpMethod is used``() =
        this.httpMethod |> should equal System.Net.Http.HttpMethod.Delete

    [<Fact>]
    member x.``onAfterSend called``() = 
        this.onAfterSend |> should equal true

type ``Delete throws an exception``() as this = 
    inherit BaseTest(OK "Hello")
    do
        Async.CancelDefaultToken()
    let response = this.client |> delete "/" |> Async.RunSynchronously

    [<Fact>]
    member x.``onException called``() =
        this.onException |> should equal true