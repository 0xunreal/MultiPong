using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public static class MessageFactory
{
    private static readonly Dictionary<MessageType, Type> MessageTypeMap = new Dictionary<MessageType, Type>();

    static MessageFactory()
    {
        RegisterMessageType<PaddleInputMessage>(MessageType.Client_PaddleInput);
        RegisterMessageType<PlayerJoinMessage>(MessageType.Client_PlayerJoin);
        RegisterMessageType<PlayerLeaveMessage>(MessageType.Client_PlayerLeave);
        RegisterMessageType<GameStateMessage>(MessageType.Server_GameStateSync);
        RegisterMessageType<PlayerStatisticsMessage>(MessageType.Server_PlayerStatisticsSync);
    }

    private static void RegisterMessageType<T>(MessageType messageType) where T : BaseMessage
    {
        MessageTypeMap[messageType] = typeof(T);
    }

    // The main method to deserialize a message from JSON.
    // 1. Deserialize to BaseMessage to get the 'type'
    // 2. Look up correct subclass type and deserialize again
    public static BaseMessage DeserializeMessage(string json)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        BaseMessage baseMsg = JsonConvert.DeserializeObject<BaseMessage>(json, settings);
        if (baseMsg == null)
            return null;

        if (MessageTypeMap.TryGetValue(baseMsg.type, out var concreteType))
        {
            return JsonConvert.DeserializeObject(json, concreteType, settings) as BaseMessage;
        }

        // If no matching type found, return the base message as is, or null.
        return baseMsg;
    }
}