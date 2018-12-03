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
    public float attackRange = 5f;
    public float weaponRange = 1f;
    public float weaponDelay = 2f;
    public float weaponAccuracy = 10f;
    public int weaponDamage = 1;
    public GameObject weaponMunitionPrefab;
    public GameObject foreTurret;
    public GameObject aftTurret;

    [Header("Deployment")]
    public int minPayload = 1;
    public int maxPayload = 3;
    public float deployRate = 1f;
    public GameObject payloadPrefab;
    public List<Transform> deployLocatons;

    private bool _isMoving = false;
    private bool _isStopping = false;
    private int _payloadMultiplier = 1;
    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;
    private CombatController _myCombatController;

    private void Awake()
    {
        // Set APC stats.
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        _myCombatController.SetMaxHealth(maxHealth);

        CombatController foreTurretCombatController = foreTurret.GetComponent<CombatController>();
        TurretAI foreTurretAI = foreTurret.GetComponent<TurretAI>();

        CombatController aftTurretCombatController = aftTurret.GetComponent<CombatController>();
        TurretAI aftTurretAI = aftTurret.GetComponent<TurretAI>();

        // Set combat stats for Fore Turret.
        foreTurretCombatController.SetWeaponRange(weaponRange);
        foreTurretCombatController.SetWeaponDelay(weaponDelay);
        foreTurretCombatController.SetWeaponAccuracy(weaponAccuracy);
        foreTurretCombatController.SetWeaponDamage(weaponDamage);
        foreTurretCombatController.SetWeaponMunition(weaponMunitionPrefab);
        foreTurretAI.SetAttackRange(attackRange);

        // Set combat stats for Aft Turret.
        aftTurretCombatController.SetWeaponRange(weaponRange);
        aftTurretCombatController.SetWeaponDelay(weaponDelay);
        aftTurretCombatController.SetWeaponAccuracy(weaponAccuracy);
        aftTurretCombatController.SetWeaponDamage(weaponDamage);
        aftTurretCombatController.SetWeaponMunition(weaponMunitionPrefab);
        aftTurretAI.SetAttackRange(attackRange);
    }

    private void Start ()
    {
        // Grab the spawn point for this unit.
        _spawnController = gameObject.GetComponent<TransportSpawnController>();
        _spawnPoint = _spawnController.GetSpawnPoint();
        _destination = _spawnController.GetDestination();
        _payloadMultiplier = _spawnController.payloadMultiplier;
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
        // TODO: Have alternative spawn locations within APC so that we don't rely on
        // a single point and if within nav barrier payload won't spawn.

        // Dump payload.
        int payload = Random.Range(minPayload, (maxPayload * _payloadMultiplier));

        _myRigidBody.bodyType = RigidbodyType2D.Kinematic;

        for (int i = 0; i < payload; i++) {
            Invoke("DropPayload", deployRate * i);
        }
    }

    private void DropPayload()
    {
        GameObject freshSpawn = Instantiate(payloadPrefab);
        freshSpawn.transform.position = transform.position;
        AIController freshSpawnAI = freshSpawn.GetComponent<AIController>();
        freshSpawnAI.DeployToRandomLocation(deployLocatons);
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
