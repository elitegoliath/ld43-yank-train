using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {
    private TransportSpawnController _spawnController;
    private Vector2 _spawnPoint;
    private Vector2 _destination;

    // Use this for initialization
    void Start () {
        // Grab the spawn point for this unit.
        _spawnController = gameObject.GetComponent<TransportSpawnController>();
        _spawnPoint = _spawnController.GetSpawnPoint();

        // Move to spawn location.
        gameObject.transform.position = _spawnPoint;

        // TODO: Get destination point.
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
