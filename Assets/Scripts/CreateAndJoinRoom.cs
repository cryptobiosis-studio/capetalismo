using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Voice.PUN;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{   
    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public void CreateRoom(){
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }
    public void JoinRoom(){
        PhotonNetwork.JoinRoom(joinInput.text);
    }
    public override void OnJoinedRoom(){
        PunVoiceClient.Instance.UsePunAppSettings = true;
        PunVoiceClient.Instance.AutoConnectAndJoin = true;
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Player " + PhotonNetwork.LocalPlayer.NickName + " entrou na sala!");
        if (PhotonNetwork.IsMasterClient){
            PhotonNetwork.LoadLevel("Multiplayer Run");
        } 
        PhotonNetwork.IsMessageQueueRunning = true;
    }
    public override void OnJoinRoomFailed(short returnCode, string message){
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Falha ao tentar entrar na sala: " + message);
    }
}

