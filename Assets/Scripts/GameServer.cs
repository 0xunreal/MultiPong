using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using WebSocketSharp.Server;

public class GameServer : MonoBehaviour
{
    public GameState CurrentGameState { get; private set; }
    private WebSocketServer _webSocketServer;
    private ConcurrentQueue<BaseMessage> _incomingMessages = new();
    public static ConcurrentDictionary<string, string> PlayerSessions = new(); // playerId -> sessionId

    private const int MaxMessagesPerFrame = 100;
    
    // Time tracking for server updates
    private float _gameStateSyncTimer = 0f;
    private float _playerStatsSyncTimer = 0f;
    [SerializeField] private float playerStatsSyncInterval = 10f; 
    [SerializeField] private float gameStateSyncInterval = 0.05f; // 20 updates per second

    private PlayerStatisticsService _playerStatisticsService = new();
    
    private void Start()
    {
        if (_webSocketServer == null)
        {
            InitializeServer();
        }
    }

    private void InitializeServer()
    {
        Debug.Log("Initializing server...");
        _webSocketServer = new WebSocketServer(2567);
        _webSocketServer.AddWebSocketService<ServerWebSocketService>("/", service => service.SetMessageHandler(message => _incomingMessages.Enqueue(message)));
        _webSocketServer.Start();
        
        // Initialize the GameState
        CurrentGameState = new GameState(SyncGameStateToClients);
        CurrentGameState.InitializeGameState();
        
        Debug.Log("Server initialized");
    }

    private void SyncGameStateToClients(GameStateMessage gameStateMessage)
    {
        BroadcastMessageToAllClients(gameStateMessage);
    }
    
    private void SyncPlayerStatsToClients(PlayerStatisticsMessage message)
    {
        BroadcastMessageToAllClients(message);
    }
    
    private void BroadcastMessageToAllClients(BaseMessage message)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        var json = JsonConvert.SerializeObject(message, settings);
        _webSocketServer.WebSocketServices.Hosts.First().Sessions.BroadcastAsync(json, () => { });
    }
    
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (CurrentGameState.CurrentMatchState == MatchState.Ingame)
        {
            // Update the state of the game
            CurrentGameState.UpdateGameState(deltaTime);
        }

        _gameStateSyncTimer += deltaTime;
        if (_gameStateSyncTimer >= gameStateSyncInterval)
        {
            _gameStateSyncTimer = 0f;
            CurrentGameState.SyncGameState();
        }
        
        _playerStatsSyncTimer += deltaTime;
        if (_playerStatsSyncTimer >= playerStatsSyncInterval)
        {
            _playerStatsSyncTimer = 0f;
            var allStats = _playerStatisticsService.GetAllStats();
            var message = PlayerStatisticsCollection.ConvertToMessage(allStats);
            SyncPlayerStatsToClients(message);
        }
        {
            _gameStateSyncTimer = 0f;
            CurrentGameState.SyncGameState();
        }

        for (int i = 0; i < MaxMessagesPerFrame; i++)
        {
            if (!_incomingMessages.TryDequeue(out BaseMessage json))
            {
                // No more messages in the queue
                break;
            }
            
            HandleMessageFromClient(json);
        }
    }
    
    // Handles messages received from the client via WebSocket
    private void HandleMessageFromClient(BaseMessage message)
    {
        // Debug.Log("message received from client: " + JsonConvert.SerializeObject(message));
        switch (message.type)
        {
            case MessageType.Client_PaddleInput:
                var paddleInput = (PaddleInputMessage)message;
                HandlePaddleInput(paddleInput);
                break;

            case MessageType.Client_PlayerJoin:
                var playerJoin = (PlayerJoinMessage)message;
                HandlePlayerJoin(playerJoin);
                break;

            case MessageType.Client_PlayerLeave:
                var playerLeave = (PlayerLeaveMessage)message;
                HandlePlayerLeave(playerLeave);
                break;

            default:
                Debug.LogWarning("Unhandled message type: " + message.type);
                break;
        }
    }

    // Handles paddle input updates from clients
    private void HandlePaddleInput(PaddleInputMessage message)
    {
        var playerState = CurrentGameState.GetPlayerState(message.playerId);
        if (playerState != null)
        {
            // Store last known vertical input
            playerState.VerticalInput = message.verticalInput;
        }
    }

    // Handles new player joining the game
    private void HandlePlayerJoin(PlayerJoinMessage message)
    {
        Debug.Log($"Player joined: {message.playerId}");
        CurrentGameState.PlayerJoined(message.playerId);
    }

    // Handles player leaving the game
    private void HandlePlayerLeave(PlayerLeaveMessage message)
    {
        Debug.Log($"Player left: {message.playerId}");
        CurrentGameState.PlayerLeft(message.playerId);
    }
}