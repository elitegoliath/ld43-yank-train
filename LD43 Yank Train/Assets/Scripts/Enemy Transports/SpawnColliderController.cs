using UnityEngine;

public class SpawnColliderController : MonoBehaviour
{
    private bool _isInFreeSpace = true;

    /// <summary>
    /// Detects existing transports in the area.
    /// </summary>
    /// <param name="collision"> The current Collision event being checked against valid tag(s). </param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        #region Consts for Readability

        const string _transport = "Transport";

        #endregion

        // If another transport is in the immediate area, space is not free.
        if (collision.tag == _transport)
        {
            _isInFreeSpace = false;
        }
    }

    /// <summary>
    /// Public function to retrieve whether the area is free.
    /// </summary>
    public bool CheckIfFree()
    {
        // Reset the value after each check in case of recursion.
        bool retVal = _isInFreeSpace;
        _isInFreeSpace = true;

        return retVal;
    }
}