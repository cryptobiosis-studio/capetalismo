using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {

        if (PhotonNetwork.IsConnected){
            PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        Debug.Log(otherPlayer.NickName + " saiu da sala.");
    }
}