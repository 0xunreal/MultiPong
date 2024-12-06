using System;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    Client_PaddleInput,
    Client_PlayerJoin,
    Client_PlayerLeave,
    Server_GameStateSync,
    Server_PlayerStatisticsSync
}

// Not abstract because we need to be able to deserialize the base class
[Serializable]
public class BaseMessage
{
    public MessageType type;
}

[Serializable]
public class PaddleInputMessage : BaseMessage
{
    public string playerId;
    public float verticalInput;

    public PaddleInputMessage(string playerId, float verticalInput)
    {
        this.type = MessageType.Client_PaddleInput;
        this.playerId = playerId;
        this.verticalInput = verticalInput;
    }
}

[Serializable]
public class GameStateMessage : BaseMessage
{
    public MatchState matchState;
    public Dictionary<string, PlayerStateData> playerStates;
    public Vector2 ballPosition;
    public Vector2 ballVelocity;
    // public float pregameTimeRemaining;
    
    public GameStateMessage()
    {
        this.type = MessageType.Server_GameStateSync;
    }
    
    public GameStateMessage(GameState gameState)
    {
        this.type = MessageType.Server_GameStateSync;
        this.matchState = gameState.CurrentMatchState;
        this.playerStates = new Dictionary<string, PlayerStateData>();
        foreach (var playerState in gameState.GetAllPlayerStates())
        {
            if (playerState == null)
            {
                Debug.LogError("Found a null PlayerState in GameState.");
                continue;
            }
            playerStates[playerState.PlayerId] = new PlayerStateData(playerState);
        }
        
        this.ballPosition = gameState.CurrentBallPosition;
        this.ballVelocity = gameState.CurrentBallVelocity;
        
        // Debug.Log("Ball velocity: " + this.ballVelocity + " Ball position: " + this.ballPosition);
        // this.pregameTimeRemaining = gameState.IsPregame ? gameState.PregameTimeRemaining : 0f;
    }
}

[Serializable]
public class PlayerStateData
{
    public string playerId;
    public int score;
    public Vector2 paddlePosition;

    // Parameterless constructor for JSON deserialization
    public PlayerStateData() { }
    
    public PlayerStateData(PlayerState playerState)
    {
        playerId = playerState.PlayerId;
        score = playerState.Score;
        paddlePosition = playerState.PaddlePosition;
    }
}

[Serializable]
public class PlayerJoinMessage : BaseMessage
{
    public string playerId;

    public PlayerJoinMessage(string playerId)
    {
        this.type = MessageType.Client_PlayerJoin;
        this.playerId = playerId;
    }
}

[Serializable]
public class PlayerLeaveMessage : BaseMessage
{
    public string playerId;

    public PlayerLeaveMessage(string playerId)
    {
        this.type = MessageType.Client_PlayerLeave;
        this.playerId = playerId;
    }
}

[Serializable]
public class PlayerStatisticsMessage : BaseMessage
{
    public Dictionary<string, PlayerStatisticsData> playersStatsData;

    public PlayerStatisticsMessage()
    {
        this.type = MessageType.Server_PlayerStatisticsSync;
    }
    
    public PlayerStatisticsMessage(Dictionary<string, PlayerStatisticsData> stats)
    {
        this.type = MessageType.Server_PlayerStatisticsSync;
        this.playersStatsData = stats;
    }
}

[Serializable]
public class PlayerStatisticsData
{
    public string playerId;
    public Dictionary<string, float> stats;
    public List<string> awardedBadges;

    public PlayerStatisticsData() { }

    public PlayerStatisticsData(string playerId, PlayerStatistics playerStats)
    {
        this.playerId = playerId;
        this.stats = new Dictionary<string, float>(playerStats.stats);
        this.awardedBadges = new List<string>(playerStats.badges);
    }
}