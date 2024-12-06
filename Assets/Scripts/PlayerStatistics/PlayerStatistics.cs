using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStatistics
{
    // Keys might be "Score", "Wins", "LevelTime", etc.
    public Dictionary<string, float> stats = new Dictionary<string, float>();
    public List<string> badges = new List<string>();
}