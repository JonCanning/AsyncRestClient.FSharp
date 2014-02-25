namespace AsyncRestClient.Tests

open System
open AsyncRestClient
open Suave.Http
open Suave.Web
open System.Net.Http

type BaseTest(webPart) =
    let baseAddress = "http://localhost:8083"
    let content = ref ""
    let httpMethod = ref HttpMethod.Options
    let hooks = {emptyHooks with onBeforeSend = (fun x -> httpMethod := x.Method
                                                          if x.Content = null then
                                                            () 
                                                          else 
                                                            content := x.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously)}
    let dispose() = Async.CancelDefaultToken()
    do
        try
            let listening, server = web_server_async default_config webPart
            Async.Start server
            Async.RunSynchronously listening
        with
            | _ -> dispose()
    member x.client = create <| defaultClientWithHooks baseAddress hooks
    member x.lastHttpMethod with get() = !httpMethod
    member x.lastContent with get() = !content

    interface IDisposable with
        member x.Dispose() =
            dispose()

type request = {Name: string}
