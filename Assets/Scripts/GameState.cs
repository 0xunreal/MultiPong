using System;
using UnityEngine;
using System.Collections.Generic;

public enum MatchState
{
    WaitingForPlayers,
    Pregame,
    Ingame,
    Endgame
}

public class GameState
{
    public MatchState CurrentMatchState { get; private set; }
    private Dictionary<string, PlayerState> playerStates = new Dictionary<string, PlayerState>();
    private List<string> playerOrder = new List<string>(); 
    private const int MaxPlayers = 2;
    private const int WinningScore = 10;
    private Action<GameStateMessage> _syncCallback;
    // public float pregameTimeRemaining;
    
    public int PlayerCount => playerStates.Count;
    
    // Ball state stored here and updated from BallController (server-side)
    public Vector2 CurrentBallPosition => BallController.Instance != null ? (Vector2)BallController.Instance.transform.position : Vector2.zero;
    public Vector2 CurrentBallVelocity => BallController.Instance != null ? BallController.Instance.GetVelocity() : Vector2.zero;
    
    // Paddle movement speed (server-side)
    private float paddleSpeed = 5f;
    
    private PlayerStatisticsService _statsService = new PlayerStatisticsService();
    
    public GameState(Action<GameStateMessage> syncCallback)
    {
        _syncCallback = syncCallback;
        InitializeGameState();
    }
    
    public void InitializeGameState()
    {
        SetMatchState(MatchState.WaitingForPlayers);
    }

    public void SetMatchState(MatchState newState)
    {
        CurrentMatchState = newState;
        OnEnterMatchState(newState);
    }

    private void OnEnterMatchState(MatchState state)
    {
        switch (state)
        {
            case MatchState.WaitingForPlayers:
                Debug.Log("Waiting for players to join...");
                //if (PlayerCount == MaxPlayers)
                if (PlayerCount > 0)
                {
                    SetMatchState(MatchState.Pregame);
                }
                break;
            case MatchState.Pregame:
                Debug.Log("Entering Pregame State");
                SetMatchState(MatchState.Ingame);
                break;
            case MatchState.Ingame:
                Debug.Log("Entering Ingame State");
                if (BallController.Instance != null)
                {
                    BallController.Instance.LaunchBall();
                }
                else
                {
                    Debug.LogError("BallController is null!");
                }
                break;
            case MatchState.Endgame:
                Debug.Log("Entering Endgame State");
                // Identify winner
                int maxScore = -1;
                string winnerId = null;
                foreach (var p in playerStates.Values)
                {
                    if (p.Score > maxScore)
                    {
                        maxScore = p.Score;
                        winnerId = p.PlayerId;
                    }
                }

                if (!string.IsNullOrEmpty(winnerId))
                {
                    // Update global stats for the winner
                    _statsService.AddStat(winnerId, "Wins", 1f);
                }
                
                // Update score stat for every player
                foreach (var p in playerStates.Values)
                {
                    _statsService.AddStat(p.PlayerId, "Score", p.Score);
                    _statsService.AddStat(p.PlayerId, "HighScore", p.Score);
                }
                
                // Display winner and reset scores
                foreach (var playerState in playerStates.Values)
                {
                    playerState.Score = 0;
                }
                SetMatchState(MatchState.WaitingForPlayers);
                break;
        }
    }

    public void PlayerJoined(string playerId)
    {
        if (!playerStates.ContainsKey(playerId))
        {
            Vector2 startPos = playerStates.Count == 0 ? new Vector2(-8f,0f) : new Vector2(8f,0f);
            playerStates.Add(playerId, new PlayerState(playerId) { PaddlePosition = startPos });
            playerOrder.Add(playerId);
            
            _statsService.AddStat(playerId, "Joins", 1f);
        }

        if (PlayerCount > 0)
        {
            SetMatchState(MatchState.Pregame);
        }

        SyncGameState();
    }

    public void PlayerLeft(string playerId)
    {
        if (playerStates.ContainsKey(playerId))
        {
            playerStates.Remove(playerId);
            playerOrder.Remove(playerId);
        }

        if (playerStates.Count < MaxPlayers && CurrentMatchState == MatchState.Ingame)
        {
            SetMatchState(MatchState.WaitingForPlayers);
        }

        SyncGameState();
    }

    public void UpdateGameState(float deltaTime)
    {
       // if (CurrentMatchState == MatchState.Ingame)
        {
            // Update paddles based on player input
            foreach (var pState in playerStates.Values)
            {
                float verticalInput = pState.VerticalInput;
                Vector2 newPos = pState.PaddlePosition;
                newPos.y += verticalInput * paddleSpeed * deltaTime;
                pState.PaddlePosition = newPos;
                pState.VerticalInput = pState.VerticalInput * 0.8f; // Dampen input
            }

            CheckForWinner();
        }
    }

    private void CheckForWinner()
    {
        foreach (var playerState in playerStates.Values)
        {
            if (playerState.Score >= WinningScore)
            {
                SetMatchState(MatchState.Endgame);
                SyncGameState();
                return;
            }
        }
    }

    public PlayerState GetPlayerState(string playerId)
    {
        return playerStates.TryGetValue(playerId, out var playerState) ? playerState : null;
    }

    public IEnumerable<PlayerState> GetAllPlayerStates()
    {
        return playerStates.Values;
    }

    public void SyncGameState()
    {
        var gameStateMessage = new GameStateMessage(this);
        _syncCallback?.Invoke(gameStateMessage);
    }
}

