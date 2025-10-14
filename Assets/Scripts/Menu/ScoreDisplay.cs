using UnityEngine;
using TMPro; // обязательно, если используешь TextMeshPro

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text bestScoreText;
    public TMP_Text lastScoreText;

    void Start()
    {
        if (StaticHolder.BestScore == 0)
        {
            StaticHolder.BestScore = PlayerPrefs.GetInt("BestScore");
        }
        if (StaticHolder.LastScore > StaticHolder.BestScore)
        {
            StaticHolder.BestScore = StaticHolder.LastScore;
            StaticHolder.LastScore = 0;
            PlayerPrefs.SetInt("BestScore", StaticHolder.BestScore);
        }
        if (bestScoreText != null)
            bestScoreText.text = StaticHolder.BestScore.ToString();
        if (lastScoreText != null)
            lastScoreText.text = StaticHolder.LastScore.ToString();
        StaticHolder.LastScore = 0;
    }

}
