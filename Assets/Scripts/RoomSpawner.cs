using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class NeighborRoom
{
    public GameObject RoomObj { get; set; }
    public int Direction { get; set; }
    public NeighborRoom(GameObject roomObj, int direction)
    {
        RoomObj = roomObj;
        Direction = direction;
    }
}

public class RoomSpawner : MonoBehaviourPun
{
    private string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
    private string[] doorNames = { "Bottom", "Top", "Left", "Right" };
    public GameObject room;
    public Transform[] spawners;
    public Transform[] doors;
    public MapManager manager;
    public int maxRooms;

    void Start()
    {
        manager = GameObject.Find("MapManager").GetComponent<MapManager>();

        if (PhotonNetwork.IsMasterClient)
        {
            TrySpawnRoom(Random.Range(0, 4));
        }
    }

    void TrySpawnRoom(int direction, int attemptCount = 0)
    {
        if (attemptCount > 3)
        {
            Debug.LogWarning("Limite de tentativas atingido. Não foi possível gerar uma sala.");
            return;
        }

        if (!this.gameObject.transform.Find(spawnerNames[direction]).gameObject.activeSelf || ExistRoom(direction))
        {
            TrySpawnRoom(Random.Range(0, 4), attemptCount + 1);
            return;
        }

        SpawnRoom(direction);
    }

    void SpawnRoom(int direction)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector3 spawnPosition = spawners[direction].position;
        GameObject newRoom = PhotonNetwork.Instantiate("Room", spawnPosition, Quaternion.identity);
        newRoom.name = "Room" + manager.numberOfRooms;

        // Configuração local no MasterClient
        doors[direction].gameObject.SetActive(false);
        SetupRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);

        manager.numberOfRooms++;
    }

    void SetupRoom(GameObject room, string doorName)
    {
        Transform door = room.transform.Find(doorName);
        if (door != null) door.gameObject.SetActive(false);

        foreach (Transform otherDoor in room.transform)
        {
            if (otherDoor.name != doorName)
                otherDoor.gameObject.SetActive(true);
        }
    }

    void DisableOppositeSpawner(int direction)
    {
        int oppositeDirection = (direction + 2) % 4;
        Transform spawner = this.gameObject.transform.Find(spawnerNames[oppositeDirection]);
        if (spawner != null) spawner.gameObject.SetActive(false);
    }

    bool ExistRoom(int direction)
    {
        return Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"));
    }

    public NeighborRoom[] NeighborRooms()
    {
        List<NeighborRoom> neighbors = new List<NeighborRoom>();
        for (int i = 0; i <= 3; i++)
        {
            Transform spawner = spawners[i];
            if (!spawner.gameObject.activeSelf || !ExistRoom(i)) continue;

            GameObject roomObj = Physics2D.OverlapCircle(spawner.position, 1f, LayerMask.GetMask("Room")).gameObject;
            neighbors.Add(new NeighborRoom(roomObj, i));
        }
        return neighbors.ToArray();
    }

    public void SetupNeighborDoors(NeighborRoom[] neighborRooms)
    {
        foreach (NeighborRoom n in neighborRooms)
        {
            SetupNeighborRoom(n.RoomObj, doorNames[n.Direction]);
            doors[n.Direction].gameObject.SetActive(false);
        }
    }

    void SetupNeighborRoom(GameObject room, string doorName)
    {
        Transform door = room.transform.Find(doorName);
        if (door != null) door.gameObject.SetActive(false);
    }
}
