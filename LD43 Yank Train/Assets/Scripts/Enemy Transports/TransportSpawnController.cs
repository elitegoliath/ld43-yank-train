using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportSpawnController : MonoBehaviour {
    private List<BoxCollider2D> _spawnSectors = new List<BoxCollider2D>();

	// Use this for initialization
	void Start () {
        GameObject[] spawnSectorObjects = GameObject.FindGameObjectsWithTag("SpawnSector");
        for (int i = 0; i < spawnSectorObjects.Length; i++) {
            _spawnSectors.Add(spawnSectorObjects[i].GetComponent<BoxCollider2D>());
        }
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

    private bool CheckForCollision(Vector2 spawnPoint)
    {

        // TODO: Check if space is occupied by another transport.

        // TODO: Return bool based on whether the space is occupied.
        return false;
    }
}
