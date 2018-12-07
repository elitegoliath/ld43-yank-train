using UnityEngine;

public class TurretAI : MonoBehaviour
{
    #region Instantiate Variables Used In Class('s)

    public CombatController leftGunCombatController;
    public CombatController rightGunCombatController;
    private bool _canFireWeapon = true;
    private bool _altFire = false;
    private float _attackRange = 1f;
    private CombatController _myCombatController;
    private Transform _playerTransform;

    #endregion Instantiate Variables Used In Class('s)

    /// <summary>
    /// Sets private Variable <see cref="_myCombatController"/> to an instance of the 
    ///     current gameObject Component <seealso cref="CombatController"/>.
    /// </summary>
    private void Awake()
    {
        _myCombatController = gameObject.GetComponent<CombatController>();
    }

    /// <summary>
    /// Called upon load, sets transports needed variables. And also grabs players information.
    /// </summary>
    private void Start()
    {
        // Set each turret's stats.
        float accuracy = _myCombatController.GetWeaponAccuracy();
        float wpnRange = _myCombatController.GetWeaponRange();
        int dmg = _myCombatController.GetWeaponDamage();
        GameObject munition = _myCombatController.GetWeaponMunition();

        leftGunCombatController.SetRangedWeaponStats(accuracy, wpnRange, dmg, munition);
        rightGunCombatController.SetRangedWeaponStats(accuracy, wpnRange, dmg, munition);

        // Grab player info.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player.transform;
    }

    /// <summary>
    /// Calls the two inner functions on
    /// </summary>
    /// <see cref="FacePlayer"/>
    /// <seealso cref="AttackPlayer"/>
    private void Update()
    {
        FacePlayer();
        AttackPlayer();
    }

    /// <summary>
    /// Sets attack range of enemy transports
    /// </summary>
    /// <param name="attackRange">A float variable fed into the method to set attack range.</param>
    public void SetAttackRange(float attackRange)
    {
        _attackRange = attackRange;
    }

    /// <summary>
    /// Always faces it's target, the Player.
    /// </summary>
    private void FacePlayer()
    {
        Vector3 direction = _playerTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    /// <summary>
    /// AI Attack Mode that's always on.
    /// </summary>
    private void AttackPlayer()
    {
        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if(distance <= _attackRange)
        {
            FireWeapons();
        }
    }

    /// <summary>
    /// Fires the turrets guns, alternating between left and right.
    /// </summary>
    /// <seealso cref="WeaponCooldown"/> is called to reset the flag for weapon cooldown.
    private void FireWeapons()
    {
        // Making string a const to avoid runtime changes and also readability in flow.
        const string weaponCooldown = "WeaponCooldown";

        if(_canFireWeapon == true)
        {
            float wpnDelay = _myCombatController.GetWeaponDelay();
            _canFireWeapon = false;

            Invoke(weaponCooldown, wpnDelay);

            if(_altFire == true)
            {
                _altFire = false;
                leftGunCombatController.FireRangedWeapon();
            }
            else
            {
                _altFire = true;
                rightGunCombatController.FireRangedWeapon();
            }
        }
    }

    /// <summary>
    /// Once weapon cooldown has been reached, reset flag.
    /// </summary>
    private void WeaponCooldown()
    {
        _canFireWeapon = true;
    }
}