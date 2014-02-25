module AsyncRestClient

open System
open System.Text
open System.Net.Http

type HttpMethod = 
    | Get
    | Delete
    | Post of request : Object
    | Put of request : Object

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

let create asyncClient = 
    let asyncRestClient (AsyncClient(SendAsync sendAsync, Serializer(contentType, serialize), hooks)) requestUri httpMethod = 
        let httpRequestMessage (requestUri : string) httpMethod content = new HttpRequestMessage(httpMethod, requestUri, Content = content)
        
        let createHttpRequestMessage request requestUri httpMethod = 
            let content = serialize request
            httpRequestMessage requestUri httpMethod <| new StringContent(content)
        
        let createHttpRequestMessageWithoutContent requestUri httpMethod = httpRequestMessage requestUri httpMethod null
        
        let sendRequestResponse httpRequestMessage = 
            hooks.onBeforeSend httpRequestMessage
            try 
                try 
                    async { 
                        use httpRequestMessage = httpRequestMessage
                        use! httpResponseMessage = sendAsync httpRequestMessage
                        match httpResponseMessage with
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
        
        let sendRequest httpRequestMessage = 
            hooks.onBeforeSend httpRequestMessage
            try 
                try 
                    async { 
                        use httpRequestMessage = httpRequestMessage
                        use! httpResponseMessage = sendAsync httpRequestMessage
                        match httpResponseMessage with
                        | hrm when hrm.IsSuccessStatusCode -> return None
                        | hrm -> 
                            hooks.onUnsuccessful (hrm)
                            return None
                    }
                with ex -> 
                    hooks.onException ex
                    async { return None }
            finally
                hooks.onAfterSend()
        
        match httpMethod with
        | Get -> createHttpRequestMessageWithoutContent requestUri System.Net.Http.HttpMethod.Get |> sendRequestResponse
        | Post request -> createHttpRequestMessage request requestUri System.Net.Http.HttpMethod.Post |> sendRequestResponse
        | Put request -> createHttpRequestMessage request requestUri System.Net.Http.HttpMethod.Put |> sendRequestResponse
        | Delete -> createHttpRequestMessageWithoutContent requestUri System.Net.Http.HttpMethod.Delete |> sendRequest
    asyncRestClient asyncClient