using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;

public class LevelCompleteByLayerPoll : MonoBehaviour
{
    [Tooltip("Имя слоя, на котором находятся враги (например 'Enemy')")]
    public string enemyLayerName = "Enemy";

    [Tooltip("Проверять наличие врагов раз в N секунд")]
    public float checkInterval = 1.0f;

    [Tooltip("Событие вызывается, когда врагов на сцене не осталось")]
    public UnityEvent onAllEnemiesDead;
    public GameObject winwindow;
    int enemyLayer = -1;
    bool completed = false;
    Coroutine checkRoutine;

    void Start()
    {
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer == -1)
        {
            Debug.LogError("[LevelCompleteByLayerPoll] Layer '" + enemyLayerName + "' not found. Please add it in Project Settings -> Tags and Layers.");
            enabled = false;
            return;
        }

        checkRoutine = StartCoroutine(CheckLoop());
    }

    IEnumerator CheckLoop()
    {
        while (!completed)
        {
            yield return new WaitForSeconds(checkInterval);
            if (AreNoEnemiesLeft())
            {
                completed = true;
                Debug.Log("[EventDrivenLevelManager] All enemies dead - level complete.");
                winwindow.SetActive(true);
                Time.timeScale = 0f;
                yield break;
            }
        }
    }

    bool AreNoEnemiesLeft()
    {
        // Находим все объекты GameObject в сцене и проверяем их слой
        // FindObjectsOfType<GameObject>() возвращает все игровые объекты.
        var all = FindObjectsOfType<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            var go = all[i];
            if (!go.activeInHierarchy) continue; // игнорируем неактивные объекты
            if (go.layer == enemyLayer)
            {
                // найден хотя бы один враг
                return false;
            }
        }
        // ничего не найдено
        return true;
    }

    // опционально: ручной вызов для немеденной проверки
    public void ForceCheckNow()
    {
        if (completed) return;
        if (AreNoEnemiesLeft())
        {
            completed = true;
            Debug.Log("[EventDrivenLevelManager] All enemies dead - level complete.");
            winwindow.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}

