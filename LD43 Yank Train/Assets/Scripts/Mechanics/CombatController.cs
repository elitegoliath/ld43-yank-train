using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    #region Variable Instantiation

    #region Character Stats

    private int _maxHealth = 20;
    private int _currentHealth = 20;
    private int _armor = 1;

    #endregion Character Stats

    #region Ranged Weapon Stats

    private float _weaponRange = 4f;
    private float _weaponDelay = 2f;
    private float _weaponAccuracy = 10f;
    private int _weaponDamage = 1;
    private bool _isCompanionOnDeath = false;
    private GameObject _weaponMunition;
    private List<SpriteRenderer> _mySprites = new List<SpriteRenderer>();
    private List<Color> _defaultColors = new List<Color>();
    private GameObject _companion;

    #endregion Ranged Weapon Stats

    #region Death

    private bool _detonatesOnDeath = false;
    private bool _isUnkillable = false;
    private bool _isPlayer = false;
    private bool _isDying = false;
    private GameObject _debris;
    private GameObject _detonationEffect;
    private PlayerControls _player;

    #endregion Death

    #endregion Variable Instantiation

    /// <summary>
    /// method that is used to init Combat Controller.
    /// </summary>
    private void Start()
    {
        #region consts for readability

        const string _playerTag = "Player";

        #endregion consts for readability

        if (gameObject.tag == _playerTag)
        {
            _isPlayer = true;
            _player = gameObject.GetComponent<PlayerControls>();
        }
    }

    /// <summary>
    /// sets AI's to "explode" on death event.
    /// </summary>
    /// <param name="deathBoom"></param>
    public void SetDetonationOnDeath(bool deathBoom)
    {
        _detonatesOnDeath = deathBoom;
    }

    /// <summary>
    /// sets the specific explosion particle effect to use for the specific AI.
    /// </summary>
    /// <param name="detonEffect"></param>
    public void SetDetonationEffect(GameObject detonEffect)
    {
        _detonationEffect = detonEffect;
    }

    #region MaxHealth

    /// <summary>
    /// set objects max health.
    /// </summary>
    /// <param name="maxHealth"></param>
    public void SetMaxHealth(int maxHealth)
    {
        _maxHealth = maxHealth;

        // TODO: If max health can change mid-game, remove this.
        _currentHealth = maxHealth;

        // Limits health to the maximum.
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    /// <summary>
    /// check objects max health.
    /// </summary>
    /// <returns></returns>
    public int GetMaxHealth()
    {
        return _maxHealth;
    }

    #endregion MaxHealth

    #region Current Health

    /// <summary>
    /// Insures objects health does not surpass the set cap.
    /// </summary>
    /// <param name="currentHealth"></param>
    public void SetCurrentHealth(int currentHealth)
    {
        _currentHealth = currentHealth;

        // Limit health to the maximum.
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    /// <summary>
    /// checks objects current HP.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    /// <summary>
    /// makes an object invulnerable.
    /// </summary>
    public void MakeUnkillable()
    {
        _isUnkillable = true;
    }

    #endregion Current Health

    #region Armor

    /// <summary>
    /// sets objects armour value.
    /// </summary>
    /// <param name="armor"></param>
    public void SetArmor(int armor)
    {
        _armor = armor;
    }

    /// <summary>
    /// checks armour value.
    /// </summary>
    /// <returns></returns>
    public int GetArmor()
    {
        return _armor;
    }

    #endregion Armor

    #region Weapon Range

    /// <summary>
    /// sets weapon range.
    /// </summary>
    /// <param name="weaponRange"></param>
    public void SetWeaponRange(float weaponRange)
    {
        _weaponRange = weaponRange;
    }

    /// <summary>
    /// checks weapon range.
    /// </summary>
    /// <returns></returns>
    public float GetWeaponRange()
    {
        return _weaponRange;
    }

    #endregion Weapon Range

    #region Weapon Delay

    /// <summary>
    /// sets the weapon delay.
    /// </summary>
    /// <param name="weaponDelay"></param>
    public void SetWeaponDelay(float weaponDelay)
    {
        _weaponDelay = weaponDelay;
    }

    /// <summary>
    /// checks the weapon delay.
    /// </summary>
    /// <returns></returns>
    public float GetWeaponDelay()
    {
        return _weaponDelay;
    }

    #endregion Weapon Delay

    #region Weapon Accuracy

    /// <summary>
    /// sets the weapons accuracy.
    /// </summary>
    /// <param name="weaponAccuracy"></param>
    public void SetWeaponAccuracy(float weaponAccuracy)
    {
        _weaponAccuracy = weaponAccuracy;
    }

    /// <summary>
    /// Checks the weapons accuracy.
    /// </summary>
    /// <returns></returns>
    public float GetWeaponAccuracy()
    {
        return _weaponAccuracy;
    }

    #endregion Weapon Accuracy

    #region Weapon Damage

    /// <summary>
    /// Sets the damage of a weapon.
    /// </summary>
    /// <param name="weaponDamage"></param>
    public void SetWeaponDamage(int weaponDamage)
    {
        _weaponDamage = weaponDamage;
    }

    /// <summary>
    /// checks the damage of a weapon.
    /// </summary>
    /// <returns></returns>
    public int GetWeaponDamage()
    {
        return _weaponDamage;
    }

    #endregion Weapon Damage

    #region Weapon Munition

    /// <summary>
    /// Sets the type of ammo in a weapon.
    /// </summary>
    /// <param name="weaponMunition"></param>
    public void SetWeaponMunition(GameObject weaponMunition)
    {
        _weaponMunition = weaponMunition;
    }

    /// <summary>
    /// checks the type of ammo in a weapon.
    /// </summary>
    /// <returns></returns>
    public GameObject GetWeaponMunition()
    {
        return _weaponMunition;
    }

    #endregion Weapon Munition

    #region Debris

    /// <summary>
    /// sets a flag on a AI after death to turn it into debris.
    /// </summary>
    /// <param name="debris"></param>
    public void SetDebris(GameObject debris)
    {
        _debris = debris;
    }

    /// <summary>
    /// sets the AI as a compantion.
    /// </summary>
    /// <param name="companion"></param>
    public void SetCompanion(GameObject companion)
    {
        _companion = companion;
    }

    /// <summary>
    /// sets a flag on the AI to dertermine if the AI was ally on death.
    /// </summary>
    /// <param name="companionOnDeath"></param>
    public void SetCompanionOnDeath(bool companionOnDeath)
    {
        _isCompanionOnDeath = companionOnDeath;
    }

    #endregion Debris

    #region Ranged Weapon

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

        if (bias <= 50f)
        {
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

    #endregion Ranged Weapon

    #region Combat Actions

    /// <summary>
    /// Calc new health amount when damage is taken.
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        if (_isUnkillable == false)
        {
            _currentHealth -= damage;
            CheckHealth();

            // If not dead yet, blink!
            DamageBlink();
        }
    }

    /// <summary>
    /// applies a color change to sprites when the recive damage.
    /// </summary>
    public void DamageBlink()
    {
        #region consts for readability

        const string _setDefaultColors = "SetDefaultSpriteColors";

        #endregion consts for readability

        // If sprites aren't cached, do it.
        if (_mySprites.Count == 0)
        {
            _mySprites = new List<SpriteRenderer>(gameObject.GetComponentsInChildren<SpriteRenderer>());
            foreach (SpriteRenderer sprite in _mySprites)
            {
                _defaultColors.Add(sprite.color);
            }
        }

        foreach (SpriteRenderer sprite in _mySprites)
        {
            sprite.color = new Color(221f, 0f, 0f, 0.5f);
        }

        Invoke(_setDefaultColors, 0.1f); ///<see cref="SetDefaultSpriteColors"/>
    }

    /// <summary>
    /// checks player health change events and if player hits 0hp launches player death event.
    /// </summary>
    public void CheckHealth()
    {
        if (_isPlayer == true)
        {
            if (_currentHealth <= 0)
            {
                _isUnkillable = true;
                _player.DisableControls();
            }
            else
            {
                _player.PlayerHealthChanged(_currentHealth);
            }
        }
        else if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// called on AI death, determines which death event to launch.
    /// </summary>
    /// <param name="isViolentDeath"></param>
    /// <param name="deathDelay"></param>
    public void Die(bool isViolentDeath = false, float deathDelay = 0)
    {
        if (_isDying == false)
        {
            _isDying = true;

            if (_detonatesOnDeath == true)
            {
                Instantiate(_detonationEffect, transform.position, transform.rotation);
            }
            else
            {
                if (_isCompanionOnDeath == true && isViolentDeath == false)
                {
                    Instantiate(_companion, transform.position, transform.rotation);
                }
                else if (deathDelay == 0)
                {
                    // TODO: Currently assuming only Assimilate uses death delay. Ya no.
                    SpawnDebris();
                }
            }

            if (deathDelay > 0)
            {
                Destroy(gameObject, deathDelay);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// On companion sacrifice charge/explosion cause enemy AI death.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        #region consts for readability

        const string _enemyTag = "Enemy";
        const string _transportTag = "Transport";

        #endregion consts for readability

        if (_detonatesOnDeath == true)
        {
            Collider2D col = collision.collider;
            string tag = col.tag;

            if (tag == _enemyTag || tag == _transportTag)
            {
                Die();
            }
        }
    }

    /// <summary>
    /// Adds value of heal to players current health pool.
    ///     Can go over max health. Check accordingly on call.
    /// </summary>
    /// <param name="healAmount"></param>
    public void Heal(int healAmount)
    {
        int newHealth = _currentHealth += healAmount;
        SetCurrentHealth(newHealth);
    }

    #endregion Combat Actions

    #region Helpers

    /// <summary>
    /// Event called on AI deaths to spawn Debris.
    /// </summary>
    private void SpawnDebris()
    {
        if (_debris != null)
        {
            Instantiate(_debris, transform.position, transform.rotation);
        }
    }

    /// <summary>
    /// accepts rgb and alpha as variables and adds the color that results from such to sprite color array.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    /// <param name="a">Alpha layer. Defaulted to 1.</param>
    public void ChangeEntityColor(int r, int g, int b, int a = 1)
    {
        Color newColor = new Color(r, g, b, a);
        // Get all the sprite renderers in the object.
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = newColor;
        }
    }

    //TODO: Confirm if summery is correct.
    /// <summary>
    /// Adds a new color to the sprite color array?
    /// </summary>
    /// <param name="newColor"></param>
    public void ChangeEntityColor(Color newColor)
    {
        // Get all the sprite renderers in the object.
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = newColor;
        }
    }

    /// <summary>
    /// changes sprite's color layout.
    /// </summary>
    /// <param name="colorList"></param>
    /// <param name="spriteList"></param>
    public void ChangeEntityColor(Color[] colorList, SpriteRenderer[] spriteList)
    {
        for (int i = 0; i < colorList.Length; i++)
        {
            spriteList[i].color = colorList[i];
        }
    }

    /// <summary>
    /// sets default color set to all sprites.
    /// </summary>
    public void SetDefaultSpriteColors()
    {
        for (int i = 0; i < _mySprites.Count; i++)
        {
            _mySprites[i].color = _defaultColors[i];
        }
    }

    /// <summary>
    /// When an enemy AI dies if it had the correct flag (indicated by a halo effect) become compantion.
    /// </summary>
    /// <param name="companionPrefab"></param>
    /// <param name="indicatorLight"></param>
    public void InstantiateCompanionOnDeath(GameObject companionPrefab, GameObject indicatorLight)
    {
        // Set properties.
        _companion = companionPrefab;
        _isCompanionOnDeath = true;

        // Instantiate light;
        GameObject newLight = Instantiate(indicatorLight, transform);
        newLight.transform.localPosition = new Vector3(0, 0, -0.7f);
    }

    #endregion Helpers
}