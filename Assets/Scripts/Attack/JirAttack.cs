using UnityEngine;

public class SimpleSphereProjectile : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private LayerMask targetLayers; // Слои для столкновения
    [SerializeField] private GameObject impactEffect; // Эффект при столкновении

    private Transform effectSpawnPoint;
    public GubkaFat player;
    public float damage;

    void Start()
    {
        // Автоматически находим дочерний объект
        FindSpawnPoint();

        Destroy(gameObject, 5f);
    }

    private void FindSpawnPoint()
    {
        // Если не нашли по имени, ищем любой дочерний с определенным тегом
        if (effectSpawnPoint == null)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("EffectSpawnPoint"))
                {
                    effectSpawnPoint = child;
                    break;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Жир попал на губку");

            Vector3 spawnPosition = effectSpawnPoint.position;
            player = collision.gameObject.GetComponent<GubkaFat>();
            if (player != null)
            {
                player.AbsorbFat(damage);
            }
            CreateImpactEffect(spawnPosition);
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        // Для триггерных коллайдеров
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            // Спавним эффект в указанной точке или в точке контакта
            Vector3 spawnPosition = effectSpawnPoint != null ? effectSpawnPoint.position : other.ClosestPoint(transform.position);
            CreateImpactEffect(spawnPosition);
            Destroy(gameObject);
        }
    }

    private void CreateImpactEffect(Vector3 position)
    {
        if (impactEffect != null)
        {
            // Используем rotation эффекта спавн точки или стандартный поворот
            Quaternion spawnRotation = effectSpawnPoint != null ? effectSpawnPoint.rotation : Quaternion.identity;
            GameObject effect = Instantiate(impactEffect, position, spawnRotation);
            Destroy(effect, 2f); // Удаляем эффект через 2 секунды
        }
    }
}