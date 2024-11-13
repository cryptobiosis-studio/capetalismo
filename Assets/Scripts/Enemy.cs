using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Rendering;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]EnemyObj enemySettings;
    public float life;
    Room room;
    Transform playerPosition;
    Animator enemyAnim;
    public bool canWalk = true;
    bool canAttack = false;
    CircleCollider2D atkCol;
    void Start()
    {
        life = enemySettings.life;
        room = GetComponentInParent<Room>();
        enemyAnim = GetComponentInChildren<Animator>();
        atkCol = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {   
        PlayerController player;
        if(room.hasPlayerIn){
             if(enemySettings.enemyTypes == EnemyTypes.Melee){
                player = room.player;
                playerPosition = player.gameObject.transform;
                if(UnityEngine.Vector3.Distance(transform.position, playerPosition.position)>0.9f && canWalk){
                    transform.position = UnityEngine.Vector2.MoveTowards(transform.position, playerPosition.position, enemySettings.speed * Time.deltaTime);
                    canAttack = Physics2D.OverlapCircle(transform.position, 0.9f, LayerMask.GetMask("Player")); 
                    if(canAttack){
                        enemyAnim.Play("Enemy2Atk");
                    }
                }
        }
    }
    }

    public void TakeDamage(float damage){
        life-=damage;
        if(life <= 0){
            Destroy(this.gameObject, 0f);
        }
    }
    public void MeleeDamage(){
        
        if(atkCol.IsTouchingLayers(6)){
            Debug.Log("Player hited!");
            atkCol.gameObject.GetComponent<PlayerController>().TakeDamage(enemySettings.damage);
        }   
        canWalk = true;
    }
}
