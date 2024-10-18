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
        // Verifica os inputs
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        if(inputX != 0 || inputY !=0){ //Activate walking animation if player is moving.
            anim.SetBool("isMoving", true);
        }else{
            anim.SetBool("isMoving", false);
        }
        if(inputX < 0){ //Auto Explicativo (character sprite face the side you are walking.)
            sprite.flipX = true;
        }else if(inputX > 0){sprite.flipX = false;}
        
    }

    void FixedUpdate()
    {   
        if(inputX != 0 && inputY !=0){ // Corrige a aceleracao na diagonal
            inputX *= 0.7f;
            inputY *= 0.7f;   
        }
        
        Move(inputX, inputY);
    }

    void Move(float x, float y){ // Move o jogador
        rb.velocity = new UnityEngine.Vector2(x * speed, y * speed);
    }
    void OnTriggerEnter2D(Collider2D other)
    { 
        if (other.gameObject.layer == LayerMask.NameToLayer("Room")) { //Posiciona a camera na sala em que o jogador se encontra
        Debug.Log(other.gameObject.name);
        Camera.main.transform.position = new UnityEngine.Vector3(other.transform.position.x - 0.53f, other.transform.position.y, -10);
        }
    }
}

