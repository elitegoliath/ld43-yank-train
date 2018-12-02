using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {
    public float speed = 1f;
    public float torque = 1f;

    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;
    private bool _isMoving = false;
    
    private void Start () {
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
	
	private void Update () {
        CheckDestinationReached();
    }

    /// <summary>
    /// Face target destination then advance towards it at a pre-defined speed.
    /// </summary>
    private void MoveToDestination()
    {
        Debug.Log("Move Order Executed");
        Vector2 direction = new Vector2(
            _destination.x - transform.position.x,
            _destination.y - transform.position.y
            );

        transform.up = direction;

        _myRigidBody.AddForce(transform.up * speed);

        _isMoving = true;
    }

    private void StopOrder()
    {
        Debug.Log("Stop Order Executed");
        // Remove the driving force behind the vehicle.
        _myRigidBody.velocity = Vector2.zero;

        // Applies some torque for dramatic effect
        float torqueDirection = Random.Range(0f, 100f);
        float torqueToApply = torque;

        if (torqueDirection <= 50f) {
            torqueToApply = -torque;
        }

        _myRigidBody.AddTorque(torqueToApply);

        // TODO: Once stopped, execute Deploy Order.

        // TODO: Construct navigation components to make this an obstacle.
    }

    private void DeployOrder()
    {
        // TODO: Spawn Troops.
    }

    /// <summary>
    /// Checks if within a specific range of the destination. If close, initiated stop order.
    /// </summary>
    private void CheckDestinationReached()
    {
        if (_isMoving == true) {

        }
    }
}
