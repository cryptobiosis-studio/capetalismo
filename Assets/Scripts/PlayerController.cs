using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region Inspector Fields

    [Header("Movement")]
    public float speed;

    [Header("Health")]
    public float MaxLife;
    [HideInInspector] public float life;
    public Slider lifeSlider;

    [Header("Animation & Sprite")]
    public Animator anim;
    public SpriteRenderer sprite;
    public SpriteRenderer fade;

    [Header("Gun")]
    public GunObj equippedGun;

    [Header("Invincibility")]
    public bool invincibility;
    public float maxInvincibilityTime;
    private float invincibilityTime;

    [Header("Multipliers")]
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;

    [Header("Relics")]
    public bool gunRelic;
    public GameObject eyeRelic;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip relicClip;
    public AudioClip gunClip;
    public AudioClip hitClip;
    public AudioClip waterClip;
    public AudioClip interactClip;
    public AudioClip deathClip;

    [Header("UI")]
    [SerializeField] private GameObject choiceText;

    #endregion

    #region Private Variables

    private Rigidbody2D rb;
    private float inputX;
    private float inputY;

    [SerializeField] private GunObj initialGun;

    #endregion

    #region Unity Callbacks

    void Start()
    {
        InitializeComponents();
        InitializeComponents();
        if (GameManager.Instance != null && GameManager.Instance.life > 0)
        {
            GameManager.Instance.LoadPlayerData(this);
        }
        else
        {
            SetupInitialValues();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateAnimationAndSprite();
        ClampValues();
        HandleInvincibility();
        HandleInteractions();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Room"))
        {
            choiceText?.SetActive(false);
            TriggerInvincibility();
            StartCoroutine(FadeAndMoveCamera(other.transform.position));
        }
    }

    #endregion

    #region Initialization

    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        equippedGun = GetComponentInChildren<Gun>().gunSettings;
    }

    void SetupInitialValues()
    {
        life = MaxLife;
        SetLifeSlider();
        life = MaxLife;
        SetLifeSlider();

        if (eyeRelic != null)
            eyeRelic.SetActive(false);

        if (choiceText != null)
            choiceText.SetActive(false);
        else
            Debug.LogWarning("choiceText não encontrado!");
        if (equippedGun == null)
        {
            ChangeGun(initialGun);
        }

    }

    #endregion

    #region Input & Movement

    void HandleInput()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
    }

    void ApplyMovement()
    {
        float x = inputX;
        float y = inputY;

        if (inputX != 0f && inputY != 0f)
        {
            x *= 0.7f;
            y *= 0.7f;
        }

        rb.velocity = new Vector2(x * speed, y * speed);
    }

    void UpdateAnimationAndSprite()
    {
        anim.SetBool("isMoving", inputX != 0f || inputY != 0f);
        sprite.flipX = inputX < 0f;
    }

    #endregion

    #region Health & Damage

    public void TakeDamage(float damage)
    {
        if (invincibility) return;

        life -= damage;
        SetLifeSlider();
        audioSource.PlayOneShot(hitClip);

        if (life <= 0f)
        {
            audioSource.PlayOneShot(deathClip);
            GameManager.Instance.ResetProgress();
            SceneManager.LoadScene("Menu");
        }
    }

    void SetLifeSlider()
    {
        if (lifeSlider == null) return;
        lifeSlider.maxValue = MaxLife;
        lifeSlider.value = life;
    }

    #endregion

    #region Gun Management

    public void ChangeGun(GunObj gun)
    {
        equippedGun = gun;
        var gunComp = GetComponentInChildren<Gun>();
        gunComp.gunSettings = gun;
        gunComp.Change();
    }

    #endregion

    #region Interaction

    void HandleInteractions()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable"));
        if (col == null) return;

        GameObject obj = col.gameObject;

        if (Input.GetKeyDown(KeyCode.E))
        {
            audioSource.PlayOneShot(interactClip);

            if (obj.TryGetComponent(out DroppedGun droppedGun))
            {
                audioSource.PlayOneShot(gunClip);
                GunObj previous = equippedGun;
                ChangeGun(droppedGun.gun);
                droppedGun.gun = previous;
                droppedGun.Change();
                return;
            }

            if (obj.TryGetComponent(out Chest chest) && chest.enabled && !chest.opened)
            {
                chest.openChest();
                return;
            }

            if (obj.CompareTag("Bobona") && life < MaxLife)
            {
                life = MaxLife;
                SetLifeSlider();
                audioSource.PlayOneShot(waterClip);
                obj.GetComponent<SpriteRenderer>().color = HexToColor("#505050");
                obj.tag = "Untagged";
                return;
            }

            if (obj.CompareTag("RH") && obj.TryGetComponent(out HumanResources rh))
            {
                if (rh.enabled)
                {
                    choiceText?.SetActive(true);
                    rh.Interacted();
                }
                obj.tag = "Untagged";
                return;
            }

            if (obj.CompareTag("Relic") && obj.TryGetComponent(out DroppedRelic relic))
            {
                audioSource.PlayOneShot(relicClip);
                relic.Pick();
                HandleRelicPickup(relic);
                return;
            }

            if (obj.CompareTag("Elevator"))
            {
                GameManager.Instance.SavePlayerData(this);
                GameManager.Instance.NextFloor();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

        }
    }

    #endregion

    #region Relic Effects

    void HandleRelicPickup(DroppedRelic relic)
    {
        switch (relic.relic.relicType)
        {
            case relicType.Life:
                MaxLife += 10;
                life = MaxLife;
                SetLifeSlider();
                break;
            case relicType.Speed:
                speed += 3f;
                break;
            case relicType.Damage:
                damageMultiplier += 1.5f;
                break;
            case relicType.Invincibility:
                maxInvincibilityTime += 0.65f;
                invincibilityTime += 0.65f;
                break;
            case relicType.Firerate:
                fireRateMultiplier -= 0.15f;
                break;
            case relicType.Size:
                transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                break;
            case relicType.Eye:
                eyeRelic.SetActive(true);
                break;
            case relicType.Gun:
                var gunComp = GetComponentInChildren<Gun>();
                gunComp.gunShootingStyle = shootingStyles.Spread;
                gunRelic = true;
                gunComp.nBullets = (gunComp.nBullets > 1) ? gunComp.nBullets * 2 : 3;
                break;
        }
    }

    #endregion

    #region Invincibility

    void TriggerInvincibility()
    {
        if (invincibilityTime <= 0f)
        {
            invincibility = true;
            invincibilityTime = maxInvincibilityTime;
        }
    }

    void HandleInvincibility()
    {
        if (invincibility)
        {
            invincibilityTime -= Time.deltaTime;
            if (invincibilityTime <= 0f)
            {
                invincibility = false;
                invincibilityTime = 0f;
            }
        }
    }

    #endregion

    #region Utilities

    void ClampValues()
    {
        life = Mathf.Clamp(life, 0f, MaxLife);
        invincibilityTime = Mathf.Clamp(invincibilityTime, 0f, maxInvincibilityTime);
        damageMultiplier = Mathf.Clamp(damageMultiplier, 1f, 3f);
        fireRateMultiplier = Mathf.Clamp(fireRateMultiplier, 0.15f, 1f);
    }

    Color HexToColor(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#') return Color.white;
        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    IEnumerator FadeAndMoveCamera(Vector3 targetPos)
    {
        yield return StartCoroutine(FadeImage(true));
        Camera.main.transform.position = new Vector3(targetPos.x - 0.33f, targetPos.y, -10f);
    }

    IEnumerator FadeImage(bool fadeAway)
    {
        float start = fadeAway ? 1f : 0f;
        float end = fadeAway ? 0f : 1f;
        for (float t = start; fadeAway ? t >= end : t <= end; t += (fadeAway ? -1 : 1) * Time.deltaTime * 5f)
        {
            fade.color = new Color(0, 0, 0, t * 1.5f);
            yield return null;
        }
    }

    #endregion
}
