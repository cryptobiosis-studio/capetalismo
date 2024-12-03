using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MapManager : MonoBehaviourPun
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
    private int emptyRoomIndex = -1;

    public GameObject lastRoom;

    private List<int> roomOrder; // Ordem das salas geradas para sincronização

    void Start()
    {
        startRoom = GameObject.Find("Room");
        canWork = true;
        usedRoomIndices = new HashSet<int>();
        generatedRooms = new List<int>();
        roomOrder = new List<int>();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && canWork && numberOfRooms >= startRoom.GetComponent<RoomSpawner>().maxRooms)
        {
            canWork = false;
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        roomOrder = new List<int>();
        for (int i = 0; i < numberOfRooms; i++)
        {
            roomOrder.Add(i);
        }
        ShuffleList(roomOrder);
        PlaceMandatoryLayouts(roomOrder);
        FillRemainingRooms(roomOrder);
        ConfigureDoors();

        Debug.Log("Map Complete!");

        // Enviar dados do mapa para os outros jogadores
        photonView.RPC("SyncMapData", RpcTarget.Others, SerializeMapData());
    }

    void PlaceMandatoryLayouts(List<int> roomOrder)
    {
        int[] mandatoryLayouts = { rdIndex, waterIndex, chestIndex };
        foreach (int layoutIndex in mandatoryLayouts)
        {
            for (int i = 0; i < 2; i++)
            {
                int randomRoomIndex = roomOrder[Random.Range(0, roomOrder.Count)];
                roomOrder.Remove(randomRoomIndex);
                usedRoomIndices.Add(randomRoomIndex);
                generatedRooms.Add(randomRoomIndex);
                GameObject targetRoom = GameObject.Find("Room" + randomRoomIndex);
                if (targetRoom != null && randomRoomIndex != emptyRoomIndex)
                {
                    Instantiate(roomLayouts[layoutIndex], targetRoom.transform);
                }
            }
        }
    }

    void FillRemainingRooms(List<int> roomOrder)
    {
        foreach (int roomIndex in roomOrder)
        {
            if (roomIndex == emptyRoomIndex) continue;
            GameObject targetRoom = GameObject.Find("Room" + roomIndex);
            if (targetRoom != null && roomIndex != 24)
            {
                List<GameObject> nonMandatoryLayouts = new List<GameObject>(roomLayouts);
                nonMandatoryLayouts.RemoveAt(rdIndex);
                nonMandatoryLayouts.RemoveAt(waterIndex - (rdIndex < waterIndex ? 1 : 0));
                nonMandatoryLayouts.RemoveAt(chestIndex - (rdIndex < chestIndex ? 1 : 0) - (waterIndex < chestIndex ? 1 : 0));
                int randomLayout = Random.Range(0, nonMandatoryLayouts.Count);
                Instantiate(nonMandatoryLayouts[randomLayout], targetRoom.transform);
                generatedRooms.Add(roomIndex);
            }
            else if (targetRoom != null && roomIndex == 24)
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
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Sincronização do mapa
    string SerializeMapData()
    {
        return JsonUtility.ToJson(new MapData { roomOrder = this.roomOrder, generatedRooms = this.generatedRooms });
    }

    [PunRPC]
    void SyncMapData(string serializedMapData)
    {
        MapData mapData = JsonUtility.FromJson<MapData>(serializedMapData);

        // Aplicar os dados do mapa recebido
        this.roomOrder = mapData.roomOrder;
        this.generatedRooms = mapData.generatedRooms;

        // Recriar o mapa localmente
        RecreateMap();
    }

    void RecreateMap()
    {
        // Lógica para recriar o mapa localmente com os dados recebidos
        Debug.Log("Recriando mapa com os dados recebidos do MasterClient...");
    }

    [System.Serializable]
    class MapData
    {
        public List<int> roomOrder;
        public List<int> generatedRooms;
    }
}
