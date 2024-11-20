using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public int numberOfRooms;
    public GameObject startRoom; 

    public GameObject[] roomLayouts;
    public bool canWork;

    void Start()
    {
        startRoom = GameObject.Find("Room");
        canWork = true;
    }

    void Update(){
        if(numberOfRooms >= startRoom.GetComponent<RoomSpawner>().maxRooms && canWork){
            for(int i = 0; i <= numberOfRooms; i++){
                GameObject targetRoom = GameObject.Find("Room" + i);
                if(targetRoom != null){
                    RoomSpawner targetRoomSpawner = targetRoom.GetComponent<RoomSpawner>();
                    targetRoomSpawner.NeighborRooms();
                    targetRoomSpawner.SetupNeighborDoors(targetRoomSpawner.NeighborRooms());
                    if(i != 12){
                        int layoutN = Random.Range(0, roomLayouts.Length);
                        Instantiate(roomLayouts[layoutN], targetRoom.transform);
                    }
                }
            }
            canWork = false;
        }
        if(canWork == false && numberOfRooms != startRoom.GetComponent<RoomSpawner>().maxRooms+1){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
