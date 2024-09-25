using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public GameObject room;
    public Transform[] spawners;
    public Transform[] doors;
    public MapManager manager;
    public int maxRooms = 20;
    bool existRoom;

    // 1-Top 2-Bottom 3-Right 4-Left

    void Start()
    {
        manager = GameObject.Find("MapManager").GetComponent<MapManager>();
        if (manager.numberOfRooms <= maxRooms)
        {
            int spawnDirection = Random.Range(0, 4);
            TrySpawnRoom(spawnDirection);
        }
    }

    void TrySpawnRoom(int direction) // Auto explicativo
    {
        string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
        string[] doorNames = { "Bottom", "Top", "Left", "Right" };
        int spawnerIndex = direction;
        existRoom = Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"));
        Debug.Log(existRoom);

        if (this.gameObject.transform.Find(spawnerNames[direction]).gameObject.activeSelf && !existRoom)
        {
            GameObject newRoom = Instantiate(room, spawners[spawnerIndex].position, Quaternion.identity);
            doors[direction].gameObject.SetActive(false);
            SetupRoom(newRoom, doorNames[direction]);
            DisableOppositeSpawner(spawnerIndex);
            manager.numberOfRooms++;
        }else{
            TrySpawnRoom(Random.Range(0,4)); //Tenta de novo se não der certo
        }
    }

    void SetupRoom(GameObject room, string doorName)
    {   
        // Posiciona as portas
        room.transform.Find(doorName).gameObject.SetActive(false);
        foreach (Transform door in room.transform)
        {
            if (door.name != doorName)
                door.gameObject.SetActive(true);
        }
    }

    void DisableOppositeSpawner(int direction)
    {
        int oppositeDirection = (direction + 2) % 4; // Calcula a direcao oposta
        string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
        this.gameObject.transform.Find(spawnerNames[oppositeDirection]).gameObject.SetActive(false);
    }
    
}
