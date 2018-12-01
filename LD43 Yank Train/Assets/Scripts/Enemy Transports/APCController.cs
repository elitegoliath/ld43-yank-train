using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {

    private TransportSpawnController spawnController = new TransportSpawnController();

    private Transform spawnPoint;
    private Transform destination;

    // Use this for initialization
    void Start () {
        //spawnPoint = spawnController.GenerateSpawnPoint();
        //destination = spawnController.GenerateWaypoint();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
