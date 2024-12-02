using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{   
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public void CreateRoom(){
        PhotonNetwork.CreateRoom(createInput.text);
    }
    public void JoinRoom(){
        PhotonNetwork.JoinRoom(createInput.text);
    }
    public override void OnJoinedRoom(){
        SceneManager.LoadScene("MultiplayerRun");
    }
}
