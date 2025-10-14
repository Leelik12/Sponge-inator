using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<GameObject> enemies; // Список противников этой волны
        public bool isActive = false;
        public bool isCompleted = false;
    }

    [Header("Настройки волн")]
    public List<Wave> waves = new List<Wave>();
    public int currentWaveIndex = 0;
    private int enemiesKilledInCurrentWave = 0;

    [Header("Ссылки")]
    public EnemyCounter enemyCounter; // Ссылка на счетчик убийств

    private void Start()
    {
        InitializeWaves();
        StartFirstWave();
    }

    void InitializeWaves()
    {
        // Отключаем все волны кроме первой
        for (int i = 0; i < waves.Count; i++)
        {
            foreach (var enemy in waves[i].enemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }
            waves[i].isActive = false;
            waves[i].isCompleted = false;
        }
    }

    void StartFirstWave()
    {
        if (waves.Count > 0)
        {
            ActivateWave(0);
        }
    }

    void ActivateWave(int waveIndex)
    {
        if (waveIndex >= waves.Count) return;

        currentWaveIndex = waveIndex;
        enemiesKilledInCurrentWave = 0;
        waves[waveIndex].isActive = true;

        // Активируем всех противников волны
        foreach (var enemy in waves[waveIndex].enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
            }
        }

        Debug.Log($"Волна {waveIndex + 1} активирована! Противников: {waves[waveIndex].enemies.Count}");

        // Обновляем счетчик
        if (enemyCounter != null)
        {
            enemyCounter.SetCurrentWave(waveIndex + 1, waves.Count, waves[waveIndex].enemies.Count);
        }
    }

    // Этот метод вызывается когда противник убит
    public void OnEnemyKilled()
    {
        if (currentWaveIndex >= waves.Count) return;

        enemiesKilledInCurrentWave++;

        // Обновляем UI счетчика
        if (enemyCounter != null)
        {
            enemyCounter.UpdateKillCount(enemiesKilledInCurrentWave);
        }

        // Проверяем завершение текущей волны
        if (enemiesKilledInCurrentWave >= waves[currentWaveIndex].enemies.Count)
        {
            CompleteCurrentWave();
        }
    }

    void CompleteCurrentWave()
    {
        waves[currentWaveIndex].isCompleted = true;
        waves[currentWaveIndex].isActive = false;

        Debug.Log($"Волна {currentWaveIndex + 1} завершена!");

        // Переходим к следующей волне
        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            // Запускаем следующую волну
            Invoke("StartNextWave", 1f); // Задержка 1 секунда перед следующей волной
        }
        else
        {
            // Все волны завершены
            Debug.Log("Все волны завершены! Уровень пройден!");
            if (enemyCounter != null)
            {
                enemyCounter.ShowLevelComplete();
            }
        }
    }

    void StartNextWave()
    {
        ActivateWave(currentWaveIndex);
    }

    // Для отладки в инспекторе
    [ContextMenu("Пропустить текущую волну")]
    public void SkipCurrentWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            CompleteCurrentWave();
        }
    }
}