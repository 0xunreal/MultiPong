using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;

    private int player1Score = 0;
    private int player2Score = 0;

    public void AddScore(int player)
    {
        if (player == 1)
            player1Score++;
        else
            player2Score++;

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        player1ScoreText.text = player1Score.ToString();
        player2ScoreText.text = player2Score.ToString();
    }
}