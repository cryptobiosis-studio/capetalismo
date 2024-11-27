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

    public int rdCounter;
    public int waterCounter;
    public int chestCounter;

    public int rdIndex;
    public int waterIndex;
    public int chestIndex;

    void Start()
    {
        startRoom = GameObject.Find("Room");
        canWork = true;
        rdCounter = 0;
        waterCounter = 0;
        chestCounter = 0;
        Mathf.Clamp(chestCounter, 0, 2);  
        Mathf.Clamp(waterCounter, 0, 2);      
        Mathf.Clamp(rdCounter, 0, 2);             
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
                        switch(layoutN){
                            case var value when value == rdIndex:
                                rdCounter+=1;
                                break;
                            case var value when value == waterIndex:
                                waterCounter+=1;
                                break;
                            case var value when value == chestIndex:
                                chestCounter+=1;
                                break;

                        }
                        if(layoutN == rdIndex && rdCounter >=2){
                            layoutN = Random.Range(0, roomLayouts.Length);
                        }else if(layoutN == waterIndex && waterCounter >=2){
                            layoutN = Random.Range(0, roomLayouts.Length);
                        }else if(layoutN == chestIndex && chestCounter >=2){
                            layoutN = Random.Range(0, roomLayouts.Length);
                        }else{

                        }
                        Instantiate(roomLayouts[layoutN], targetRoom.transform);
                    }
                }
            }
            canWork = false;
        }
        if(canWork == false && numberOfRooms != startRoom.GetComponent<RoomSpawner>().maxRooms+1 || chestCounter == 0 && !canWork|| rdCounter == 0 && !canWork || waterCounter == 0 && !canWork){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
