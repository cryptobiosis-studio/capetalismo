using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour

{
    public float speed;
    public float life;
    Rigidbody2D rb;
    float inputX;
    float inputY;
    public Animator anim;
    public SpriteRenderer sprite;

    public SpriteRenderer fade;

    public GunObj equippedGun;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim  = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        equippedGun = GetComponentInChildren<Gun>().gunSettings;
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
        InteractableArea();
        
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
        StartCoroutine("FadeImage", false);
        Camera.main.transform.position = new UnityEngine.Vector3(other.transform.position.x - 0.53f, other.transform.position.y, -10);
         StartCoroutine("FadeImage", true);
        }
    }
    void InteractableArea(){
        bool canInteract = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")); 
        DroppedGun droppedGun = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject.GetComponent<DroppedGun>();
        Chest chest = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject.GetComponent<Chest>();
        if(Input.GetKeyDown(KeyCode.E)){
            if(droppedGun != null){
            GunObj previousGun = equippedGun;
            ChangeGun(droppedGun.gun);
            droppedGun.gun = previousGun;
            droppedGun.Change();
            }else if(chest != null && chest.isActiveAndEnabled && !chest.opened){
                chest.openChest();
            
        }
        }
        
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
     void ChangeGun(GunObj gun){
      equippedGun = gun;
      GetComponentInChildren<Gun>().gunSettings = gun;
      GetComponentInChildren<Gun>().Change();
    }

    public void TakeDamage(float damage){
        life -= damage;
    }
    IEnumerator FadeImage(bool fadeAway)
    {
        // fade from opaque to transparent
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime*2.5f)
            {
                fade.color = new Color(0, 0, 0, i);
                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {
            for (float i = 0; i <= 1; i += Time.deltaTime*2.5f)
            {
                fade.color = new Color(0, 0, 0, i);
                yield return null;
            }
        }
    }

}

