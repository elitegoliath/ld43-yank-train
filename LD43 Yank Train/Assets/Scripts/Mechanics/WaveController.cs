using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaveController : MonoBehaviour {
    public float waveDelay = 60f;
    public float initialWaveDelay = 6f;
    public int enemyMultiplierPerWave = 2;
    public int startingWave = 1;
    public int numberOfWaves = 10;
    public int baseTransportCount = 2;
    public int enemyEndWaveThreshold = 2;
    public GameObject[] transportPrefabList;
    
    private float _waveStartTimer;
    private int _currentWave;
    private int _enemiesAlive = 0;
    private int _companionsAlive = 0;
    private int _spawnCheckerCloneCount = 0;
    private bool _isWaveActive;
    private GameObject _destinationCollider;
    private DestinationColliderController _destinationColliderController;
    private Text _uiWaveCounter;
    private Text _uiNextWaveTimer;
    private Text _uiCompanionTracker;
    private int _playerMaxHealth;
    private int _playerCurrenthealth;
    private int _coreHealth;

    private void Awake()
    {
        EventManager.StartListening("registerEnemy", RegisterEnemy);
        EventManager.StartListening("registerCompanion", RegisterCompanion);
        EventManager.StartListening("enemyDeath", EnemyDeath);
        EventManager.StartListening("companionDeath", CompanionDeath);
        EventManager.StartListening("spawnCheckerCloneRemoved", SpawnCheckerCloneRemoved);
    }

    private void OnDestroy()
    {
        EventManager.StopListening("registerEnemy", RegisterEnemy);
        EventManager.StopListening("registerCompanion", RegisterCompanion);
        EventManager.StopListening("enemyDeath", EnemyDeath);
        EventManager.StopListening("companionDeath", CompanionDeath);
        EventManager.StopListening("spawnCheckerCloneRemoved", SpawnCheckerCloneRemoved);
    }

    private void Start () {
        // Set defaults.
        _waveStartTimer = initialWaveDelay;
        _isWaveActive = false;
        _currentWave = startingWave - 1;

        // Get destination collider assets.
        _destinationCollider = GameObject.FindGameObjectWithTag("DestinationArea");
        _destinationColliderController = _destinationCollider.GetComponent<DestinationColliderController>();

        // Get UI Elements.
        GameObject waveCounter = GameObject.Find("WaveCounter");
        _uiWaveCounter = waveCounter.GetComponent<Text>();

        GameObject nextWaveTimer = GameObject.Find("NextWaveTimer");
        _uiNextWaveTimer = nextWaveTimer.GetComponent<Text>();

        GameObject companionTracker = GameObject.Find("CompanionTracker");
        _uiCompanionTracker = companionTracker.GetComponent<Text>();
    }

    private void Update()
    {
        // If the wave ain't goin, let the counter start blowin.
        if (_isWaveActive == false && _uiNextWaveTimer != null) {
            // Get the remaining time.
            float timeRemaining = _waveStartTimer - Time.time;

            // Convert it to M:SS.
            float minutes = Mathf.FloorToInt(timeRemaining / 60f);
            float seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);

            // If time's up, make sure the counter didn't go negative, then begin next wave.
            if (timeRemaining <= 0) {
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

        // TODO: Make minimum wave time.

        _currentWave++;

        if (_currentWave > numberOfWaves) {
            // JK, leave this be.
            // TODO: Do some end-game shit here. Also, early return because fuck it.
        }

        _waveStartTimer = waveDelay;
        _isWaveActive = true;
        _uiWaveCounter.text = _currentWave.ToString();

        // Re-activate the destination collider so waypoints can be generated.
        _destinationColliderController.Activate();

        // Determine wave size.
        int transportCount = Random.Range(baseTransportCount, (baseTransportCount * _currentWave));
        _spawnCheckerCloneCount = transportCount;

        for (int i = 0; i < transportCount; i++) {
            GameObject newTransport = Instantiate(transportPrefabList[Random.Range(0, transportPrefabList.Length)]);
            TransportSpawnController spawnController = newTransport.GetComponent<TransportSpawnController>();
            // TODO: Make the multiplier be friendly to floats.
            spawnController.payloadMultiplier = _currentWave * enemyMultiplierPerWave;
        }
    }

    private void RegisterEnemy()
    {
        _enemiesAlive += 1;
    }

    private void RegisterCompanion()
    {
        _companionsAlive += 1;
        UpdateCompanionTracker();
    }

    private void EnemyDeath()
    {
        _enemiesAlive -= 1;
        CheckDeathThreshold();
    }

    private void CompanionDeath()
    {
        _companionsAlive -= 1;
        UpdateCompanionTracker();
    }

    private void SpawnCheckerCloneRemoved()
    {
        _spawnCheckerCloneCount -= 1;

        if (_spawnCheckerCloneCount <= 0) {
            _destinationColliderController.Deactivate();
        }
    }

    private void CheckDeathThreshold()
    {
        // If enough enemies are dead, set timer for the next wave.
        // Threshold not important.
        //if (_enemiesAlive <= enemyEndWaveThreshold && _isWaveActive == true) {
        if (_enemiesAlive == 0 && _isWaveActive == true) {
                _isWaveActive = false;

            // If we start the countdown now, we need to use "now" as a point of reference.
            // Time is all made up. It's all relative. Whatever.
            _waveStartTimer = Time.time + _waveStartTimer;
        }
    }

    private void UpdateCompanionTracker()
    {
        if (_uiCompanionTracker != null) {
            _uiCompanionTracker.text = _companionsAlive.ToString();
        }
    }

    public void GameOver()
    {
        EventManager.TriggerEvent("GameEnded");

        // Enter loss condition stuff here.
    }

    public void SetHealthbar(int maxHealth)
    {
        _playerMaxHealth = maxHealth;
        _playerCurrenthealth = maxHealth;

        _coreHealth = maxHealth / 4;
    }

    public void UpdateHealthbar(int currentHealth)
    {

    }
}
