namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Delete'() as this =
    inherit BaseTest(OK "Hello")

    let result = base.client |> delete "/" |> Async.RunSynchronously

    [<Fact>]
    member x.``delete HttpMethod is used``() =
        this.httpMethod |> should equal System.Net.Http.HttpMethod.Delete