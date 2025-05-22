using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Room Configuration")]
    public int numberOfRooms;
    public GameObject startRoom;

    [Header("Layouts")]
    public GameObject[] roomLayouts;
    public GameObject lastRoomLayout;

    [Header("Required Layouts (two of each)")]
    public int rdIndex;
    public int waterIndex;
    public int chestIndex;

    private HashSet<int> usedRoomIndices;
    private List<int> generatedRooms;
    public bool canWork;

    public GameObject lastRoom;

    void Start()
    {
        LoadLayouts();
        startRoom = GameObject.Find("Room");
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
            GenerateMapLayout();
        }
    }

    void LoadLayouts()
    {
        Object[] loaded = Resources.LoadAll("Layouts", typeof(GameObject));
        List<GameObject> layouts = new List<GameObject>();

        foreach (var obj in loaded)
        {
            if (obj.name.StartsWith("Layout"))
                layouts.Add((GameObject)obj);
            else if (obj.name == "LastRoomLayout")
                lastRoomLayout = (GameObject)obj;
        }

        roomLayouts = layouts.ToArray();

        if (lastRoomLayout == null)
            Debug.LogError("LastRoomLayout not found in Resources/Layouts");

        if (roomLayouts.Length == 0)
            Debug.LogError("No layouts found in Resources/Layouts");
    }

    void GenerateMapLayout()
    {
        List<int> roomOrder = new List<int>(numberOfRooms);
        for (int i = 0; i < numberOfRooms; i++)
            roomOrder.Add(i);

        ShuffleList(roomOrder);
        PlaceRequiredLayouts(roomOrder);
        PlaceRemainingLayouts(roomOrder);
        SetupAllNeighborDoors();

        Debug.Log("Map Complete! Rooms generated: " + generatedRooms.Count);
    }

    void PlaceRequiredLayouts(List<int> roomOrder)
    {
        int lastIndex = numberOfRooms - 1;
        int[] required = { rdIndex, waterIndex, chestIndex };

        foreach (int layoutIdx in required)
        {
            int placed = 0;
            while (placed < 2 && roomOrder.Count > 0)
            {
                int rndRoom = roomOrder[Random.Range(0, roomOrder.Count)];
                if (rndRoom == lastIndex) continue;

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

    void PlaceRemainingLayouts(List<int> roomOrder)
    {
        int lastIndex = numberOfRooms - 1;
        roomOrder.Remove(lastIndex);

        foreach (int idx in roomOrder)
        {
            var target = GameObject.Find("Room" + idx);
            if (target == null) continue;

            List<GameObject> available = new List<GameObject>(roomLayouts);
            List<int> mandatory = new List<int> { rdIndex, waterIndex, chestIndex };
            mandatory.Sort();
            for (int i = mandatory.Count - 1; i >= 0; i--)
                available.RemoveAt(mandatory[i]);

            int pick = Random.Range(0, available.Count);
            Instantiate(available[pick], target.transform);
            generatedRooms.Add(idx);
        }

        var lastTarget = GameObject.Find("Room" + lastIndex);
        if (lastTarget != null)
        {
            lastRoom = lastTarget;
            Instantiate(lastRoomLayout, lastRoom.transform);
            generatedRooms.Add(lastIndex);
            Debug.Log("Last room placed: " + lastRoom.name);
        }
    }

    void SetupAllNeighborDoors()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            var roomObj = GameObject.Find("Room" + i);
            if (roomObj == null) continue;

            var sp = roomObj.GetComponent<RoomSpawner>();
            if (sp != null)
                sp.ConfigureNeighborDoors(sp.GetNeighborRooms());
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
