using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVelocity : MonoBehaviour {
    public float velocity = 1f;

	// Use this for initialization
	void Start () {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Rigidbody2D _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        Vector2 direction = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.y - transform.position.y);
        transform.up = direction;

        //_myRigidBody.velocity = Vector3.zero;

        _myRigidBody.AddForce(transform.up * velocity);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
