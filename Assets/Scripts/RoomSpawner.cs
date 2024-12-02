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

public class RoomSpawner : MonoBehaviourPunCallbacks
{
    private string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
    private string[] doorNames = { "Bottom", "Top", "Left", "Right" };
    public GameObject room;
    public Transform[] spawners;
    public Transform[] doors;
    public MapManager manager;
    public int maxRooms = 16;

    void Start()
    {
        manager = GameObject.Find("MapManager").GetComponent<MapManager>();

        if (PhotonNetwork.IsMasterClient) // Apenas o Master Client gera as salas
        {
            if (manager.numberOfRooms <= maxRooms) // Verifica se não ultrapassou o número máximo de salas
            {
                int spawnDirection = Random.Range(0, 4); // Roda uma direção aleatória
                TrySpawnRoom(spawnDirection);
            }
        }
    }

    //-----------------------------------------Gera_Salas-------------------------------------------------------------
    void TrySpawnRoom(int direction, int attemptCount = 0) // Adiciona um contador de tentativas
    {
        if (attemptCount > 3) // Limita as tentativas para 3
        {
            Debug.LogWarning("Limite de tentativas atingido. Não foi possível gerar uma sala.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    // Método para instanciar a sala usando PhotonNetwork.Instantiate
    void spawnRoom(int direction)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Apenas o Master Client instanciando as salas

        GameObject newRoom = PhotonNetwork.Instantiate(room.name, spawners[direction].position, Quaternion.identity); // Instancia o prefab da sala
        newRoom.name = "Room" + manager.numberOfRooms;
        doors[direction].gameObject.SetActive(false); // Desabilita a porta na direção da nova sala
        SetupRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);
        manager.numberOfRooms++; // Incrementa o número de salas no manager
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

    void DisableOppositeSpawner(int direction) // Desabilita o spawner na direção oposta
    {
        int oppositeDirection = (direction + 2) % 4; // Calcula a direção oposta
        this.gameObject.transform.Find(spawnerNames[oppositeDirection]).gameObject.SetActive(false);
    }

    bool ExistRoom(int direction) // Verifica se já existe uma sala na direção escolhida
    {
        return Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room")); 
    }
    //---------------------------------------Configura_Portas---------------------------------------------------------
    public NeighborRoom[] NeighborRooms() // Verifica a existência de salas vizinhas
    {
        List<NeighborRoom> neighbors = new List<NeighborRoom>();
        for (int i = 0; i <= 3; i++)
        {
            Transform spawner = spawners[i];
            if (!spawner.gameObject.activeSelf)
            {
                continue;
            }
            if (!ExistRoom(i))
            {
                continue;
            }
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

    void SetupNeighborRoom(GameObject room, string doorName) // Configura a porta de uma sala vizinha
    {   
        // Posiciona as portas
        room.transform.Find(doorName).gameObject.SetActive(false);
    }
}
