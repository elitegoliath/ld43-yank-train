using PolyNav;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    #region Variable Instantiation

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

    #endregion Variable Instantiation

    #region LifeCycles

    /// <summary>
    /// Sets upon start all variables to current onjects.
    /// </summary>
    private void Awake()
    {
        #region Consts for readability

        const string _forceColliderActivation = "ForceColliderActivation";

        #endregion Consts for readability

        _myNavAgent = gameObject.GetComponent<PolyNavAgent>();
        _myCombatController = gameObject.GetComponent<CombatController>();

        // Register events listeners.
        _myNavAgent.OnDestinationReached += ActivateAI;
        _myNavAgent.OnDestinationInvalid += PickNewDeployLocation;

        Invoke(_forceColliderActivation, 2f); ///<see cref="ForceColliderActivation"/>
    }

    /// <summary>
    /// Handles death events, and unregistering of event listiners.
    /// </summary>
    private void OnDestroy()
    {
        #region consts for readability

        const string _activateAlternateAI = "ActivateAltAI";

        #endregion consts for readability

        // Un-register events listeners again, just in case.
        _myNavAgent.OnDestinationReached -= ActivateAI;
        _myNavAgent.OnDestinationReached -= SelfDestruct;
        _myNavAgent.OnDestinationInvalid -= PickNewDeployLocation;
        _myNavAgent.OnDestinationInvalid -= ShamefulDeath;
        _myNavAgent.OnDestinationInvalid -= SelfDestruct;

        CancelInvoke(_activateAlternateAI);///<see cref="ActivateAltAI"/>
    }

    /// <summary>
    /// Insures that the gameObjects collider event is enabled upon call.
    /// </summary>
    private void ForceColliderActivation()
    {
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    #endregion LifeCycles

    #region Setters

    /// <summary>
    /// Sets attack range of game object when called.
    /// </summary>
    /// <param name="attackRange">The variable amount the attack range needs to be set to.</param>
    public void SetAttackRange(float attackRange)
    {
        _attackRange = attackRange;
    }

    /// <summary>
    /// Sets the attack delay on the game object when called.
    /// </summary>
    /// <param name="attackDelay">the variable amount the attack delay needs to be set to.</param>
    public void SetAttackDelay(float attackDelay)
    {
        _attackDelay = attackDelay;
    }

    /// <summary>
    /// Sets the range game object is allowed to engage an enemy at when called.
    /// </summary>
    /// <param name="engagementRange">the variable amount the engagment range needs to be set to.</param>
    public void SetEnagementRange(float engagementRange)
    {
        _engagementRange = engagementRange;
    }

    /// <summary>
    /// sets the target of the current game object.
    /// </summary>
    /// <param name="target">the target to well.... target.</param>
    public void SetTarget(GameObject target)
    {
        _target = target.GetComponent<Transform>();
    }

    /// <summary>
    /// Sets cooldown timer of a game object.
    /// </summary>
    /// <param name="cooldown"></param>
    public void SetCheckRangesCooldown(float cooldown)
    {
        _checkRangesCooldown = cooldown;
    }

    /// <summary>
    /// sets the combat controller in charge of ranged attack to the fed in instance.
    /// </summary>
    /// <param name="rangedWeaponCombatController"></param>
    public void SetRangedWeaponCombatController(CombatController rangedWeaponCombatController)
    {
        _rangedWeaponCombatController = rangedWeaponCombatController;
    }

    /// <summary>
    /// sets how fast a gmaeObject can turn while they are attacking an enemy.
    /// </summary>
    /// <param name="attackingTurnSpeed"></param>
    public void SetAttackingTurnSpeed(float attackingTurnSpeed)
    {
        _attackingTurnSpeed = attackingTurnSpeed;
    }

    /// <summary>
    /// sets a gmae object as  the "Player".
    /// </summary>
    public void SetPlayer()
    {
        #region consts for readability

        const string _playerTag = "Player";

        #endregion consts for readability

        GameObject playerRef = GameObject.FindGameObjectWithTag(_playerTag);
        _player = playerRef.GetComponent<Transform>();
    }

    /// <summary>
    /// sets range an AI is to follow the "parent" at.
    /// </summary>
    /// <param name="followRange"></param>
    public void SetFollowRange(float followRange)
    {
        _followRange = followRange;
    }

    /// <summary>
    /// nulls out an objects current nav path.
    /// </summary>
    public void StopNavigation()
    {
        _myNavAgent.Stop();
    }

    #endregion Setters

    #region Behaviors

    /// <summary>
    /// chooses a set of random coords for transport to deploy enemy AI's on, And activates transports AI logic.
    /// </summary>
    /// <param name="waypoints"></param>
    public void DeployToRandomLocation(List<Transform> waypoints)
    {
        #region consts for readability

        const string _activateAI = "ActivateAI";

        #endregion consts for readability

        // TODO: Make this code better like MephDaddyX's.
        _availableDeployLocations = new List<Transform>(waypoints);
        Transform chosenDeployLocation = _availableDeployLocations[Random.Range(0, _availableDeployLocations.Count)];

        // Remove deploy location from the list in case it isn't valid.
        // TODO: Maybe have a deployment validity checker on the transport so this isn't needed.
        _availableDeployLocations.Remove(chosenDeployLocation);

        // Attempt to deploy to location.
        _myNavAgent.SetDestination(chosenDeployLocation.position);

        CancelInvoke(_activateAI); ///<see cref="ActivateAI"/>
        Invoke(_activateAI, 2f);
    }

    /// <summary>
    /// Called when a Deploy location was blocked returns a "new" deploy coords.
    /// </summary>
    private void PickNewDeployLocation()
    {
        // If no deploy locations are viable, fuckin die.
        if (_availableDeployLocations.Count == 0)
        {
            ShamefulDeath();
        }
        else
        {
            DeployToRandomLocation(_availableDeployLocations);
        }
    }

    /// <summary>
    /// Activates the alternative AI after the provided delay.
    /// </summary>
    /// <param name="delay"></param>
    public void ActivateAIAfterDelay(float delay)
    {
        #region consts for readability

        const string _activateAltAI = "ActivateAltAI";

        #endregion consts for readability

        // TODO: If args need to be passed, make coroutine instead, maybe?
        Invoke(_activateAltAI, delay);///<see cref="ActivateAltAI"/>
    }

    /// <summary>
    /// Inits the Alternate AI logic for the calling game object.
    /// </summary>
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

    /// <summary>
    /// Activates the logic running the AI and provides and flags related to the calling game object.
    /// </summary>
    /// <see cref="ActivateAI"/>
    public void ActivateAI()
    {
        #region consts for readability

        const string _activateAI = "ActivateAI";

        #endregion consts for readability

        CancelInvoke(_activateAI); ///<see cref="ActivateAI"/>
        // TODO: Do better than this.
        _canUpdateTracks = true;
        _canSetDestination = true;
        _canFireRangedWeapon = true;

        // TODO: Circle collider not guarenteed. Pass in reference to desired collider instead.
        CircleCollider2D[] myColliders = gameObject.GetComponents<CircleCollider2D>();
        foreach (CircleCollider2D collider in myColliders)
        {
            collider.enabled = true;
        }

        // Un-register events listeners.
        _myNavAgent.OnDestinationReached -= ActivateAI;
        _myNavAgent.OnDestinationInvalid -= PickNewDeployLocation;

        // Register new "stuck" event listener;
        // TODO: Handle entities that get stuck better.
        _myNavAgent.OnDestinationInvalid += ShamefulDeath;
    }

    /// <summary>
    /// Method called by AI's to both find closest target and create a valid path to it.
    /// </summary>
    /// <see cref="TrackClosestTargetCooldown"/>
    /// <seealso cref="SetTarget(GameObject)"/>
    public void AIFindClosestTarget()
    {
        #region consts for readability

        const string _trackTargetCooldown = "TrackClosestTargetCooldown";
        const string _enemyTag = "Enemy";
        const string _transportTag = "Transport";

        #endregion consts for readability

        if (_canFindClosestTarget == true)
        {
            _canFindClosestTarget = false;
            GameObject foundTarget = null;
            float targetDistance = Mathf.Infinity;

            // Since other methods may call this again before the invoke fires...
            // and I don't know if they stack...
            CancelInvoke(_trackTargetCooldown); ///<see cref="TrackClosestTargetCooldown"/>

            // Doesn't need to update nearly as much as the other routines.
            Invoke(_trackTargetCooldown, _checkRangesCooldown * 1.2f); ///<see cref="TrackClosestTargetCooldown"/>

            GameObject[] enemies = GameObject.FindGameObjectsWithTag(_enemyTag);
            GameObject[] transports = GameObject.FindGameObjectsWithTag(_transportTag);
            Vector3 myPos = transform.position;

            foreach (GameObject enemy in enemies)
            {
                float dist = Vector3.Distance(enemy.transform.position, myPos);
                if (dist < targetDistance)
                {
                    foundTarget = enemy;
                    targetDistance = dist;
                }
            }

            foreach (GameObject transport in transports)
            {
                float dist = Vector3.Distance(transport.transform.position, myPos);
                if (dist < targetDistance)
                {
                    foundTarget = transport;
                    targetDistance = dist;
                }
            }

            if (foundTarget != null)
            {
                SetTarget(foundTarget);
            }
        }
    }

    /// <summary>
    /// Like radar, track and report. Other behaviors rely on this information.
    /// </summary>
    public void AITrackTarget()
    {
        #region const for readbility

        const string _updateTrackingCooldown = "UpdateTracksCooldown";

        #endregion const for readbility

        if (_canUpdateTracks == true)
        {
            _canUpdateTracks = false;

            if (_target == null)
            {
                //_canFindClosestTarget = true;
                //AIFindClosestTarget();
            }
            else
            {
                // Let's not check too much. Performance + robots can be slow.
                Invoke(_updateTrackingCooldown, _checkRangesCooldown); ///<see cref="UpdateTracksCooldown"/>

                // Set flag to allow for attacks.
                _attackTargetModeActive = CheckWithinRange(_target.position, _attackRange);

                // If attacking, we want to be in charge of rotation. Otherwise let the nav do it.
                if (_attackTargetModeActive == true)
                {
                    _myNavAgent.rotateTransform = false;
                }
                else
                {
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
        #region consts for readability

        const string _pathingCooldown = "SetDestinationCooldown";

        #endregion consts for readability

        // Refresh the destination to the target since, you know, they tend to move.
        if (_canSetDestination)
        {
            _canSetDestination = false;

            // If AI is currently pathing after the traget...
            if (_myNavAgent.hasPath == true)
            {
                bool isInEngagementRange = _myNavAgent.remainingDistance <= _engagementRange;

                // Stop navigation, clear path.
                if (isInEngagementRange == true)
                {
                    _myNavAgent.Stop();
                }
                else if (_target != null)
                {
                    // Otherwise, update the path.
                    _myNavAgent.SetDestination(_target.position);

                    Invoke(_pathingCooldown, _checkRangesCooldown);///<see cref="SetDestinationCooldown"/>
                }
            }
            else if (_target != null)
            {
                // If no path exists, AI is not moving. Check range then start moving if needed.
                _myNavAgent.SetDestination(_target.position);

                bool isInRange = _myNavAgent.remainingDistance <= _engagementRange;

                // If AI is still close enough to the target, don't start moving again. Clear the set path.
                if (isInRange == true)
                {
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
        #region consts for readability

        const string _rangedWeaponCD = "FireRangedWeaponCooldown";

        #endregion consts for readability

        if (_attackTargetModeActive == true)
        {
            if (_target == null)
            {
                _attackTargetModeActive = false;
                _myNavAgent.rotateTransform = true;
            }
            else
            {
                FaceTarget();

                // Fire ranged weapon if not on cooldown. Set on cooldown.
                if (_canFireRangedWeapon == true)
                {
                    _canFireRangedWeapon = false;

                    Invoke(_rangedWeaponCD, _attackDelay);///<see cref="FireRangedWeaponCooldown"/>
                    _rangedWeaponCombatController.FireRangedWeapon();
                }
            }
        }
    }

    /// <summary>
    /// Method called by AI's to create/update pathing to player.
    /// </summary>
    public void AIFollowPlayer()
    {
        #region consts for readability

        const string _updatePathToPlayerCD = "UpdatePathToPlayerCooldown";

        #endregion consts for readability

        if (_canUpdatePathToPlayer == true)
        {
            _canUpdatePathToPlayer = false;

            Invoke(_updatePathToPlayerCD, _checkRangesCooldown * 1.2f);///<see cref="UpdatePathToPlayerCooldown"/>

            bool isWithinFollowRange = CheckWithinRange(_player.position, _followRange);

            // If outside of the follow range, move and stuff.
            if (isWithinFollowRange == false)
            {
                _myNavAgent.SetDestination(_player.position);
            }
        }
    }

    /// <summary>
    /// Turns to face the target at the specified rate of rotation.
    /// </summary>
    private void FaceTarget()
    {
        if (_target != null)
        {
            Vector2 dir = _target.position - transform.position;
            float rot = -Mathf.Atan2(dir.x, dir.y) * 180 / Mathf.PI;
            float angle = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, rot, _attackingTurnSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
        }
    }

    /// <summary>
    /// Method that is called by AI's to "Self Destruct".
    /// </summary>
    /// <param name="destination">Current coords of bot upon init of self destruct.</param>
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

    /// <summary>
    /// Method called to init a AI to self destruct.
    /// </summary>
    private void SelfDestruct()
    {
        _myNavAgent.Stop();
        _myCombatController.Die();
    }

    /// <summary>
    /// The death event called by "stuck" AI's.
    /// </summary>
    private void ShamefulDeath()
    {
        Destroy(gameObject);
    }

    #endregion Behaviors

    #region Helpers

    /// <summary>
    /// Returns whether the target is within given range.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool CheckWithinRange(Vector3 targetPosition, float range)
    {
        bool retVal = false;
        float distance = Vector3.Distance(targetPosition, transform.position);

        if (distance <= range)
        {
            retVal = true;
        }

        return retVal;
    }

    /// <summary>
    /// sets a game objects min speed based on max speed - param amount.
    /// </summary>
    /// <param name="differencePotential"></param>
    public void InstantiateSpeedDifferential(float differencePotential)
    {
        float maxSpeed = _myNavAgent.maxSpeed;
        float minSpeed = Mathf.Abs(maxSpeed - differencePotential);

        _myNavAgent.maxSpeed = Random.Range(minSpeed, maxSpeed);
    }

    #endregion Helpers

    #region CoolDowns

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

    /// <summary>
    /// sets a flag on game object that tells it if it can track other entities that are near itself.
    /// </summary>
    private void TrackClosestTargetCooldown()
    {
        _canFindClosestTarget = true;
    }

    /// <summary>
    /// sets a flag to allow AI's to reset/update their paths to the player.
    /// </summary>
    private void UpdatePathToPlayerCooldown()
    {
        _canUpdatePathToPlayer = true;
    }

    #endregion CoolDowns
}