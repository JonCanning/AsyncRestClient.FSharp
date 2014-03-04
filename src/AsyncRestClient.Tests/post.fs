namespace AsyncRestClient.Tests

open FsUnit.Xunit
open Xunit
open Suave.Http
open System.Net.Http
open AsyncRestClient

type Post() as this = 
    inherit BaseTest(OK "Hello")
    
    let request = {Name = "Foo"}
    let (Some response) = this.client |> post request "/" |> Async.RunSynchronously

    [<Fact>]
    member x.``response is returned``() =
        response.text |> should equal "Hello"

    [<Fact>]
    member x.``content is serialized``() =
        let (Serializer (contentType, serialize)) = jsonNetSerializer
        this.content |> should equal (serialize request)

    [<Fact>]
    member x.``post HttpMethod is used``() = 
        this.httpMethod |> should equal HttpMethod.Post

    [<Fact>]
    member x.``onAfterSend called``() = 
        this.onAfterSend |> should equal true

type ``Post throws an exception``() as this = 
    inherit BaseTest(OK "Hello")
    do
        Async.CancelDefaultToken()
    let request = {Name = "Foo"}
    let response = this.client |> post request "/" |> Async.RunSynchronously

    [<Fact>]
    member x.``onException called``() =
        this.onException |> should equal true