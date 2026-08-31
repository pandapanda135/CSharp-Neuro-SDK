using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Xna.Framework;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Utilities;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Websocket;

public class RecievedMessageData(string data) : EventArgs
{
    public string Data = data;
}

public abstract class BaseWebsocket<T> : GameComponent where T : BaseWebsocket<T>
{
    public BaseWebsocket(Game game, string gameName, string? uriString, List<OutgoingMessageHandler> starterMessages) : base(game)
    {
        Game = game;
        GameName = gameName;
        UriString = uriString;
        
        MessageQueue = new MessageQueue(starterMessages);
        CommandHandler = new CommandHandler();
        
        OnConnect += OnConnection;
        OnDisconnect += OnDisconnected;
        OnMessageReceived += OnRecieveMessage;
        
        game.Components.Add(this);
    }
    
    protected bool TryingReconnect;
    protected const float ReconnectInterval = 5;
    protected ClientWebSocket? WebSocket = new();
    protected readonly MessageQueue MessageQueue;
    protected readonly CommandHandler CommandHandler;
    
    protected static T? _instance;
    public static T? Instance
    {
        get
        {
            if (_instance is null) LogHolder.Error($"Websocket was accessed without an instance being present: {new StackTrace()}");
            return _instance;
        }
        private set => _instance = value;
    }


    public Game Game;
    public readonly string GameName; // will be used for Messages
    public EventHandler OnConnect;
    public EventHandler OnDisconnect;
    public EventHandler<RecievedMessageData> OnMessageReceived;
    
    protected string? UriString; // this will be changed to be able to be changed through file in future
    
    public override async void Initialize()
    {
        try
        {
            await StartWs();
        }
        catch (Exception e)
        {
            LogHolder.Error($"issue in initialize: {e}");
        }
    }

    private async Task Reconnect(bool fromUpdate = false)
    {
        if (TryingReconnect && fromUpdate) return;
        TryingReconnect = true;
        await Task.Delay(TimeSpan.FromSeconds(ReconnectInterval));
        await StartWs();
    }

    protected Task? ConnectTask;
    private async Task StartWs()
    {
        try
        {
            if (WebSocket!.State is WebSocketState.Open or WebSocketState.Connecting)
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Websocket has closed, as one is already open.", CancellationToken.None);
        }
        catch (Exception e)
        {
            LogHolder.Error($"issue with closing websocket if already open: {e}");
            throw;
        }

        UriString = GetWebsocketUrl();
        
        if (UriString is null or "")
        {
            LogHolder.Error("Could not get websocket URL. You need to set the NEURO_SDK_WS_URL environment variable");
            return;
        }
        
        WebSocket = new ClientWebSocket();
        Uri websocketUri = new Uri(UriString);

        WebSocket.Options.KeepAliveInterval = TimeSpan.FromMinutes(10); // should substitute ping pong
        
        try
        {
            ConnectTask = WebSocket.ConnectAsync(websocketUri, CancellationToken.None);
            ConnectTask.Wait();

            LogHolder.Info($"Starting Task    Websocket state: {WebSocket.State}");
            OnConnect.Invoke(this, EventArgs.Empty);
            
            _ = ReceiveMessage();
            TryingReconnect = false;
        }
        catch (Exception e)
        {
            if (e is WebSocketException we && we.WebSocketErrorCode is WebSocketError.Faulted)
            {
                LogHolder.Error($"Error code is {we.WebSocketErrorCode}  message: {we.Message}  error code: {we.ErrorCode}");
                _ = Reconnect();
            }
        }
    }

    private async Task SendTask(OutgoingMessageHandler handler)
    {
        WsMessage wsMessage = handler.GetWsMessage(); 
        
        string message = JsonSerialize.Serialize(wsMessage);
        
        LogHolder.Info($"Sending the Ws Message {message}");

        var sendBytes = Encoding.UTF8.GetBytes(message);
        
        try
        {
            await WebSocket!.SendAsync(sendBytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            LogHolder.Error($"error when sending message: {e}");
            MessageQueue.Enqueue(handler);
        }
    }

    public void Send(OutgoingMessageHandler messageHandler) => MessageQueue.Enqueue(messageHandler);

    public async Task SendImmediate(OutgoingMessageHandler messageHandler)
    {
        string message = JsonSerialize.Serialize(messageHandler.GetWsMessage());

        if (WebSocket is null) return;
        
        if (WebSocket.State is not WebSocketState.Open)
        {
            LogHolder.Error($"Websocket is not open. Could not send message: {message}");
        }

        LogHolder.Info($"Sending Immediate message {message}");

        var sendBytes = Encoding.UTF8.GetBytes(message);
        await WebSocket!.SendAsync(sendBytes, WebSocketMessageType.Text, false, CancellationToken.None);
    }

    private async Task ReceiveMessage()
    {
        LogHolder.Info("Start of ReceiveMessage");

        if (WebSocket is null) return;
        
        var buffer = new byte[1024 * 4];
        
        while (WebSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            MemoryStream memoryStream = new MemoryStream();
            do
            {
                result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                memoryStream.Write(buffer,0,result.Count);
            } while (!result.EndOfMessage);
            
            LogHolder.Info($"Receive message result: {result} || {result.MessageType}  || {result.CloseStatus}");
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                LogHolder.Warning("Server closed connection.");
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                break;
            }
            
            await HandleReceivedMessage(memoryStream, result);
        }
    }

    protected virtual async Task HandleReceivedMessage(MemoryStream stream, WebSocketReceiveResult result)
    {
        var memoryStreamArray = stream.ToArray();
        var messageData = Encoding.UTF8.GetString(memoryStreamArray,0,memoryStreamArray.Length);

        await TaskDispatcher.SwitchToMainThread();
        OnMessageReceived.Invoke(this, new RecievedMessageData(messageData));
        GetMessage(messageData);
    }

    public override async void Update(GameTime gameTime)
    {
        TaskDispatcher.RunPending();
        if (ConnectTask is not null && ConnectTask.IsCompleted)
        {
            LogHolder.Info(ConnectTask.IsFaulted ? $"Issue with connecting. Exception was: {ConnectTask.Exception}.\nTrying again." : "Connected successfully!");

            TryingReconnect = false;
            ConnectTask = null;
        }
        
        if (WebSocket is { State: WebSocketState.Closed or WebSocketState.Aborted or WebSocketState.CloseReceived or WebSocketState.CloseSent} or null) // Best I can get to OnClose event with default websocket :(
        {
            OnDisconnect.Invoke(this, EventArgs.Empty);
            _ = Reconnect(true);
            return;
        }
        
        try
        {
            if (WebSocket is null) throw new NullReferenceException("Websocket was null.");
            
            if (WebSocket.State != WebSocketState.Open) return;

            while (MessageQueue.Count > 0)
            {
                OutgoingMessageHandler handler = MessageQueue.Dequeue()!;
                await SendTask(handler);
            }
        }
        catch (Exception e)
        {
            LogHolder.Error($"Issue in update of ws: {e}");
        }
    }
    
    private void GetMessage(string messageData)
    {
        try
        {
            Dictionary<string, object> dataArray = ProcessJsonMessage(messageData);
            CommandHandler.Handle((string)dataArray["command"], (IncomingData)dataArray["data"]);
        }
        catch (Exception e)
        {
            LogHolder.Error($"Error in GetMessage try   {e}");
        }
    }

    public Dictionary<string, object> ProcessJsonMessage(string messageData)
    {
        LogHolder.Info($"Processing JSON message: {messageData}");
        JObject message = JObject.Parse(messageData);
        
        string? command = message["command"]?.Value<string>();
        IncomingData data = new(message["data"]);

        if (command is null)
        {
            LogHolder.Warning("Received command that could not be deserialized.");
            return new();
        }
        
        Dictionary<string, object> dataDictionary = new Dictionary<string, object>{{"message", message},{"command",command},{"data",data}};
        return dataDictionary;
    }

    protected virtual string GetWebsocketUrl()
    {
        string? newUrl = "";
        if (UriString is not (null or "")) return UriString;
        
        newUrl = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
                 Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
                 Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
        LogHolder.Info($"Uri string found by environment variables: {newUrl}");

        return newUrl ?? "";
    }
    
    public abstract void OnConnection(object? sender, EventArgs? e);
    
    public abstract void OnDisconnected(object? sender, EventArgs? e);

    public abstract void OnRecieveMessage(object? sender, RecievedMessageData? e);
}