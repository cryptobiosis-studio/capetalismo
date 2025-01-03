using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public float speed;
    public float life;
    public float MaxLife;
    private Rigidbody2D rb;
    private float inputX;
    private float inputY;
    public Animator anim;
    public SpriteRenderer sprite;

    public SpriteRenderer fade;

    public GunObj equippedGun;

    public bool invincibility;
    public float invincibilityTime;
    public float maxInvincibilityTime;

    public GameObject choiceText;

    public Slider lifeSlider;

    public float damageMultiplier;
    public float fireRateMultiplier;

    public bool gunRelic;

    public GameObject eyeRelic;
    public AudioSource audioSource;
    public AudioClip relicClip;
    public AudioClip gunClip;
    public AudioClip hitClip;
    public AudioClip waterClip;
    public AudioClip interactClip;
    public AudioClip deathClip;

    public bool isSinglePlayer;


    void Start()
    {      
        isSinglePlayer = !PhotonNetwork.IsConnected;
        if(GetComponent<PhotonView>() == null){
            isSinglePlayer = true;
        }
        if(!isSinglePlayer){
            DontDestroyOnLoad(this.gameObject);
        }
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        equippedGun = GetComponentInChildren<Gun>().gunSettings;
        eyeRelic.SetActive(false);
        invincibility = false;
        life = MaxLife;
        fireRateMultiplier = 1f;
        damageMultiplier = 1f;
        choiceText = GameObject.Find("CurriculumChoice");
        choiceText.SetActive(false);
        lifeSlider = GameObject.Find("PlayerLifeSlider").GetComponent<Slider>();
        SetLifeSlider();
        gunRelic = false;
        if(isSinglePlayer){
            SetLifeSlider();
            choiceText = GameObject.Find("CurriculumChoice");
            choiceText.SetActive(false);
        }
        if (!isSinglePlayer && !photonView.IsMine){
            this.enabled = false;
            return;
        }
    }

    void Update()
    {   
        if(choiceText == null){
            choiceText = GameObject.Find("CurriculumChoice");
        }
        if(lifeSlider == null){
            lifeSlider = GameObject.Find("PlayerLifeSlider").GetComponent<Slider>();
            SetLifeSlider();
        }
        
        if (!photonView.IsMine){
            sprite.enabled = true;
        }
        else{
            sprite.enabled = true;
        }
        if (!photonView.IsMine && !isSinglePlayer)
            return; // Não executa as ações do jogador se não for o jogador local

        // Verifica os inputs
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        if (inputX != 0 || inputY != 0){
            anim.SetBool("isMoving", true);
        }
        else{
            anim.SetBool("isMoving", false);
        }

        if (inputX < 0){
            sprite.flipX = true;
        }
        else if (inputX > 0){
            sprite.flipX = false;
        }

        Mathf.Clamp(life, 0, MaxLife);
        Mathf.Clamp(invincibilityTime, 0f, maxInvincibilityTime);
        Mathf.Clamp(damageMultiplier, 1f, 3f);
        Mathf.Clamp(fireRateMultiplier, 0.15f, 1f);

        if (invincibility){
            invincibilityTime -= Time.deltaTime;
            if (invincibilityTime <= 0f){
                invincibility = false;
                invincibilityTime = 0f;
            }
        }
        InteractableArea();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine && !isSinglePlayer)
            return; // Não move o jogador se não for o jogador local

        if (inputX != 0 && inputY != 0){
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
        if (!photonView.IsMine && !isSinglePlayer)
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Room")){
            choiceText.SetActive(false);
            if (invincibilityTime <= 0f){
                invincibility = true;
                invincibilityTime = maxInvincibilityTime;
            }

            Debug.Log(other.gameObject.name);
            StartCoroutine(FadeAndMoveCamera(other.transform.position));
        }
    }



    void InteractableArea()
    {
        if (!photonView.IsMine && !isSinglePlayer)
            return;

        bool canInteract = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable"));
        DroppedGun droppedGun = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject.GetComponent<DroppedGun>();
        Chest chest = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject.GetComponent<Chest>();
        GameObject genericInteractable = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject;

        AudioClip audioClip;

        if (Input.GetKeyDown(KeyCode.E)){
            audioSource.clip = interactClip;
            audioSource.Play();

            if (droppedGun != null){
                audioClip = gunClip;
                audioSource.clip = audioClip;
                audioSource.Play();
                GunObj previousGun = equippedGun;
                ChangeGun(droppedGun.gun);
                droppedGun.gun = previousGun;
                droppedGun.Change();
            }
            else if (chest != null && chest.isActiveAndEnabled && !chest.opened){
                chest.openChest();
            }
            else if (genericInteractable.tag == "Bobona" && life != MaxLife){
                life = MaxLife;
                audioClip = waterClip;
                audioSource.clip = audioClip;
                audioSource.Play();
                SetLifeSlider();
                genericInteractable.GetComponent<SpriteRenderer>().color = HexToColor("#505050");
                genericInteractable.tag = "Untagged";
            }
            else if (genericInteractable.tag == "RH"){
                HumanResources rh = genericInteractable.GetComponent<HumanResources>();
                if (rh.enabled){
                    choiceText.SetActive(true);
                    rh.Interacted();
                }
                genericInteractable.tag = "Untagged";
            }
            else if (genericInteractable.tag == "Relic"){
                audioClip = relicClip;
                DroppedRelic relic = genericInteractable.GetComponent<DroppedRelic>();
                relic.Pick();
                audioSource.clip = audioClip;
                audioSource.Play();
                HandleRelicPickup(relic);
            }else if(genericInteractable.tag == "Elevator"){
                transform.position = new Vector3(-501.14f, -3.32f);
                Camera.main.orthographicSize = 6.683172f;
                
            }
        }
    }

    void ChangeGun(GunObj gun){
        equippedGun = gun;
        GetComponentInChildren<Gun>().gunSettings = gun;
        if(isSinglePlayer){
            GetComponentInChildren<Gun>().Change();
        }else{
            GetComponentInChildren<Gun>().Change();
        }
        
    }

    public void TakeDamage(float damage){
        if (!photonView.IsMine && !isSinglePlayer)
            return;

        if (!invincibility){
            life -= damage;
            lifeSlider.value = life;
            audioSource.clip = hitClip;
            audioSource.Play();

            if (life <= 0){
                audioSource.clip = deathClip;
                audioSource.Play();
                if(!isSinglePlayer){
                    PhotonNetwork.Disconnect();
                    SceneManager.LoadScene("Menu");
                    StartCoroutine(LoadMenuAfterDisconnect()); 
                }else{
                    Destroy(this.gameObject, 2f);
                    SceneManager.LoadScene("Menu");     
                }
                
            }
        }
    }


    IEnumerator FadeAndMoveCamera(UnityEngine.Vector3 newCameraPosition)
    {   
        if(isSinglePlayer){
            yield return StartCoroutine(FadeImage(true));

        }
       
        Camera.main.transform.position = new UnityEngine.Vector3(newCameraPosition.x - 0.33f, newCameraPosition.y, -10);

       yield break;
    }
    private IEnumerator LoadMenuAfterDisconnect(){
        SceneManager.LoadScene("Menu");
        while (PhotonNetwork.IsConnected){
            yield return null; 
        }
    }

    IEnumerator FadeImage(bool fadeAway){
        if (fadeAway){
            for (float i = 1; i >= 0; i -= Time.deltaTime * 5f){
                fade.color = new Color(0, 0, 0, i * 1.5f);
                yield return null;
            }
        }
        else{
            for (float i = 0; i <= 1; i += Time.deltaTime * 5f){
                fade.color = new Color(0, 0, 0, i * 1.5f);
                yield return null;
            }
        }
    }

    void SetLifeSlider(){
        lifeSlider.maxValue = MaxLife;
        lifeSlider.value = life;
    }

    Color HexToColor(string hex){
        if (hex.Length != 7 || hex[0] != '#')
            return Color.white;

        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    private void HandleRelicPickup(DroppedRelic relic){
        if (relic.relic.relicType == relicType.Life){
            MaxLife += 10;
            if (life == MaxLife - 10){
                life = MaxLife;
            }
            if(isSinglePlayer){
                SetLifeSlider();
            }
            
        }
        else if (relic.relic.relicType == relicType.Speed){
            speed += 3;
        }
        else if (relic.relic.relicType == relicType.Damage){
            damageMultiplier += 1.5f;
        }
        else if (relic.relic.relicType == relicType.Invincibility){
            maxInvincibilityTime += 0.65f;
            invincibilityTime += 0.65f;
        }
        else if (relic.relic.relicType == relicType.Firerate){
            fireRateMultiplier -= 0.15f;
        }
        else if (relic.relic.relicType == relicType.Size){
            transform.localScale = new UnityEngine.Vector3(0.60f, 0.60f, 0f);
        }
        else if (relic.relic.relicType == relicType.Eye){
            eyeRelic.SetActive(true);
        }
        else if (relic.relic.relicType == relicType.Gun){
            GetComponentInChildren<Gun>().gunShootingStyle = shootingStyles.Spread;
            gunRelic = true;
            if (GetComponentInChildren<Gun>().nBullets == 1){
                GetComponentInChildren<Gun>().nBullets = 3;
            }
            else{
                GetComponentInChildren<Gun>().nBullets *= 2;
            }
        }
    }
}
