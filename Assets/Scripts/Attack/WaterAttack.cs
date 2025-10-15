using UnityEngine;

public class WaterJetAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform jetStartPoint;      // Откуда летит струя
    [SerializeField] private GameObject waterJetPrefab;    // Префаб струи воды
    [SerializeField] private Camera playerCamera;          // Камера игрока
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject DeathEffect;
    [Header("Settings")]
    [SerializeField] private float ShootDelay = 0.5f;
    [SerializeField] private float jetSpeed = 100f;
    [SerializeField] private float attackRange = 40f;
    [SerializeField] private float waterCost = 1f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private LayerMask enemyLayer;
    private float timer;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem waterSplashEffect;

    private GubkaWater gubkaWater;
    private HeroVoice speaker;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.LogWarning("Camera not assigned, using Camera.main");
        }

        gubkaWater = GetComponent<GubkaWater>();
        speaker = GetComponent<HeroVoice>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        // ЛКМ — выстрел
        if (Input.GetMouseButton(0) && timer <=0f)
        {
            timer = ShootDelay;
            TryShoot(jetSpeed); // Передаем скорость снаряда
        }
    }

    private void TryShoot(float launchForce)
    {
        if (gubkaWater != null && gubkaWater.currentWater < waterCost*2f)
        {
            Debug.Log("Not enough water!");
            return;
        }

        if (speaker != null)
            speaker.SayAttack();

        // Тратим воду
        if (gubkaWater != null)
        {
            gubkaWater.currentWater -= waterCost;
            gubkaWater.UpdateUI();
            gubkaWater.UpdateScaleAndSpeed();
        }

        ShootProjectile(launchForce);
    }

    private void ShootProjectile(float launchForce)
    {
        Vector3 dir = playerCamera.transform.forward;
        Vector3 target = playerCamera.transform.position + dir * attackRange;

        if (Physics.Raycast(playerCamera.transform.position, dir, out RaycastHit hit, attackRange))
            target = hit.point;

        GameObject waterJet = Instantiate(waterJetPrefab, jetStartPoint.position, Quaternion.LookRotation(dir));
        WaterJetProjectile projectile = waterJet.GetComponent<WaterJetProjectile>();

        if (projectile != null)
        {
            animator.SetBool("Plevok", true);
            // Передаем силу снаряда
            projectile.Initialize(jetStartPoint.position, target, launchForce, jetSpeed, damageAmount, enemyLayer, this, attackRange);
        }

        if (waterSplashEffect != null)
            waterSplashEffect.Play();
    }

    public void OnWaterJetHit(Vector3 hitPosition, Collider hitEnemy)
    {
        if (hitEnemy == null) return;

        if (((1 << hitEnemy.gameObject.layer) & enemyLayer) != 0)
        {
            EnemyHealth hp = hitEnemy.GetComponent<EnemyHealth>();
            if (hp != null)
            {
                hp.deathEffect = DeathEffect;
                hp.TakeDamage(damageAmount);
                
            }
        }
    }
}
