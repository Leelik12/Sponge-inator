using UnityEngine;

public class SimpleSphereProjectile : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private LayerMask targetLayers; // Слои для столкновения
    [SerializeField] private GameObject impactEffect; // Эффект при столкновении

    public GubkaFat player;
    public float damage;
    void Start()
    {
        // Автоматическое уничтожение через 5 секунд на всякий случай
        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Проверяем столкновение с нужными слоями
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Жир попал на губку");
            CreateImpactEffect(collision.contacts[0].point);

            player = collision.gameObject.GetComponent<GubkaFat>();
            player.AbsorbFat(damage);

            Destroy(gameObject);
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