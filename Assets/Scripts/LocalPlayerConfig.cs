using UnityEngine;

public static class LocalPlayerConfig
{
    private const string PlayerNameKey = "PlayerName";
    private const string ServerUrlKey = "ServerUrl";

    public static void SetPlayerName(string playerName)
    {
        PlayerPrefs.SetString(PlayerNameKey, playerName);
        PlayerPrefs.Save();
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, "DefaultPlayer");
    }

    public static void SetServerUrl(string serverUrl)
    {
        PlayerPrefs.SetString(ServerUrlKey, serverUrl);
        PlayerPrefs.Save();
    }

    public static string GetServerUrl()
    {
        return PlayerPrefs.GetString(ServerUrlKey, "ws://localhost:2567");
    }
}
