using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public TMP_InputField serverUrlInput;
    public Button joinGameButton;
    public Button quitGameButton;

    private void Start()
    {
        playerNameInput.text = LocalPlayerConfig.GetPlayerName();
        serverUrlInput.text = LocalPlayerConfig.GetServerUrl();
        playerNameInput.onSubmit.AddListener(UpdatePlayerName);
        serverUrlInput.onSubmit.AddListener(UpdateServerUrl);
        joinGameButton.onClick.AddListener(JoinGame);
        quitGameButton.onClick.AddListener(QuitGame);
    }

    private void JoinGame()
    {
        SceneManager.LoadScene("Scenes/Game");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
    
    private void UpdatePlayerName(string playerName)
    {
        LocalPlayerConfig.SetPlayerName(playerName);
    }
    
    private void UpdateServerUrl(string serverUrl)
    {
        LocalPlayerConfig.SetServerUrl(serverUrl);
    }
}