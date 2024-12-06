using System.Collections.Generic;

public static class GlobalBadgeDefinitions
{
    private static readonly List<BadgeDefinition> Badges = new()
    {
        new BadgeDefinition { BadgeId = "Join5Times", Type = BadgeType.StatThreshold, StatName = "Joins", Threshold = 5f },
        new BadgeDefinition { BadgeId = "Win10Times", Type = BadgeType.StatThreshold, StatName = "Wins", Threshold = 10f },
        new BadgeDefinition { BadgeId = "Score100", Type = BadgeType.StatThreshold, StatName = "TotalScore", Threshold = 100f },
        new BadgeDefinition { BadgeId = "FirstGoalScored", Type = BadgeType.OneOff, EventKey = "FirstGoalScored" }
    };

    public static IEnumerable<BadgeDefinition> GetAllBadges() => Badges;
}
