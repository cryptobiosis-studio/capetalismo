using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{   
    public GameObject playerPrefab;
   
    void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
        GameObject.Find("CurriculumChoice").SetActive(false);
    }

  
}
