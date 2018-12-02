using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour {
    public float weaponRange = 4f;
    public float weaponDelay = 2f;
    public float weaponAccuracy = 10f;
    public int weaponDamage = 1;
    public GameObject leftGun;
    public GameObject rightGun;

    private Transform _playerTransform;
    private bool _canFireWeapon = true;
    private bool _altFire = false;
    private GunController _leftGunController;
    private GunController _rightGunController;
    
    private void Start () {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player.transform;

        _leftGunController = leftGun.GetComponent<GunController>();
        _rightGunController = rightGun.GetComponent<GunController>();
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

        if (distance <= weaponRange) {
            FireWeapons();
        }
    }

    /// <summary>
    /// Fires the turrets guns, alternating between left and right.
    /// </summary>
    private void FireWeapons()
    {
        if (_canFireWeapon == true) {
            _canFireWeapon = false;
            Invoke("WeaponCooldown", weaponDelay);

            if (_altFire == true) {
                _altFire = false;
                _leftGunController.FireGun(weaponAccuracy);
            } else {
                _altFire = true;
                _rightGunController.FireGun(weaponAccuracy);
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
