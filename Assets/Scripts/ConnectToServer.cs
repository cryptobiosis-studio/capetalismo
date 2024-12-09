using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice;
using UnityEngine.SceneManagement;
using Photon.Voice.PUN;
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    
    void Start()
    {   
        PunVoiceClient.Instance.UsePunAppSettings = true;
        PunVoiceClient.Instance.AutoConnectAndJoin = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster(){
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {      
        SceneManager.LoadScene("ChooseRoom");
    }
}
