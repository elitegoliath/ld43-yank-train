using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    #region Variable Initilization

    #region Characterisics

    [Header("Characteristics")]
    public float moveSpeed = 100f;
    public int assimilateHealAmount = 200;

    #endregion Characterisics

    #region Combat Stats

    [Header("Combat Stats")]
    public float rangedAttackDelay = 0.5f;
    public float rangedWeaponRange = 1f;
    public GameObject rangedWeaponMunition;
    public int armor = 1;
    public int maxHealth = 200;
    public int rangedWeaponDamage = 2;

    #endregion Combat Stats

    #region Weapons

    [Header("Weapons")]
    private bool _canFireRangedWeapons = true;
    private bool _controlsEnabled = true;
    private bool _isAssimilateOnCooldown = false;
    private bool _isAssimilatingBro = false;
    private bool _isSacrificeOnCooldown = false;
    private bool _isSacrificingPal = false;
    private bool _sacTrigger = false;
    private bool _simTrigger = false;
    private Camera _cam;
    private CombatController _myCombatController;
    private Rigidbody2D _myRigidBody;
    private WaveController _waveController;
    public CombatController leftGun;
    public CombatController rightGun;

    #endregion Weapons

    #endregion Variable Initilization

    /// <summary>
    /// Player init method.
    /// </summary>
    private void Awake()
    {
        _myCombatController = gameObject.GetComponent<CombatController>();
        leftGun.SetRangedWeaponStats(0f, rangedWeaponRange, rangedWeaponDamage, rangedWeaponMunition);
        rightGun.SetRangedWeaponStats(0f, rangedWeaponRange, rangedWeaponDamage, rangedWeaponMunition);
    }

    /// <summary>
    /// Runs once when the script has been initialized.
    /// </summary>
    private void Start()
    {
        _myRigidBody = GetComponent<Rigidbody2D>();
        _cam = Camera.main;

        _waveController = FindObjectOfType<WaveController>();
        _waveController.SetHealthbar(maxHealth);

        _myCombatController.SetMaxHealth(maxHealth);
    }

    /// <summary>
    /// It's the update function. Called once per frame.
    /// </summary>
    private void Update()
    {
        if(_controlsEnabled == true)
        {
            FaceAvatar();               ///<see cref="FaceAvatar"/>
            FireRangedWeapons();        ///<see cref="FireRangedWeapons"/>
            SacrificeAFriend();         ///<see cref="SacrificeAFriend"/>
            AssimilateABuddy();         ///<see cref="AssimilateABuddy"/>
        }
    }

    /// <summary>
    /// Fixed update is better used to calculate physics based events.
    /// </summary>
    private void FixedUpdate()
    {
        MoveAvatar();
    }

    /// <summary>
    /// toggles players controls to disabled.
    /// </summary>
    public void DisableControls()
    {
        _controlsEnabled = false;
    }

    /// <summary>
    /// Listens for user movement inputs and applies the proper physics.
    /// </summary>
    private void MoveAvatar()
    {
        #region consts for readability

        const string _horizonal = "Horizontal";
        const string _vertical = "Vertical";

        #endregion consts for readability

        // Get the combination of movement inputs from the player.
        float moveX = (Input.GetAxis(_horizonal) * Time.deltaTime) * moveSpeed;
        float moveY = (Input.GetAxis(_vertical) * Time.deltaTime) * moveSpeed;

        _myRigidBody.velocity = new Vector2(moveX, moveY);
    }

    /// <summary>
    /// Ensures the player avatar is always facing the mouse direction.
    /// </summary>
    private void FaceAvatar()
    {
        Vector3 mousePos = GetMousePos();

        float AngleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angle = (180 / Mathf.PI) * AngleRad;

        _myRigidBody.rotation = angle + 90;
    }

    /// <summary>
    /// Gets the position of the players mouse.
    /// </summary>
    private Vector3 GetMousePos()
    {
        return _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cam.transform.position.y - transform.position.y));
    }

    /// <summary>
    /// method called to shoot weapons.
    /// </summary>
    private void FireRangedWeapons()
    {
        #region consts for readability

        const string _fire = "Fire1";
        const string _rangedWeaponCD = "FireRangedWeaponsCooldown";

        #endregion consts for readability

        if(Input.GetAxis(_fire) != 0 && _canFireRangedWeapons == true)
        {
            _canFireRangedWeapons = false;

            leftGun.FireRangedWeapon();
            rightGun.FireRangedWeapon();

            Invoke(_rangedWeaponCD, rangedAttackDelay);     ///<see cref="FireRangedWeaponsCooldown"/>
        }
    }

    /// <summary>
    /// event called to sacrifice a companion.
    /// </summary>
    private void SacrificeAFriend()
    {
        #region consts for readability

        const string _fire = "Fire2";
        const string _innocentLambCD = "SacrificeCooldown";

        #endregion consts for readability

        if(Input.GetAxis(_fire) != 0 && _isSacrificeOnCooldown == false)
        {
            _isSacrificingPal = true;

            if(_sacTrigger == false)
            {
                _sacTrigger = true;
                SacrificeRandomCompanion();
            }
        }
        else if(_isSacrificingPal == true)
        {
            // Begin cooldown after input stops.
            _isSacrificingPal = false;
            _isSacrificeOnCooldown = true;

            Invoke(_innocentLambCD, 0.5f);      ///<see cref="SacrificeCooldown"/>
        }
    }

    /// <summary>
    /// called to sacrifice a companion to heal player.
    /// </summary>
    private void AssimilateABuddy()
    {
        #region consts for readability

        const string _fire = "Q";
        const string _assimilationCD = "AssimilateCooldown";

        #endregion consts for readability

        if(Input.GetAxis(_fire) != 0 && _isAssimilateOnCooldown == false)
        {
            _isAssimilatingBro = true;

            if(_simTrigger == false)
            {
                _simTrigger = true;
                AssimilateRandomCompanion();
            }
        }
        else if(_isAssimilatingBro == true)
        {
            // Begin cooldown after input stops.
            _isAssimilatingBro = false;
            _isAssimilateOnCooldown = true;

            Invoke(_assimilationCD, 0.5f);      ///<see cref="AssimilateCooldown"/>
        }
    }

    /// <summary>
    /// tells a companion to charge enemies before exploding.
    /// </summary>
    private void SacrificeRandomCompanion()
    {
        // Tell him to wreck himself into them.
        FriendlyGroundBotAI newDeathBuddy = FindRandomCompanion();

        if(newDeathBuddy != null)
        {
            newDeathBuddy.SacrificeSelf(GetMousePos());
        }
    }

    /// <summary>
    /// uses a random companion to heal player.
    /// </summary>
    private void AssimilateRandomCompanion()
    {
        FriendlyGroundBotAI newHealthBuddy = FindRandomCompanion();     ///<see cref="FindRandomCompanion"/>

        if(newHealthBuddy != null)
        {
            #region Variable Inintialization

            int newHealth = _myCombatController.GetCurrentHealth();

            #endregion Variable Inintialization

            newHealthBuddy.AssimilateIntoPlayer();
            _myCombatController.Heal(assimilateHealAmount);
            PlayerHealthChanged(newHealth);
        }
    }

    private FriendlyGroundBotAI FindRandomCompanion()
    {
        #region consts for readability

        const string _companionTag = "CompanionBot";

        #endregion consts for readability

        GameObject[] companions = GameObject.FindGameObjectsWithTag(_companionTag);
        FriendlyGroundBotAI foundBuddy = null;

        if(companions.Length > 0)
        {
            int chosenIndex = Random.Range(0, companions.Length);
            return foundBuddy = companions[chosenIndex].GetComponent<FriendlyGroundBotAI>();
        }
    }

    /// <summary>
    /// When a players health changes feed the current health into the update health bar method.
    /// </summary>
    /// <param name="currentHealth"></param>
    public void PlayerHealthChanged(int currentHealth)
    {
        _waveController.UpdateHealthbar(currentHealth);
    }

    /// <summary>
    /// when weapons cooldown is over sets can fire flag to true.
    /// </summary>
    private void FireRangedWeaponsCooldown()
    {
        _canFireRangedWeapons = true;
    }

    /// <summary>
    /// flags that handle weather player can currently sacrifice a companion
    /// </summary>
    private void SacrificeCooldown()
    {
        _isSacrificeOnCooldown = false;
        _sacTrigger = false;
    }

    /// <summary>
    /// flags that handle weather player can currently sacrifice a companion
    /// </summary>
    private void AssimilateCooldown()
    {
        _isAssimilateOnCooldown = false;
        _simTrigger = false;
    }
}