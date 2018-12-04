using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisController : MonoBehaviour {
    public GameObject particles;

	// Use this for initialization
	void Start () {
        if (particles != null) {
            GameObject parts = Instantiate(particles, transform);
            parts.transform.localPosition = Vector3.zero;
            parts.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        } else {
            Debug.Log("No particles associated with this debris controller.");
        }
	}
}
