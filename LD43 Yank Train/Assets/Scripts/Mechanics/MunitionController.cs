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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        CombatController entity = collider.GetComponent<CombatController>();

        if (entity == null) {
            // This was a mistake. How did we get here? What is life, even?
        } else {
            entity.TakeDamage(_damage);
            Die();
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
