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
    public float invincibilityTime;

    [Header("Multipliers")]
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;

    [Header("Relics")]
    public bool gunRelic;
    public GameObject eyeRelic;

    public bool hasSizeRelic;

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

    public FloorUIController floorUI;

    #endregion

    #region Unity Callbacks

    void Start()
    {
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

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Room"))
        {
            Camera.main.transform.position = new Vector3(other.transform.position.x - 0.32f, other.transform.position.y, -10f);
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
        if (GameManager.Instance.floorLevel == 1)
        {
            life = MaxLife;
            eyeRelic.SetActive(false);

        }
        else
        {
            MaxLife = GameManager.Instance.maxLife;
            life = GameManager.Instance.life;
            speed = GameManager.Instance.speed;
            damageMultiplier = GameManager.Instance.damageMultiplier;
            fireRateMultiplier = GameManager.Instance.fireRateMultiplier;
            maxInvincibilityTime = GameManager.Instance.maxInvincibilityTime;
            invincibilityTime = GameManager.Instance.invincibilityTime;
            hasSizeRelic = GameManager.Instance.hasSizeRelic;
            gunRelic = GameManager.Instance.gunRelic;

            eyeRelic.SetActive(GameManager.Instance.hasEyeRelic);

        }

        SetLifeSlider();

        if (choiceText != null)
            choiceText.SetActive(false);

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
        StartCoroutine(DamageFlash());

        CameraShake.Instance.TriggerShake(0.15f, 0.1f);

        Vector2 knockbackDir = -rb.velocity.normalized;
        rb.AddForce(knockbackDir * 200f, ForceMode2D.Impulse);

        if (life <= 0f)
        {
            audioSource.PlayOneShot(deathClip);
            GameManager.Instance.ResetProgress();
            SceneManager.LoadScene("Menu");
        }
    }

    IEnumerator DamageFlash()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
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
            CameraShake.Instance.TriggerShake(0.05f, 0.02f);

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
                GameManager.Instance.maxLife += 10;
                GameManager.Instance.life = GameManager.Instance.maxLife;
                MaxLife = GameManager.Instance.maxLife;
                life = GameManager.Instance.maxLife;
                SetLifeSlider();
                break;

            case relicType.Speed:
                GameManager.Instance.speed += 3f;
                speed = GameManager.Instance.speed;
                break;

            case relicType.Damage:
                GameManager.Instance.damageMultiplier += 1.5f;
                damageMultiplier = GameManager.Instance.damageMultiplier;
                break;

            case relicType.Invincibility:
                GameManager.Instance.maxInvincibilityTime += 0.65f;
                GameManager.Instance.invincibilityTime += 0.65f;
                maxInvincibilityTime = GameManager.Instance.maxInvincibilityTime;
                invincibilityTime = GameManager.Instance.invincibilityTime;
                break;

            case relicType.Firerate:
                GameManager.Instance.fireRateMultiplier -= 0.15f;
                fireRateMultiplier = GameManager.Instance.fireRateMultiplier;
                break;

            case relicType.Size:
                hasSizeRelic = true;
                GameManager.Instance.hasSizeRelic = true;
                transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                break;

            case relicType.Eye:
                GameManager.Instance.hasEyeRelic = true;
                eyeRelic.SetActive(true);
                break;

            case relicType.Gun:
                GameManager.Instance.gunRelic = true;
                damageMultiplier = damageMultiplier - (damageMultiplier * 0.25f);
                GameManager.Instance.damageMultiplier = GameManager.Instance.damageMultiplier - (GameManager.Instance.damageMultiplier * 0.25f);
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

        Vector3 startPos = Camera.main.transform.position;
        Vector3 endPos = new Vector3(targetPos.x - 0.32f, targetPos.y, -10f);
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        Camera.main.transform.position = endPos;

        yield return StartCoroutine(FadeImage(false));
    }
    IEnumerator FadeImage(bool fadeAway)
    {
        float start = fadeAway ? 0f : 1f;
        float end = fadeAway ? 1f : 0f;
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Lerp(start, end, elapsed / duration);
            fade.color = new Color(0, 0, 0, t);
            yield return null;
        }

        fade.color = new Color(0, 0, 0, end);
    }

    #endregion
}
