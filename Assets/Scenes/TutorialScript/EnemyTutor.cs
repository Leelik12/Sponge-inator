using UnityEngine;

public class EnemyTutor : MonoBehaviour
{
    [Header("Настройки врага")]
    public int health = 100;
    public int points = 10;

    private WaveManager waveManager;
    private bool isDead = false;

    private void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
    }

    // Метод для получения урона
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        // Сообщаем WaveManager о убийстве
        if (waveManager != null)
        {
            waveManager.OnEnemyKilled();
        }

        // Отключаем врага (или уничтожаем)
        gameObject.SetActive(false);

        // Можно добавить эффекты смерти, звуки и т.д.
        Debug.Log("Враг убит!");
    }

    // Для тестирования - убить при клике
    private void OnMouseDown()
    {
        TakeDamage(100);
    }
}