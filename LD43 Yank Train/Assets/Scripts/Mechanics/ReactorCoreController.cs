using UnityEngine;
using UnityEngine.UI;

public class ReactorCoreController : MonoBehaviour
{
    #region Instantiate variables

    private bool _isCoreDead = false;
    private float _currentCoreHealth = 100f;
    private Image _myImage;
    private ParticleSystem smoke;
    private ParticleSystem sparks;
    public ParticleSystem assimilator;
    public ParticleSystem assimilatorMaybe;
    public ParticleSystem smokeMaybe;
    public ParticleSystem sparksMaybe;
    public Sprite aliveSprite;
    public Sprite deadSprite;

    #endregion Instantiate variables

    /// <summary>
    /// init method of reactor core.
    /// </summary>
    private void Start()
    {
        _myImage = gameObject.GetComponent<Image>();
        ParticleSystem[] ps = gameObject.GetComponentsInChildren<ParticleSystem>();
        sparks = ps[0];
        smoke = ps[1];
        assimilator = ps[2];
        _currentCoreHealth = 100f;
    }

    /// <summary>
    /// when core takes damage does calculations and set new health.
    /// </summary>
    /// <param name="percent"></param>
    public void UpdateCoreHealth(float percent)
    {
        #region init variables

        bool checkCoreDead = _isCoreDead;

        #endregion init variables

        // Heal effects.
        if(_currentCoreHealth < percent)
        {
            PlayAssimilator(); ///<see cref="PlayAssimilator"/>
            StopSmoke();       ///<see cref="StopSmoke"/>
            StopSparks();      ///<see cref="StopSparks"/>
            checkCoreDead = false;
        }

        _currentCoreHealth = percent;

        // Check damage levels.
        if(percent >= 90)
        {
            StopSparks();
            StopSmoke();
        }
        else if(percent >= 65)
        {
            PlaySmoke();
        }
        else if(percent >= 30)
        {
            PlaySparks();
        }
        else if(percent <= 5)
        {
            checkCoreDead = true;
            StopSparks();
            StopSmoke();
        }

        if(checkCoreDead != _isCoreDead)
        {
            if(_isCoreDead == true)
            {
                _myImage.sprite = aliveSprite;
            }
            else
            {
                _myImage.sprite = deadSprite;
            }
        }

        _isCoreDead = checkCoreDead;
    }

    /// <summary>
    /// Checks if smoke is already going if not starts it.
    /// </summary>
    private void PlaySmoke()
    {
        if(smoke.isPlaying == false)
        {
            smoke.Play();
        }
    }

    /// <summary>
    /// Checks if sparks are already going if not starts them.
    /// </summary>
    private void PlaySparks()
    {
        if(sparks.isPlaying == false)
        {
            sparks.Play();
        }
    }

    /// <summary>
    /// starts assimilator particles.
    /// </summary>
    private void PlayAssimilator()
    {
        if(assimilator.isPlaying == true)
        {
            assimilator.Stop();
        }

        assimilator.Play();
    }

    /// <summary>
    /// Stops Smoke Effect.
    /// </summary>
    private void StopSmoke()
    {
        if(smoke.isPlaying == true)
        {
            smoke.Stop();
        }
    }

    /// <summary>
    /// Stops Spark Effects.
    /// </summary>
    private void StopSparks()
    {
        if(sparks.isPlaying == true)
        {
            sparks.Stop();
        }
    }

    /// <summary>
    /// Stops Assimilator Effects.
    /// </summary>
    private void StopAssimilator()
    {
        if(assimilator.isPlaying == true)
        {
            assimilator.Stop();
        }
    }
}