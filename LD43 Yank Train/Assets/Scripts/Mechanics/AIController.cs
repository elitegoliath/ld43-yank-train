using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class AIController : MonoBehaviour {
    private float _checkRangesCooldown = 0.2f;
    private float _attackRange = 5f;
    private float _attackDelay = 1f;
    private float _engagementRange = 3f;
    private float _attackingTurnSpeed = 30f;
    private bool _attackTargetModeActive = false;
    private bool _engageTargetModeActive = false;
    private bool _canUpdateTracks = false;
    private bool _canSetDestination = false;
    private bool _canFireRangedWeapon = false;
    private PolyNavAgent _myNavAgent;
    private List<Transform> _availableDeployLocations;
    private Transform _target;
    private CombatController _targetCombatController;
    private CombatController _rangedWeaponCombatController;

    /*****************************************
     *              Lifecycles               *
     ****************************************/
    private void Awake()
    {
        _myNavAgent = gameObject.GetComponent<PolyNavAgent>();

        // Register events listeners.
        _myNavAgent.OnDestinationReached += ActivateAI;
        _myNavAgent.OnDestinationInvalid += PickNewDeployLocation;
    }

    private void OnDestroy()
    {
        // Un-register events listeners again, just in case.
        _myNavAgent.OnDestinationReached -= ActivateAI;
        _myNavAgent.OnDestinationInvalid -= PickNewDeployLocation;
    }

    /*****************************************
     *               Setters                 *
     ****************************************/
    public void SetAttackRange(float attackRange)
    {
        _attackRange = attackRange;
    }

    public void SetAttackDelay(float attackDelay)
    {
        _attackDelay = attackDelay;
    }

    public void SetEnagementRange(float engagementRange)
    {
        _engagementRange = engagementRange;
    }

    public void SetTarget(GameObject target)
    {
        _target = target.GetComponent<Transform>();
        _targetCombatController = target.GetComponent<CombatController>();
    }

    public void SetCheckRangesCooldown(float cooldown)
    {
        _checkRangesCooldown = cooldown;
    }

    public void SetRangedWeaponCombatController(CombatController rangedWeaponCombatController)
    {
        _rangedWeaponCombatController = rangedWeaponCombatController;
    }

    public void SetAttackingTurnSpeed(float attackingTurnSpeed)
    {
        _attackingTurnSpeed = attackingTurnSpeed;
    }

    /*****************************************
     *               Behaviors               *
     ****************************************/
    public void DeployToRandomLocation(List<Transform> waypoints)
    {
        // TODO: Make this code better like MephDaddyX's.
        _availableDeployLocations = new List<Transform>(waypoints);
        Transform chosenDeployLocation = _availableDeployLocations[Random.Range(0, _availableDeployLocations.Count)];

        // Remove deploy location from the list in case it isn't valid.
        // TODO: Maybe have a deployment validity checker on the transport so this isn't needed.
        _availableDeployLocations.Remove(chosenDeployLocation);

        // Attempt to deploy to location.
        _myNavAgent.SetDestination(chosenDeployLocation.position);
    }

    private void PickNewDeployLocation()
    {
        // If no deploy locations are viable, fuckin die.
        if (_availableDeployLocations.Count == 0) {
            Die();
        } else {
            DeployToRandomLocation(_availableDeployLocations);
        }
    }

    public void ActivateAI()
    {
        _engageTargetModeActive = true;
        _canUpdateTracks = true;
        _canSetDestination = true;
        _canFireRangedWeapon = true;

        gameObject.GetComponent<CircleCollider2D>().enabled = true;

        // Un-register events listeners.
        _myNavAgent.OnDestinationReached -= ActivateAI;
        _myNavAgent.OnDestinationInvalid -= PickNewDeployLocation;
    }

    /// <summary>
    /// Like radar, track and report. Other behaviors rely on this information.
    /// </summary>
    public void AITrackTarget()
    {
        if(_canUpdateTracks == true) {
            _canUpdateTracks = false;

            // Let's not check too much. Performance + robots can be slow.
            Invoke("UpdateTracksCooldown", _checkRangesCooldown);

            // Set flag to allow for attacks.
            _attackTargetModeActive = CheckWithinRange(_target.position, _attackRange);

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
    public void AIEngageTarget()
    {
        // Refresh the destination to the target since, you know, they tend to move.
        if(_canSetDestination) {
            _canSetDestination = false;
            Invoke("SetDestinationCooldown", _checkRangesCooldown);

            // If AI is currently pathing after the traget...
            if(_myNavAgent.hasPath == true) {
                bool isInEngagementRange = _myNavAgent.remainingDistance <= _engagementRange;

                // Stop navigation, clear path.
                if(isInEngagementRange == true) {
                    _myNavAgent.Stop();
                    //_myNavAgent.activePath.Clear();
                } else {
                    // Otherwise, update the path.
                    _myNavAgent.SetDestination(_target.position);
                }
            } else {
                // If no path exists, AI is not moving. Check range then start moving if needed.
                _myNavAgent.SetDestination(_target.position);
                bool isInRange = _myNavAgent.remainingDistance <= _engagementRange;

                // If Ai is still close enough to the target, don't start moving again. Clear the set path.
                if(isInRange == true) {
                    //_myNavAgent.activePath.Clear();
                    _myNavAgent.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Handles tracking the player by facing their direction,
    /// and firing the weapon.
    /// </summary>
    public void AIAttackTarget()
    {
        if(_attackTargetModeActive == true) {
            FaceTarget();

            // Fire ranged weapon if not on cooldown. Set on cooldown.
            if(_canFireRangedWeapon == true) {
                _canFireRangedWeapon = false;
                Invoke("FireRangedWeaponCooldown", _attackDelay);
                _rangedWeaponCombatController.FireRangedWeapon();
            }
        }
    }

    /// <summary>
    /// Turns to face the target at the specified rate of rotation.
    /// </summary>
    private void FaceTarget()
    {
        Vector2 dir = _target.position - transform.position;
        float rot = -Mathf.Atan2(dir.x, dir.y) * 180 / Mathf.PI;
        float angle = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, rot, _attackingTurnSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
    }

    private void Die()
    {
        // TODO: Spawn debris on death.
        // TODO: Cause explosion FX on death (sound and viz);
        Destroy(gameObject);
    }

    /*****************************************
     *                Helpers                *
     ****************************************/
    /// <summary>
    /// Returns whether the target is within given range.
    /// </summary>
    /// <returns></returns>
    private bool CheckWithinRange(Vector2 targetPosition, float range)
    {
        bool retVal = false;
        float distance = Vector2.Distance(transform.position, targetPosition);

        if(distance <= range) {
            retVal = true;
        }

        return retVal;
    }

    /*****************************************
     *               Cooldowns               *
     ****************************************/
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
