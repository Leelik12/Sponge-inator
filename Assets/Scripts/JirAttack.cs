using UnityEngine;

public class SimpleSphereProjectile : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private LayerMask targetLayers; // Слои для столкновения
    [SerializeField] private GameObject impactEffect; // Эффект при столкновении

    void Start()
    {
        // Автоматическое уничтожение через 5 секунд на всякий случай
        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Проверяем столкновение с нужными слоями
        if (((1 << collision.gameObject.layer) & targetLayers) != 0)
        {
            CreateImpactEffect(collision.contacts[0].point);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Для триггерных коллайдеров
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            CreateImpactEffect(collisionPoint);
            Destroy(gameObject);
        }
    }

    private void CreateImpactEffect(Vector3 position)
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, position, Quaternion.identity);
            Destroy(effect, 2f); // Удаляем эффект через 2 секунды
        }
    }
}