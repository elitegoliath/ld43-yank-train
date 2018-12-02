using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour {
    public GameObject leftGun;
    public GameObject rightGun;
    
    private bool _canFireWeapon = true;
    private bool _altFire = false;
    private float _engagementRange = 1f;
    private CombatController _myCombatController;
    private CombatController _leftGunCombatController;
    private CombatController _rightGunCombatController;
    private Transform _playerTransform;

    private void Awake()
    {
        _myCombatController = gameObject.GetComponent<CombatController>();
        _leftGunCombatController = leftGun.GetComponent<CombatController>();
        _rightGunCombatController = rightGun.GetComponent<CombatController>();
    }

    private void Start ()
    {
        // Set each turret's stats.
        float accuracy = _myCombatController.GetWeaponAccuracy();
        float wpnRange = _myCombatController.GetWeaponRange();
        int dmg = _myCombatController.GetWeaponDamage();
        GameObject munition = _myCombatController.GetWeaponMunition();
        _engagementRange = _myCombatController.GetWeaponRange();
        
        _leftGunCombatController.SetRangedWeaponStats(accuracy, wpnRange, dmg, munition);
        _rightGunCombatController.SetRangedWeaponStats(accuracy, wpnRange, dmg, munition);

        // Grab player info.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player.transform;
    }
	
	private void Update () {
        FacePlayer();
        AttackPlayer();
    }

    /// <summary>
    /// Always faces it's target, the Player.
    /// </summary>
    private void FacePlayer()
    {
        Vector3 direction = _playerTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));
    }

    /// <summary>
    /// AI Attack Mode that's always on.
    /// </summary>
    private void AttackPlayer()
    {
        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if(distance <= _engagementRange) {
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
                _leftGunCombatController.FireRangedWeapon();
            } else {
                _altFire = true;
                _rightGunCombatController.FireRangedWeapon();
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
