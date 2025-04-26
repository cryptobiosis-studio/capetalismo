using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float offset; // Ajuste da rotação da arma
    public SpriteRenderer sprRen; // Sprite da arma
    public GunObj gunSettings; // Configurações da arma
    public GameObject bullet; // Prefab da bala
    public Transform pointer; // Ponto de saída da bala
    private float fireTimer;
    List<GameObject> firedBullets = new List<GameObject>();

    public PlayerController player;
    public AudioSource audioSource;
    public AudioClip shootSound;

    public shootingStyles gunShootingStyle;
    public int nBullets;

    void Start()
    {
        sprRen = GetComponent<SpriteRenderer>();
        sprRen.sprite = gunSettings.gunSprite;
        sprRen.flipX = false;
        player = GetComponentInParent<PlayerController>();
        gunShootingStyle = gunSettings.shootingStyle;
        pointer = transform.Find("SpawnerC");
        nBullets = gunSettings.numberOfBullets;
    }

    void Update()
    {
        RotateGun();

        if (Input.GetKeyDown(KeyCode.Mouse0) && fireTimer <= 0f)
        {
            Fire();
            fireTimer = gunSettings.firerate * player.fireRateMultiplier;
        }
        else
        {
            fireTimer -= Time.deltaTime;
        }
    }

    void RotateGun()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 diff = worldPos - transform.position;
        float rotZ = Mathf.Atan2(-diff.y, -diff.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
        sprRen.flipY = rotZ < 85f && rotZ > -85f;
    }

    void Fire()
    {
        audioSource.clip = shootSound;
        audioSource.Play();

        if (gunShootingStyle == shootingStyles.Spread)
            FireSpread();
        else
            FireSimple();
    }

    void FireSpread()
    {
        firedBullets.Clear();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 baseDir = (mouseWorld - pointer.position).normalized;

        int numberOfBullets = nBullets;
        float spreadAngle = 35f;
        float angleStep = spreadAngle / (numberOfBullets - 1);

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = -spreadAngle / 2f + angleStep * i;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, baseAngle - 90f + angle);

            GameObject _bullet = Instantiate(bullet, pointer.position, rot);
            firedBullets.Add(_bullet);

            Rigidbody2D rb = _bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = _bullet.transform.up * gunSettings.bulletSpeed;
        }

        ApplyBulletSettings(firedBullets.ToArray());
    }

    void FireSimple()
    {
        firedBullets.Clear();
        GameObject _bullet = Instantiate(bullet, pointer.position, Quaternion.identity);
        firedBullets.Add(_bullet);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - pointer.position).normalized;

        Rigidbody2D rb = _bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * gunSettings.bulletSpeed;

        ApplyBulletSettings(firedBullets.ToArray());
    }

    void ApplyBulletSettings(GameObject[] bullets)
    {
        foreach (GameObject b in bullets)
        {
            Bullet bul = b.GetComponent<Bullet>();
            bul.bulSprite = gunSettings.bulletSprite;
            bul.damage = gunSettings.damage * player.damageMultiplier;
        }
    }

    public void Change()
    {
        sprRen.sprite = gunSettings.gunSprite;
        nBullets = gunSettings.numberOfBullets;
        gunShootingStyle = gunSettings.shootingStyle;

        if (player.gunRelic)
        {
            gunShootingStyle = shootingStyles.Spread;
            nBullets = (nBullets > 1) ? nBullets * 2 : 3;
        }
    }
}
