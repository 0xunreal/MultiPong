using System.Collections.Generic;

public enum BadgeType
{
    StatThreshold,
    OneOff
}

[System.Serializable]
public class BadgeDefinition
{
    public string BadgeId;
    public BadgeType Type;

    // For StatThreshold type:
    public string StatName;
    public float Threshold;

    // For OneOff type:
    public string EventKey; // e.g. "NameChanged"
}