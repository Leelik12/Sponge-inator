using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class EventDrivenLevelManager : MonoBehaviour
{
    [Tooltip("Слой, на котором находятся враги (например 'Enemy')")]
    public string enemyLayerName = "Enemy";

    [Tooltip("Событие, вызываемое при завершении уровня")]
    public UnityEvent onAllEnemiesDead;
    public GameObject winwindow;
    private int enemyLayer;
    private int aliveCount;

    void OnEnable()
    {
        EnemyHealth.OnAnyDeath += OnEnemyDeath;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyDeath -= OnEnemyDeath;
    }

    void Start()
    {
        // Определяем ID слоя
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer == -1)
        {
            Debug.LogError("[EventDrivenLevelManager] Слой '" + enemyLayerName + "' не найден!");
            return;
        }

        // Находим всех существующих врагов по слою
        var allObjects = FindObjectsOfType<GameObject>();
        var enemies = allObjects.Where(o => o.layer == enemyLayer).ToArray();

        aliveCount = enemies.Length;

        // Проверяем, не мертвы ли некоторые
        foreach (var e in enemies)
        {
            var h = e.GetComponent<EnemyHealth>();
            if (h != null && !h.IsAlive())
            {
                aliveCount--;
            }
        }

        Debug.Log("[EventDrivenLevelManager] Initial enemy count: " + aliveCount);

        if (aliveCount <= 0)
        {
            LevelCompleted();
        }
    }

    void OnEnemyDeath(EnemyHealth h)
    {
        // Проверяем, что объект действительно был на слое врагов
        if (h.gameObject.layer != enemyLayer) return;

        aliveCount = Mathf.Max(0, aliveCount - 1);
        Debug.Log("[EventDrivenLevelManager] Enemy died. Remaining: " + aliveCount);

        if (aliveCount == 0)
        {
            LevelCompleted();
        }
    }

    void LevelCompleted()
    {
        Debug.Log("[EventDrivenLevelManager] All enemies dead - level complete.");
        onAllEnemiesDead?.Invoke();
        winwindow.SetActive(true);
        Time.timeScale = 0f;
    }
}
