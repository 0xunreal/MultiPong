using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class GameClient : MonoBehaviour
{
    public string PlayerId => LocalPlayerConfig.GetPlayerName();
    private WebSocketConnection _webSocketConnection;
    public GameObject PaddlePrefab;
    public GameObject BallPrefab;
    private Dictionary<string, GameObject> _playerPaddles = new Dictionary<string, GameObject>();
    
    public GameState CurrentGameState { get; private set; }

    private GameObject _localBall;
    
    private void Start()
    {
        InitializeClient();
    }

    private void InitializeClient()
    {
        // Connect to server as a client
        _webSocketConnection = gameObject.AddComponent<WebSocketConnection>();
        _webSocketConnection.RegisterMessageHandler<GameStateMessage>(UpdateGameStateFromServer);
        _webSocketConnection.RegisterMessageHandler<PlayerStatisticsMessage>(UpdatePlayerStatsFromServer);
        
        // Spawn local ball
        _localBall = Instantiate(BallPrefab, Vector3.zero, Quaternion.identity);
        
        if (PlayerId == "DefaultPlayer")
        {
            LocalPlayerConfig.SetPlayerName("Player" + Random.Range(1, 10000));
        }
    }

    private void UpdatePlayerStatsFromServer(PlayerStatisticsMessage message)
    {
        // logging these for now
        if (message.playersStatsData.TryGetValue(PlayerId, out PlayerStatisticsData localStatsData))
        {
            // Print player ID
            Debug.Log("Player ID: " + localStatsData.playerId);

            // Print all stats
            Debug.Log("Stats:");
            foreach (var statPair in localStatsData.stats)
            {
                Debug.Log(statPair.Key + ": " + statPair.Value);
            }

            // Print awarded badges
            Debug.Log("Badges:");
            foreach (var badge in localStatsData.awardedBadges)
            {
                Debug.Log("- " + badge);
            }
        }
        else
        {
            // Debug.Log("No stats found for local player ID: " + PlayerId);
        }
    }

    private void UpdateGameStateFromServer(GameStateMessage message)
    {
        // Update or create paddles for all players
        foreach (var kvp in message.playerStates)
        {
            string playerId = kvp.Key;
            var playerData = kvp.Value;

            if (!_playerPaddles.ContainsKey(playerId))
            {
                // Instantiate a new paddle for this player
                var paddle = Instantiate(PaddlePrefab, 
                    new Vector3(playerData.paddlePosition.x, playerData.paddlePosition.y, 0), 
                    Quaternion.identity);

                _playerPaddles[playerId] = paddle;

                // If this is the local player's paddle, add input handler
                if (playerId == PlayerId)
                {
                    var controller = paddle.AddComponent<PaddleController>();
                    controller.Initialize(PlayerId, this);
                }
            }

//            Debug.Log("Player " + playerId + "paddlePosition: " + playerData.paddlePosition.x + ", " + playerData.paddlePosition.y);
            // Update paddle position
            _playerPaddles[playerId].transform.position = new Vector3(
                playerData.paddlePosition.x,
                playerData.paddlePosition.y,
                0);
        }

        var localBallPosition = new Vector3(
            message.ballPosition.x,
            message.ballPosition.y,
            0
        );
        
        // Update Ball position
        if (_localBall == null)
        {
            _localBall = Instantiate(BallPrefab, localBallPosition, Quaternion.identity);
        }
        if (_localBall != null)
        {
            _localBall.transform.position = localBallPosition;
            
            // Update Ball velocity
            //  Debug.Log("Ball velocity: " + message.ballVelocity.x + ", " + message.ballVelocity.y);
        }
        else
        {
            Debug.Log("Local ball is null");
        }

        if (message.matchState == MatchState.Pregame)
        {
           // Debug.Log("Pregame countdown: " + message.pregameTimeRemaining.ToString("F1"));
        }
    }
    
    public void SendMessageToServer(BaseMessage message)
    {
        _webSocketConnection.SendMessageToServer(message);
    }
}
