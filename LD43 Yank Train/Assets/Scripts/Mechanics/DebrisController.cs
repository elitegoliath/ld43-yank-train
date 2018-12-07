using System.Collections;
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
    
	private void Start () {
        float fireSize = hasLargeFire ? 0.3f : 0.1f;
        float explosionVolume = hasLargeFire ? 0.7f : 0.4f;

        if (particles != null) {
            GameObject parts = Instantiate(particles, transform);
            parts.transform.localPosition = Vector3.zero;
            parts.transform.localScale = new Vector3(fireSize, fireSize, fireSize);

            ParticleSystem explosion = Instantiate(explosionParts, transform.position, transform.rotation);
            explosion.transform.localScale = new Vector3(fireSize, fireSize, fireSize);
            
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        } else {
            Debug.Log("No particles associated with this debris controller.");
        }

        if (isPolyNav == true) {
            _myObstacle = gameObject.GetComponent<PolyNavObstacle>();
            _navMapRef = FindObjectOfType<PolyNav2D>();
            _navMapRef.AddObstacle(_myObstacle);
        }
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "LargeExplosion") {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (isPolyNav == true) {
            _navMapRef.RemoveObstacle(_myObstacle);
        }
    }
}
