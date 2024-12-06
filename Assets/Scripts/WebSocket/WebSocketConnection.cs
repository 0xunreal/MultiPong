using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

public class WebSocketConnection : MonoBehaviour
{
    private WebSocket _websocket;
    private readonly Dictionary<MessageType, Action<BaseMessage>> _messageHandlers = new();
    private readonly Dictionary<MessageType, Type> _messageTypes = new(); // Maps message type to concrete class type
    private ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
    private const int MaxMessagesPerFrame = 100;

    private void Awake()
    {
        Debug.Log("WebSocketConnection Start");
        _websocket = new WebSocket("ws://localhost:2567");

        _websocket.OnOpen += (sender, e) => Debug.Log("Connection open!");
        _websocket.OnError += (sender, e) => Debug.LogError("WebSocket error: " + e);
        _websocket.OnClose += (sender, e) => Debug.Log("Connection closed!");
        _websocket.OnMessage += (sender, e) => _messageQueue.Enqueue(e.Data);

        string playerId = LocalPlayerConfig.GetPlayerName(); // playerid == playername for now
        Debug.Log("WebSocketConnection playerId: " + playerId);
        if (!string.IsNullOrWhiteSpace(playerId))
        {
            Connect(playerId);
        }
    }

    private void Connect(string inPlayerId)
    {
        _websocket.Connect();

        Debug.Log("Connected to server");
        if (!string.IsNullOrEmpty(inPlayerId))
        {
            var joinMessage = new PlayerJoinMessage(inPlayerId);
            SendMessageToServer(joinMessage);
            Debug.Log("Sent join message to server");
        }
        else
        {
            Debug.LogWarning("No playerId provided to connect to server");
        }
    }

    private void Update()
    {
        for (int i = 0; i < MaxMessagesPerFrame; i++)
        {
            if (!_messageQueue.TryDequeue(out string json))
            {
                // No more messages in the queue
                break;
            }
            
            HandleIncomingMessage(json);
        }
    }
    
    private void HandleIncomingMessage(string json)
    {
        try
        {
           // Debug.Log("HandleIncomingMessage: " + json);
            BaseMessage message = MessageFactory.DeserializeMessage(json);
            if (message != null && _messageHandlers.TryGetValue(message.type, out var handler))
            {
                handler(message);
            }
            else
            {
                Debug.LogWarning("Unhandled message type: " + (message?.type.ToString() ?? "null"));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deserializing message: {e.Message}. Message: {json}. Stack trace: {e.StackTrace}");
        }
    }

    public void RegisterMessageHandler<T>(Action<T> handler) where T : BaseMessage, new()
    {
        var temp = new T();
        if (!_messageHandlers.ContainsKey(temp.type))
        {
            // Wrap Action<T> in Action<BaseMessage>
            Action<BaseMessage> baseHandler = (baseMsg) => handler((T)baseMsg);
            _messageHandlers[temp.type] = baseHandler;
            _messageTypes[temp.type] = typeof(T);
        }
    }

    public void SendMessageToServer(BaseMessage message)
    {
        if (_websocket.ReadyState == WebSocketState.Open)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            string json = JsonConvert.SerializeObject(message, settings);
//            Debug.Log($"SendMessageToServer: {json}");

            _websocket.SendAsync(json, b => { });
        }
    }

    private void OnApplicationQuit()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (_websocket != null && _websocket.ReadyState == WebSocketState.Open)
        {
            _websocket.Close();
        }
    }
}
