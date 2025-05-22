using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NeighborRoom
{
    public GameObject RoomObject { get; }
    public int Direction { get; }

    public NeighborRoom(GameObject roomObject, int direction)
    {
        RoomObject = roomObject;
        Direction = direction;
    }
}

public class RoomSpawner : MonoBehaviour
{
    private readonly string[] spawnerNames = { "RoomSpawnerTop", "RoomSpawnerBottom", "RoomSpawnerRight", "RoomSpawnerLeft" };
    private readonly string[] doorNames = { "Bottom", "Top", "Left", "Right" };

    public GameObject roomPrefab;
    public Transform[] spawners;
    public Transform[] doors;
    public MapManager mapManager;
    public int maxRooms = 16;

    private void Start()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        if (mapManager.numberOfRooms <= maxRooms)
        {
            int direction = Random.Range(0, 4);
            TrySpawnRoom(direction);
        }
    }

    private void TrySpawnRoom(int direction, int attempt = 0)
    {
        if (attempt > 3)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        bool canSpawn = spawners[direction].gameObject.activeSelf && !RoomExists(direction);

        if (!canSpawn)
        {
            TrySpawnRoom(Random.Range(0, 4), attempt + 1);
            return;
        }

        SpawnRoom(direction);
    }

    private void SpawnRoom(int direction)
    {
        GameObject newRoom = Instantiate(roomPrefab, spawners[direction].position, Quaternion.identity);
        newRoom.name = $"Room{mapManager.numberOfRooms}";
        doors[direction].gameObject.SetActive(false);
        ConfigureRoom(newRoom, doorNames[direction]);
        DisableOppositeSpawner(direction);
        mapManager.numberOfRooms++;
    }

    private void ConfigureRoom(GameObject room, string disabledDoor)
    {
        room.transform.Find(disabledDoor).gameObject.SetActive(false);
        foreach (Transform door in room.transform)
        {
            if (door.name != disabledDoor)
                door.gameObject.SetActive(true);
        }
    }

    private void DisableOppositeSpawner(int direction)
    {
        int opposite = (direction + 2) % 4;
        spawners[opposite].gameObject.SetActive(false);
    }

    private bool RoomExists(int direction)
    {
        return Physics2D.OverlapCircle(spawners[direction].position, 1f, LayerMask.GetMask("Room"));
    }

    public NeighborRoom[] GetNeighborRooms()
    {
        List<NeighborRoom> neighbors = new List<NeighborRoom>();

        for (int i = 0; i < 4; i++)
        {
            if (!spawners[i].gameObject.activeSelf || !RoomExists(i))
                continue;

            GameObject neighborRoom = Physics2D.OverlapCircle(spawners[i].position, 1f, LayerMask.GetMask("Room")).gameObject;
            neighbors.Add(new NeighborRoom(neighborRoom, i));
        }

        return neighbors.ToArray();
    }

    public void ConfigureNeighborDoors(NeighborRoom[] neighbors)
    {
        foreach (NeighborRoom neighbor in neighbors)
        {
            neighbor.RoomObject.transform.Find(doorNames[neighbor.Direction]).gameObject.SetActive(false);
            doors[neighbor.Direction].gameObject.SetActive(false);
        }
    }
}
