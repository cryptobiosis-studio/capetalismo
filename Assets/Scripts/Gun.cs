using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float offset; // Ajuste da rotação da arma
    public SpriteRenderer sprRen; // Sprite da arma
    public GunObj gunSettings; // Configurações da arma
    public GameObject bullet; // Prefab da bala
    public Transform pointer; // Ponto de saída da bala

    void Start()
    {
        sprRen = GetComponent<SpriteRenderer>();
        sprRen.sprite = gunSettings.gunSprite;
        sprRen.flipX = false; // Manter a orientação da arma
    }

    void Update()
    {
        RotateGun();
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(Fire());
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

   IEnumerator Fire()
  {
    yield return new WaitForSeconds(gunSettings.firerate);

    // Posição do mouse na tela
    Vector3 mousePos = Input.mousePosition;
    mousePos.z = 5.23f; // Distância da câmera

    // Converte a posição do mouse para coordenadas do mundo
    Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

    // Calcula a direção da bala
    Vector3 direction = (worldMousePos - pointer.position).normalized; // Normaliza a direção

    // Cria a bala na posição do pointer
    GameObject _bullet = Instantiate(bullet, pointer.position, Quaternion.identity);
    
    // Configura a rotação da bala para seguir a direção do mouse
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    _bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

    // Obtém o Rigidbody2D da bala
    Rigidbody2D rb = _bullet.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        // Aplica a velocidade na direção, usando uma velocidade constante
        rb.velocity = direction.normalized * gunSettings.bulletSpeed; // Define a velocidade
    }

    // Configura a sprite da bala
    Bullet bulScr = _bullet.GetComponent<Bullet>();
    bulScr.bulSprite = gunSettings.bulletSprite;
    bulScr.damage = gunSettings.damage;

     yield return new WaitForSeconds(gunSettings.firerate);

  }


}