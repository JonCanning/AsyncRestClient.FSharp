namespace AsyncRestClient.Tests

open System
open AsyncRestClient
open Suave.Http
open Suave.Web
open System.Net.Http

type BaseTest(webPart) as this = 
    [<DefaultValue>] val mutable baseAddress : string
    [<DefaultValue>] val mutable content : string
    [<DefaultValue>] val mutable contentType :  Headers.MediaTypeWithQualityHeaderValue
    [<DefaultValue>] val mutable httpMethod : HttpMethod
    [<DefaultValue>] val mutable client : AsyncClient
    [<DefaultValue>] val mutable onException : bool
    [<DefaultValue>] val mutable onUnsuccessful : bool
    [<DefaultValue>] val mutable onAfterSend : bool
    
    let hooks = 
        { 
            onBeforeSend = 
                              (fun x -> 
                              this.httpMethod <- x.Method
                              if x.Content = null then ()
                              else 
                                  this.content <- x.Content.ReadAsStringAsync()
                                               |> Async.AwaitTask
                                               |> Async.RunSynchronously);
            onException = (fun x -> this.onException <- true);
            onUnsuccessful = (fun x -> this.onUnsuccessful <- true);
            onAfterSend = (fun () -> this.onAfterSend <- true) 
         }
    
    let dispose() = Async.CancelDefaultToken()
    
    do
        this.baseAddress <- "http://localhost:8083"
        try 
            let listening, server = web_server_async default_config webPart
            Async.Start server
            Async.RunSynchronously listening
        with _ -> dispose()
        this.client <- defaultClientWithHooks this.baseAddress hooks

    interface IDisposable with
        member x.Dispose() = dispose()

type request = 
    { Name : string }
