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
        // Отключаем все волны КРОМЕ ПЕРВОЙ
        for (int i = 0; i < waves.Count; i++)
        {
            // ПЕРВУЮ волну НЕ отключаем - она должна быть активна с начала
            if (i > 0) // Начинаем со второй волны (индекс 1)
            {
                foreach (var enemy in waves[i].enemies)
                {
                    if (enemy != null)
                    {
                        enemy.SetActive(false);
                    }
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

                // ВОССТАНАВЛИВАЕМ здоровье врагов при активации волны
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // Если у тебя есть метод для восстановления здоровья, вызови его здесь
                    // enemyHealth.ResetHealth();
                }
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

        Debug.Log($"Убито врагов в волне {currentWaveIndex + 1}: {enemiesKilledInCurrentWave}/{waves[currentWaveIndex].enemies.Count}");

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
            Debug.Log($"Запускаем волну {currentWaveIndex + 1} через 1 секунду");
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
        Debug.Log($"Активируем волну {currentWaveIndex + 1}");
        ActivateWave(currentWaveIndex);
    }

    // Для отладки - посмотреть текущее состояние
    void Update()
    {
        // Для отладки - можно удалить потом
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Текущая волна: {currentWaveIndex + 1}, Убито: {enemiesKilledInCurrentWave}");
        }
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