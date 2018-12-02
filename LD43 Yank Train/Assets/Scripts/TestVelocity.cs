using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVelocity : MonoBehaviour {
    public float velocity = 1f;
    public float torque = 1f;
    public float drag = 1f;
    private Rigidbody2D _myRigidBody;

    // Use this for initialization
    void Start () {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        Vector2 direction = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.y - transform.position.y);
        transform.up = direction;

        _myRigidBody.AddForce(transform.up * velocity);
        Delay(2f);
        _myRigidBody.AddTorque(torque);
        _myRigidBody.drag = drag;
        _myRigidBody.angularDrag = (drag * 2);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        //_myRigidBody.MoveRotation(_myRigidBody.rotation + stopRotationSpeed * Time.fixedDeltaTime);
        
    }

    private IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
