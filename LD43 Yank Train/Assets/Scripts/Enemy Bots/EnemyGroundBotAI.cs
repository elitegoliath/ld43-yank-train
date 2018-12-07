using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundBotAI : MonoBehaviour
{
    #region Instantiate Variables Used In Class('s)

    [Header("Navigation")]
    public float checkRangesCooldown = 0.2f;

    [Header("Characteristics")]
    public int maxHealth = 10;
    public int armor = 1;
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

    [Header("Effects")]
    public GameObject deathDebris;
    private CombatController _myCombatController;
    private AIController _myAIController;

    #endregion Instantiate Variables Used In Class('s)

    /// <summary>
    /// Sets instance variable to current gameObject(s) the they respectivly relate to.
    /// </summary>
    /// <see cref="_myCombatController"/>
    /// <seealso cref="_myAIController"/>
    private void Awake()
    {
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myAIController = gameObject.GetComponent<AIController>();
    }

    /// <summary>
    /// Upon call sets all of the enemy AI params to values defined above,
    ///     or to an instance of whatever the current gameObject
    /// </summary>
    private void Start()
    {
        #region consts for readability

        const string _player = "Player";
        const string _enemyRegister = "registerEnemy";

        #endregion consts for readability

        // TODO: Categorize these.
        _myAIController.SetAttackDelay(attackDelay);
        _myAIController.SetAttackingTurnSpeed(attackingTurnSpeed);
        _myAIController.SetAttackRange(attackRange);
        _myAIController.SetEnagementRange(engagementRange);
        _myAIController.SetRangedWeaponCombatController(rangedWeaponCombatController);
        _myCombatController.SetDebris(deathDebris);

        // Initialize nav.
        _myAIController.SetCheckRangesCooldown(checkRangesCooldown);

        // Initialize characteristics.
        _myCombatController.SetArmor(armor);
        _myCombatController.SetCurrentHealth(maxHealth);
        _myCombatController.SetMaxHealth(maxHealth);

        // Initialize weapon.
        rangedWeaponCombatController.SetRangedWeaponStats(accuracy, weaponRange, weaponDamage, rangedWeaponMunition);

        // Initialize target. Player is gona be the default target for now.
        GameObject player = GameObject.FindGameObjectWithTag(_player);
        _myAIController.SetTarget(player);

        // Register self with Wave Controller.
        EventManager.TriggerEvent(_enemyRegister);
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
        #region Const for readbility

        const string _enemyDeath = "enemyDeath";

        #endregion Const for readbility

        EventManager.TriggerEvent(_enemyDeath);
    }

    /// <summary>
    /// Public method for telling the AI to deploy to one of the locations given.
    /// Each location will be tested before deployment begins.
    /// </summary>
    /// <param name="waypoints">
    /// A list of the Cords that the current set of transports will be using
    ///     to "drop off" or deploy the enemy bots.
    /// </param>
    public void DeployToRandomLocation(List<Transform> waypoints)
    {
        _myAIController.DeployToRandomLocation(waypoints);
    }
}