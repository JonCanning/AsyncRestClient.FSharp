module AsyncRestClient

open System
open System.Text
open System.Net.Http

type SendAsync = 
    | SendAsync of (HttpRequestMessage -> Async<HttpResponseMessage>)

type Hooks = 
    { onBeforeSend : HttpRequestMessage -> unit
      onException : Exception -> unit
      onAfterSend : unit -> unit
      onUnsuccessful : HttpResponseMessage -> unit }

type Serializer = 
    | Serializer of contentType : string * serializer : (obj -> string)

type AsyncClient = 
    | AsyncClient of sendAsync : SendAsync * serializer : Serializer * hooks : Hooks

let httpRequestMessage (requestUri : string) httpMethod content = new HttpRequestMessage(httpMethod, requestUri, Content = content)

let createHttpRequestMessage serialize request requestUri httpMethod = 
    let content = serialize request
    httpRequestMessage requestUri httpMethod <| new StringContent(content)

let createHttpRequestMessageWithoutContent requestUri httpMethod = httpRequestMessage requestUri httpMethod null

let sendRequestResponse sendAsync hooks httpRequestMessage = 
    hooks.onBeforeSend httpRequestMessage
    try 
        try 
            async { 
                use httpRequestMessage = httpRequestMessage
                use! httpResponseMessage = sendAsync httpRequestMessage
                match (httpResponseMessage : HttpResponseMessage) with
                | hrm when hrm.IsSuccessStatusCode -> 
                    use content = httpResponseMessage.Content
                    let! s = content.ReadAsStringAsync() |> Async.AwaitTask
                    return Some s
                | hrm -> 
                    hooks.onUnsuccessful (hrm)
                    return None
            }
        with ex -> 
            hooks.onException ex
            async { return None }
    finally
        hooks.onAfterSend()

let sendRequest sendAsync hooks httpRequestMessage = 
    hooks.onBeforeSend httpRequestMessage
    try 
        try 
            async { 
                use httpRequestMessage = httpRequestMessage
                use! httpResponseMessage = sendAsync httpRequestMessage
                match (httpResponseMessage : HttpResponseMessage) with
                | hrm when hrm.IsSuccessStatusCode -> ()
                | hrm -> hooks.onUnsuccessful (hrm)
            }
        with ex -> 
            hooks.onException ex
            async { return () }
    finally
        hooks.onAfterSend()

let get requestUri (AsyncClient(SendAsync sendAsync, Serializer(contentType, serialize), hooks)) = 
    createHttpRequestMessageWithoutContent requestUri System.Net.Http.HttpMethod.Get |> sendRequestResponse sendAsync hooks
let delete requestUri (AsyncClient(SendAsync sendAsync, Serializer(contentType, serialize), hooks)) = 
    createHttpRequestMessageWithoutContent requestUri System.Net.Http.HttpMethod.Delete |> sendRequest sendAsync hooks
let post request requestUri (AsyncClient(SendAsync sendAsync, Serializer(contentType, serialize), hooks)) = 
    createHttpRequestMessage serialize request requestUri System.Net.Http.HttpMethod.Post |> sendRequestResponse sendAsync hooks
let put request requestUri (AsyncClient(SendAsync sendAsync, Serializer(contentType, serialize), hooks)) = 
    createHttpRequestMessage serialize request requestUri System.Net.Http.HttpMethod.Put |> sendRequestResponse sendAsync hooks
