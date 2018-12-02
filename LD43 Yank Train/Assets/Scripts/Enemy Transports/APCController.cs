using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {
    public float speed = 1f;
    public float drag = 1f;
    public float angularDrag = 1f;
    public float torque = 1f;

    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;
    private bool _isMoving = false;
    private bool _isStopping = false;

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
        CheckAPCHasStopped();
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

        _isMoving = true;
    }

    /// <summary>
    /// When the APC is ready to stop, it will gradually slow it's velocity until completely stopped.
    /// Will then execute the Deploy Order.
    /// </summary>
    private void StopOrder()
    {
        _myRigidBody.AddTorque(torque);
        _myRigidBody.drag = drag;
        _myRigidBody.angularDrag = angularDrag;
    }

    private void DeployOrder()
    {
        Debug.Log("Deploy Order Executed");
        // TODO: Construct navigation components to make this an obstacle.

        // TODO: Spawn Troops.
    }

    /// <summary>
    /// Checks if within a specific range of the destination. If close, initiated stop order.
    /// </summary>
    private void CheckDestinationReached()
    {
        if (_isMoving == true) {
            float distance = Vector2.Distance(transform.position, _destination);
            
            if (distance <= 2f) {
                _isMoving = false;
                _isStopping = true;
                StopOrder();
            }
        }
    }

    /// <summary>
    /// Once the AP has stopped, discard uneeded AI and deploy!
    /// </summary>
    private void CheckAPCHasStopped()
    {
        if (_isStopping == true) {
            if (_myRigidBody.velocity.magnitude <= 0f) {
                _isStopping = false;
                DeployOrder();
            }
        }
    }
}
