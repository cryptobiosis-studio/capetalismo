using System.Globalization;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Rendering;
using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviour
{
    [SerializeField]EnemyObj enemySettings;
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
        this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = enemySettings.enemySprite;
        lastShotTime = Time.time;
    }

    void Update(){
        PlayerController player;
        if (room.hasPlayerIn)
        {   
            player = room.player;
            playerPosition = player.gameObject.transform;
            if (enemySettings.enemyTypes == EnemyTypes.Melee){
                if (UnityEngine.Vector3.Distance(transform.position, playerPosition.position) > 0.9f && canWalk)
                {
                    transform.position = UnityEngine.Vector2.MoveTowards(transform.position, playerPosition.position, enemySettings.speed * Time.deltaTime);
                    canAttack = Physics2D.OverlapCircle(transform.position, 0.9f, LayerMask.GetMask("Player")); 
                    if (canAttack){
                        enemyAnim.Play("Enemy2Atk");
                    }
                    if (playerPosition.position.x < transform.position.x){
                        transform.localScale = new UnityEngine.Vector3(1, 1, 1);
                    }
                    else if (playerPosition.position.x > transform.position.x){
                        transform.localScale = new UnityEngine.Vector3(-1, 1, 1); 
                    }
                }
            }else if (enemySettings.enemyTypes == EnemyTypes.Turret)  {
                UnityEngine.Vector2 direction = (playerPosition.position - transform.position).normalized;
                if(direction.x > 0){
                    transform.localScale = new UnityEngine.Vector3(-1, 1, 1);
                    gun.transform.localScale = new UnityEngine.Vector3(-0.8f, 0.8f, 1);
                }else{
                    transform.localScale = new UnityEngine.Vector3(1, 1, 1);
                    gun.transform.localScale = new UnityEngine.Vector3(0.8f, -0.8f, 1);
                }
                if (gun != null){
                    gun.transform.right = direction; 
                }
                if (Time.time - lastShotTime >= shootingInterval) {
                    ShootAtPlayer(direction);
                    lastShotTime = Time.time; 
                }
            }if (enemySettings.enemyTypes == EnemyTypes.Boss){
                if (UnityEngine.Vector3.Distance(transform.position, playerPosition.position) > 3.3f && canWalk)
                {
                    transform.position = UnityEngine.Vector2.MoveTowards(transform.position, playerPosition.position, enemySettings.speed * Time.deltaTime);
                    canAttack = Physics2D.OverlapCircle(transform.position,3.3f, LayerMask.GetMask("Player")); 
                    if (canAttack){
                        enemyAnim.Play("ElonAtk");
                    }
                    if (playerPosition.position.x < transform.position.x){
                        transform.localScale = new UnityEngine.Vector3(1, 1, 1);
                    }
                    else if (playerPosition.position.x > transform.position.x){
                        transform.localScale = new UnityEngine.Vector3(-1, 1, 1); 
                    }
                }
            }
        }
    }

    void ShootAtPlayer(UnityEngine.Vector2 direction) {
        if (projectilePrefab != null && shootPoint != null){
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, UnityEngine.Quaternion.identity);
            projectile.GetComponent<EnemyBullet>().damage = enemySettings.damage;
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null){
                rb.velocity = direction * projectilePrefab.GetComponent<EnemyBullet>().speed;
            }
        }
    }
    public void TakeDamage(float damage){
        life-=damage;
        Debug.Log(damage);
        Debug.Log(life);
        if(life <= 0){
            room.player.audioSource.clip = enemyDestroyClip;
            room.player.audioSource.Play(); 
            if(room.player.isSinglePlayer){
                Destroy(this.gameObject, 0f);
            }else{
                PhotonNetwork.Destroy(this.gameObject);
            }
            
        }
    }
    public void MeleeDamage(){
        
        if(atkCol.IsTouchingLayers(LayerMask.GetMask("Player"))){
            Debug.Log("Player hited!");
            room.player.TakeDamage(enemySettings.damage);
        }   
        canWalk = true;
    }
}
