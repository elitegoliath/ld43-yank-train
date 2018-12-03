using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour {
    public CombatController leftGunCombatController;
    public CombatController rightGunCombatController;

    private bool _canFireWeapon = true;
    private bool _altFire = false;
    private float _attackRange = 1f;
    private CombatController _myCombatController;
    private Transform _playerTransform;

    private void Awake()
    {
        _myCombatController = gameObject.GetComponent<CombatController>();
    }

    private void Start ()
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
	
	private void Update () {
        FacePlayer();
        AttackPlayer();
    }

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

        if(distance <= _attackRange) {
            FireWeapons();
        }
    }

    /// <summary>
    /// Fires the turrets guns, alternating between left and right.
    /// </summary>
    private void FireWeapons()
    {
        if(_canFireWeapon == true) {
            float wpnDelay = _myCombatController.GetWeaponDelay();
            _canFireWeapon = false;

            Invoke("WeaponCooldown", wpnDelay);

            if(_altFire == true) {
                _altFire = false;
                leftGunCombatController.FireRangedWeapon();
            } else {
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
