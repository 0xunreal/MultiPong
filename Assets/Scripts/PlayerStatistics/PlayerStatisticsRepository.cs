using System;
using UnityEngine;
using System.Collections.Generic;

public static class PlayerStatisticsRepository
{
    private const string AllStatsKey = "AllPlayerStatsJson";

    public static void SaveAll(PlayerStatisticsCollection collection)
    {
        string json = JsonUtility.ToJson(new PlayerStatisticsCollectionWrapper(collection));
        PlayerPrefs.SetString(AllStatsKey, json);
        PlayerPrefs.Save();
    }

    public static PlayerStatisticsCollection LoadAll()
    {
        if (!PlayerPrefs.HasKey(AllStatsKey))
        {
            return new PlayerStatisticsCollection();
        }

        string json = PlayerPrefs.GetString(AllStatsKey);
        var wrapper = JsonUtility.FromJson<PlayerStatisticsCollectionWrapper>(json);
        return wrapper.ToPlayerStatisticsCollection();
    }

    // Convenience methods for individual players
    public static void SavePlayerStats(string playerId, PlayerStatistics stats)
    {
        var all = LoadAll();
        all.playerStatsMap[playerId] = stats;
        SaveAll(all);
    }

    public static PlayerStatistics LoadPlayerStats(string playerId)
    {
        var all = LoadAll();
        if (all.playerStatsMap.TryGetValue(playerId, out var pStats))
        {
            return pStats;
        }
        return new PlayerStatistics(); // return empty if not found
    }

    [Serializable]
    private class PlayerStatisticsCollectionWrapper
    {
        public List<string> playerIds = new List<string>();
        public List<PlayerStatisticsWrapper> playerStatsList = new List<PlayerStatisticsWrapper>();

        public PlayerStatisticsCollectionWrapper(PlayerStatisticsCollection collection)
        {
            foreach (var kvp in collection.playerStatsMap)
            {
                playerIds.Add(kvp.Key);
                playerStatsList.Add(new PlayerStatisticsWrapper(kvp.Value));
            }
        }

        public PlayerStatisticsCollection ToPlayerStatisticsCollection()
        {
            PlayerStatisticsCollection coll = new PlayerStatisticsCollection();
            for (int i = 0; i < playerIds.Count; i++)
            {
                coll.playerStatsMap[playerIds[i]] = playerStatsList[i].ToPlayerStatistics();
            }
            return coll;
        }
    }

    [Serializable]
    private class PlayerStatisticsWrapper
    {
        public List<string> keys = new List<string>();
        public List<float> values = new List<float>();

        public PlayerStatisticsWrapper(PlayerStatistics stats)
        {
            foreach (var kvp in stats.stats)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public PlayerStatistics ToPlayerStatistics()
        {
            PlayerStatistics ps = new PlayerStatistics();
            for (int i = 0; i < keys.Count; i++)
            {
                ps.stats[keys[i]] = values[i];
            }
            return ps;
        }
    }
}
