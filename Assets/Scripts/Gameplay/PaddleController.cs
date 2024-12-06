using UnityEngine;

// Input handling
public class PaddleController : MonoBehaviour
{
    // private string _playerId;
    private GameClient _gameClient;

    public void Initialize(string playerId, GameClient gameClient)
    {
        // _playerId = playerId;
        _gameClient = gameClient;
    }

    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");

        // Only send if there's meaningful input
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            var msg = new PaddleInputMessage(LocalPlayerConfig.GetPlayerName(), verticalInput);
            
            if (_gameClient == null) 
            {
//                Debug.LogError("GameClient is null in PaddleController!");
                _gameClient = FindFirstObjectByType<GameClient>();
            }
            
            _gameClient.SendMessageToServer(msg);
        }
    }
}