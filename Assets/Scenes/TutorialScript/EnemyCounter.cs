using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyCounter : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI killCountText;
    public GameObject levelCompleteCanvas;

    private void Start()
    {
        if (levelCompleteCanvas != null)
        {
            levelCompleteCanvas.SetActive(false);
        }
    }

    public void SetCurrentWave(int currentWave, int totalWaves, int enemiesInWave)
    {
        if (waveText != null)
        {
            waveText.text = $"{currentWave}/{totalWaves}";
        }

        UpdateKillCount(0); // Сбрасываем счетчик убийств
    }

    public void UpdateKillCount(int kills)
    {
        if (killCountText != null)
        {
            // Находим общее количество врагов в текущей волне через WaveManager
            WaveManager waveManager = FindObjectOfType<WaveManager>();
            if (waveManager != null && waveManager.waves.Count > waveManager.currentWaveIndex)
            {
                int totalEnemies = waveManager.waves[waveManager.currentWaveIndex].enemies.Count;
                killCountText.text = $"{kills}/{totalEnemies}";
            }
        }
    }

    public void ShowLevelComplete()
    {
        if (levelCompleteCanvas != null)
        {
            levelCompleteCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // Можно добавить звук, анимацию и т.д.
        Debug.Log("Уровень завершен!");
    }
}