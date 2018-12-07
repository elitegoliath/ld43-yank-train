using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MunitionController : MonoBehaviour {
    public float speed = 1f;
    public ParticleSystem damageSparks;
    public AudioClip sfx;
    public float volume = 1f;

    private float _lifeSpan = 1f;
    private int _damage = 1;

    private void Start()
    {
        Rigidbody2D _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myRigidBody.AddForce(transform.up * speed);
        AudioSource.PlayClipAtPoint(sfx, transform.position, volume);

        Destroy(gameObject, _lifeSpan);
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Collider2D _collider = collision.collider;

    //    CombatController entity = _collider.GetComponent<CombatController>();
    //    string _tag = _collider.tag;

    //    if (_tag == "Wall" || _tag == "LargeDebris") {
    //        Die();
    //    } else {
    //        if (entity != null) {
    //            entity.TakeDamage(_damage);
    //            Die();
    //        }
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collider)
    {
        CombatController entity = collider.GetComponent<CombatController>();
        string _tag = collider.tag;

        if (_tag == "Wall" || _tag == "TransportDebris") {
            Die();
        } else {
            if (entity != null) {
                entity.TakeDamage(_damage);
                Die();
            }
        }
    }

        public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void SetLifeSpan(float lifeSpan)
    {
        _lifeSpan = lifeSpan;
    }

    public void Die()
    {
        Instantiate(damageSparks, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
