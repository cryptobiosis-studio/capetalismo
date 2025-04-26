using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float speed;

    [Header("Vida")]
    public float MaxLife;
    [HideInInspector] public float life;
    public Slider lifeSlider;

    [Header("Animação e Sprite")]
    public Animator anim;
    public SpriteRenderer sprite;
    public SpriteRenderer fade;

    [Header("Arma")]
    public GunObj equippedGun;

    [Header("Invencibilidade")]
    public bool invincibility;
    public float maxInvincibilityTime;
    private float invincibilityTime;

    [Header("Multiplicadores")]
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;

    [Header("Relics")]
    public bool gunRelic;
    public GameObject eyeRelic;

    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip relicClip;
    public AudioClip gunClip;
    public AudioClip hitClip;
    public AudioClip waterClip;
    public AudioClip interactClip;
    public AudioClip deathClip;

    [Header("UI")]
    public GameObject choiceText;

    private Rigidbody2D rb;
    private float inputX;
    private float inputY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        life = MaxLife;
        SetLifeSlider();

        equippedGun = GetComponentInChildren<Gun>().gunSettings;
        eyeRelic.SetActive(false);
        choiceText = GameObject.Find("CurriculumChoice");
        choiceText?.SetActive(false);
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        anim.SetBool("isMoving", inputX != 0f || inputY != 0f);
        sprite.flipX = inputX < 0f;

        life = Mathf.Clamp(life, 0f, MaxLife);
        invincibilityTime = Mathf.Clamp(invincibilityTime, 0f, maxInvincibilityTime);
        damageMultiplier = Mathf.Clamp(damageMultiplier, 1f, 3f);
        fireRateMultiplier = Mathf.Clamp(fireRateMultiplier, 0.15f, 1f);

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
        float x = inputX, y = inputY;
        if (inputX != 0f && inputY != 0f)
        {
            x *= 0.7f;
            y *= 0.7f;
        }
        Move(x, y);
    }

    void Move(float x, float y)
    {
        rb.velocity = new Vector2(x * speed, y * speed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Room"))
        {
            choiceText?.SetActive(false);
            if (invincibilityTime <= 0f)
            {
                invincibility = true;
                invincibilityTime = maxInvincibilityTime;
            }
            StartCoroutine(FadeAndMoveCamera(other.transform.position));
        }
    }

    void InteractableArea()
    {
        bool hit = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable"));
        if (!hit) return;

        Collider2D col = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Interactable"));
        GameObject obj = col.gameObject;

        if (Input.GetKeyDown(KeyCode.E))
        {
            audioSource.PlayOneShot(interactClip);

            var droppedGun = obj.GetComponent<DroppedGun>();
            if (droppedGun)
            {
                audioSource.PlayOneShot(gunClip);
                GunObj previous = equippedGun;
                ChangeGun(droppedGun.gun);
                droppedGun.gun = previous;
                droppedGun.Change();
                return;
            }

            var chest = obj.GetComponent<Chest>();
            if (chest && chest.enabled && !chest.opened)
            {
                chest.openChest();
                return;
            }

            if (obj.CompareTag("Bobona") && life < MaxLife)
            {
                life = MaxLife;
                audioSource.PlayOneShot(waterClip);
                SetLifeSlider();
                obj.GetComponent<SpriteRenderer>().color = HexToColor("#505050");
                obj.tag = "Untagged";
                return;
            }

            if (obj.CompareTag("RH"))
            {
                var rh = obj.GetComponent<HumanResources>();
                if (rh && rh.enabled)
                {
                    choiceText.SetActive(true);
                    rh.Interacted();
                }
                obj.tag = "Untagged";
                return;
            }

            if (obj.CompareTag("Relic"))
            {
                var relic = obj.GetComponent<DroppedRelic>();
                audioSource.PlayOneShot(relicClip);
                relic.Pick();
                HandleRelicPickup(relic);
                return;
            }

            if (obj.CompareTag("Elevator"))
            {
                transform.position = new Vector3(-501.14f, -3.32f);
                Camera.main.orthographicSize = 6.683172f;
            }
        }
    }

    void ChangeGun(GunObj gun)
    {
        equippedGun = gun;
        var gunComp = GetComponentInChildren<Gun>();
        gunComp.gunSettings = gun;
        gunComp.Change();
    }

    public void TakeDamage(float damage)
    {
        if (invincibility) return;

        life -= damage;
        lifeSlider.value = life;
        audioSource.PlayOneShot(hitClip);

        if (life <= 0f)
        {
            audioSource.PlayOneShot(deathClip);
            Destroy(gameObject, 2f);
            SceneManager.LoadScene("Menu");
        }
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

    void SetLifeSlider()
    {
        if (lifeSlider == null) return;
        lifeSlider.maxValue = MaxLife;
        lifeSlider.value = life;
    }

    Color HexToColor(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#') return Color.white;
        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

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
}
