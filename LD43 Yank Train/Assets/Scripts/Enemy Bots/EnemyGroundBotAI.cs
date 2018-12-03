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
    public float turnSpeed = 10f;
    public float engagementRange = 3f;
    
    [Header("Weapon Stats")]
    public float attackRange = 5f;
    public float attackDelay = 1f;
    public float accuracy = 10f;
    public int weaponDamage = 2;
    public CombatController rangedWeapon;
    public GameObject rangedWeaponMunition;

    // Private Properties
    private bool _engageTargetModeActive = true;
    private bool _attackTargetModeActive = false;
    private bool _canUpdateTracks = true;
    private bool _canSetDestination = true;
    private bool _canFireRangedWeapon = true;
    private bool _isNavStopped = false;
    private CombatController _myCombatController;
    private Rigidbody2D _myRigidBody;
    private Transform _target;
    private CombatController _targetCombatController;
    private PolyNavAgent _myNavAgent;

    private void Start()
    {
        // Get components
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myNavAgent = gameObject.GetComponent<PolyNavAgent>();

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
        AITrackTarget();

        // Fuck it, we're always lookin to engage. Every fuckin frame.
        AIEngageTarget();

        // If within weapon range, fire fire fire!!
        if (_attackTargetModeActive == true) {
            AIAttackTarget();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // TODO: Listen for when the APC has been exitted. Continue a short distance then activate AI Modes.
    }

    /// <summary>
    /// Like radar, track and report. Other behaviors rely on this information.
    /// </summary>
    private void AITrackTarget()
    {
        if(_canUpdateTracks == true) {
            _canUpdateTracks = false;

            // Let's not check too much. Performance + robots can be slow.
            Invoke("UpdateTracksCooldown", checkRangesCooldown);

            // Set flag to allow for attacks.
            _attackTargetModeActive = CheckWithinRange(_target.position, attackRange);

            // If attacking, we want to be in charge of rotation. Otherwise let the nav do it.
            if(_attackTargetModeActive) {
                _myNavAgent.rotateTransform = false;
            } else {
                _myNavAgent.rotateTransform = true;
            }
        }
    }

    /// <summary>
    /// Handles all navigational behaviors.
    /// </summary>
    private void AIEngageTarget()
    {
        // Refresh the destination to the target since, you know, they tend to move.
        if (_canSetDestination) {
            _canSetDestination = false;
            Invoke("SetDestinationCooldown", checkRangesCooldown);

            // If AI is currently pathing after the traget...
            if (_myNavAgent.hasPath == true) {
                bool isInEngagementRange = _myNavAgent.remainingDistance <= engagementRange;

                // Stop navigation, clear path.
                if (isInEngagementRange == true) {
                    _myNavAgent.activePath.Clear();
                } else {
                    // Otherwise, update the path.
                    _myNavAgent.SetDestination(_target.position);
                }
            } else {
                // If no path exists, AI is not moving. Check range then start moving if needed.
                _myNavAgent.SetDestination(_target.position);
                bool isInRange = _myNavAgent.remainingDistance <= engagementRange;
                
                // If Ai is still close enough to the target, don't start moving again. Clear the set path.
                if (isInRange == true) {
                    _myNavAgent.activePath.Clear();
                }
            }
        }
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
        Vector2 dir = _target.position - transform.position;
        float rot = -Mathf.Atan2(dir.x, dir.y) * 180 / Mathf.PI;
        float angle = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, rot, turnSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
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
    /// Cooldown flag that resets the range checking flag.
    /// </summary>
    private void UpdateTracksCooldown()
    {
        _canUpdateTracks = true;
    }

    /// <summary>
    /// Cooldown flag that resets the Can Fire Ranged Weapon flag.
    /// </summary>
    private void FireRangedWeaponCooldown()
    {
        _canFireRangedWeapon = true;
    }

    /// <summary>
    /// Cooldown flag that resets the flag that allows for pathing updates.
    /// </summary>
    private void SetDestinationCooldown()
    {
        _canSetDestination = true;
    }
}
