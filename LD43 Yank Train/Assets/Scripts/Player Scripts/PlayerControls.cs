using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour
{
    [Header("Characteristics")]
    public float moveSpeed = 100f;
    public float transcodeDifficulty = 2f;
    public int assimilateHealAmount = 200;

    [Header("Combat Stats")]
    public int maxHealth = 200;
    public int armor = 1;
    public float rangedAttackDelay = 0.5f;
    public float rangedWeaponRange = 1f;
    public int rangedWeaponDamage = 2;
    public GameObject rangedWeaponMunition;

    [Header("Weapons")]
    public CombatController leftGun;
    public CombatController rightGun;

    private bool _canFireRangedWeapons = true;
    private bool _isSacrificingPal = false;
    private bool _isAssimilatingBro = false;
    private bool _isSacrificeOnCooldown = false;
    private bool _isAssimilateOnCooldown = false;
    private bool _sacTrigger = false;
    private bool _simTrigger = false;
    private bool _controlsEnabled = true;
    private Rigidbody2D _myRigidBody;
    private Camera _cam;
    private CombatController _myCombatController;
    private WaveController _waveController;
    private IEnumerator _transcodeMind;

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

        _transcodeMind = TranscodeMind();

        StartCoroutine(_transcodeMind);
    }

    /// <summary>
    /// It's the update function. Called once per frame.
    /// </summary>
    private void Update()
    {
        if (_controlsEnabled == true) {
            FaceAvatar();
            FireRangedWeapons();
            SacrificeAFriend();
            AssimilateABuddy();
        }
    }

    /// <summary>
    /// Fixed update is better used to calculate physics based events.
    /// </summary>
    private void FixedUpdate()
    {
        MoveAvatar();
    }

    public void DisableControls()
    {
        _controlsEnabled = false;
    }

    /// <summary>
    /// Listens for user movement inputs and applies the proper physics.
    /// </summary>
    private void MoveAvatar()
    {
        if (_controlsEnabled == true)
        {
            // Get the combination of movement inputs from the player.
            float moveX = (Input.GetAxis("Horizontal") * Time.deltaTime) * moveSpeed;
            float moveY = (Input.GetAxis("Vertical") * Time.deltaTime) * moveSpeed;

            _myRigidBody.velocity = new Vector2(moveX, moveY);
        }
    }

    /// <summary>
    /// Ensures the player avatar is always facing the mouse direction.
    /// </summary>
    private void FaceAvatar()
    {
        Vector3 mousePos = GetMousePos();

        // Fuckin fancy ass math... I really don't get it, but it works, so yolo.
        float AngleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angle = (180 / Mathf.PI) * AngleRad;

        _myRigidBody.rotation = angle + 90;
    }

    private Vector3 GetMousePos()
    {
        // Distance between player avatar and the camera so the angle can be properly derived.
        float camDis = _cam.transform.position.y - transform.position.y;

        // Get the mouse position in the world space, while using the camera distance for Z, for some reason.
        Vector3 mousePos = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDis));

        return mousePos;
    }

    private void FireRangedWeapons()
    {
        if (Input.GetAxis("Fire1") != 0 && _canFireRangedWeapons == true) {
            _canFireRangedWeapons = false;

            leftGun.FireRangedWeapon();
            rightGun.FireRangedWeapon();

            Invoke("FireRangedWeaponsCooldown", rangedAttackDelay);
        }
    }

    private void SacrificeAFriend()
    {
        if (Input.GetAxis("Fire2") != 0 && _isSacrificeOnCooldown == false) {
            _isSacrificingPal = true;

            if (_sacTrigger == false) {
                _sacTrigger = true;
                SacrificeRandomCompanion();
            }
        } else if (_isSacrificingPal == true) {
            // Begin cooldown after input stops.
            _isSacrificingPal = false;
            _isSacrificeOnCooldown = true;

            Invoke("SacrificeCooldown", 0.5f);
        }
    }

    private void AssimilateABuddy()
    {
        if (Input.GetAxis("Q") != 0 && _isAssimilateOnCooldown == false) {
            _isAssimilatingBro = true;

            if (_simTrigger == false) {
                _simTrigger = true;
                AssimilateRandomCompanion();
            }
        } else if(_isAssimilatingBro == true) {
            // Begin cooldown after input stops.
            _isAssimilatingBro = false;
            _isAssimilateOnCooldown = true;

            Invoke("AssimilateCooldown", 0.5f);
        }
    }

    private void SacrificeRandomCompanion()
    {
        // Tell him to wreck himself into them.
        FriendlyGroundBotAI newDeathBuddy = FindRandomCompanion();

        if (newDeathBuddy != null) {
            newDeathBuddy.SacrificeSelf(GetMousePos());
        }
    }

    private void AssimilateRandomCompanion()
    {
        // Tell him to wreck himself into yourself.
        FriendlyGroundBotAI newHealthBuddy = FindRandomCompanion();

        if (newHealthBuddy != null) {
            newHealthBuddy.AssimilateIntoPlayer();
            _myCombatController.Heal(assimilateHealAmount);
            int newHealth = _myCombatController.GetCurrentHealth();
            PlayerHealthChanged(newHealth);
        }
    }

    private FriendlyGroundBotAI FindRandomCompanion()
    {
        GameObject[] companions = GameObject.FindGameObjectsWithTag("CompanionBot");
        FriendlyGroundBotAI foundBuddy = null;

        if (companions.Length > 0) {
            int chosenIndex = Random.Range(0, companions.Length);
            foundBuddy = companions[chosenIndex].GetComponent<FriendlyGroundBotAI>();
        }

        return foundBuddy;
    }

    public void PlayerHealthChanged(int currentHealth)
    {
        _waveController.UpdateHealthbar(currentHealth);
    }

    private void FireRangedWeaponsCooldown()
    {
        _canFireRangedWeapons = true;
    }

    private void SacrificeCooldown()
    {
        _isSacrificeOnCooldown = false;
        _sacTrigger = false;
    }

    private void AssimilateCooldown()
    {
        _isAssimilateOnCooldown = false;
        _simTrigger = false;
    }

    private IEnumerator TranscodeMind() {
        yield return null;

        while (_controlsEnabled == true) {
            yield return new WaitForSeconds(transcodeDifficulty);
            _waveController.AddPlayerTranscode();
        }
    }

    public void Died()
    {
        _waveController.GameOver();
    }
}