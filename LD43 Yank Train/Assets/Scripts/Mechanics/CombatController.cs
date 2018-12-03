using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour {
    // Character Stats
    private int _maxHealth = 20;
    private int _currentHealth = 20;
    private int _armor = 1;

    // Ranged Weapon Stats
    private float _weaponRange = 4f;
    private float _weaponDelay = 2f;
    private float _weaponAccuracy = 10f;
    private int _weaponDamage = 1;
    private GameObject _weaponMunition;
    private GameObject _debris;

    /*****************************************
     *              Max Health               *
     ****************************************/
    public void SetMaxHealth(int maxHealth)
    {
        _maxHealth = maxHealth;

        // Limits health to the maximum.
        if (_currentHealth > _maxHealth) {
            _currentHealth = _maxHealth;
        }
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
    }

    /*****************************************
     *            Current Health             *
     ****************************************/
    public void SetCurrentHealth(int currentHealth)
    {
        _currentHealth = currentHealth;

        // Limit health to the maximum.
        if (_currentHealth > _maxHealth) {
            _currentHealth = _maxHealth;
        }
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    /*****************************************
     *                Armor                  *
     ****************************************/
    public void SetArmor(int armor)
    {
        _armor = armor;
    }

    public int GetArmor()
    {
        return _armor;
    }

    /*****************************************
     *             Weapon Range              *
     ****************************************/
    public void SetWeaponRange(float weaponRange)
    {
        _weaponRange = weaponRange;
    }

    public float GetWeaponRange()
    {
        return _weaponRange;
    }

    /*****************************************
     *             Weapon Delay              *
     ****************************************/
    public void SetWeaponDelay(float weaponDelay)
    {
        _weaponDelay = weaponDelay;
    }

    public float GetWeaponDelay()
    {
        return _weaponDelay;
    }

    /*****************************************
     *            Weapon Accuracy            *
     ****************************************/
    public void SetWeaponAccuracy(float weaponAccuracy)
    {
        _weaponAccuracy = weaponAccuracy;
    }

    public float GetWeaponAccuracy()
    {
        return _weaponAccuracy;
    }

    /*****************************************
     *             Weapon Damage             *
     ****************************************/
    public void SetWeaponDamage(int weaponDamage)
    {
        _weaponDamage = weaponDamage;
    }

    public int GetWeaponDamage()
    {
        return _weaponDamage;
    }

    /*****************************************
     *            Weapon Munition            *
     ****************************************/
    public void SetWeaponMunition(GameObject weaponMunition)
    {
        _weaponMunition = weaponMunition;
    }

    public GameObject GetWeaponMunition()
    {
        return _weaponMunition;
    }

    /*****************************************
     *                Debris                 *
     ****************************************/
    public void SetDebris(GameObject debris)
    {
        _debris = debris;
    }

    /*****************************************
     *            Ranged Weapon              *
     ****************************************/
    /// <summary>
    /// Helper function for entities that only need stats related to firing ranged weapons.
    /// </summary>
    public void SetRangedWeaponStats(float weaponAccuracy, float weaponRange, int weaponDamage, GameObject weaponMunitionPrefab)
    {
        _weaponAccuracy = weaponAccuracy;
        _weaponDamage = weaponDamage;
        _weaponMunition = weaponMunitionPrefab;
        _weaponRange = weaponRange;
    }

    /// <summary>
    /// Fire any ranged weapon with a specific munition.
    /// </summary>
    public void FireRangedWeapon()
    {
        // Determine deviation of munition due to accuracy implications.
        float bias = Random.Range(0f, 100f);
        float influence = Random.Range(0f, _weaponAccuracy);
        float offset = influence;

        if(bias <= 50f) {
            offset = -influence;
        }

        // Face appropriate direction with included offset from deviation.
        Quaternion direction = transform.rotation;
        direction *= Quaternion.Euler(0, 0, offset);

        // Instantiate munition with necessary data.
        GameObject newMunition = Instantiate(_weaponMunition, transform.position, direction);
        MunitionController newMunitionController = newMunition.GetComponent<MunitionController>();
        newMunitionController.SetDamage(_weaponDamage);
        newMunitionController.SetLifeSpan(_weaponRange);
    }

    /*****************************************
     *            Combat Actions             *
     ****************************************/
    public void TakeDamage(int damage)
    {

        _currentHealth -= damage;
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (_currentHealth <= 0) {
            Die();
        }
    }

    public void Die()
    {
        //Instantiate(_debris, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
