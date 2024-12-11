using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Sprite bulSprite;
    public float speed;
    public float damage;

    public GameObject boom;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = bulSprite;
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(this.gameObject, 1f);

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Enemy"){
            Debug.Log("Enemy shooted!");
            Debug.Log(other.gameObject.GetComponent<Enemy>());
            other.gameObject.GetComponent<Enemy>().TakeDamage(damage);
            Instantiate(boom, transform.position, Quaternion.identity);
            Destroy(this.gameObject, 0.05f);
        }
        if(other.gameObject.tag == "Wall"){
            Instantiate(boom, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
        
    }
    
}
