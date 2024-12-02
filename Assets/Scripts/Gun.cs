using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Gun : MonoBehaviourPunCallbacks
{
    public float offset; // Ajuste da rotação da arma
    public SpriteRenderer sprRen; // Sprite da arma
    public GunObj gunSettings; // Configurações da arma
    public GameObject bullet; // Prefab da bala
    public Transform pointer; // Ponto de saída da bala
    private float fireTimer;
    List<GameObject> firedBullets = new List<GameObject> { };

    public PlayerController player;

    public AudioSource audioSource;
    public AudioClip shootSound;

    public shootingStyles gunShootingStyle;
    public int nBullets;

    void Start(){
        sprRen = GetComponent<SpriteRenderer>();
        sprRen.sprite = gunSettings.gunSprite;
        sprRen.flipX = false; // Manter a orientação da arma
        player = GetComponentInParent<PlayerController>();
        gunShootingStyle = gunSettings.shootingStyle;
        nBullets = gunSettings.numberOfBullets;
    }

    void Update(){
        RotateGun();

        // Verifica se o jogador pressionou o botão de disparo (Mouse0)
        if (Input.GetKeyDown(KeyCode.Mouse0) && fireTimer <= 0f)
        {
            if (photonView.IsMine)  // Verifique se o PhotonView é do jogador local
            {
                photonView.RPC("Fire", RpcTarget.All);  // Chamando o RPC para todos os clientes
                fireTimer = gunSettings.firerate * player.fireRateMultiplier;
            }
        }
        else
        {
            fireTimer -= Time.deltaTime;
        }
    }

    void RotateGun()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f; // Distância da câmera

        Vector3 difference = Camera.main.ScreenToWorldPoint(mousePos) - transform.position;
        float rotZ = Mathf.Atan2(-difference.y, -difference.x) * Mathf.Rad2Deg;

        // Aplica a rotação com o offset
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);

        // Verifica a rotação para flipar o sprite
        sprRen.flipY = rotZ < 85 && rotZ > -85;
    }

    [PunRPC]  // Certifique-se de que o método está marcado com [PunRPC]
    void Fire()
    {
        audioSource.clip = shootSound;
        audioSource.Play();

        // Dispara de acordo com o estilo de tiro
        if (gunShootingStyle == shootingStyles.Spread)
        {
            FireSpread();
        }
        else
        {
            FireSimple();
        }
    }

    void FireSpread()
    {
        firedBullets.Clear();
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 directionToMouse = (mousePosition - pointer.position).normalized;

        int numberOfBullets = nBullets;
        float spreadAngle = 35f;
        float angleIncrement = spreadAngle / (numberOfBullets - 1);

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = -spreadAngle / 2 + angleIncrement * i;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90 + angle);
            GameObject _bullet = Instantiate(bullet, pointer.position, bulletRotation);
            firedBullets.Add(_bullet);
            Vector3 bulletDirection = _bullet.transform.up; 
            _bullet.GetComponent<Rigidbody2D>().velocity = bulletDirection * gunSettings.bulletSpeed;
        }
        BulletSettings(firedBullets.ToArray());
    }

    void FireSimple()
    {
        firedBullets.Clear();
        GameObject _bullet = Instantiate(bullet, pointer.position, Quaternion.identity);
        firedBullets.Add(_bullet);
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 directionToMouse = (mousePosition - pointer.position).normalized;

        _bullet.GetComponent<Rigidbody2D>().velocity = directionToMouse * gunSettings.bulletSpeed;
        BulletSettings(firedBullets.ToArray());
    }

    void BulletSettings(GameObject[] bullets)
    {
        foreach (GameObject b in bullets)
        {
            Bullet bulScr = b.GetComponent<Bullet>();
            bulScr.bulSprite = gunSettings.bulletSprite;
            bulScr.damage = gunSettings.damage * player.damageMultiplier;
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
            if (nBullets != 1)
            {
                nBullets = nBullets * 2;
            }
            else
            {
                nBullets = 3;
            }
        }
    }
}
