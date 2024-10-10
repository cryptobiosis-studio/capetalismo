using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

class NeighborRoom
{
    public GameObject RoomObj { get; set; }
    public int Direction { get; set; }
    public NeighborRoom(GameObject roomObj, int direction)
    {
        RoomObj = roomObj;
        Direction = direction;
    }
}

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

public class RoomSpawner : MonoBehaviour
{
    private string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
    private string[] doorNames = { "Bottom", "Top", "Left", "Right" };
    public GameObject room;
    public Transform[] spawners;
    public Transform[] doors;
    public MapManager manager;
    public int maxRooms = 20;
    public neighboorRoom

    void Start()
    {
        manager = GameObject.Find("MapManager").GetComponent<MapManager>();
        if (manager.numberOfRooms <= maxRooms) // Verifica se ja ultrapassou o numero de salas
        {
            int spawnDirection = Random.Range(0, 4); // Roda uma direcao aleatoria
            TrySpawnRoom(spawnDirection);
        }
    }

    //-----------------------------------------Gera_Salas-------------------------------------------------------------
    void TrySpawnRoom(int direction) // Tenta gerar uma sala
    {
        bool canSpawnRoom = this.gameObject.transform.Find(spawnerNames[direction]).gameObject.activeSelf && !ExistRoom(direction); // Verifica se é possivel gerar uma sala na direcao escolhida
        if (!canSpawnRoom)
        {
            TrySpawnRoom(Random.Range(0, 4)); // Tenta de novo se não der certo
            return;
        }

        spawnRoom(direction); 
    }

    void spawnRoom(int direction) // Gera uma sala
    {
        GameObject newRoom = Instantiate(room, spawners[direction].position, Quaternion.identity); // Instancia o prefab da sala
        newRoom.name = "Room" + manager.numberOfRooms;
        doors[direction].gameObject.SetActive(false); // Desabilita a porta na direcao da nova sala
        SetupRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);
        manager.numberOfRooms++; // Incrementa o numero de salas no manager
    }

    void SetupRoom(GameObject room, string doorName) // Configura a sala
    {   
        // Posiciona as portas
        room.transform.Find(doorName).gameObject.SetActive(false);
        foreach (Transform door in room.transform)
        {
            if (door.name != doorName)
                door.gameObject.SetActive(true);
        }
    }

    void DisableOppositeSpawner(int direction) // Desabilita o spawner na direcao
    {
        int oppositeDirection = (direction + 2) % 4; // Calcula a direcao oposta
        this.gameObject.transform.Find(spawnerNames[oppositeDirection]).gameObject.SetActive(false);
    }

    bool ExistRoom(int direction){ // Verifica se já existe uma sala na direcao escolhida
        return(Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"))); 
    }
    //---------------------------------------Configura_Portas---------------------------------------------------------
    NeighborRoom[] neighborRooms(){ // Verifica a existencia de salas vizinhas
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
        return(neighbors.ToArray());
    }

    void SetupNeighborDoors(){
        foreach(NeighborRoom n in neighborRooms()){
            SetupRoom(n.RoomObj, doorNames[n.Direction]);
        }
    }

    bool ExistRoom(direction){ // Verifica se já existe uma sala na direcao escolhida
        return(Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"))); 
    }
    //---------------------------------------Configura_Portas---------------------------------------------------------
    NeighborRoom[] neighborRooms(){ // Verifica a existencia de salas vizinhas
        List<NeighborRoom> neighbors = new List<NeighborRoom>();
        for(int i = 0; i<=3, i++){
            Transform spawner = spawners[i];
            if(!spawner.gameObject.activeSelf){
                continue;
            }
            if(!ExistRoom(i)){
                continue
            }
            GameObject roomObj = Physics2D.OverlapCircle(spawner.position, 1f, LayerMask.GetMask("Room")).gameObject;
            NeighborRoom neighbor = new NeighborRoom(roomObj, i);
            neighbors.Add(neighbor);
        }
        return(neighbors.ToArray());
    }

    void SetupNeighborDoors(){
        foreach(NeighborRoom n in neighborRooms){
            SetupRoom(n.RoomObj, n.Direction);
        }
    }
}
