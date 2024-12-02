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
            GenerateMap(); // O Master Client gera o mapa
        }
    }

    void Update()
    {
        // O Master Client não precisa fazer mais nada aqui, já que ele gerou o mapa na Start()
        if (PhotonNetwork.IsMasterClient && !canWork)
        {
            photonView.RPC("NotifyMapGenerated_RPC", RpcTarget.Others); // Notifica os outros jogadores que o mapa foi gerado
        }
    }

    // Geração do mapa apenas para o Master Client
    void GenerateMap()
    {
        if (canWork)
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

            canWork = false;
            Debug.Log("Map Complete!");

            photonView.RPC("NotifyMapGenerated_RPC", RpcTarget.All); // Notifica todos os jogadores que o mapa foi gerado
        }
    }

    // RPC para notificar os outros jogadores que o mapa foi gerado
    [PunRPC]
    void NotifyMapGenerated_RPC()
    {
        // Os outros jogadores simplesmente esperam até que o mapa seja gerado pelo Master Client
        Debug.Log("Map has been generated and is now synchronized!");
        // Se necessário, aqui você pode incluir código para que os jogadores realizem alguma ação após o mapa ser gerado.
    }

    // A lógica de colocar layouts obrigatórios nas salas (mandatórios)
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

    // Lógica para preencher as salas restantes com layouts não obrigatórios
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

    // Configuração das portas entre as salas (se necessário)
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

    // Método para embaralhar a lista de salas
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
