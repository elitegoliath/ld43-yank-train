using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool _isWaveActive;

    private GameObject _destinationCollider;
    private DestinationColliderController _destinationColliderController;

    private void Start () {
        _waveStartTimer = waveDelay;
        _isWaveActive = false;
        _currentWave = startingWave - 1;

        _destinationCollider = GameObject.FindGameObjectWithTag("DestinationArea");
        _destinationColliderController = _destinationCollider.GetComponent<DestinationColliderController>();
    }

    private void Update() {
        if (_isWaveActive == false) {
            _waveStartTimer -= Time.deltaTime;

            if (_waveStartTimer <= 0f) {
                StartWave();
            }
        } else {
            MonitorWave();
        }
	}

    /// <summary>
    /// Begin a new wave.
    /// </summary>
    private void StartWave()
    {
        _currentWave++;
        _waveStartTimer = waveDelay;
        _isWaveActive = true;

        _destinationColliderController.Activate();

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

        // TODO: Offer up perks

        // TODO: Reset wave timer

        // TODO: Reset active wave flag
    }
}
