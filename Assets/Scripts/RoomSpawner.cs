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
        if (!PhotonNetwork.IsMasterClient) return; // Apenas o MasterClient gera as salas

        manager = GameObject.Find("MapManager").GetComponent<MapManager>();
        if (manager.numberOfRooms <= maxRooms)
        {
            int spawnDirection = Random.Range(0, 4);
            TrySpawnRoom(spawnDirection);
        }
    }

    //-----------------------------------------Gera_Salas-------------------------------------------------------------
    void TrySpawnRoom(int direction, int attemptCount = 0)
    {
        if (attemptCount > 3)
        {
            Debug.LogWarning("Limite de tentativas atingido. Não foi possível gerar uma sala.");
            return;
        }

        bool canSpawnRoom = this.gameObject.transform.Find(spawnerNames[direction]).gameObject.activeSelf && !ExistRoom(direction);

        if (!canSpawnRoom)
        {
            TrySpawnRoom(Random.Range(0, 4), attemptCount + 1);
            return;
        }

        SpawnRoom(direction);
    }

    void SpawnRoom(int direction)
    {
        // Instancia a sala no ambiente multiplayer
        Vector3 spawnPosition = spawners[direction].position;
        GameObject newRoom = PhotonNetwork.Instantiate("Room", spawnPosition, Quaternion.identity); // Instancia no Photon
        newRoom.name = "Room" + manager.numberOfRooms;

        // Configuração local no MasterClient
        doors[direction].gameObject.SetActive(false);
        SetupRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);

        // Incrementa o número de salas e notifica os outros jogadores
        manager.numberOfRooms++;
        photonView.RPC("SyncRoom", RpcTarget.Others, newRoom.name, direction);
    }

    [PunRPC]
    void SyncRoom(string roomName, int direction)
    {
        GameObject newRoom = GameObject.Find(roomName);
        if (newRoom != null)
        {
            doors[direction].gameObject.SetActive(false);
            SetupRoom(newRoom, doorNames[direction]);
            DisableOppositeSpawner(direction);
        }
    }

    void SetupRoom(GameObject room, string doorName)
    {
        room.transform.Find(doorName).gameObject.SetActive(false);
        foreach (Transform door in room.transform)
        {
            if (door.name != doorName)
                door.gameObject.SetActive(true);
        }
    }

    void DisableOppositeSpawner(int direction)
    {
        int oppositeDirection = (direction + 2) % 4;
        this.gameObject.transform.Find(spawnerNames[oppositeDirection]).gameObject.SetActive(false);
    }

    bool ExistRoom(int direction)
    {
        return Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"));
    }

    //---------------------------------------Configura_Portas---------------------------------------------------------
    public NeighborRoom[] NeighborRooms()
    {
        List<NeighborRoom> neighbors = new List<NeighborRoom>();
        for (int i = 0; i <= 3; i++)
        {
            Transform spawner = spawners[i];
            if (!spawner.gameObject.activeSelf) continue;
            if (!ExistRoom(i)) continue;

            GameObject roomObj = Physics2D.OverlapCircle(spawner.position, 1f, LayerMask.GetMask("Room")).gameObject;
            NeighborRoom neighbor = new NeighborRoom(roomObj, i);
            neighbors.Add(neighbor);
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
        room.transform.Find(doorName).gameObject.SetActive(false);
    }
}
