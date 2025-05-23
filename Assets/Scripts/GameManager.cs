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
    public float maxInvincibilityTime;
    public float invincibilityTime;

    public bool gunRelic;
    public bool hasEyeRelic;
    public bool hasSizeRelic;

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
        maxInvincibilityTime = player.maxInvincibilityTime;
        invincibilityTime = player.invincibilityTime;
        gunRelic = player.gunRelic;
        hasEyeRelic = player.eyeRelic.activeSelf;
        hasSizeRelic = player.transform.localScale.x < 0.85f;
        equippedGun = player.equippedGun;
    }

    public void LoadPlayerData(PlayerController player)
    {
        player.life = life;
        player.MaxLife = maxLife;
        player.speed = speed;
        player.damageMultiplier = damageMultiplier;
        player.fireRateMultiplier = fireRateMultiplier;
        player.maxInvincibilityTime = maxInvincibilityTime;
        player.invincibilityTime = invincibilityTime;
        player.gunRelic = gunRelic;
        player.eyeRelic.SetActive(hasEyeRelic);
        player.hasSizeRelic = hasSizeRelic;
        if (hasSizeRelic)
        {
            player.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }
        else
        {
            player.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
        }
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
        maxLife = 25;
        speed = 7f;
        damageMultiplier = 1f;
        fireRateMultiplier = 1f;
        maxInvincibilityTime = 1f;
        invincibilityTime = 1f;
        gunRelic = false;
        hasEyeRelic = false;
        hasSizeRelic = false;
        equippedGun = initialGun;
        floorLevel = 1;
        Debug.Log("Progress reset, floor level back to 1");
    }
}
