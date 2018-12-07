using UnityEngine;

public class SelfDestructExplosion : MonoBehaviour
{
    #region Variable Initiation

    public AudioClip explosionSound;
    public GameObject explosionParticles;

    #endregion Variable Initiation

    /// <summary>
    /// Init method for self destruct behavior.
    /// </summary>
    private void Start()
    {
        GameObject explosion = Instantiate(explosionParticles);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = new Vector3(2, 2, 2);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
    }

    /// <summary>
    /// an update event that destroys calling object.
    /// </summary>
    private void LateUpdate()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// checks if collision tag matches then calls its die event.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        #region consts for readabilty

        const string _enemy = "Enemy";

        #endregion consts for readabilty

        if(collision.tag == _enemy)
        {
            CombatController combatController = collision.GetComponent<CombatController>();
            combatController.Die(true);
        }
    }
}