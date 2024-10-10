using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public int numberOfRooms;
    public GameObject startRoom; 

    void Start()
    {
        startRoom = GameObject.Find("Room");
    }

    void Update(){
        if(numberOfRooms >= startRoom.GetComponent<RoomSpawner>().maxRooms){
            for(int i = 0; i <= numberOfRooms; i++){
                GameObject targetRoom = GameObject.Find("Room" + i);
                Debug.Log(targetRoom);
                RoomSpawner targetRoomSpawner = targetRoom.GetComponent<RoomSpawner>();
                targetRoomSpawner.NeighborRooms();
                targetRoomSpawner.SetupNeighborDoors(targetRoomSpawner.NeighborRooms());
                if(i == numberOfRooms){
                    gameObject.GetComponent<MapManager>().enabled = false;
                }
            }
        }
    }
}
