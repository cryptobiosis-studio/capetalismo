using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MapManager : MonoBehaviourPunCallbacks
{
    public int numberOfRooms;
    public GameObject startRoom;

    public GameObject[] roomLayouts;
    public bool canWork;

    public int rdIndex;
    public int waterIndex;
    public int chestIndex;

    private HashSet<int> usedRoomIndices;
    private List<int> generatedRooms;

    public GameObject lastRoom;

    private int mapSeed;  // Variável para armazenar a semente do mapa

    void Start()
    {
        startRoom = GameObject.Find("Room");
        canWork = true;
        usedRoomIndices = new HashSet<int>();
        generatedRooms = new List<int>();

        if (PhotonNetwork.IsMasterClient)
        {
            // Apenas o Master Client gera a semente
            mapSeed = Random.Range(0, 10000); // Gera uma semente aleatória para o mapa
            photonView.RPC("SetMapSeed_RPC", RpcTarget.All, mapSeed); // Envia a semente para todos os jogadores
        }
    }

    void Update()
    {
        if (canWork && numberOfRooms >= startRoom.GetComponent<RoomSpawner>().maxRooms)
        {
            canWork = false;
            // Gera o mapa agora que todos têm a mesma semente
            photonView.RPC("GenerateMap_RPC", RpcTarget.All);
        }
    }

    // RPC para sincronizar a semente com todos os jogadores
    [PunRPC]
    void SetMapSeed_RPC(int seed)
    {
        // Todos os jogadores recebem a mesma semente
        mapSeed = seed;
        Random.InitState(mapSeed);  // Inicializa o Random com a semente compartilhada
    }

    // RPC para gerar o mapa em todos os jogadores
    [PunRPC]
    void GenerateMap_RPC()
    {
        List<int> roomOrder = new List<int>();
        for (int i = 0; i < numberOfRooms; i++)
        {
            roomOrder.Add(i);
        }

        ShuffleList(roomOrder);
        PlaceMandatoryLayouts(roomOrder);
        FillRemainingRooms(roomOrder);
        ConfigureDoors();

        Debug.Log("Map Complete!");
    }

    void PlaceMandatoryLayouts(List<int> roomOrder)
    {
        int[] mandatoryLayouts = { rdIndex, waterIndex, chestIndex };
        foreach (int layoutIndex in mandatoryLayouts)
        {
            for (int i = 0; i < 2; i++)
            {
                int randomRoomIndex = roomOrder[Random.Range(0, roomOrder.Count - 1)];
                roomOrder.Remove(randomRoomIndex);
                usedRoomIndices.Add(randomRoomIndex);
                generatedRooms.Add(randomRoomIndex);

                GameObject targetRoom = GameObject.Find("Room" + randomRoomIndex);
                if (targetRoom != null && randomRoomIndex != startRoom.gameObject.GetComponent<RoomSpawner>().maxRooms)
                {
                    PhotonNetwork.Instantiate(roomLayouts[layoutIndex].name, targetRoom.transform.position, targetRoom.transform.rotation);
                }
            }
        }
    }

    void FillRemainingRooms(List<int> roomOrder)
    {
        foreach (int roomIndex in roomOrder)
        {
            GameObject targetRoom = GameObject.Find("Room" + roomIndex);
            if (targetRoom != null && roomIndex != startRoom.gameObject.GetComponent<RoomSpawner>().maxRooms)
            {
                List<GameObject> nonMandatoryLayouts = new List<GameObject>(roomLayouts);
                nonMandatoryLayouts.RemoveAt(rdIndex);
                nonMandatoryLayouts.RemoveAt(waterIndex - (rdIndex < waterIndex ? 1 : 0));
                nonMandatoryLayouts.RemoveAt(chestIndex - (rdIndex < chestIndex ? 1 : 0) - (waterIndex < chestIndex ? 1 : 0));

                int randomLayout = Random.Range(0, nonMandatoryLayouts.Count);
                PhotonNetwork.Instantiate(nonMandatoryLayouts[randomLayout].name, targetRoom.transform.position, targetRoom.transform.rotation);

                generatedRooms.Add(roomIndex);
            }
            else if (targetRoom != null && roomIndex == startRoom.gameObject.GetComponent<RoomSpawner>().maxRooms)
            {
                generatedRooms.Add(roomIndex);
                lastRoom = targetRoom;
            }
        }
    }

    void ConfigureDoors()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            GameObject room = GameObject.Find("Room" + i);
            if (room != null)
            {
                RoomSpawner roomSpawner = room.GetComponent<RoomSpawner>();
                if (roomSpawner != null)
                {
                    roomSpawner.NeighborRooms();
                    roomSpawner.SetupNeighborDoors(roomSpawner.NeighborRooms());
                }
            }
        }
    }

    void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); // Usa a semente sincronizada
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
