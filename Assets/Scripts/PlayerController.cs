using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour

{
    public float speed;
    Rigidbody2D rb;
    float inputX;
    float inputY;
    public Animator anim;
    public SpriteRenderer sprite;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim  = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
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

        if(inputX != 0 || inputY !=0){ //Activate walking animation if player is moving.
            anim.SetBool("isMoving", true);
        }else{
            anim.SetBool("isMoving", false);
        }
        if(inputX < 0){ //Auto Explicativo (character sprite face the side you are walking.)
            sprite.flipX = true;
        }else{
            sprite.flipX = false;
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

