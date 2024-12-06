using System.Linq;
using UnityEngine;

public class BadgeService
{
    public void CheckAndAwardBadges(string playerId)
    {
        var stats = PlayerStatisticsRepository.LoadPlayerStats(playerId);

        foreach (var badge in GlobalBadgeDefinitions.GetAllBadges())
        {
            // Skip if already awarded
            if (stats.badges.Contains(badge.BadgeId))
                continue;

            bool award = false;
            switch (badge.Type)
            {
                case BadgeType.StatThreshold:
                    if (stats.stats.TryGetValue(badge.StatName, out float val))
                    {
                        if (val >= badge.Threshold)
                        {
                            award = true;
                        }
                    }
                    break;

                case BadgeType.OneOff:
                    // One-off badges must be triggered by a specific event method
                    // We'll handle one-off in a separate method
                    break;
            }

            if (award)
            {
                AwardBadge(playerId, badge.BadgeId);
            }
        }
    }

    public void TriggerOneOffBadgeEvent(string playerId, string eventKey)
    {
        var stats = PlayerStatisticsRepository.LoadPlayerStats(playerId);
        foreach (var badge in GlobalBadgeDefinitions.GetAllBadges().Where(b => b.Type == BadgeType.OneOff && b.EventKey == eventKey))
        {
            if (!stats.badges.Contains(badge.BadgeId))
            {
                AwardBadge(playerId, badge.BadgeId);
            }
        }
    }

    private void AwardBadge(string playerId, string badgeId)
    {
        var stats = PlayerStatisticsRepository.LoadPlayerStats(playerId);
        stats.badges.Add(badgeId);
        PlayerStatisticsRepository.SavePlayerStats(playerId, stats);
        Debug.Log($"Awarded badge '{badgeId}' to player '{playerId}'");
    }
}