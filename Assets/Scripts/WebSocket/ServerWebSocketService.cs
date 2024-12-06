using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class ServerWebSocketService : WebSocketBehavior
{
    private System.Action<BaseMessage> _messageHandler;
    public void SetMessageHandler(System.Action<BaseMessage> messageHandler)
    {
        _messageHandler = messageHandler;
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            BaseMessage message = MessageFactory.DeserializeMessage(e.Data);
            if (message != null)
            {
                _messageHandler(message);
            }
        }
        catch (JsonException ex)
        {
            // TODO - do not debug log on background thread
            Debug.LogError($"Invalid message format: {ex.Message}");
        }
    }
    
    protected override void OnOpen()
    {
        Debug.Log("Client connected");
    }
    
    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log("Client disconnected");
    }
    
    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError($"WebSocket error: {e.Message}");
    }
}