using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.SceneManagement;

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

public class RoomSpawner : MonoBehaviourPunCallbacks
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
        if (!PhotonNetwork.IsMasterClient) return;

        manager = GameObject.Find("MapManager").GetComponent<MapManager>();
        if (manager.numberOfRooms <= maxRooms) // Verifica se já ultrapassou o número de salas
        {
            int spawnDirection = Random.Range(0, 4); // Roda uma direção aleatória
            TrySpawnRoom(spawnDirection);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    void TrySpawnRoom(int direction, int attemptCount = 0)
    {
        if (attemptCount > 3) // Limita as tentativas para 3
        {
            Debug.LogWarning("Limite de tentativas atingido. Não foi possível gerar uma sala.");
            return;
        }

        bool canSpawnRoom = this.gameObject.transform.Find(spawnerNames[direction]).gameObject.activeSelf && !ExistRoom(direction);

        if (!canSpawnRoom)
        {
            TrySpawnRoom(Random.Range(0, 4), attemptCount + 1); // Passa o contador para a próxima tentativa
            return;
        }

        spawnRoom(direction);
    }

    void spawnRoom(int direction)
    {   
        manager.numberOfRooms++;
        GameObject newRoom = PhotonNetwork.Instantiate("Room", spawners[direction].position, Quaternion.identity); // Instancia o prefab da sala
        newRoom.name = "Room" + manager.numberOfRooms;
        doors[direction].gameObject.SetActive(false); // Desabilita a porta na direção da nova sala
        SetupRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);
    }

    void SetupRoom(GameObject room, string doorName)
    {
        // Posiciona as portas
        room.transform.Find(doorName).gameObject.SetActive(false);
        foreach (Transform door in room.transform)
        {
            if (door.name != doorName)
                door.gameObject.SetActive(true);
                PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    void DisableOppositeSpawner(int direction)
    {
        int oppositeDirection = (direction + 2) % 4; // Calcula a direção oposta
        this.gameObject.transform.Find(spawnerNames[oppositeDirection]).gameObject.SetActive(false);
    }

    bool ExistRoom(int direction)
    {
        return Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"));
    }

    //---------------------------------------Configura_Portas---------------------------------------------------------
    public NeighborRoom[] NeighborRooms(){ // Verifica a existencia de salas vizinhas
        List<NeighborRoom> neighbors = new List<NeighborRoom>();
        for(int i = 0; i<=3; i++){
            Transform spawner = spawners[i];
            if(!spawner.gameObject.activeSelf){
                continue;
            }
            if(!ExistRoom(i)){
                continue;
            }
            GameObject roomObj = Physics2D.OverlapCircle(spawner.position, 1f, LayerMask.GetMask("Room")).gameObject;
            NeighborRoom neighbor = new NeighborRoom(roomObj, i);
            neighbors.Add(neighbor);
        }
        PhotonNetwork.SendAllOutgoingCommands();
        return neighbors.ToArray();
    }

    public void SetupNeighborDoors(NeighborRoom[] neighborRooms){
        foreach(NeighborRoom n in neighborRooms){
            SetupNeighborRoom(n.RoomObj, doorNames[n.Direction]);
            doors[n.Direction].gameObject.SetActive(false);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }
    void SetupNeighborRoom(GameObject room, string doorName) // Configura a sala
    {   
        // Posiciona as portas
        room.transform.Find(doorName).gameObject.SetActive(false);
        PhotonNetwork.SendAllOutgoingCommands();
    }

}