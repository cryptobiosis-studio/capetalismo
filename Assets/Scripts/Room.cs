using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public bool hasPlayerIn;
    public PlayerController player;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player"){
            hasPlayerIn = true;
            player = other.gameObject.GetComponent<PlayerController>();

        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player"){
            hasPlayerIn = false;
        }
    }
}
