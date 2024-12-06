using UnityEngine;

public class PlayerStatisticsService
{
    private BadgeService _badgeService = new BadgeService();
    
    public void AddStat(string playerId, string statName, float value)
    {
        var stats = PlayerStatisticsRepository.LoadPlayerStats(playerId);
        float oldVal = 0f;
        stats.stats.TryGetValue(statName, out oldVal);

        AggregationType aggType = GlobalPlayerStatistics.GetAggregationForStat(statName);
        float newVal = AggregateValues(oldVal, value, aggType);
        stats.stats[statName] = newVal;

        PlayerStatisticsRepository.SavePlayerStats(playerId, stats);
        
        // After updating stats, check for badges
        _badgeService.CheckAndAwardBadges(playerId);
    }

    public float GetStat(string playerId, string statName)
    {
        var stats = PlayerStatisticsRepository.LoadPlayerStats(playerId);
        if (stats.stats.TryGetValue(statName, out float val))
        {
            return val;
        }
        return 0f;
    }
    
    public void TriggerBadgeEvent(string playerId, string eventKey)
    {
        _badgeService.TriggerOneOffBadgeEvent(playerId, eventKey);
    }

    public PlayerStatisticsCollection GetAllStats()
    {
        return PlayerStatisticsRepository.LoadAll();
    }
    
    private float AggregateValues(float oldVal, float newVal, AggregationType type)
    {
        switch (type)
        {
            case AggregationType.MIN:
                return Mathf.Min(oldVal, newVal);
            case AggregationType.MAX:
                return Mathf.Max(oldVal, newVal);
            case AggregationType.SUM:
                return oldVal + newVal;
            case AggregationType.LAST:
            default:
                return newVal;
        }
    }
}
