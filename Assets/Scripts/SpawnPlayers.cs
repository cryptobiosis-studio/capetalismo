using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{   
    public GameObject playerPrefab;
   
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {   
        DontDestroyOnLoad(this.gameObject);
        PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
    }

  
}
