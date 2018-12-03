using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class APCController : MonoBehaviour {
    [Header("Engines")]
    public float speed = 1f;
    public float drag = 1f;
    public float angularDrag = 1f;
    public float torque = 1f;
    
    [Header("Armor")]
    public int maxHealth = 50;
    public int armor = 5;

    [Header("Weapons")]
    public float weaponRange = 4f;
    public float weaponDelay = 2f;
    public float weaponAccuracy = 10f;
    public int weaponDamage = 1;
    public GameObject weaponMunitionPrefab;
    public CombatController foreTurretCombatController;
    public CombatController aftTurretCombatController;

    [Header("Deployment")]
    public int minPayload = 1;
    public int maxPayload = 3;
    public GameObject payloadPrefab;
    public List<Transform> deployLocatons;

    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;
    private bool _isMoving = false;
    private bool _isStopping = false;
    private bool _isWaitingToBecomeObstacle = false;

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
        //_spawnPoint = _spawnController.GetSpawnPoint();
        //_destination = _spawnController.GetDestination();
        //_spawnController.TearDown();

        // Move to spawn location.
        //transform.position = _spawnPoint;

        // Apply movement towards destination.
        //MoveToDestination();

        // TODO: Remove when done testing.
        DeployOrder();
    }
	
	private void Update () {
        //CheckDestinationReached();
        //CheckAPCHasStopped();

        //if (_isWaitingToBecomeObstacle == true) {

        //}
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
        // TODO: When the event system can handle event with parameters, add wave multiplier to payload.

        // Dump payload.
        int payload = Random.Range(minPayload, maxPayload);

        for (int i = 0; i < payload; i++) {
            GameObject freshSpawn = Instantiate(payloadPrefab);
            freshSpawn.transform.position = transform.position;
            AIController freshSpawnAI = freshSpawn.GetComponent<AIController>();
            freshSpawnAI.DeployToRandomLocation(deployLocatons);
        }

        gameObject.AddComponent<PolyNavObstacle>();
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
