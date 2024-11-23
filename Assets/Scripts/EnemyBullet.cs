using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public Sprite bulSprite;
    public float speed;
    public float damage;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = bulSprite;
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(this.gameObject, 2f);

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player"){
            Debug.Log("Player shooted!");
            other.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
        }if(other.gameObject.tag == "Wall"){
            Destroy(this.gameObject);
        }
        if(other.gameObject.tag == "EyeRelic"){
            Destroy(this.gameObject);
        }
        if(other.gameObject.layer == LayerMask.GetMask("Wall")){
            Destroy(this.gameObject);
        }
        
    }
    
}
