using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorCoreController : MonoBehaviour {
    public ParticleSystem sparksMaybe;
    public ParticleSystem smokeMaybe;
    public ParticleSystem assimilatorMaybe;
    public Sprite aliveSprite;
    public Sprite deadSprite;

    private float _currentCoreHealth = 100f;
    private bool _isCoreDead = false;
    private Image _myImage;
    private ParticleSystem sparks;
    private ParticleSystem smoke;
    public ParticleSystem assimilator;

    private void Start()
    {
        _myImage = gameObject.GetComponent<Image>();
        ParticleSystem[] ps = gameObject.GetComponentsInChildren<ParticleSystem>();
        sparks = ps[0];
        smoke = ps[1];
        assimilator = ps[2];
        _currentCoreHealth = 100f;
    }

    public void UpdateCoreHealth(float percent)
    {
        bool checkCoreDead = _isCoreDead;

        // Heal effects.
        if (_currentCoreHealth < percent) {
            PlayAssimilator();
            StopSmoke();
            StopSparks();
            checkCoreDead = false;
        }

        _currentCoreHealth = percent;

        // Check damage levels.
        if (percent >= 90) {
            StopSparks();
            StopSmoke();
        } else if (percent >= 65) {
            PlaySmoke();
        } else if (percent >= 30) {
            PlaySparks();
        } else if (percent <= 5) {
            checkCoreDead = true;
            StopSparks();
            StopSmoke();
        }

        if (checkCoreDead != _isCoreDead) {
            if (_isCoreDead == true) {
                _myImage.sprite = aliveSprite;
            } else {
                _myImage.sprite = deadSprite;
            }
        }

        _isCoreDead = checkCoreDead;
    }

    private void PlaySmoke()
    {
        if(smoke.isPlaying == false) {
            smoke.Play();
        }
    }

    private void PlaySparks()
    {
        if(sparks.isPlaying == false) {
            sparks.Play();
        }
    }

    private void PlayAssimilator()
    {
        if (assimilator.isPlaying == true) {
            assimilator.Stop();
        }

        assimilator.Play();
    }

    private void StopSmoke()
    {
        if(smoke.isPlaying == true) {
            smoke.Stop();
        }
    }

    private void StopSparks()
    {
        if(sparks.isPlaying == true) {
            sparks.Stop();
        }
    }

    private void StopAssimilator()
    {
        if(assimilator.isPlaying == true) {
            assimilator.Stop();
        }
    }
}
