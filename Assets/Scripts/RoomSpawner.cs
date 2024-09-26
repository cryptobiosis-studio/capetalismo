using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class RoomSpawner : MonoBehaviour
{
    private string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
    private string[] doorNames = { "Bottom", "Top", "Left", "Right" };
    public GameObject room;
    public Transform[] spawners;
    public Transform[] doors;
    public MapManager manager;
    public int maxRooms = 20;

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
        bool existRoom = Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"));

        bool canSpawnRoom = this.gameObject.transform.Find(spawnerNames[direction]).gameObject.activeSelf && !existRoom;
        if (!canSpawnRoom)
        {
            TrySpawnRoom(Random.Range(0, 4)); //Tenta de novo se não der certo
            return;
        }

        spawnRoom(direction);
    }

    void spawnRoom(int direction)
    {
        GameObject newRoom = Instantiate(room, spawners[direction].position, Quaternion.identity);
        newRoom.name = "Room" + manager.numberOfRooms;
        doors[direction].gameObject.SetActive(false);
        SetupRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);
        manager.numberOfRooms++;
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
        this.gameObject.transform.Find(spawnerNames[oppositeDirection]).gameObject.SetActive(false);
    }
    
}
