using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public float life;
    public float maxLife;
    public float speed;
    public float damageMultiplier;
    public float fireRateMultiplier;
    public bool gunRelic;
    public bool eyeRelicActive;
    public GunObj equippedGun;

    public GunObj initialGun;

    [Header("Game Progression")]
    public int floorLevel = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerData(PlayerController player)
    {
        life = player.life;
        maxLife = player.MaxLife;
        speed = player.speed;
        damageMultiplier = player.damageMultiplier;
        fireRateMultiplier = player.fireRateMultiplier;
        gunRelic = player.gunRelic;
        eyeRelicActive = player.eyeRelic.activeSelf;
        equippedGun = player.equippedGun;
    }

    public void LoadPlayerData(PlayerController player)
    {
        player.life = life;
        player.MaxLife = maxLife;
        player.speed = speed;
        player.damageMultiplier = damageMultiplier;
        player.fireRateMultiplier = fireRateMultiplier;
        player.gunRelic = gunRelic;
        player.eyeRelic.SetActive(eyeRelicActive);
        player.ChangeGun(equippedGun);
    }

    public void NextFloor()
    {
        floorLevel++;
        Debug.Log("Floor level increased to: " + floorLevel);
    }

    public void ResetProgress()
    {
        life = 25;
        maxLife = 15;
        speed = 7f;
        damageMultiplier = 1f;
        fireRateMultiplier = 1f;
        gunRelic = false;
        eyeRelicActive = false;
        equippedGun = initialGun;
        floorLevel = 1;
        Debug.Log("Progress reset, floor level back to 1");
    }
}
