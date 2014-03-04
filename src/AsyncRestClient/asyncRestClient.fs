module AsyncRestClient

open System
open System.Text
open System.Net
open System.Net.Http

type SendAsync = HttpRequestMessage -> Async<HttpResponseMessage>

type Hooks = 
    { onBeforeSend : HttpRequestMessage -> unit
      onException : Exception -> unit
      onAfterSend : unit -> unit
      onUnsuccessful : HttpResponseMessage -> unit }

type Serializer = Serializer of contentType : string * serializer : (obj -> string)

type AsyncClient = AsyncClient of sendAsync : SendAsync * serializer : Serializer * hooks : Hooks

type Response = {statusCode: HttpStatusCode; text: string}

let httpRequestMessage (requestUri : string) httpMethod content = new HttpRequestMessage(httpMethod, requestUri, Content = content)
let createHttpRequestMessage serialize request requestUri httpMethod = 
    let content = serialize request
    httpRequestMessage requestUri httpMethod <| new StringContent(content)

let createHttpRequestMessageWithoutContent requestUri httpMethod = httpRequestMessage requestUri httpMethod null

let sendRequest sendAsync hooks httpRequestMessage = 
    hooks.onBeforeSend httpRequestMessage
    try 
        async { 
            try 
                use httpRequestMessage = httpRequestMessage
                use! httpResponseMessage = sendAsync httpRequestMessage
                use content = (httpResponseMessage :> HttpResponseMessage).Content
                let! response = content.ReadAsStringAsync() |> Async.AwaitTask
                if not httpResponseMessage.IsSuccessStatusCode then hooks.onUnsuccessful httpResponseMessage
                return Some {statusCode = httpResponseMessage.StatusCode; text = response}
            with ex -> 
                hooks.onException ex
                return None
            }
    finally
        hooks.onAfterSend()

let emptyRequest requestUri (AsyncClient(sendAsync, Serializer(contentType, serialize), hooks)) httpMethod = 
    createHttpRequestMessageWithoutContent requestUri httpMethod |> sendRequest sendAsync hooks
let contentRequest request requestUri (AsyncClient(sendAsync, Serializer(contentType, serialize), hooks)) httpMethod = 
    createHttpRequestMessage serialize request requestUri httpMethod |> sendRequest sendAsync hooks
let get requestUri asyncClient = emptyRequest requestUri asyncClient System.Net.Http.HttpMethod.Get
let delete requestUri asyncClient = emptyRequest requestUri asyncClient System.Net.Http.HttpMethod.Delete
let post request requestUri asyncClient = contentRequest request requestUri asyncClient System.Net.Http.HttpMethod.Post
let put request requestUri asyncClient = contentRequest request requestUri asyncClient System.Net.Http.HttpMethod.Put
let patch request requestUri asyncClient = contentRequest request requestUri asyncClient <| System.Net.Http.HttpMethod("PATCH")