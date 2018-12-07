﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class DebrisController : MonoBehaviour {
    public GameObject particles;
    public ParticleSystem explosionParts;
    public AudioClip explosionSound;
    public bool isPolyNav = false;
    public bool hasLargeFire = false;
    
    private PolyNav2D _navMapRef;
    private PolyNavObstacle _myObstacle;
    private bool _canBeDestroyed = false;
    
	private void Start () {
        float fireSize = hasLargeFire ? 0.3f : 0.1f;
        float explosionVolume = hasLargeFire ? 0.8f : 0.5f;
        float explosionSize = hasLargeFire ? 1.2f : 0.5f;

        if (particles != null) {
            GameObject parts = Instantiate(particles, transform);
            parts.transform.localPosition = Vector3.zero;
            parts.transform.localScale = new Vector3(fireSize, fireSize, fireSize);

            ParticleSystem explosion = Instantiate(explosionParts, transform.position, transform.rotation);
            explosion.transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize);
            
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        } else {
            Debug.Log("No particles associated with this debris controller.");
        }

        if (isPolyNav == true) {
            _myObstacle = gameObject.GetComponent<PolyNavObstacle>();
            _navMapRef = FindObjectOfType<PolyNav2D>();
            _navMapRef.AddObstacle(_myObstacle);
        }

        if (hasLargeFire == true)
        {
            StartCoroutine(ActivateVulnerability());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "LargeExplosion" && _canBeDestroyed == true) {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (isPolyNav == true) {
            _navMapRef.RemoveObstacle(_myObstacle);
        }
    }

    private IEnumerator ActivateVulnerability()
    {
        yield return 0;
        _canBeDestroyed = true;
    }
}
