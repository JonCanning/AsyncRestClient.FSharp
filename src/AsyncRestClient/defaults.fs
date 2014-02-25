[<AutoOpen>]
module Defaults

open AsyncRestClient
open Newtonsoft.Json
open System.Net.Http
open System

let jsonNetSerializer = Serializer ("application/json", fun x -> JsonConvert.SerializeObject(x))

let httpClient baseAddress = SendAsync(fun x -> 
    let httpClient = new HttpClient(BaseAddress = new Uri(baseAddress))
    httpClient.SendAsync x |> Async.AwaitTask)

let emptyHooks = {onBeforeSend = (fun (x:HttpRequestMessage) -> ()); onException = (fun (x:Exception) -> ()); onAfterSend = (fun () -> ()); onUnsuccessful = (fun (x:HttpResponseMessage) -> ())}
let defaultClientWithHooks baseAddress hooks = AsyncClient(httpClient baseAddress, jsonNetSerializer, hooks)
let defaultClient baseAddress = defaultClientWithHooks baseAddress emptyHooks