using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{   
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public void CreateRoom(){
        PhotonNetwork.AutomaticallySyncScene = true;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }
    public void JoinRoom(){
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinRoom(joinInput.text);
    }
    public override void OnJoinedRoom(){
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene("MultiplayerRun");
        PhotonNetwork.IsMessageQueueRunning = true;
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Falha ao tentar entrar na sala: " + message);
    }
}
