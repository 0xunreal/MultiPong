using System.Collections.Generic;

public static class GlobalPlayerStatistics
{
    private static readonly Dictionary<string, AggregationType> GlobalAggregations = new()
    {
        { "Joins", AggregationType.SUM },
        { "Score", AggregationType.SUM },
        { "HighScore", AggregationType.MAX },
        { "Wins", AggregationType.SUM },
    };

    public static AggregationType GetAggregationForStat(string statName)
    {
        return GlobalAggregations.TryGetValue(statName, out var agg) ? agg : AggregationType.LAST; // Default if not found
    }
}
