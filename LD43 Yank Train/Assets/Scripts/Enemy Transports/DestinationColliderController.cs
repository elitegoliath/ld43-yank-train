using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationColliderController : MonoBehaviour {

    private PolygonCollider2D _myCollider;
    private Bounds _myBounds;
    private Vector2 _myCenter;

    private void Start()
    {
        _myCollider = gameObject.GetComponent<PolygonCollider2D>();
        _myBounds = _myCollider.bounds;
        _myCenter = _myBounds.center;
    }

    /// <summary>
    /// Used to turn on the collider programmatically.
    /// </summary>
    public void Activate()
    {
        _myCollider.enabled = true;
    }

    /// <summary>
    /// Used to turn on the collider programmatically.
    /// </summary>
    public void Deactivate()
    {
        _myCollider.enabled = false;
    }

    /// <summary>
    /// Generates a random point within the bounds of the collider.
    /// If the point is within the polygonal collider itself, return the coordinate.
    /// Otherwise, recurse.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRandomPoint()
    {
        Vector2 retVal = new Vector2();

        float x = Random.Range(_myCenter.x - _myBounds.extents.x, _myCenter.x + _myBounds.extents.x);
        float y = Random.Range(_myCenter.y - _myBounds.extents.y, _myCenter.y + _myBounds.extents.y);

        Vector2 foundCoordinate = new Vector2(x, y);

        RaycastHit2D[] foundHits = Physics2D.RaycastAll(foundCoordinate, Vector2.zero, Mathf.Infinity);

        bool isHitWithinBounds = false;

        foreach (RaycastHit2D hit in foundHits) {
            if (hit.collider.tag == "DestinationArea") {
                isHitWithinBounds = true;
                retVal = foundCoordinate;
            }
        }

        if (isHitWithinBounds == false) {
            retVal = GetRandomPoint();
        }

        return retVal;
    }
}
