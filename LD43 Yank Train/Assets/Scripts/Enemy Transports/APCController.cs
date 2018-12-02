using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour {
    // Vehicle Attributes
    public float speed = 1f;
    public float drag = 1f;
    public float angularDrag = 1f;
    public float torque = 1f;

    // Armor Stats
    public int maxHealth = 50;
    public int armor = 5;

    // Weapon Stats
    public float weaponRange = 4f;
    public float weaponDelay = 2f;
    public float weaponAccuracy = 10f;
    public int weaponDamage = 1;
    public GameObject weaponMunitionPrefab;
    public CombatController foreTurretCombatController;
    public CombatController aftTurretCombatController;

    // Deployment Vars
    public GameObject payloadPrefab;
    public Transform[] deployDirections;

    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;
    private bool _isMoving = false;
    private bool _isStopping = false;

    private void Awake()
    {
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        // Set combat stats for Fore Turret.
        foreTurretCombatController.SetWeaponRange(weaponRange);
        foreTurretCombatController.SetWeaponDelay(weaponDelay);
        foreTurretCombatController.SetWeaponAccuracy(weaponAccuracy);
        foreTurretCombatController.SetWeaponDamage(weaponDamage);
        foreTurretCombatController.SetWeaponMunition(weaponMunitionPrefab);

        // Set combat stats for Aft Turret.
        aftTurretCombatController.SetWeaponRange(weaponRange);
        aftTurretCombatController.SetWeaponDelay(weaponDelay);
        aftTurretCombatController.SetWeaponAccuracy(weaponAccuracy);
        aftTurretCombatController.SetWeaponDamage(weaponDamage);
        aftTurretCombatController.SetWeaponMunition(weaponMunitionPrefab);
    }

    private void Start ()
    {
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
        // TODO: Spawn Troops and 
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
