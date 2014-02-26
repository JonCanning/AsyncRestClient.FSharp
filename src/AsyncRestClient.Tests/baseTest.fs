namespace AsyncRestClient.Tests

open System
open AsyncRestClient
open Suave.Http
open Suave.Web
open System.Net.Http

type BaseTest(webPart) as this = 
    let baseAddress = "http://localhost:8083"
    [<DefaultValue>] val mutable content : string
    [<DefaultValue>] val mutable contentType :  Headers.MediaTypeWithQualityHeaderValue
    [<DefaultValue>] val mutable httpMethod : HttpMethod
    
    let hooks = 
        { emptyHooks with onBeforeSend = 
                              (fun x -> 
                              this.httpMethod <- x.Method
                              if x.Content = null then ()
                              else 
                                  this.content <- x.Content.ReadAsStringAsync()
                                               |> Async.AwaitTask
                                               |> Async.RunSynchronously) }
    
    let dispose() = Async.CancelDefaultToken()
    
    do 
        try 
            let listening, server = web_server_async default_config webPart
            Async.Start server
            Async.RunSynchronously listening
        with _ -> dispose()
    
    member x.client = defaultClientWithHooks baseAddress hooks
    interface IDisposable with
        member x.Dispose() = dispose()

type request = 
    { Name : string }
