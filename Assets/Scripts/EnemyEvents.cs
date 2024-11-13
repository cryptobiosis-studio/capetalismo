using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvents : MonoBehaviour
{
    Enemy enemyScript;
    void Start()
    {
        enemyScript = GetComponentInParent<Enemy>();
    }

    public void MeleeDamage(){
        enemyScript.MeleeDamage();
    }
    public void CanWalk(){
        enemyScript.canWalk = false;
    }

    
}
