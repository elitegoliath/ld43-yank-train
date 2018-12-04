using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Characteristics")]
    public float moveSpeed = 100f;

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
    private Rigidbody2D _myRigidBody;
    private Camera _cam;

    private void Awake()
    {
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
    }

    /// <summary>
    /// It's the update function. Called once per frame.
    /// </summary>
    private void Update()
    {
        FaceAvatar();
        FireRangedWeapons();
    }

    /// <summary>
    /// Fixed update is better used to calculate physics based events.
    /// </summary>
    private void FixedUpdate()
    {
        MoveAvatar();
    }

    /// <summary>
    /// Listens for user movement inputs and applies the proper physics.
    /// </summary>
    private void MoveAvatar()
    {
        // Get the combination of movement inputs from the player.
        float moveX = (Input.GetAxis("Horizontal") * Time.deltaTime) * moveSpeed;
        float moveY = (Input.GetAxis("Vertical") * Time.deltaTime) * moveSpeed;

        _myRigidBody.velocity = new Vector2(moveX, moveY);
    }

    /// <summary>
    /// Ensures the player avatar is always facing the mouse direction.
    /// </summary>
    private void FaceAvatar()
    {
        // Distance between player avatar and the camera so the angle can be properly derived.
        float camDis = _cam.transform.position.y - transform.position.y;

        // Get the mouse position in the world space, while using the camera distance for Z, for some reason.
        Vector3 mousePos = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDis));

        // Fuckin fancy ass math... I really don't get it, but it works, so yolo.
        float AngleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angle = (180 / Mathf.PI) * AngleRad;

        _myRigidBody.rotation = angle + 90;
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

    private void FireRangedWeaponsCooldown()
    {
        _canFireRangedWeapons = true;
    }
}