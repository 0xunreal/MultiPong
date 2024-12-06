using UnityEngine;

public class NetworkMode : MonoBehaviour
{
    public GameServer gameServerPrefab;
    public GameClient gameClientPrefab;

    public bool IsServer { get; private set; }
    public bool IsClient => !IsServer;

    private GameServer _serverInstance;
    private GameClient _clientInstance;
    
    private void Awake()
    {
        // Check command line arguments
        string[] args = System.Environment.GetCommandLineArgs();
        IsServer = System.Array.Exists(args, arg => arg == "-batchmode"); // bit implicit, but we're assuming that if we're in batchmode, we're a server

        Debug.Log(IsServer ? "Running as Server" : "Running as Client");
        
        if (IsServer)
        {
            // Instantiate the GameServer prefab
            if (gameServerPrefab != null)
            {
                if (_serverInstance == null)
                {
                    _serverInstance = Instantiate(gameServerPrefab);
                    _serverInstance.gameObject.name = "GameServer";
                }
                else
                {
                    Debug.LogWarning("GameServer already exists in the scene!");
                }
            }
            else
            {
                Debug.LogError("GameServer prefab is not assigned!");
            }
        }
        else
        {
            // Instantiate the GameClient prefab
            if (gameClientPrefab != null)
            {
                if (_clientInstance == null)
                {
                    _clientInstance = Instantiate(gameClientPrefab);
                    _clientInstance.gameObject.name = "GameClient";
                }
                else
                {
                    Debug.LogWarning("GameClient already exists in the scene!");
                }
            }
            else
            {
                Debug.LogError("GameClient prefab is not assigned!");
            }
        }
    }
}