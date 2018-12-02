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
    private Rigidbody2D _myRigidBody;
    private bool _canFireWeapon = true;
    private bool _altFire = false;
    private GunController _leftGunController;
    private GunController _rightGunController;

    // Use this for initialization
    void Start () {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player.transform;
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        _leftGunController = leftGun.GetComponent<GunController>();
        _rightGunController = rightGun.GetComponent<GunController>();
    }
	
	// Update is called once per frame
	void Update () {
        FacePlayer();
        AttackPlayer();
    }

    private void FacePlayer()
    {
        Vector2 playerPos = _playerTransform.position;
        float AngleRad = Mathf.Atan2(playerPos.y - transform.position.y, playerPos.x - transform.position.x);
        float angle = (180 / Mathf.PI) * AngleRad;

        _myRigidBody.rotation = angle;
    }

    private void AttackPlayer()
    {
        // TODO: Check if within firing range.
        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if (distance <= weaponRange) {
            FireWeapons();
        }
    }

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

    private void WeaponCooldown()
    {
        _canFireWeapon = true;
    }
}
