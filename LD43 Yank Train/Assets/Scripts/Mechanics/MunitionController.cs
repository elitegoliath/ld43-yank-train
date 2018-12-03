using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MunitionController : MonoBehaviour {
    public float speed = 1f;
    public string[] collidableTags;

    private float _lifeSpan = 1f;
    private int _damage = 1;

    private void Start()
    {
        Rigidbody2D _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myRigidBody.AddForce(transform.up * speed);

        Destroy(gameObject, _lifeSpan);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: Apply damage to whatever was hit defined by the array of collidable tags.

        //Destroy(gameObject);
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void SetLifeSpan(float lifeSpan)
    {
        _lifeSpan = lifeSpan;
    }
}
