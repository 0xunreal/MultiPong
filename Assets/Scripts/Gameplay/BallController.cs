using UnityEngine;

public class BallController : MonoBehaviour
{
    private static BallController _instance;
    public static BallController Instance;
    public float initialSpeed = 10f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // Destroy the duplicate
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void LaunchBall()
    {
        Debug.Log("Launching ball...");
        var xDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        var yDirection = Random.Range(-1f, 1f);

        var launchDirection = new Vector2(xDirection, yDirection).normalized;
        _rb.linearVelocity = launchDirection * initialSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var server = FindFirstObjectByType<GameServer>();
        var states = server.CurrentGameState.GetAllPlayerStates();

        // Identify which goal was hit and which player scored
        bool goalOnLeft = collision.CompareTag("Goal2"); // If true, player on left side scores
        bool goalOnRight = collision.CompareTag("Goal1"); // If true, player on right side scores

        string scoringPlayerId = null;

        foreach (var p in states)
        {
            // If goal is on the left side (Goal2 triggered), the left-side player scores
            if (goalOnLeft && p.PaddlePosition.x < 0)
            {
                p.Score++;
                scoringPlayerId = p.PlayerId;
            }
            // If goal is on the right side (Goal1 triggered), the right-side player scores
            if (goalOnRight && p.PaddlePosition.x > 0)
            {
                p.Score++;
                scoringPlayerId = p.PlayerId;
            }
        }

        ResetBall();
        server.CurrentGameState.SyncGameState();

        // If we identified a scoring player, trigger the "FirstGoalScored" event
        if (!string.IsNullOrEmpty(scoringPlayerId))
        {
            var statsService = new PlayerStatisticsService();
            statsService.TriggerBadgeEvent(scoringPlayerId, "FirstGoalScored");
        }
    }

    public Vector2 GetVelocity()
    {
        return _rb != null ? _rb.linearVelocity : Vector2.zero;
    }
    
    private void ResetBall()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;
        LaunchBall();
    }
}