﻿using PolyNav;
using System.Collections.Generic;
using UnityEngine;

public class APCController : MonoBehaviour
{
    #region Instantiate Variables Used In Class('s)

    #region Engine

    [Header("Engines")]
    public float speed = 1f;
    public float drag = 1f;
    public float angularDrag = 1f;
    public float torque = 1f;

    #endregion Engine

    #region Armor

    [Header("Armor")]
    public int maxHealth = 50;
    public int armor = 5;

    #endregion Armor

    #region Weapons

    [Header("Weapons")]
    public float attackRange = 5f;
    public float weaponRange = 1f;
    public float weaponDelay = 2f;
    public float weaponAccuracy = 10f;
    public int weaponDamage = 1;
    public GameObject weaponMunitionPrefab;
    public GameObject foreTurret;
    public GameObject aftTurret;

    #endregion Weapons

    #region Deployment

    [Header("Deployment")]
    public int minPayload = 1;
    public int maxPayload = 3;
    public int companionSpawnChance = 100;
    public float deployRate = 1f;
    public GameObject payloadPrefab;
    public GameObject companionLightIndicator;
    public GameObject companionPrefab;
    public List<Transform> deployLocatons;

    #endregion Deployment

    #region Effects

    [Header("Effects")]
    public GameObject deathDebris;
    private bool _isMoving = false;
    private bool _isStopping = false;
    private int _payloadMultiplier = 1;
    private TransportSpawnController _spawnController;
    private Rigidbody2D _myRigidBody;
    private Vector2 _spawnPoint;
    private Vector2 _destination;
    private CombatController _myCombatController;
    private PolyNav2D _navMapRef;
    private PolyNavObstacle _myObstacle;

    #endregion Effects

    #endregion Instantiate Variables Used In Class('s)

    /// <summary>
    /// Sets up Transport stats, as well as its turret params
    /// </summary>
    /// <seealso cref="TurretAI"/>
    private void Awake()
    {
        // Set APC stats.
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        _myCombatController.SetMaxHealth(maxHealth);
        _myCombatController.SetDebris(deathDebris);

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

    /// <summary>
    /// Instantiation of spawn logic per Transport
    /// </summary>
    private void Start()
    {
        // Grab the spawn point for this unit.
        _spawnController = gameObject.GetComponent<TransportSpawnController>();
        _spawnPoint = _spawnController.GetSpawnPoint();
        _destination = _spawnController.GetDestination();
        _payloadMultiplier = _spawnController.PayloadMultiplier;
        _spawnController.TearDown();

        _navMapRef = FindObjectOfType<PolyNav2D>();
        _myObstacle = gameObject.GetComponent<PolyNavObstacle>();

        // Move to spawn location.
        transform.position = _spawnPoint;

        // Apply movement towards destination.
        MoveToDestination();
    }

    /// <summary>
    /// Calls methods per frame to check state of Transport
    /// </summary>
    /// <see cref="CheckDestinationReached"/>
    /// <seealso cref="CheckAPCHasStopped"/>
    private void Update()
    {
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
    /// <see cref="DeployOrder"/>
    private void StopOrder()
    {
        _myRigidBody.AddTorque(torque);
        _myRigidBody.drag = drag;
        _myRigidBody.angularDrag = angularDrag;
    }

    /// <summary>
    /// When called handles logic of Transport's spawning enemy bots.
    /// </summary>
    private void DeployOrder()
    {
        #region Consts for readability

        const string _becomeClutter = "MakeIntoObstacle";
        const string _dropCompanion = "DropCompanionPayload";
        const string _dropPayload = "DropPayload";

        #endregion Consts for readability

        // TODO: Have alternative spawn locations within APC so that we don't rely on
        // a single point and if within nav barrier payload won't spawn.

        // Dump payload.
        int payload = Random.Range(minPayload, (maxPayload * _payloadMultiplier));

        _myRigidBody.bodyType = RigidbodyType2D.Kinematic;

        for (int i = 0; i < payload; i++)
        {
            float companionOrNot = Random.Range(0f, 100f);

            if (companionOrNot <= companionSpawnChance)
            {
                ///<see cref="DropCompanionPayload"/>
                Invoke(_dropCompanion, deployRate * i);
            }
            else
            {
                ///<see cref="DropPayload"/>
                Invoke(_dropPayload, deployRate * i);
            }
        }
        ///<see cref="MakeIntoObstacle"/>
        Invoke(_becomeClutter, payload);
    }

    /// <summary>
    /// Called upon "death" of transport to make it into "clutter" or "trash" on the map.
    ///     Removable by companion explosion<see cref="OnDestroy"/>.
    /// </summary>
    private void MakeIntoObstacle()
    {
        _navMapRef.AddObstacle(_myObstacle);
    }

    /// <summary>
    /// Called when companion used to "remove" the obstacle.
    /// </summary>
    private void OnDestroy()
    {
        _navMapRef.RemoveObstacle(_myObstacle);
    }

    /// <summary>
    /// Checks when a collision is detected if it is valid to remove a section of map clutter.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        #region Const for readability

        const string _largeExplosion = "LargeExplosion";

        #endregion Const for readability

        if (collision.tag == _largeExplosion)
        {
            _myCombatController.Die();
        }
    }

    /// <summary>
    /// Event called to deploy AI's from transport(s).
    /// </summary>
    private void DropPayload()
    {
        GameObject freshSpawn = Instantiate(payloadPrefab);
        freshSpawn.transform.position = transform.position;
        AIController freshSpawnAI = freshSpawn.GetComponent<AIController>();
        freshSpawnAI.DeployToRandomLocation(deployLocatons);
    }

    /// <summary>
    /// Similar to DropPayload() but controls deployment of Companionable AI Enenmies.
    /// </summary>
    private void DropCompanionPayload()
    {
        GameObject freshSpawn = Instantiate(payloadPrefab);
        freshSpawn.transform.position = transform.position;
        AIController freshSpawnAI = freshSpawn.GetComponent<AIController>();
        CombatController freshSpawnCombatController = freshSpawn.GetComponent<CombatController>();
        freshSpawnCombatController.InstantiateCompanionOnDeath(companionPrefab, companionLightIndicator);
        freshSpawnAI.DeployToRandomLocation(deployLocatons);
    }

    /// <summary>
    /// Checks if within a specific range of the destination. If close, initiated stop order.
    /// </summary>
    private void CheckDestinationReached()
    {
        if (_isMoving == true)
        {
            float distance = Vector2.Distance(transform.position, _destination);

            if (distance <= 2f)
            {
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
        if (_isStopping == true)
        {
            if (_myRigidBody.velocity.magnitude <= 0f)
            {
                _isStopping = false;
                DeployOrder();
            }
        }
    }
}