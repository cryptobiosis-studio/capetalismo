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
    public int maxRooms;

    void Start()
    {
        manager = GameObject.Find("MapManager").GetComponent<MapManager>();

        if (PhotonNetwork.IsMasterClient)
        {
            // Gera o mapa e armazena o estado na propriedade da sala
            GenerateAndSyncMap();
        }
        else
        {
            // Aqui você pode sincronizar o estado do mapa
            SyncMapFromMasterClient();
        }
    }

    void GenerateAndSyncMap()
    {
        // Exemplo de como gerar o mapa (o que você já fez antes)
        if(manager.numberOfRooms <= maxRooms){
             for (int i = 0; i < maxRooms; i++)
            {
            // Aqui você cria as salas conforme sua lógica (exemplo: Random.Range etc.)
            TrySpawnRoom(Random.Range(0, 4));
            }
        }
       

        // Após a geração, envia o estado do mapa para os outros jogadores
        SyncMapToOtherPlayers();
    }

    void SyncMapToOtherPlayers()
    {
        // Envia os dados do mapa (por exemplo, posições das salas) via Photon
        // Usando uma propriedade customizada do Photon Room (PhotonNetwork.CurrentRoom.SetCustomProperties)
        ExitGames.Client.Photon.Hashtable mapData = new ExitGames.Client.Photon.Hashtable
        {
            { "MapGenerated", true },
            { "Rooms", manager.numberOfRooms }
            // Você pode armazenar as posições das salas ou qualquer outra coisa necessária
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(mapData);
    }

    void SyncMapFromMasterClient()
    {
        // Sincroniza o mapa para jogadores que entram
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("MapGenerated"))
        {
            bool mapGenerated = (bool)PhotonNetwork.CurrentRoom.CustomProperties["MapGenerated"];
            if (mapGenerated)
            {
                // Lógica para aplicar as modificações do mapa no cliente
                // Isso pode incluir a reconstrução do mapa com base nos dados sincronizados
                Debug.Log("Mapa gerado pelo Master Client está sendo sincronizado.");
            }
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
