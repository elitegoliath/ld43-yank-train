using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundBotAI : MonoBehaviour {
    // Navigation
    public float checkRangesCooldown = 0.2f;
    
    // Characteristics
    public int maxHealth = 10;
    public int armor = 1;
    public float moveSpeed = 100f;
    public float turnSpeed = 10f;
    public float engagementRange = 3f;
    
    // Weapon Stats
    public float attackRange = 5f;
    public float attackDelay = 1f;
    public float accuracy = 10f;
    public int weaponDamage = 2;
    public CombatController rangedWeapon;
    public GameObject rangedWeaponMunition;

    private bool _engageTargetModeActive = true;
    private bool _attackTargetModeActive = false;
    private bool _checkRanges = true;
    private bool _canFireRangedWeapon = true;
    private CombatController _myCombatController;
    private Rigidbody2D _myRigidBody;
    private Transform _target;
    private CombatController _targetCombatController;

    private void Start()
    {
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myCombatController = gameObject.GetComponent<CombatController>();

        // Initialize characteristics.
        _myCombatController.SetMaxHealth(maxHealth);
        _myCombatController.SetCurrentHealth(maxHealth);
        _myCombatController.SetArmor(armor);

        // Initialize weapon.
        rangedWeapon.SetRangedWeaponStats(accuracy, attackRange, weaponDamage, rangedWeaponMunition);

        // Initialize target. Player is gona be the default target for now.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _target = player.GetComponent<Transform>();
        _targetCombatController = player.GetComponent<CombatController>();
    }

    private void Update()
    {
        // These ranges drive the aggressive behavior of this robot.
        if (_checkRanges == true) {
            // Let's not check too much. Performance + robots can be slow.
            _checkRanges = false;
            Invoke("CheckRangesCooldown", checkRangesCooldown);

            // Depending on distance from target, do some things.
            _engageTargetModeActive = !CheckWithinRange(_target.position, engagementRange);
            _attackTargetModeActive = CheckWithinRange(_target.position, attackRange);
        }

        // If too far away from target, engage.
        if (_engageTargetModeActive == true) {
            AIEngageTarget();
        }

        // If within weapon range, fire fire fire!!
        if (_attackTargetModeActive == true) {
            AIAttackTarget();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // TODO: Listen for when the APC has been exitted. Continue a short distance then activate AI Modes.
    }

    private void AIEngageTarget()
    {
        // TODO: Insert Pathfinding AI here
    }

    /// <summary>
    /// Handles tracking the player by facing their direction,
    /// and firing the weapon.
    /// </summary>
    private void AIAttackTarget()
    {
        FaceTarget();

        // Fire ranged weapon if not on cooldown. Set on cooldown.
        if (_canFireRangedWeapon == true) {
            _canFireRangedWeapon = false;
            Invoke("FireRangedWeaponCooldown", attackDelay);
            rangedWeapon.FireRangedWeapon();
        }
    }

    /// <summary>
    /// Turns to face the target at the specified rate of rotation.
    /// </summary>
    private void FaceTarget()
    {
        Vector2 direction = _target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Returns whether the target is within given range.
    /// </summary>
    /// <returns></returns>
    private bool CheckWithinRange(Vector2 targetPosition, float range)
    {
        bool retVal = false;
        float distance = Vector2.Distance(transform.position, targetPosition);

        if (distance <= range) {
            retVal = true;
        }

        return retVal;
    }

    /// <summary>
    /// Cooldown flag that resets, allowing for AI to do stuff.
    /// </summary>
    private void CheckRangesCooldown()
    {
        _checkRanges = true;
    }

    /// <summary>
    /// Cooldown flag that resets the Can Fire Ranged Weapon flag.
    /// </summary>
    private void FireRangedWeaponCooldown()
    {
        _canFireRangedWeapon = true;
    }
}
