using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyObj enemySettings;

    private float life;
    private Room room;
    private Transform playerPosition;
    private Animator enemyAnim;
    private CircleCollider2D atkCol;

    private float damage;
    private float speed;

    public bool canWalk = true;
    private bool canAttack = false;

    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootingInterval = 1.5f;

    public GameObject killParticle;
    private float lastShotTime;

    [SerializeField] private AudioClip enemyDestroyClip;

    private enum EnemySize { Normal, Big, Small }
    private EnemySize currentSize = EnemySize.Normal;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        room = GetComponentInParent<Room>();
        enemyAnim = GetComponentInChildren<Animator>();
        atkCol = GetComponent<CircleCollider2D>();

        InitializeEnemy();
    }

    private void OnEnable()
    {
        ApplySizeVisuals();
    }

    private void InitializeEnemy()
    {
        float baseDamage = enemySettings.damage;
        float baseSpeed = enemySettings.speed;
        float baseLife = enemySettings.life;

        int chance = UnityEngine.Random.Range(1, 16);

        if (chance == 1)
        {
            currentSize = EnemySize.Big;
            baseDamage *= 2f;
            baseSpeed *= 2f;
            baseLife *= 2f;
        }
        else if (chance == 2)
        {
            currentSize = EnemySize.Small;
            baseDamage *= 0.5f;
            baseSpeed *= 1.5f;
            baseLife *= 0.5f;
        }
        else
        {
            currentSize = EnemySize.Normal;
        }

        float difficultyMultiplier = 1 + (GameManager.Instance != null ? (GameManager.Instance.floorLevel - 1) * 0.2f : 0);

        damage = baseDamage * difficultyMultiplier;
        speed = baseSpeed * difficultyMultiplier;
        life = baseLife * difficultyMultiplier;

        if (enemySettings.enemySprite != null && spriteRenderer != null)
            spriteRenderer.sprite = enemySettings.enemySprite;

        ApplySizeVisuals();

        lastShotTime = Time.time;
    }

    private void ApplySizeVisuals()
    {
        switch (currentSize)
        {
            case EnemySize.Big:
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.red;
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                break;

            case EnemySize.Small:
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.yellow;
                transform.localScale = new Vector3(0.75f, 0.75f, 1f);
                break;

            default:
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
                transform.localScale = new Vector3(1f, 1f, 1f);
                break;
        }
    }

    private void Update()
    {
        if (!room.hasPlayerIn) return;

        playerPosition = room.player.transform;

        switch (enemySettings.enemyTypes)
        {
            case EnemyTypes.Melee:
                HandleMelee();
                break;

            case EnemyTypes.Turret:
                HandleTurret();
                break;

            case EnemyTypes.Boss:
                HandleBoss();
                break;
        }
    }

    private void HandleMelee()
    {
        float distance = Vector3.Distance(transform.position, playerPosition.position);

        if (distance > 0.9f && canWalk)
        {
            MoveTowardsPlayer(playerPosition.position, speed);
            CheckMeleeAttack(0.9f);
        }
    }

    private void HandleTurret()
    {
        Vector2 direction = (playerPosition.position - transform.position).normalized;
        FlipGunTowards(direction);

        if (Time.time - lastShotTime >= shootingInterval)
        {
            ShootAtPlayer(direction);
            lastShotTime = Time.time;
        }
    }

    private void HandleBoss()
    {
        float distance = Vector3.Distance(transform.position, playerPosition.position);

        if (distance > 3.3f && canWalk)
        {
            MoveTowardsPlayer(playerPosition.position, speed);
            CheckMeleeAttack(3.3f, "ElonAtk");
        }
    }

    private void MoveTowardsPlayer(Vector3 targetPos, float speed)
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        FlipTowards(targetPos);
    }

    private void CheckMeleeAttack(float attackRange, string animationName = "Enemy2Atk")
    {
        canAttack = Physics2D.OverlapCircle(transform.position, attackRange, LayerMask.GetMask("Player"));

        if (canAttack && enemyAnim != null)
            enemyAnim.Play(animationName);
    }

    private void FlipTowards(Vector3 targetPos)
    {
        transform.localScale = (targetPos.x < transform.position.x) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    private void FlipGunTowards(Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            if (gun != null)
                gun.transform.localScale = new Vector3(0.8f, -0.8f, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
            if (gun != null)
                gun.transform.localScale = new Vector3(-0.8f, 0.8f, 1);
        }

        if (gun != null)
            gun.transform.right = direction;
    }

    private void ShootAtPlayer(Vector2 direction)
    {
        if (projectilePrefab == null || shootPoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        var bullet = projectile.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
            bullet.damage = damage;
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = direction * bullet.speed;
        }
    }

    public void TakeDamage(float damage)
    {
        life -= damage;

        if (life <= 0)
        {
            PlayDestroySound();
            if (CameraShake.Instance != null)
                CameraShake.Instance.TriggerShake(0.2f, 0.2f);
            Instantiate(killParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void PlayDestroySound()
    {
        if (room?.player?.audioSource != null && enemyDestroyClip != null)
        {
            room.player.audioSource.clip = enemyDestroyClip;
            room.player.audioSource.Play();
        }
    }

    public void MeleeDamage()
    {
        if (atkCol.IsTouchingLayers(LayerMask.GetMask("Player")))
            room.player.TakeDamage(damage);

        canWalk = true;
    }
}
