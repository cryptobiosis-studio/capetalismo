using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{   
    public GameObject playerPrefab;
   
    [PunRPC]
    void SpawnPlayerRPC(Vector3 position)
    {
        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

void Start()
{
    if (PhotonNetwork.IsMasterClient)
    {
        photonView.RPC("SpawnPlayerRPC", RpcTarget.All, transform.position);
    }
}

  
}
