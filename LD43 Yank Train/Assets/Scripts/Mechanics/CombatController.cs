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
    private bool _isUnkillable = false;
    private bool _isPlayer = false;
    private bool _isDying = false;
    private GameObject _debris;
    private GameObject _detonationEffect;
    private PlayerControls _player;

    private void Start()
    {
        string _tag = gameObject.tag;

        if (_tag == "Player") {
            _isPlayer = true;
            _player = gameObject.GetComponent<PlayerControls>();
        }
    }

    public void SetDetonationOnDeath(bool deathBoom)
    {
        _detonatesOnDeath = deathBoom;
    }

    public void SetDetonationEffect(GameObject detonEffect)
    {
        _detonationEffect = detonEffect;
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

    public void MakeUnkillable()
    {
        _isUnkillable = true;
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
        if (_isUnkillable == false) {
            _currentHealth -= damage;
            CheckHealth();

            // If not dead yet, blink!
            DamageBlink();
        }
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
        if (_isPlayer == true) {
            if (_currentHealth <= 0) {
                _isUnkillable = true;
                _player.DisableControls();
                _player.Died();
            } else {
                _player.PlayerHealthChanged(_currentHealth);
            }
        } else if (_currentHealth <= 0) {
            Die();
        }
    }

    public void Die(bool isViolentDeath = false, float deathDelay = 0)
    {
        if (_isDying == false) {
            _isDying = true;

            if(_detonatesOnDeath == true) {
                Instantiate(_detonationEffect, transform.position, transform.rotation);
            } else {
                if(_isCompanionOnDeath == true && isViolentDeath == false) {
                    Instantiate(_companion, transform.position, transform.rotation);
                } else if (deathDelay == 0) {
                    // TODO: Currently assuming only Assimilate uses death delay. Ya no.
                    SpawnDebris();
                }
            }

            
            if (deathDelay > 0) {
                Destroy(gameObject, deathDelay);
            } else {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_detonatesOnDeath == true) {
            Collider2D col = collision.collider;
            string _tag = col.tag;

            if (_tag == "Enemy" || _tag == "Transport") {
                Die();
            }
        }
    }

    public void Heal(int healAmount)
    {
        int newHealth = _currentHealth += healAmount;
        SetCurrentHealth(newHealth);
    }

    /*****************************************
     *               Helpers                 *
     ****************************************/
    private void SpawnDebris()
    {
        if (_debris != null) {
            Instantiate(_debris, transform.position, transform.rotation);
        }
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
