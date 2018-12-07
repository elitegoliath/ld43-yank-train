using UnityEngine;

public class MunitionController : MonoBehaviour
{
    #region Instantiate Variables

    private float _lifeSpan = 1f;
    private int _damage = 1;
    public float speed = 1f;

    #endregion Instantiate Variables

    /// <summary>
    /// init method for amunition.
    /// </summary>
    private void Start()
    {
        Rigidbody2D _myRigidBody = gameObject.GetComponent<Rigidbody2D>();
        _myRigidBody.AddForce(transform.up * speed);

        Destroy(gameObject, _lifeSpan);
    }

    /// <summary>
    /// On impact with game object if object is damagable apply damage.
    /// </summary>
    /// <param name="collider"></param>
    private void OnTriggerEnter2D(Collider2D collider)
    {
        CombatController entity = collider.GetComponent<CombatController>();

        if(entity != null)
        {
            entity.TakeDamage(_damage);
            Die();
        }
    }

    /// <summary>
    /// sets the damage of a munition type.
    /// </summary>
    /// <param name="damage"></param>
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    /// <summary>
    /// set ammo's flight/life span.
    /// </summary>
    /// <param name="lifeSpan"></param>
    public void SetLifeSpan(float lifeSpan)
    {
        _lifeSpan = lifeSpan;
    }

    /// <summary>
    /// Event called when a munitions life span is over to destroy itself.
    /// </summary>
    public void Die()
    {
        Destroy(gameObject);
    }
}