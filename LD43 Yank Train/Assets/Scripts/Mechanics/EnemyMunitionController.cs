using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMunitionController : MonoBehaviour {
    public float speed = 1f;
    public int damage = 1;

	// Use this for initialization
	void Start () {
        Rigidbody2D _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myRigidBody.AddForce(transform.up * speed);
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: Apply damage to whatever was hit.

        Destroy(gameObject);
    }
}
