using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundBotAI : MonoBehaviour {
    // Characteristics
    public int maxHealth = 10;
    public int armor = 1;
    public float moveSpeed = 100f;
    public float turnSpeed = 10f;
    
    // Weapon Stats
    public float attackRange = 5f;
    public float attackDelay = 1f;
    public float accuracy = 10f;
    public int weaponDamage = 2;
    public CombatController rangedWeapon;
    public GameObject rangedWeaponMunition;

    // Target Data
    public Transform target;
    public CombatController targetCombatController;

    private bool _seekTargetModeActive = true;
    private bool _engageTargetModeActive = false;
    private CombatController _myCombatController;
    private Rigidbody2D _myRigidBody;

    private void Start()
    {
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        // Initialize characteristics.
        _myCombatController.SetMaxHealth(maxHealth);
        _myCombatController.SetCurrentHealth(maxHealth);
        _myCombatController.SetArmor(armor);

        // Initialize weapon.
        rangedWeapon.SetRangedWeaponStats(accuracy, attackRange, weaponDamage, rangedWeaponMunition);
    }

    private void Update()
    {
        if (_seekTargetModeActive == true) {
            AISeekTarget();
        }

        if (_engageTargetModeActive == true) {
            AIEngageTarget();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // TODO: Listen for when the APC has been exitted. Continue a short distance then activate AI Modes.
    }

    private void AISeekTarget()
    {
        bool isTargetInRange = CheckTargetInRange();

        // If within firing range, turn off seek mode, and activate enagement mode.
        if (isTargetInRange == true) {
            _seekTargetModeActive = false;
            _engageTargetModeActive = true;
        }
    }

    private void AIEngageTarget()
    {
        bool isTargetInRange = CheckTargetInRange();

        // If the target is no longer in range, deactivate engagement mode, activate seek mode.
        if (isTargetInRange == false) {
            _seekTargetModeActive = true;
            _engageTargetModeActive = false;
        }

        // TODO: Stay facing the target.
    }

    private void FaceTarget()
    {
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Returns whether the target is within weapon range.
    /// </summary>
    /// <returns></returns>
    private bool CheckTargetInRange()
    {
        bool retVal = false;
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= attackRange) {
            retVal = true;
        }

        return retVal;
    }
}
