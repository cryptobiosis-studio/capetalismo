using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour

{
    public float speed;
    public float life;
    public float MaxLife;
    Rigidbody2D rb;
    float inputX;
    float inputY;
    public Animator anim;
    public SpriteRenderer sprite;

    public SpriteRenderer fade;

    public GunObj equippedGun;

    public  bool invincibility;
    public float invincibilityTime;
    public float maxInvincibilityTime;

    GameObject choiceText;

    public Slider lifeSlider;

    public float damageMultiplier;
    public float fireRateMultiplier;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim  = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        equippedGun = GetComponentInChildren<Gun>().gunSettings;
        invincibility = false;
        life = MaxLife;
        SetLifeSlider();
        choiceText = GameObject.Find("CurriculumChoice");
        choiceText.SetActive(false);
        fireRateMultiplier = 1f;
        damageMultiplier = 1f;
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
        Math.Clamp(life, 0, MaxLife);
        Math.Clamp(invincibilityTime, 0, maxInvincibilityTime);
        if (invincibility) {
            invincibilityTime -= Time.deltaTime;
            if (invincibilityTime <= 0f) {
                invincibility = false;
                invincibilityTime = 0f;
            }
        }
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
        choiceText.SetActive(false);
            if (invincibilityTime <= 0f) {
                invincibility = true; 
                invincibilityTime = maxInvincibilityTime;
            } 
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
        GameObject genericInteractable = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject;
        if(Input.GetKeyDown(KeyCode.E)){
            if(droppedGun != null){
            GunObj previousGun = equippedGun;
            ChangeGun(droppedGun.gun);
            droppedGun.gun = previousGun;
            droppedGun.Change();
            }else if(chest != null && chest.isActiveAndEnabled && !chest.opened){
                chest.openChest();
            }else if(genericInteractable.tag == "Bobona" && life != MaxLife){
                life = MaxLife;
                SetLifeSlider();
                genericInteractable.GetComponent<SpriteRenderer>().color = HexToColor("#505050");
                genericInteractable.tag = "Untagged";
            }else if(genericInteractable.tag == "RH"){
                HumanResources rh = genericInteractable.GetComponent<HumanResources>();
                if(rh.enabled){
                    choiceText.SetActive(true);
                    rh.Interacted();
                }
                genericInteractable.tag = "Untagged";
            }
            else if(genericInteractable.tag == "Relic"){
                DroppedRelic relic = genericInteractable.GetComponent<DroppedRelic>();
                relic.Pick();
                if(relic.relic.relicType == relicType.Life){
                    MaxLife+=10;
                    if(life == MaxLife-10){
                        life = MaxLife;
                    }
                    SetLifeSlider();
                }else if(relic.relic.relicType == relicType.Speed){
                    speed+=3;
                }else if(relic.relic.relicType == relicType.Damage){
                    damageMultiplier=1.5f;
                }else if(relic.relic.relicType == relicType.Invincibility){
                    invincibilityTime=0.65f;
                }else if(relic.relic.relicType == relicType.Firerate){
                    fireRateMultiplier=0.85f;
                }
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
        if(invincibility == false){
            life -= damage;
            lifeSlider.value = life;
            if(life<=0){
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
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

    void SetLifeSlider(){
        lifeSlider.maxValue = MaxLife;
        lifeSlider.value = life;
    }

    Color HexToColor(string hex)
    {
        if (hex.StartsWith("#")){
            hex = hex.Substring(1);
        }
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color(r / 255f, g / 255f, b / 255f);
    }
}

