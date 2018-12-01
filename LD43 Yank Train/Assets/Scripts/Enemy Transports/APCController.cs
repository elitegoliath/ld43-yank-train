using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {
    private TransportSpawnController _spawnController;
    private Transform _spawnPoint;
    private Transform _destination;

    // Use this for initialization
    void Start () {
        _spawnController = gameObject.GetComponent<TransportSpawnController>();
        _spawnController.EngageSpawnPoint();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
