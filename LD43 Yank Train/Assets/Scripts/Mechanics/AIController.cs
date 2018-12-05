using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class AIController : MonoBehaviour {
    private float _checkRangesCooldown = 0.2f;
    private float _attackRange = 5f;
    private float _attackDelay = 1f;
    private float _followRange = 2f;
    private float _engagementRange = 3f;
    private float _attackingTurnSpeed = 30f;
    private bool _attackTargetModeActive = false;
    private bool _canUpdateTracks = false;
    private bool _canSetDestination = false;
    private bool _canFireRangedWeapon = false;
    private bool _canFindClosestTarget = false;
    private bool _canUpdatePathToPlayer = false;
    private PolyNavAgent _myNavAgent;
    private List<Transform> _availableDeployLocations;
    private Transform _target;
    private CombatController _rangedWeaponCombatController;
    private CombatController _myCombatController;
    private Transform _player;

    /*****************************************
     *              Lifecycles               *
     ****************************************/
    private void Awake()
    {
        _myNavAgent = gameObject.GetComponent<PolyNavAgent>();
        _myCombatController = gameObject.GetComponent<CombatController>();

        // Register events listeners.
        _myNavAgent.OnDestinationReached += ActivateAI;
        _myNavAgent.OnDestinationInvalid += PickNewDeployLocation;

        Invoke("ForceColliderActivation", 2f);
    }

    private void OnDestroy()
    {
        // Un-register events listeners again, just in case.
        _myNavAgent.OnDestinationReached -= ActivateAI;
        _myNavAgent.OnDestinationReached -= SelfDestruct;
        _myNavAgent.OnDestinationInvalid -= PickNewDeployLocation;
        _myNavAgent.OnDestinationInvalid -= ShamefulDeath;
        _myNavAgent.OnDestinationInvalid -= SelfDestruct;
        CancelInvoke("ActivateAltAI");
    }

    private void ForceColliderActivation()
    {
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
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

    public void SetPlayer()
    {
        GameObject playerRef = GameObject.FindGameObjectWithTag("Player");
        _player = playerRef.GetComponent<Transform>();
    }

    public void SetFollowRange(float followRange)
    {
        _followRange = followRange;
    }

    public void StopNavigation()
    {
        _myNavAgent.Stop();
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

        CancelInvoke("ActivateAI");
        Invoke("ActivateAI", 2f);
    }

    private void PickNewDeployLocation()
    {
        // If no deploy locations are viable, fuckin die.
        if (_availableDeployLocations.Count == 0) {
            ShamefulDeath();
        } else {
            DeployToRandomLocation(_availableDeployLocations);
        }
    }

    public void ActivateAIAfterDelay(float delay)
    {
        // TODO: If args need to be passed, make coroutine instead, maybe?
        Invoke("ActivateAltAI", delay);
    }

    public void ActivateAltAI()
    {
        // TODO: Do better than this.
        _canUpdateTracks = true;
        _canFireRangedWeapon = true;
        _canFindClosestTarget = true;
        _canUpdatePathToPlayer = true;

        // TODO: Circle collider not guarenteed. Pass in reference to desired collider instead.
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    public void ActivateAI()
    {
        CancelInvoke("ActivateAI");
        // TODO: Do better than this.
        _canUpdateTracks = true;
        _canSetDestination = true;
        _canFireRangedWeapon = true;

        // TODO: Circle collider not guarenteed. Pass in reference to desired collider instead.
        CircleCollider2D[] myColliders = gameObject.GetComponents<CircleCollider2D>();
        foreach (CircleCollider2D collider in myColliders) {
            collider.enabled = true;
        }

        // Un-register events listeners.
        _myNavAgent.OnDestinationReached -= ActivateAI;
        _myNavAgent.OnDestinationInvalid -= PickNewDeployLocation;

        // Register new "stuck" event listener;
        // TODO: Handle entities that get stuck better.
        _myNavAgent.OnDestinationInvalid += ShamefulDeath;
    }

    public void AIFindClosestTarget()
    {
        if (_canFindClosestTarget == true) {
            _canFindClosestTarget = false;
            GameObject foundTarget = null;
            float targetDistance = Mathf.Infinity;

            // Since other methods may call this again before the invoke fires...
            // and I don't know if they stack...
            CancelInvoke("TrackClosestTargetCooldown");

            // Doesn't need to update nearly as much as the other routines.
            Invoke("TrackClosestTargetCooldown", _checkRangesCooldown * 1.2f);

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] transports = GameObject.FindGameObjectsWithTag("Transport");
            Vector3 myPos = transform.position;

            foreach (GameObject enemy in enemies) {
                float dist = Vector3.Distance(enemy.transform.position, myPos);
                if (dist < targetDistance) {
                    foundTarget = enemy;
                    targetDistance = dist;
                }
            }

            foreach(GameObject transport in transports) {
                float dist = Vector3.Distance(transport.transform.position, myPos);
                if(dist < targetDistance) {
                    foundTarget = transport;
                    targetDistance = dist;
                }
            }

            if (foundTarget != null) {
                SetTarget(foundTarget);
            }
        }
    }

    /// <summary>
    /// Like radar, track and report. Other behaviors rely on this information.
    /// </summary>
    public void AITrackTarget()
    {
        if (_canUpdateTracks == true) {
            _canUpdateTracks = false;

            if (_target == null) {
                //_canFindClosestTarget = true;
                //AIFindClosestTarget();
            } else {
                // Let's not check too much. Performance + robots can be slow.
                Invoke("UpdateTracksCooldown", _checkRangesCooldown);

                // Set flag to allow for attacks.
                _attackTargetModeActive = CheckWithinRange(_target.position, _attackRange);

                // If attacking, we want to be in charge of rotation. Otherwise let the nav do it.
                if(_attackTargetModeActive == true) {
                    _myNavAgent.rotateTransform = false;
                } else {
                    _myNavAgent.rotateTransform = true;
                }
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

            // If AI is currently pathing after the traget...
            if(_myNavAgent.hasPath == true) {
                bool isInEngagementRange = _myNavAgent.remainingDistance <= _engagementRange;

                // Stop navigation, clear path.
                if (isInEngagementRange == true) {
                    _myNavAgent.Stop();
                } else if (_target != null) {
                    // Otherwise, update the path.
                    _myNavAgent.SetDestination(_target.position);

                    Invoke("SetDestinationCooldown", _checkRangesCooldown);
                }
            } else if (_target != null) {
                // If no path exists, AI is not moving. Check range then start moving if needed.
                _myNavAgent.SetDestination(_target.position);

                bool isInRange = _myNavAgent.remainingDistance <= _engagementRange;

                // If AI is still close enough to the target, don't start moving again. Clear the set path.
                if(isInRange == true) {
                    _myNavAgent.Stop();
                }

                Invoke("SetDestinationCooldown", _checkRangesCooldown * 1.5f);
            }
        }
    }

    /// <summary>
    /// Handles tracking the player by facing their direction,
    /// and firing the weapon.
    /// </summary>
    public void AIAttackTarget()
    {
        if (_attackTargetModeActive == true) {
            if (_target == null) {
                _attackTargetModeActive = false;
                _myNavAgent.rotateTransform = true;
            } else {
                FaceTarget();

                // Fire ranged weapon if not on cooldown. Set on cooldown.
                if(_canFireRangedWeapon == true) {
                    _canFireRangedWeapon = false;

                    Invoke("FireRangedWeaponCooldown", _attackDelay);
                    _rangedWeaponCombatController.FireRangedWeapon();
                }
            }
        }
    }

    public void AIFollowPlayer()
    {
        if (_canUpdatePathToPlayer == true) {
            _canUpdatePathToPlayer = false;

            Invoke("UpdatePathToPlayerCooldown", _checkRangesCooldown * 1.2f);

            bool isWithinFollowRange = CheckWithinRange(_player.position, _followRange);

            // If outside of the follow range, move and stuff.
            if (isWithinFollowRange == false) {
                _myNavAgent.SetDestination(_player.position);
            }
        }
    }

    /// <summary>
    /// Turns to face the target at the specified rate of rotation.
    /// </summary>
    private void FaceTarget()
    {
        if (_target != null) {
            Vector2 dir = _target.position - transform.position;
            float rot = -Mathf.Atan2(dir.x, dir.y) * 180 / Mathf.PI;
            float angle = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, rot, _attackingTurnSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
        }
    }

    public void InitializeSelfDestructSequence(Vector3 destination)
    {
        _myNavAgent.OnDestinationInvalid -= ShamefulDeath;
        _myNavAgent.rotateTransform = true;

        // Explode if no collision and pathing is finished.
        _myNavAgent.OnDestinationReached += SelfDestruct;
        _myNavAgent.OnDestinationInvalid += SelfDestruct;

        // Send him on his way.
        _myNavAgent.maxSpeed = _myNavAgent.maxSpeed * 2;
        _myNavAgent.SetDestination(destination);
    }

    private void SelfDestruct()
    {
        _myNavAgent.Stop();
        _myCombatController.Die();
    }

    private void ShamefulDeath()
    {
        Destroy(gameObject);
    }

    /*****************************************
     *                Helpers                *
     ****************************************/
    /// <summary>
    /// Returns whether the target is within given range.
    /// </summary>
    /// <returns></returns>
    private bool CheckWithinRange(Vector3 targetPosition, float range)
    {
        bool retVal = false;
        float distance = Vector3.Distance(targetPosition, transform.position);

        if(distance <= range) {
            retVal = true;
        }

        return retVal;
    }

    public void InstantiateSpeedDifferential(float differencePotential)
    {
        float maxSpeed = _myNavAgent.maxSpeed;
        float minSpeed = Mathf.Abs(maxSpeed - differencePotential);

        _myNavAgent.maxSpeed = Random.Range(minSpeed, maxSpeed);
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

    private void TrackClosestTargetCooldown()
    {
        _canFindClosestTarget = true;
    }

    private void UpdatePathToPlayerCooldown()
    {
        _canUpdatePathToPlayer = true;
    }
}
