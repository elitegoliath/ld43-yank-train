using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructExplosion : MonoBehaviour {
    public GameObject explosionParticles;
    public AudioClip explosionSound;

	// Use this for initialization
	void Start () {
        GameObject explosion = Instantiate(explosionParticles);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = new Vector3(2, 2, 2);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        Destroy(gameObject, 0.1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        if (tag == "Enemy" || tag == "Transport") {
            CombatController combatController = collision.GetComponent<CombatController>();
            combatController.Die(true);
        } else if (tag == "TransportDebris") {
            Destroy(collision.gameObject);
        }
    }
}
