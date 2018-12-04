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
    private bool _isCompanionOnDeath = false;
    private GameObject _weaponMunition;
    private List<SpriteRenderer> _mySprites = new List<SpriteRenderer>();
    private List<Color> _defaultColors = new List<Color>();
    private GameObject _companion;

    // Death
    private bool _detonatesOnDeath = false;
    private GameObject _debris;
    private AudioClip _deathExplosionSound;

    public void SetDetonationOnDeath(bool deathBoom)
    {
        _detonatesOnDeath = deathBoom;
    }

    /*****************************************
     *              Max Health               *
     ****************************************/
    public void SetMaxHealth(int maxHealth)
    {
        _maxHealth = maxHealth;

        // TODO: If max health can change mid-game, remove this.
        _currentHealth = maxHealth;

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

    public void SetCompanion(GameObject companion)
    {
        _companion = companion;
    }

    public void SetCompanionOnDeath(bool companionOnDeath)
    {
        _isCompanionOnDeath = companionOnDeath;
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

        // If not dead yet, blink!
        DamageBlink();
    }

    public void DamageBlink()
    {
        // If sprites aren't cached, do it.
        if (_mySprites.Count == 0) {
            _mySprites = new List<SpriteRenderer>(gameObject.GetComponentsInChildren<SpriteRenderer>());
            foreach (SpriteRenderer sprite in _mySprites) {
                _defaultColors.Add(sprite.color);
            }
        }

        foreach (SpriteRenderer sprite in _mySprites) {
            sprite.color = new Color(221f, 0f, 0f, 0.5f);
        }

        Invoke("SetDefaultSpriteColors", 0.1f);
    }

    public void CheckHealth()
    {
        if (_currentHealth <= 0) {
            Die();
        }
    }

    public void DieViolently()
    {
        SpawnDebris();
        Destroy(gameObject);
    }

    public void Die()
    {
        if (_detonatesOnDeath == true) {
            // Cause self-destruct then...
            DieViolently();
        } else {
            if(_isCompanionOnDeath == true) {
                Instantiate(_companion, transform.position, transform.rotation);
            } else {
                SpawnDebris();
            }

            Destroy(gameObject);
        }
    }

    /*****************************************
     *               Helpers                 *
     ****************************************/
    private void SpawnDebris()
    {
        Instantiate(_debris, transform.position, transform.rotation);
        // TODO: Cause explosion FX on death (sound and viz);
    }

    public void ChangeEntityColor(int r, int g, int b, int a = 1)
    {
        Color newColor = new Color(r, g, b, a);
        // Get all the sprite renderers in the object.
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
            spriteRenderer.color = newColor;
        }
    }

    public void ChangeEntityColor(Color newColor)
    {
        // Get all the sprite renderers in the object.
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer spriteRenderer in spriteRenderers) {
            spriteRenderer.color = newColor;
        }
    }

    public void ChangeEntityColor(Color[] colorList, SpriteRenderer[] spriteList)
    {
        for (int i = 0; i < colorList.Length; i++) {
            spriteList[i].color = colorList[i];
        }
    }

    public void SetDefaultSpriteColors()
    {
        for (int i = 0; i < _mySprites.Count; i++) {
            _mySprites[i].color = _defaultColors[i];
        }
    }

    public void InstantiateCompanionOnDeath(GameObject companionPrefab, GameObject indicatorLight)
    {
        // Set properties.
        _companion = companionPrefab;
        _isCompanionOnDeath = true;

        // Instantiate light;
        GameObject newLight = Instantiate(indicatorLight, transform);
        newLight.transform.localPosition = new Vector3(0, 0, -0.7f);
    }
}
