using PolyNav;
using UnityEngine;

public class FriendlyGroundBotAI : MonoBehaviour
{
    #region Variable Instantiation

    #region Navigation

    [Header("Navigation")]
    public float checkRangesCooldown = 0.2f;

    #endregion Navigation

    #region Characteristics

    [Header("Characteristics")]
    public int maxHealth = 10;
    public int armor = 1;
    public float attackingTurnSpeed = 30f;
    public float followRange = 2f;
    public float speedVarience = 2f;

    #endregion Characteristics

    #region Weapon Stats

    [Header("Weapon Stats")]
    public float attackRange = 5f;
    public float attackDelay = 1f;
    public float accuracy = 10f;
    public int weaponDamage = 2;
    public float weaponRange = 1f;
    public CombatController rangedWeaponCombatController;
    public GameObject rangedWeaponMunition;

    #endregion Weapon Stats

    #region Effects

    [Header("Effects")]
    public GameObject selfDestructIndicator;
    public GameObject deathDebris;
    public GameObject detonationEffect;
    public GameObject assimilationIndicator;
    public ParticleSystem assimilationParticles;

    #endregion Effects

    private CombatController _myCombatController;
    private AIController _myAIController;
    private bool _isAIActive = true;

    #endregion Variable Instantiation

    /// <summary>
    /// Sets Variables <see cref="_myCombatController"/> and <seealso cref="_myAIController"/>
    ///     to current/starting state of gameObject Components <see cref="CombatController"/> and <seealso cref="AIController"/> respectivly.
    /// </summary>
    private void Awake()
    {
        // Get components
        _myCombatController = gameObject.GetComponent<CombatController>();
        _myAIController = gameObject.GetComponent<AIController>();
    }

    /// <summary>
    /// Instantiates Friendly AI's with params on load.
    /// </summary>
    private void Start()
    {
        // TODO: Categorize these.
        _myAIController.SetAttackRange(attackRange);
        _myAIController.SetFollowRange(followRange);
        _myAIController.SetAttackDelay(attackDelay);
        _myAIController.SetRangedWeaponCombatController(rangedWeaponCombatController);
        _myAIController.SetAttackingTurnSpeed(attackingTurnSpeed);
        _myAIController.SetPlayer();
        _myAIController.InstantiateSpeedDifferential(speedVarience);
        _myCombatController.SetDebris(deathDebris);
        _myCombatController.SetDetonationEffect(detonationEffect);

        // Initialize nav.
        _myAIController.SetCheckRangesCooldown(checkRangesCooldown);

        // Initialize characteristics.
        _myCombatController.SetMaxHealth(maxHealth);
        _myCombatController.SetCurrentHealth(maxHealth);
        _myCombatController.SetArmor(armor);

        // Initialize weapon.
        rangedWeaponCombatController.SetRangedWeaponStats(accuracy, weaponRange, weaponDamage, rangedWeaponMunition);

        //// Initialize target. Player is gona be the default target for now.
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //_myAIController.SetTarget(player);

        // Register self with Wave Controller.
        EventManager.TriggerEvent("registerCompanion");

        // Activate initial AI behaviors.
        // Do so after delay for effect and for the enemy to clear the space.
        _myAIController.ActivateAltAI();
    }

    /// <summary>
    /// Event called opon Friendly AI death
    /// </summary>
    private void OnDestroy()
    {
        #region Consts for readability

        const string _friendlyDeath = "companionDeath";

        #endregion Consts for readability

        EventManager.TriggerEvent(_friendlyDeath);
    }

    /// <summary>
    /// For the most part, the Update function in this class acts as an AI registry.
    /// They all know to behave with one another.
    /// </summary>
    private void Update()
    {
        if (_isAIActive == true)
        {
            // Lot's of targets to chose from. Keep an eye out...
            _myAIController.AIFindClosestTarget();

            // These ranges drive the aggressive behavior of this robot.
            _myAIController.AITrackTarget();

            // Follow the leader, as they say.
            _myAIController.AIFollowPlayer();

            // If within weapon range, fire fire fire!!
            _myAIController.AIAttackTarget();
        }
    }

    /// <summary>
    /// Method called when a Friendly AI is used as a "bomb" either for map clearing or enemy AI kills.
    /// </summary>
    /// <param name="mousePos">Cords retrieved from mouse at time of command.</param>
    public void SacrificeSelf(Vector3 mousePos)
    {
        #region Consts for readability

        const string _selfSacrifier = "Suicider";

        #endregion Consts for readability

        _isAIActive = false;

        // Remove from list of possible sac choices.
        gameObject.tag = _selfSacrifier;

        _myAIController.InitializeSelfDestructSequence(mousePos);
        _myCombatController.SetDetonationOnDeath(true);

        GameObject newLight = Instantiate(selfDestructIndicator, transform);
        newLight.transform.localPosition = new Vector3(0, 0, -0.7f);
    }

    /// <summary>
    /// Called to sacrifice Ally AI to heal player.
    /// </summary>
    public void AssimilateIntoPlayer()
    {
        #region Consts for readability

        const string _assimilatedAlly = "Assimilator";

        #endregion Consts for readability

        #region Variable Instantiation

        _isAIActive = false;
        PolyNavAgent agent = gameObject.GetComponent<PolyNavAgent>();

        #endregion Variable Instantiation

        agent.Stop();

        gameObject.tag = _assimilatedAlly;
        gameObject.GetComponent<CircleCollider2D>().enabled = false;

        GameObject newLight = Instantiate(assimilationIndicator, transform);
        newLight.transform.localPosition = new Vector3(0, 0, -0.7f);

        // emit particles for a bit before dying;
        ParticleSystem regenSparkles = Instantiate(assimilationParticles, transform);
        regenSparkles.transform.localScale = new Vector3(0.02f, 0.06f, 0.06f);

        _myCombatController.Die(false, 2.5f);
    }
}