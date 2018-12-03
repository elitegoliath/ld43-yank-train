using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class EnemyGroundBotAI : MonoBehaviour {
    [Header("Navigation")]
    public float checkRangesCooldown = 0.2f;
    
    [Header("Characteristics")]
    public int maxHealth = 10;
    public int armor = 1;
    public float moveSpeed = 100f;
    public float attackingTurnSpeed = 30f;
    public float engagementRange = 3f;
    
    [Header("Weapon Stats")]
    public float attackRange = 5f;
    public float attackDelay = 1f;
    public float accuracy = 10f;
    public int weaponDamage = 2;
    public float weaponRange = 1f;
    public CombatController rangedWeaponCombatController;
    public GameObject rangedWeaponMunition;

    // Private Properties
    private CombatController _myCombatController;
    private Rigidbody2D _myRigidBody;
    private PolyNavAgent _myNavAgent;
    private AIController _myAIController;

    private void Awake()
    {
        // Get components
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myNavAgent = gameObject.GetComponent<PolyNavAgent>();
        _myAIController = gameObject.GetComponent<AIController>();
    }

    private void Start()
    {
        // TODO: Categorize these.
        _myAIController.SetAttackRange(attackRange);
        _myAIController.SetEnagementRange(engagementRange);
        _myAIController.SetAttackDelay(attackDelay);
        _myAIController.SetRangedWeaponCombatController(rangedWeaponCombatController);
        _myAIController.SetAttackingTurnSpeed(attackingTurnSpeed);

        // Initialize nav.
        _myAIController.SetCheckRangesCooldown(checkRangesCooldown);

        // Initialize characteristics.
        _myCombatController.SetMaxHealth(maxHealth);
        _myCombatController.SetCurrentHealth(maxHealth);
        _myCombatController.SetArmor(armor);

        // Initialize weapon.
        rangedWeaponCombatController.SetRangedWeaponStats(accuracy, weaponRange, weaponDamage, rangedWeaponMunition);

        // Initialize target. Player is gona be the default target for now.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _myAIController.SetTarget(player);

        // Register self with Wave Controller.
        EventManager.TriggerEvent("registerEnemy");
    }

    /// <summary>
    /// For the most part, the Update function in this class acts as an AI registry.
    /// They all know to behave with one another.
    /// </summary>
    private void Update()
    {
        // These ranges drive the aggressive behavior of this robot.
        _myAIController.AITrackTarget();

        // Fuck it, we're always lookin to engage. Every fuckin frame.
        _myAIController.AIEngageTarget();

        // If within weapon range, fire fire fire!!
        _myAIController.AIAttackTarget();
    }

    private void OnDestroy()
    {
        EventManager.TriggerEvent("enemyDeath");
    }

    /// <summary>
    /// Public method for telling the AI to deploy to one of the locations given.
    /// Each location will be tested before deployment begins.
    /// </summary>
    /// <param name="waypoints"></param>
    public void DeployToRandomLocation(List<Transform> waypoints)
    {
        _myAIController.DeployToRandomLocation(waypoints);
    }
}
