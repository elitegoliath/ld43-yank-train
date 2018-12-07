using UnityEngine;
using UnityEngine.UI;

public class WaveController : MonoBehaviour
{
    #region Variable Initilization

    private bool _isWaveActive;
    private DestinationColliderController _destinationColliderController;
    private float _playerMaxHealth;
    private float _waveStartTimer;
    private GameObject _destinationCollider;
    private int _companionsAlive = 0;
    private int _currentWave;
    private int _enemiesAlive = 0;
    private int _spawnCheckerCloneCount = 0;
    private Text _uiCompanionTracker;
    private Text _uiNextWaveTimer;
    private Text _uiWaveCounter;
    public float initialWaveDelay = 6f;
    public float waveDelay = 60f;
    public GameObject core1;
    public GameObject core2;
    public GameObject core3;
    public GameObject core4;
    public GameObject[] transportPrefabList;
    public int baseTransportCount = 2;
    public int enemyEndWaveThreshold = 2;
    public int enemyMultiplierPerWave = 2;
    public int numberOfWaves = 10;
    public int startingWave = 1;
    public ReactorCoreController coreController1;
    public ReactorCoreController coreController2;
    public ReactorCoreController coreController3;
    public ReactorCoreController coreController4;

    #endregion Variable Initilization

    /// <summary>
    /// called on init, starts all needed Event Managers.
    /// </summary>
    private void Awake()
    {
        #region consts for readability

        const string _registerEnemy = "registerEnemy";
        const string _registerCompanion = "registerCompanion";
        const string _enemyDeath = "enemyDeath";
        const string _companionDeath = "companionDeath";
        const string _spawnCloneRemoved = "spawnCheckerCloneRemoved";

        #endregion consts for readability

        EventManager.StartListening(_registerEnemy, RegisterEnemy);                     ///<see cref="RegisterEnemy"/>
        EventManager.StartListening(_registerCompanion, RegisterCompanion);             ///<see cref="RegisterCompanion"/>
        EventManager.StartListening(_enemyDeath, EnemyDeath);                           ///<see cref="EnemyDeath"/>
        EventManager.StartListening(_companionDeath, CompanionDeath);                   ///<see cref="CompanionDeath"/>
        EventManager.StartListening(_spawnCloneRemoved, SpawnCheckerCloneRemoved);      ///<see cref="SpawnCheckerCloneRemoved"/>
    }

    /// <summary>
    /// once event is raise stops listening for the events.
    /// </summary>
    private void OnDestroy()
    {
        #region consts for readability

        const string _registerEnemy = "registerEnemy";
        const string _registerCompanion = "registerCompanion";
        const string _enemyDeath = "enemyDeath";
        const string _companionDeath = "companionDeath";
        const string _spawnCloneRemoved = "spawnCheckerCloneRemoved";

        #endregion consts for readability

        EventManager.StopListening(_registerEnemy, RegisterEnemy);                                      ///<see cref="RegisterEnemy"/>
        EventManager.StopListening(_registerCompanion, RegisterCompanion);                              ///<see cref="RegisterCompanion"/>
        EventManager.StopListening(_enemyDeath, EnemyDeath);                                            ///<see cref="EnemyDeath"/>
        EventManager.StopListening(_companionDeath, CompanionDeath);                                    ///<see cref="CompanionDeath"/>
        EventManager.StopListening(_spawnCloneRemoved, SpawnCheckerCloneRemoved);                       ///<see cref="SpawnCheckerCloneRemoved"/>
    }

    /// <summary>
    /// init method for wave.
    /// </summary>
    private void Start()
    {
        #region consts for readability

        const string _companionTracker = "CompanionTracker";
        const string _destination = "DestinationArea";
        const string _nextWaveTimer = "NextWaveTimer";
        const string _waveCounter = "WaveCounter";

        #endregion consts for readability

        #region Variable Initialization

        _currentWave = startingWave - 1;
        _destinationCollider = GameObject.FindGameObjectWithTag(_destination);
        _isWaveActive = false;
        _waveStartTimer = initialWaveDelay;
        GameObject companionTracker = GameObject.Find(_companionTracker);
        GameObject nextWaveTimer = GameObject.Find(_nextWaveTimer);
        GameObject waveCounter = GameObject.Find(_waveCounter);

        #endregion Variable Initialization

        // Get destination collider assets.
        _destinationColliderController = _destinationCollider.GetComponent<DestinationColliderController>();

        // Get UI Elements.
        _uiCompanionTracker = companionTracker.GetComponent<Text>();
        _uiNextWaveTimer = nextWaveTimer.GetComponent<Text>();
        _uiWaveCounter = waveCounter.GetComponent<Text>();
    }

    /// <summary>
    /// update method for the wave, and needed checks.
    /// </summary>
    private void Update()
    {
        // If the wave ended, start next.
        if(_isWaveActive == false && _uiNextWaveTimer != null)
        {
            // Get the remaining time.
            float timeRemaining = _waveStartTimer - Time.time;

            // Convert it to MM:SS.
            float minutes = Mathf.FloorToInt(timeRemaining / 60f);
            float seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);

            // If time's up, reset, start new wave.
            if(timeRemaining <= 0)
            {
                minutes = 0;
                seconds = 0;
                StartWave();
            }

            _uiNextWaveTimer.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Begin a new wave.
    /// </summary>
    private void StartWave()
    {
        #region variable initiation

        _waveStartTimer = waveDelay;
        _isWaveActive = true;
        int transportCount = Random.Range(baseTransportCount, baseTransportCount * _currentWave);

        #endregion variable initiation

        _currentWave++;

        if(_currentWave > numberOfWaves)
        {
            // JK, leave this be.
            // TODO: Do some end-game shit here.
        }

        _uiWaveCounter.text = _currentWave.ToString();

        // Re-activate the destination collider so waypoints can be generated.
        _destinationColliderController.Activate();

        // Determine wave size.
        _spawnCheckerCloneCount = transportCount;

        for(int i = 0; i < transportCount; i++)
        {
            GameObject newTransport = Instantiate(transportPrefabList[Random.Range(0, transportPrefabList.Length)]);
            TransportSpawnController spawnController = newTransport.GetComponent<TransportSpawnController>();
            // TODO: Make the multiplier be friendly to floats.
            spawnController.PayloadMultiplier = _currentWave * enemyMultiplierPerWave;
        }

        // TODO: WORKAROUND CODE BELOW
        _isWaveActive = true;
        waveDelay += 10f;
    }

    /// <summary>
    /// on spawns add 1 to enemy count.
    /// </summary>
    private void RegisterEnemy()
    {
        _enemiesAlive++;
    }

    /// <summary>
    /// if spawn is a companion register it as such.
    /// </summary>
    private void RegisterCompanion()
    {
        _companionsAlive++;
        UpdateCompanionTracker();       ///<see cref="UpdateCompanionTracker"/>
    }

    /// <summary>
    /// called on enemy death(s).
    /// </summary>
    private void EnemyDeath()
    {
        _enemiesAlive--;
        CheckDeathThreshold();          ///<see cref="CheckDeathThreshold"/>
    }

    /// <summary>
    /// called on companion death.
    /// </summary>
    private void CompanionDeath()
    {
        _companionsAlive--;
        UpdateCompanionTracker();       ///<see cref="UpdateCompanionTracker"/>
    }

    /// <summary>
    /// when spawn que is empty stop spawn logic.
    /// </summary>
    private void SpawnCheckerCloneRemoved()
    {
        _spawnCheckerCloneCount--;

        if(_spawnCheckerCloneCount <= 0)
        {
            _destinationColliderController.Deactivate();
        }
    }

    /// <summary>
    /// if enemies are dead start new wave.
    /// </summary>
    private void CheckDeathThreshold()
    {
        if(_enemiesAlive == 0 && _isWaveActive == true)
        {
            _isWaveActive = false;

            // If we start the countdown now, we need to use "now" as a point of reference.
            // Time is all made up. It's all relative. Whatever.
            _waveStartTimer = Time.time + _waveStartTimer;
        }
    }

    /// <summary>
    /// updates companion tracker on ally state changes.
    /// </summary>
    private void UpdateCompanionTracker()
    {
        if(_uiCompanionTracker != null)
        {
            _uiCompanionTracker.text = _companionsAlive.ToString();
        }
    }

    /// <summary>
    /// called on game end.
    /// </summary>
    public void GameOver()
    {
        #region consts for readability

        const string _gameOver = "GameEnded";

        #endregion consts for readability

        EventManager.TriggerEvent(_gameOver);       ///<see cref="GameOver"/>

        // Enter loss condition stuff here.
    }

    /// <summary>
    /// sets players max health.
    /// </summary>
    /// <param name="maxHealth"></param>
    public void SetHealthbar(int maxHealth)
    {
        _playerMaxHealth = (float)maxHealth;
    }

    /// <summary>
    /// update health.
    /// </summary>
    /// <param name="currentHealth"></param>
    public void UpdateHealthbar(int currentHealth)
    {
        #region Initiate Variables

        float core1Health = 100f;
        float core2Health = 100f;
        float core3Health = 100f;
        float core4Health = 100f;

        #endregion Initiate Variables

        core1Health = (currentHealth / _playerMaxHealth * 100f / 25f) * 100f;
        core2Health = ((currentHealth / _playerMaxHealth * 100f - 25f) / 25f) * 100f;
        core3Health = ((currentHealth / _playerMaxHealth * 100f - 50f) / 25f) * 100f;
        core4Health = ((currentHealth / _playerMaxHealth * 100f - 75f) / 25f) * 100f;

        coreController1.UpdateCoreHealth(core1Health);
        coreController2.UpdateCoreHealth(core2Health);
        coreController3.UpdateCoreHealth(core3Health);
        coreController4.UpdateCoreHealth(core4Health);
    }
}