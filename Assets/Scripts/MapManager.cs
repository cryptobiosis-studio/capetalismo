using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Configuração de salas")]
    public int numberOfRooms;
    public GameObject startRoom;

    [Header("Layouts")]
    public GameObject[] roomLayouts;
    public GameObject lastRoomLayout;

    [Header("Layouts Obrigatórios (dois de cada)")]
    public int rdIndex;
    public int waterIndex;
    public int chestIndex;

    private HashSet<int> usedRoomIndices;
    private List<int> generatedRooms;
    public bool canWork;

    public GameObject lastRoom;

    void Start()
    {
        startRoom = GameObject.Find("Room0");
        usedRoomIndices = new HashSet<int>();
        generatedRooms = new List<int>();
        canWork = true;
    }

    void Update()
    {
        int maxRooms = startRoom.GetComponent<RoomSpawner>().maxRooms;
        if (canWork && numberOfRooms >= maxRooms)
        {
            canWork = false;
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        List<int> roomOrder = new List<int>(numberOfRooms);
        for (int i = 0; i < numberOfRooms; i++)
            roomOrder.Add(i);

        ShuffleList(roomOrder);

        PlaceMandatoryLayouts(roomOrder);

        FillRemainingRooms(roomOrder);

        ConfigureDoors();

        Debug.Log("Map Complete! Salas geradas: " + generatedRooms.Count);
    }

    void PlaceMandatoryLayouts(List<int> roomOrder)
    {
        int lastIndex = numberOfRooms - 1;
        int[] mandatory = { rdIndex, waterIndex, chestIndex };

        foreach (int layoutIdx in mandatory)
        {
            int placed = 0;
            while (placed < 2 && roomOrder.Count > 0)
            {
                int rndRoom = roomOrder[Random.Range(0, roomOrder.Count)];
                if (rndRoom == lastIndex)
                    continue;

                roomOrder.Remove(rndRoom);
                usedRoomIndices.Add(rndRoom);
                generatedRooms.Add(rndRoom);

                var target = GameObject.Find("Room" + rndRoom);
                if (target != null)
                    Instantiate(roomLayouts[layoutIdx], target.transform);

                placed++;
            }
        }
    }

    void FillRemainingRooms(List<int> roomOrder)
    {
        int lastIndex = numberOfRooms - 1;

        roomOrder.Remove(lastIndex);

        foreach (int idx in roomOrder)
        {
            var target = GameObject.Find("Room" + idx);
            if (target == null) continue;

            List<GameObject> avail = new List<GameObject>(roomLayouts);
            List<int> mandatory = new List<int> { rdIndex, waterIndex, chestIndex };
            mandatory.Sort();
            for (int i = mandatory.Count - 1; i >= 0; i--)
                avail.RemoveAt(mandatory[i]);

            int pick = Random.Range(0, avail.Count);
            Instantiate(avail[pick], target.transform);
            generatedRooms.Add(idx);
        }

        var lastTarget = GameObject.Find("Room" + lastIndex);
        if (lastTarget != null)
        {
            lastRoom = lastTarget;
            Instantiate(lastRoomLayout, lastRoom.transform);
            generatedRooms.Add(lastIndex);
            Debug.Log("Last room correta: " + lastRoom.name);
        }
    }

    void ConfigureDoors()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            var roomObj = GameObject.Find("Room" + i);
            if (roomObj == null) continue;

            var sp = roomObj.GetComponent<RoomSpawner>();
            if (sp != null)
                sp.SetupNeighborDoors(sp.NeighborRooms());
        }
    }

    void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}
