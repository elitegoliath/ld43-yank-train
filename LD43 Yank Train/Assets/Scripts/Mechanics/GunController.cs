using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {
    public GameObject munition;

    public void FireGun(float accuracy)
    {
        float bias = Random.Range(0f, 100f);
        float influence = Random.Range(0f, accuracy);
        float offset = influence;

        if (bias <= 50f) {
            offset = -influence;
        }

        Quaternion direction = transform.rotation;
        direction *= Quaternion.Euler(0, 0, offset);

        GameObject newPewPew = Instantiate(munition, transform.position, direction);
    }
}
