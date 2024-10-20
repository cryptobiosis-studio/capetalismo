using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
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
         Destroy(this.gameObject, 1f);

    }
    
}
