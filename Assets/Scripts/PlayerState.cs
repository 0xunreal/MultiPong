using UnityEngine;

public class PlayerState
{
    public string PlayerId { get; private set; }
    public int Score { get; set; }
    public Vector2 PaddlePosition { get; set; }
    public float VerticalInput { get; set; }

    public PlayerState(string playerId)
    {
        PlayerId = playerId;
        Score = 0;
        PaddlePosition = Vector2.zero;
        VerticalInput = 0f;
    }
}
