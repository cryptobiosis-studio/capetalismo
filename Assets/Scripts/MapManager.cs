using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

[System.Serializable]
    public class MapData
    {
        public int numberOfRooms;
        public List<int> generatedRooms;
        public List<int> usedRoomIndices;
    }
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
    private int emptyRoomIndex = -1;

    public GameObject lastRoom;

    void Start()
    {
        startRoom = GameObject.Find("Room");
        canWork = true;
        usedRoomIndices = new HashSet<int>();
        generatedRooms = new List<int>();

    }
    void Update()   
    {
        if (PhotonNetwork.IsMasterClient && canWork && numberOfRooms >= startRoom.GetComponent<RoomSpawner>().maxRooms)
        {
            canWork = false;
            Debug.Log("All rooms generated. Total rooms: " + numberOfRooms);
            GenerateMap();
        }
    }


    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string serializedMap = PhotonNetwork.CurrentRoom.CustomProperties["MapLayout"].ToString();
            photonView.RPC("LoadMapOnJoin", newPlayer, serializedMap);
        }
    }

    [PunRPC]
    void LoadMapOnJoin(string serializedMap)
    {
        DeserializeMap(serializedMap);
        RebuildMap();
    }

    void GenerateMap()
    {
        List<int> roomOrder = new List<int>();
        for (int i = 0; i < numberOfRooms; i++)
        {
            roomOrder.Add(i);
            Debug.Log("Added room"+i);
        }

        ShuffleList(roomOrder);
        PlaceMandatoryLayouts(roomOrder);
        FillRemainingRooms(roomOrder);

        string serializedMap = SerializeMap();
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "MapLayout", serializedMap } });

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
                if (roomOrder.Count == 0)
                {
                    Debug.LogWarning("Não há mais salas disponíveis no roomOrder para posicionar layouts obrigatórios.");
                    return;
                }

                int randomIndex = Random.Range(0, roomOrder.Count);
                int randomRoomIndex = roomOrder[randomIndex];
                roomOrder.RemoveAt(randomIndex);

                usedRoomIndices.Add(randomRoomIndex);
                generatedRooms.Add(randomRoomIndex);

                GameObject targetRoom = GameObject.Find("Room" + randomRoomIndex);
                if (targetRoom != null && randomRoomIndex != startRoom.GetComponent<RoomSpawner>().maxRooms)
                {
                    // Instanciando o layout e configurando o transform
                    GameObject layoutInstance = PhotonNetwork.Instantiate(roomLayouts[layoutIndex].name, targetRoom.transform.position, Quaternion.identity);
                    
                    // Definindo a sala como o pai do layout instanciado
                    layoutInstance.transform.SetParent(targetRoom.transform);

                    // Opcional: Posicionar o layout corretamente dentro da sala (dependendo da sua lógica de layout)
                    layoutInstance.transform.localPosition = Vector3.zero; // Ajuste conforme necessário
                }
            }
        }
    }


    void FillRemainingRooms(List<int> roomOrder)
    {
        foreach (int roomIndex in roomOrder)
        {
            GameObject targetRoom = GameObject.Find("Room" + roomIndex);
            if (targetRoom != null)
            {
                // Cria uma lista de layouts não obrigatórios
                List<GameObject> nonMandatoryLayouts = new List<GameObject>(roomLayouts);
                
                // Escolhe um layout aleatório
                int randomLayout = Random.Range(0, nonMandatoryLayouts.Count);
                
                // Instancia o layout na posição da sala
                GameObject layoutInstance = PhotonNetwork.Instantiate(nonMandatoryLayouts[randomLayout].name, targetRoom.transform.position, Quaternion.identity);
                
                // Definindo a sala como o pai do layout instanciado
                layoutInstance.transform.SetParent(targetRoom.transform);
                
                // Opcional: Ajusta a posição local do layout dentro da sala
                layoutInstance.transform.localPosition = Vector3.zero; // Ajuste conforme necessário

                // Adiciona a sala ao conjunto de salas geradas
                generatedRooms.Add(roomIndex);
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

    string SerializeMap()
    {
        return JsonUtility.ToJson(new
        {
            numberOfRooms = this.numberOfRooms,
            generatedRooms = this.generatedRooms,
            usedRoomIndices = new List<int>(this.usedRoomIndices)
        });
    }

    void DeserializeMap(string serializedMap)
    {
        var mapData = JsonUtility.FromJson<MapData>(serializedMap);
        this.numberOfRooms = mapData.numberOfRooms;
        this.generatedRooms = mapData.generatedRooms;
        this.usedRoomIndices = new HashSet<int>(mapData.usedRoomIndices);
    }
    void RebuildMap()
    {
        foreach (int roomIndex in generatedRooms)
        {
            GameObject targetRoom = GameObject.Find("Room" + roomIndex);
            if (targetRoom == null)
            {
                PhotonNetwork.Instantiate("Room", Vector3.zero, Quaternion.identity).name = "Room" + roomIndex;
            }
        }
        Debug.Log("Map Rebuilt Successfully!");
    }

    void ShuffleList(List<int> list){
        for (int i = list.Count - 1; i > 0; i--){
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}