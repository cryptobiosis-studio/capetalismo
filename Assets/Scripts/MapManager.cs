using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MapManager : MonoBehaviourPunCallbacks
{
    public int numberOfRooms;
    public GameObject startRoom;

    public GameObject[] roomLayouts;
    public bool canWork;

    public int rdIndex;  
    public int waterIndex;  
    public int chestIndex;  

    private List<int> generatedRooms;  
    private int emptyRoomIndex = -1;  

    public GameObject lastRoom;

    void Start()
    {
        startRoom = GameObject.Find("Room");
        canWork = true;
        generatedRooms = new List<int>();

        if (PhotonNetwork.IsMasterClient)
        {
            GenerateMap();
        }
    }

    void GenerateMap()
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

        // Sincroniza os dados do mapa com todos os jogadores
        SyncMapData();
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
                generatedRooms.Add(randomRoomIndex);
            }
        }
    }

    void FillRemainingRooms(List<int> roomOrder)
    {
        foreach (int roomIndex in roomOrder)
        {
            generatedRooms.Add(roomIndex);
        }
    }

    void ConfigureDoors()
    {
        Debug.Log("Configuring Doors...");
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

    void SyncMapData()
    {
        photonView.RPC("SyncMapToPlayers", RpcTarget.Others, generatedRooms.ToArray());
    }

    [PunRPC]
    void SyncMapToPlayers(int[] roomData)
    {
        foreach (int roomIndex in roomData)
        {
            Debug.Log("Syncing Room: " + roomIndex);
        }
    }
}
