namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Post'() = 
    inherit BaseTest(OK "Hello")
    
    let request = {Name = "Foo"}
    let (Some response) = base.client "/" <| Post(request) |> Async.RunSynchronously

    [<Fact>]
    member x.``response is returned``() =
        response |> should equal "Hello"

    [<Fact>]
    member x.``content is serialized``() =
        let (Serializer (contentType, serialize)) = jsonNetSerializer
        base.lastContent |> should equal (serialize request)

    [<Fact>]
    member x.``post HttpMethod is used``() = 
        base.lastHttpMethod |> should equal System.Net.Http.HttpMethod.Post