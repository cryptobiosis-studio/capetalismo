using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyObj enemySettings;
    public float life;
    Room room;
    Transform playerPosition;
    Animator enemyAnim;
    public bool canWalk = true;
    bool canAttack = false;
    public CircleCollider2D atkCol;

    public GameObject gun;
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootingInterval = 1.5f;
    private float lastShotTime;
    public AudioClip enemyDestroyClip;

    void Start()
    {
        life = enemySettings.life;
        room = GetComponentInParent<Room>();
        enemyAnim = GetComponentInChildren<Animator>();
        atkCol = GetComponent<CircleCollider2D>();
        GetComponentInChildren<SpriteRenderer>().sprite = enemySettings.enemySprite;
        lastShotTime = Time.time;
    }

    void Update()
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
            transform.position = Vector2.MoveTowards(transform.position, playerPosition.position, enemySettings.speed * Time.deltaTime);
            canAttack = Physics2D.OverlapCircle(transform.position, 0.9f, LayerMask.GetMask("Player"));
            if (canAttack) enemyAnim.Play("Enemy2Atk");
            FlipTowards(playerPosition.position);
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
            transform.position = Vector2.MoveTowards(transform.position, playerPosition.position, enemySettings.speed * Time.deltaTime);
            canAttack = Physics2D.OverlapCircle(transform.position, 3.3f, LayerMask.GetMask("Player"));
            if (canAttack) enemyAnim.Play("ElonAtk");
            FlipTowards(playerPosition.position);
        }
    }

    private void FlipTowards(Vector3 targetPos)
    {
        if (targetPos.x < transform.position.x)
            transform.localScale = Vector3.one;
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void FlipGunTowards(Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = Vector3.one;
            gun.transform.localScale = new Vector3(0.8f, -0.8f, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
            gun.transform.localScale = new Vector3(-0.8f, 0.8f, 1);
        }
        gun.transform.right = direction;
    }

    void ShootAtPlayer(Vector2 direction)
    {
        if (projectilePrefab == null || shootPoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        var bullet = projectile.GetComponent<EnemyBullet>();
        bullet.damage = enemySettings.damage;
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = direction * bullet.speed;
    }

    public void TakeDamage(float damage)
    {
        life -= damage;
        Debug.Log($"Damage: {damage}  |  Remaining life: {life}");

        if (life > 0) return;

        room.player.audioSource.clip = enemyDestroyClip;
        room.player.audioSource.Play();

        if (enemySettings.enemyTypes == EnemyTypes.Boss)
        {
            SceneManager.LoadScene("Win");
        }

        Destroy(gameObject);
    }

    public void MeleeDamage()
    {
        if (atkCol.IsTouchingLayers(LayerMask.GetMask("Player")))
        {
            Debug.Log("Player hit!");
            room.player.TakeDamage(enemySettings.damage);
        }
        canWalk = true;
    }
}
