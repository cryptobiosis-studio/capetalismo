using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    Rigidbody2D rb;
    float inputX;
    float inputY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        

    }
    void Update()
    {  
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        
    }

    void FixedUpdate()
    {   
        if(inputX != 0 && inputY !=0){
            inputX *= 0.7f;
            inputY *= 0.7f;
        }
        Move(inputX, inputY);
    }

    void Move(float x, float y){
        rb.velocity = new UnityEngine.Vector2(x * speed, y * speed);
    }
    void OnTriggerEnter2D(Collider2D other)
    { 
        if (other.gameObject.layer == LayerMask.NameToLayer("Room")) {
        Debug.Log(other.gameObject.name);
        Camera.main.transform.position = new UnityEngine.Vector3(other.transform.position.x, other.transform.position.y, -10);
        }
    }
}

