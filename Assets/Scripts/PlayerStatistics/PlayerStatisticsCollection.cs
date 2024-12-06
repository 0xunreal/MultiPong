using System.Collections.Generic;

[System.Serializable]
public class PlayerStatisticsCollection
{
    public Dictionary<string, PlayerStatistics> playerStatsMap = new Dictionary<string, PlayerStatistics>();
    
    public static PlayerStatisticsMessage ConvertToMessage(PlayerStatisticsCollection collection)
    {
        var dataDict = new Dictionary<string, PlayerStatisticsData>();
    
        foreach (var kvp in collection.playerStatsMap)
        {
            string playerId = kvp.Key;
            PlayerStatistics playerStats = kvp.Value;
            var statsData = new PlayerStatisticsData(playerId, playerStats);
            dataDict[playerId] = statsData;
        }

        return new PlayerStatisticsMessage(dataDict);
    }
}