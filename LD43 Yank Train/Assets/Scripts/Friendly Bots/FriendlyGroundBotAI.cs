using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class FriendlyGroundBotAI : MonoBehaviour {
    [Header("Navigation")]
    public float checkRangesCooldown = 0.2f;

    [Header("Characteristics")]
    public int maxHealth = 10;
    public int armor = 1;
    public float attackingTurnSpeed = 30f;
    public float followRange = 2f;
    public float speedVarience = 2f;

    [Header("Weapon Stats")]
    public float attackRange = 5f;
    public float attackDelay = 1f;
    public float accuracy = 10f;
    public int weaponDamage = 2;
    public float weaponRange = 1f;
    public CombatController rangedWeaponCombatController;
    public GameObject rangedWeaponMunition;

    [Header("Effects")]
    public GameObject selfDestructIndicator;
    public GameObject deathDebris;
    public GameObject detonationEffect;

    // Private Properties
    private CombatController _myCombatController;
    private AIController _myAIController;
    private bool _isAIActive = true;

    private void Awake()
    {
        // Get components
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myAIController = gameObject.GetComponent<AIController>();
    }

    private void Start()
    {
        // TODO: Categorize these.
        _myAIController.SetAttackRange(attackRange);
        _myAIController.SetFollowRange(followRange);
        _myAIController.SetAttackDelay(attackDelay);
        _myAIController.SetRangedWeaponCombatController(rangedWeaponCombatController);
        _myAIController.SetAttackingTurnSpeed(attackingTurnSpeed);
        _myAIController.SetPlayer();
        _myAIController.InstantiateSpeedDifferential(speedVarience);
        _myCombatController.SetDebris(deathDebris);
        _myCombatController.SetDetonationEffect(detonationEffect);

        // Initialize nav.
        _myAIController.SetCheckRangesCooldown(checkRangesCooldown);

        // Initialize characteristics.
        _myCombatController.SetMaxHealth(maxHealth);
        _myCombatController.SetCurrentHealth(maxHealth);
        _myCombatController.SetArmor(armor);

        // Initialize weapon.
        rangedWeaponCombatController.SetRangedWeaponStats(accuracy, weaponRange, weaponDamage, rangedWeaponMunition);

        //// Initialize target. Player is gona be the default target for now.
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //_myAIController.SetTarget(player);

        // Register self with Wave Controller.
        EventManager.TriggerEvent("registerCompanion");

        // Activate initial AI behaviors.
        // Do so after delay for effect and for the enemy to cleat the space.
        _myAIController.ActivateAIAfterDelay(0.05f);
    }

    private void OnDestroy()
    {
        EventManager.TriggerEvent("companionDeath");
    }

    /// <summary>
    /// For the most part, the Update function in this class acts as an AI registry.
    /// They all know to behave with one another.
    /// </summary>
    private void Update()
    {
        if (_isAIActive == true) {
            // Lot's of targets to chose from. Keep an eye out...
            _myAIController.AIFindClosestTarget();

            // These ranges drive the aggressive behavior of this robot.
            _myAIController.AITrackTarget();

            // Follow the leader, as they say.
            _myAIController.AIFollowPlayer();

            // If within weapon range, fire fire fire!!
            _myAIController.AIAttackTarget();
        }
    }

    public void SacrificeSelf(Vector3 mousePos)
    {
        _isAIActive = false;

        // Remove from list of possible sac choices.
        gameObject.tag = "Suicider";

        _myAIController.InitializeSelfDestructSequence(mousePos);
        _myCombatController.SetDetonationOnDeath(true);

        GameObject newLight = Instantiate(selfDestructIndicator, transform);
        newLight.transform.localPosition = new Vector3(0, 0, -0.7f);
    }

    public bool GetCanSac()
    {
        return _isAIActive;
    }
}
