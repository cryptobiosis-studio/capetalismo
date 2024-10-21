using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float offset; // Ajuste da rotação da arma
    public SpriteRenderer sprRen; // Sprite da arma
    public GunObj gunSettings; // Configurações da arma
    public GameObject bullet; // Prefab da bala
    public Transform pointer; // Ponto de saída da bala

    List<GameObject> firedBullets = new List<GameObject> { };

    void Start(){
        sprRen = GetComponent<SpriteRenderer>();
        sprRen.sprite = gunSettings.gunSprite;
        sprRen.flipX = false; // Manter a orientação da arma
    }

    void Update(){
        RotateGun();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(Fire());
        }
    }

    void RotateGun(){
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f; // Distância da câmera

        Vector3 difference = Camera.main.ScreenToWorldPoint(mousePos) - transform.position;
        float rotZ = Mathf.Atan2(-difference.y, -difference.x) * Mathf.Rad2Deg;

        // Aplica a rotação com o offset
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);

        // Verifica a rotação para flipar o sprite
        sprRen.flipY = rotZ < 85 && rotZ > -85;
    }

    IEnumerator Fire(){
        yield return new WaitForSeconds(gunSettings.firerate);
        if (gunSettings.shootingStyle == shootingStyles.Spread)
        {
            Debug.Log("Spread");
            FireSpread();
        }
        else
        {
            Debug.Log("Simple");
            FireSimple();
        }

        yield return new WaitForSeconds(gunSettings.firerate);
    }

    void FireSpread(){
    firedBullets.Clear();
    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector3 directionToMouse = (mousePosition - pointer.position).normalized;

    int numberOfBullets = gunSettings.numberOfBullets;
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


    void FireSimple(){
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
            bulScr.damage = gunSettings.damage;
        }
    }

    public void Change(){
      sprRen.sprite = gunSettings.gunSprite;
    }

}
