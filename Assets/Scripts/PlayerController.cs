using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    GameObject choiceText;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        equippedGun = GetComponentInChildren<Gun>().gunSettings;
        eyeRelic.SetActive(false);
        invincibility = false;
        life = MaxLife;
        // SetLifeSlider();
        // choiceText = GameObject.Find("CurriculumChoice");
        // choiceText.SetActive(false);
        fireRateMultiplier = 1f;
        damageMultiplier = 1f;
        gunRelic = false;
    }

    void Update()
    {
        if (!photonView.IsMine)
            return; // Não executa as ações do jogador se não for o jogador local

        // Verifica os inputs
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        if (inputX != 0 || inputY != 0)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        if (inputX < 0)
        {
            sprite.flipX = true;
        }
        else if (inputX > 0)
        {
            sprite.flipX = false;
        }

        Mathf.Clamp(life, 0, MaxLife);
        Mathf.Clamp(invincibilityTime, 0f, maxInvincibilityTime);
        Mathf.Clamp(damageMultiplier, 1f, 3f);
        Mathf.Clamp(fireRateMultiplier, 0.15f, 1f);

        if (invincibility)
        {
            invincibilityTime -= Time.deltaTime;
            if (invincibilityTime <= 0f)
            {
                invincibility = false;
                invincibilityTime = 0f;
            }
        }
        InteractableArea();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
            return; // Não move o jogador se não for o jogador local

        if (inputX != 0 && inputY != 0)
        {
            inputX *= 0.7f;
            inputY *= 0.7f;
        }

        Move(inputX, inputY);
    }

    void Move(float x, float y)
    {
        rb.velocity = new UnityEngine.Vector2(x * speed, y * speed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine)
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Room"))
        {
            //choiceText.SetActive(false);
            if (invincibilityTime <= 0f)
            {
                invincibility = true;
                invincibilityTime = maxInvincibilityTime;
            }

            Debug.Log(other.gameObject.name);
            StartCoroutine(FadeAndMoveCamera(other.transform.position));

            // Caso seja multiplayer, ao trocar de sala, todos os jogadores devem ir para a posição do jogador que trocou de sala
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("MoveToNewRoom", RpcTarget.All, other.transform.position);
            }
        }
    }

    [PunRPC]
    void MoveToNewRoom(Vector3 newRoomPosition)
    {
        // Sincroniza a posição de todos os jogadores para a nova sala
        transform.position = this.transform.position;

        // Mover a câmera para a nova posição
        Camera.main.transform.position = new Vector3(newRoomPosition.x - 0.33f, newRoomPosition.y, -10);
    }

    void InteractableArea()
    {
        if (!photonView.IsMine)
            return;

        bool canInteract = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable"));
        DroppedGun droppedGun = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject.GetComponent<DroppedGun>();
        Chest chest = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject.GetComponent<Chest>();
        GameObject genericInteractable = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable")).gameObject;

        AudioClip audioClip;

        if (Input.GetKeyDown(KeyCode.E))
        {
            audioSource.clip = interactClip;
            audioSource.Play();

            if (droppedGun != null)
            {
                audioClip = gunClip;
                audioSource.clip = audioClip;
                audioSource.Play();
                GunObj previousGun = equippedGun;
                ChangeGun(droppedGun.gun);
                droppedGun.gun = previousGun;
                droppedGun.Change();
            }
            else if (chest != null && chest.isActiveAndEnabled && !chest.opened)
            {
                chest.openChest();
            }
            else if (genericInteractable.tag == "Bobona" && life != MaxLife)
            {
                life = MaxLife;
                audioClip = waterClip;
                audioSource.clip = audioClip;
                audioSource.Play();
                // SetLifeSlider();
                genericInteractable.GetComponent<SpriteRenderer>().color = HexToColor("#505050");
                genericInteractable.tag = "Untagged";
            }
            else if (genericInteractable.tag == "RH")
            {
                HumanResources rh = genericInteractable.GetComponent<HumanResources>();
                if (rh.enabled)
                {
                    // choiceText.SetActive(true);
                    rh.Interacted();
                }
                genericInteractable.tag = "Untagged";
            }
            else if (genericInteractable.tag == "Relic")
            {
                audioClip = relicClip;
                DroppedRelic relic = genericInteractable.GetComponent<DroppedRelic>();
                relic.Pick();
                audioSource.clip = audioClip;
                audioSource.Play();
                HandleRelicPickup(relic);
            }
        }
    }

    void ChangeGun(GunObj gun)
    {
        equippedGun = gun;
        GetComponentInChildren<Gun>().gunSettings = gun;
        GetComponentInChildren<Gun>().Change();
    }

    public void TakeDamage(float damage)
    {
        if (!photonView.IsMine)
            return;

        if (!invincibility)
        {
            life -= damage;
            // lifeSlider.value = life;
            audioSource.clip = hitClip;
            audioSource.Play();

            if (life <= 0)
            {
                audioSource.clip = deathClip;
                audioSource.Play();
                PhotonNetwork.Destroy(gameObject); // Destroi o jogador local
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reseta a cena
            }
        }
    }

    IEnumerator FadeAndMoveCamera(UnityEngine.Vector3 newCameraPosition)
    {
       // yield return StartCoroutine(FadeImage(false));

        Camera.main.transform.position = new UnityEngine.Vector3(newCameraPosition.x - 0.33f, newCameraPosition.y, -10);

       yield break;
    }

    // IEnumerator FadeImage(bool fadeAway)
    // {
    //     if (fadeAway)
    //     {
    //         for (float i = 1; i >= 0; i -= Time.deltaTime * 5f)
    //         {
    //             fade.color = new Color(0, 0, 0, i * 1.5f);
    //             yield return null;
    //         }
    //     }
    //     else
    //     {
    //         for (float i = 0; i <= 1; i += Time.deltaTime * 5f)
    //         {
    //             fade.color = new Color(0, 0, 0, i * 1.5f);
    //             yield return null;
    //         }
    //     }
    // }

    // void SetLifeSlider()
    // {
    //     lifeSlider.maxValue = MaxLife;
    //     lifeSlider.value = life;
    // }

    Color HexToColor(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#')
            return Color.white;

        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    private void HandleRelicPickup(DroppedRelic relic)
    {
        if (relic.relic.relicType == relicType.Life)
        {
            MaxLife += 10;
            if (life == MaxLife - 10)
            {
                life = MaxLife;
            }
            //SetLifeSlider();
        }
        else if (relic.relic.relicType == relicType.Speed)
        {
            speed += 3;
        }
        else if (relic.relic.relicType == relicType.Damage)
        {
            damageMultiplier += 1.5f;
        }
        else if (relic.relic.relicType == relicType.Invincibility)
        {
            maxInvincibilityTime += 0.65f;
            invincibilityTime += 0.65f;
        }
        else if (relic.relic.relicType == relicType.Firerate)
        {
            fireRateMultiplier -= 0.15f;
        }
        else if (relic.relic.relicType == relicType.Size)
        {
            transform.localScale = new UnityEngine.Vector3(0.60f, 0.60f, 0f);
        }
        else if (relic.relic.relicType == relicType.Eye)
        {
            eyeRelic.SetActive(true);
        }
        else if (relic.relic.relicType == relicType.Gun)
        {
            GetComponentInChildren<Gun>().gunShootingStyle = shootingStyles.Spread;
            gunRelic = true;
            if (GetComponentInChildren<Gun>().nBullets == 1)
            {
                GetComponentInChildren<Gun>().nBullets = 3;
            }
            else
            {
                GetComponentInChildren<Gun>().nBullets *= 2;
            }
        }
    }
}
