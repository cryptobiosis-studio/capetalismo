using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public void SinglePlayer(){
        SceneManager.LoadScene("SingleplayerRun");
    }
    public void MultiPlayer(){
        SceneManager.LoadScene("ChooseRoom");
    }
}
