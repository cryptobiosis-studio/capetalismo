using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyTypes{
    Turret,
    Melee,
    Ranged
}
[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/New Enemy", order = 1)]
public class EnemyObj : ScriptableObject
{   
    public string enemyName;
    [TextArea]public string desciption;

    public Sprite enemySprite;
    //public Animator enemyAnimator;
    public EnemyTypes enemyTypes;
    public float life;
    public float damage;
    public float speed;
}
