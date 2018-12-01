using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnColliderController : MonoBehaviour {
    private bool _isInFreeSpace = true;

    /// <summary>
    /// Detects existing transports in the area.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If another transport is in the immediate area, space is not free.
        if (collision.tag == "Transport") {
            _isInFreeSpace = false;
        }
    }

    /// <summary>
    /// Public function to retrieve whether the area is free.
    /// </summary>
    /// <returns></returns>
    public bool CheckIfFree()
    {
        // Reset the value after each check in case of recursion.
        bool retVal = _isInFreeSpace;
        _isInFreeSpace = true;
        return retVal;
    }
}
