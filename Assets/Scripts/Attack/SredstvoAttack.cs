using UnityEngine;

[RequireComponent(typeof(GubkaClear))]
public class SredstvoAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform jetStartPoint;      // Точка выстрела
    [SerializeField] private GameObject sredstvoJetPrefab; // Префаб мыльного снаряда
    [SerializeField] private Camera playerCamera;          // Камера игрока

    [Header("Settings")]
    [SerializeField] private float jetSpeed = 60f;         // Скорость полета
    [SerializeField] private float launchForce = 20f;      // Сила выстрела
    [SerializeField] private float attackRange = 30f;      // Дистанция прицела
    [SerializeField] private float sredstvoCost = 1f;      // Расход моющего средства
    [SerializeField] private int damageAmount = 10;        // Урон
    [SerializeField] private LayerMask enemyLayer;         // Слой врагов

    [Header("Visuals")]
    [SerializeField] private ParticleSystem splashEffect;  // Эффект всплеска (пена)

    private GubkaClear gubkaClear;
    private HeroVoice speaker;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.LogWarning("Camera not assigned. Using Camera.main.");
        }

        if (jetStartPoint == null)
        {
            jetStartPoint = transform;
            Debug.LogWarning("JetStartPoint not assigned! Using transform.");
        }

        gubkaClear = GetComponent<GubkaClear>();
        speaker = GetComponent<HeroVoice>();
    }

    private void Update()
    {
        // ЛКМ — стреляем мыльным зарядом
        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (gubkaClear == null) return;

        if (gubkaClear.currentClear < sredstvoCost)
        {
            Debug.Log("Not enough cleaning agent!");
            // Можно добавить звук “пуста бутылка”
            return;
        }

        // Тратим средство
        gubkaClear.currentClear -= sredstvoCost;
        gubkaClear.UpdateUI();

        if (speaker != null)
            speaker.SayAttack();

        ShootProjectile();

        if (splashEffect != null)
            splashEffect.Play();
    }

    private void ShootProjectile()
    {
        Vector3 direction = playerCamera.transform.forward;
        Vector3 target = playerCamera.transform.position + direction * attackRange;

        if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, attackRange))
        {
            target = hit.point;
        }

        GameObject projectileObj = Instantiate(sredstvoJetPrefab, jetStartPoint.position, Quaternion.LookRotation(direction));
        SredstvoJetProjectile projectile = projectileObj.GetComponent<SredstvoJetProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                jetStartPoint.position,
                target,
                launchForce,
                jetSpeed,
                damageAmount,
                enemyLayer,
                this,
                attackRange
            );
        }
    }

    // Метод вызывается из SredstvoJetProjectile при попадании
    public void OnSredstvoJetHit(Vector3 hitPosition, Collider hitEnemy)
    {
        if (hitEnemy == null) return;

        if (((1 << hitEnemy.gameObject.layer) & enemyLayer) != 0)
        {
            EnemyHealth hp = hitEnemy.GetComponent<EnemyHealth>();
            if (hp != null)
                hp.TakeDamage(damageAmount);
        }

        // Можно добавить эффект пены на месте попадания
        Debug.Log($"Soap hit {hitEnemy.name} at {hitPosition}");
    }
}
