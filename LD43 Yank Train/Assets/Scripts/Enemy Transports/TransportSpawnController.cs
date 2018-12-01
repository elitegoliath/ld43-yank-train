using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportSpawnController : MonoBehaviour {
    public GameObject spawnCollider;

    private List<BoxCollider2D> _spawnSectors = new List<BoxCollider2D>();
    private GameObject _spawnColliderClone;
    private SpawnColliderController _spawnColliderController;

    // Use this for initialization
    void Start () {
        // Compile list of spawn sector colliders.
        GameObject[] spawnSectorObjects = GameObject.FindGameObjectsWithTag("SpawnSector");
        for (int i = 0; i < spawnSectorObjects.Length; i++) {
            _spawnSectors.Add(spawnSectorObjects[i].GetComponent<BoxCollider2D>());
        }

        // Instantiate the collider that will be used to check for free space.
        _spawnColliderClone = Instantiate(spawnCollider);
        _spawnColliderController = _spawnColliderClone.GetComponent<SpawnColliderController>();
    }

    /// <summary>
    /// Returns a Vector2 of a free location for the transport to spawn.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetSpawnPoint()
    {
        // Gets a potential spawn point.
        Vector2 foundSpawnPoint = FindSpawnPoint();

        // Check to see if it's free.
        bool canSpawnHere = CheckForCollision(foundSpawnPoint);

        Debug.Log(canSpawnHere);

        // If not, invoke recursion until one is found.
        if(canSpawnHere == false) {
            foundSpawnPoint = GetSpawnPoint();
        }

        return foundSpawnPoint;
    }

    public void GetWaypoint()
    {
        // TODO: Find a random location inside the arena to go to.
    }

    /// <summary>
    /// Finds and returns a Vector2 coordinate inside one of the sectors.
    /// </summary>
    /// <returns></returns>
    private Vector2 FindSpawnPoint()
    {
        BoxCollider2D chosenSector = _spawnSectors[Random.Range(0, _spawnSectors.Count)];

        Bounds bounds = chosenSector.bounds;
        Vector2 center = bounds.center;

        float x = Random.Range(center.x - bounds.extents.x, center.x + bounds.extents.x);
        float y = Random.Range(center.y - bounds.extents.y, center.y + bounds.extents.y);

        return new Vector2(x, y);
    }

    /// <summary>
    /// Checks for collisions with other Transports and returns whether the space is free of them.
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <returns></returns>
    private bool CheckForCollision(Vector2 spawnPoint)
    {
        _spawnColliderClone.transform.position = spawnPoint;
        bool isFree = _spawnColliderController.CheckIfFree();

        return isFree;
    }
}
