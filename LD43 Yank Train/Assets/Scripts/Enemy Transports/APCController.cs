using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {
    public float speed = 1f;

    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;

    // Use this for initialization
    void Start () {
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        // Grab the spawn point for this unit.
        _spawnController = gameObject.GetComponent<TransportSpawnController>();
        _spawnPoint = _spawnController.GetSpawnPoint();
        _destination = _spawnController.GetDestination();
        _spawnController.TearDown();

        // Move to spawn location.
        transform.position = _spawnPoint;

        // Apply movement towards destination.
        MoveToDestination();
    }
	
	// Update is called once per frame
	void Update () {
        // TODO: Check distance from destination. If close, execute Stop Order.
    }

    /// <summary>
    /// Face target destination then advance towards it at a pre-defined speed.
    /// </summary>
    private void MoveToDestination()
    {
        Vector2 direction = new Vector2(
            _destination.x - transform.position.x,
            _destination.y - transform.position.y
            );

        transform.up = direction;

        _myRigidBody.AddForce(transform.up * speed);
    }

    private void StopOrder()
    {
        // TODO: Apply rotation and no longer apply force.
    }
}
