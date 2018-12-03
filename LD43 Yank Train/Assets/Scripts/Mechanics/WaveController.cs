using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveController : MonoBehaviour {
    public float enemyWaveMultiplier = 1.2f;
    public float waveDelay = 100f;
    public int startingWave = 1;
    public int numberOfWaves = 10;
    public int baseTransportCount = 2;
    public int enemyEndWaveThreshold = 10;
    public GameObject[] transportPrefabList;
    
    private float _waveStartTimer;
    private int _currentWave;
    private int _enemiesAlive = 0;
    private int _newWaveEnemyCount = 0;
    private bool _isWaveActive;
    private GameObject _destinationCollider;
    private DestinationColliderController _destinationColliderController;

    private void Awake()
    {
        EventManager.StartListening("registerEnemy", RegisterEnemy);
        EventManager.StartListening("enemyDeath", EnemyDeath);
    }

    private void Start () {
        _waveStartTimer = waveDelay;
        _isWaveActive = false;
        _currentWave = startingWave - 1;

        //_destinationCollider = GameObject.FindGameObjectWithTag("DestinationArea");
        //_destinationColliderController = _destinationCollider.GetComponent<DestinationColliderController>();
    }

    private void Update() {
        //if (_isWaveActive == false) {
        //    _waveStartTimer -= Time.deltaTime;

        //    if (_waveStartTimer <= 0f) {
        //        StartWave();
        //    }
        //} else {
        //    MonitorWave();
        //}
	}

    /// <summary>
    /// Begin a new wave.
    /// </summary>
    private void StartWave()
    {
        _currentWave++;
        _waveStartTimer = waveDelay;
        _newWaveEnemyCount = 0;
        _isWaveActive = true;

        //_destinationColliderController.Activate();

        // Determine wave size.
        // TODO: Currently linear, want to make more of a ramp.
        int transportCount = baseTransportCount * _currentWave;
        List<GameObject> transports = new List<GameObject>();

        for (int i = 0; i < transportCount; i++) {
            transports.Add(Instantiate(transportPrefabList[Random.Range(0, transportPrefabList.Length)]));
        }
    }

    /// <summary>
    /// Track wave progress and trigger events when appropriate.
    /// </summary>
    private void MonitorWave()
    {
        // TODO: Once there are no instances of SpawnCollider clones, deactivate the Destination Collider.

        // TODO: Watch for death toll to reach threshold. Once reached...

        // TODO: Reset wave timer.

        // TODO: Reset active wave flag.
    }

    private void RegisterEnemy()
    {
        _enemiesAlive += 1;
    }

    private void EnemyDeath()
    {
        _enemiesAlive -= 1;
    }
}
