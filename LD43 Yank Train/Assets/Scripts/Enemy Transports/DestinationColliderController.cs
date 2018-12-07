using UnityEngine;

public class DestinationColliderController : MonoBehaviour
{
    #region Instantiate Variables Used In Class('s)

    private PolygonCollider2D _myCollider;
    private Bounds _myBounds;
    private Vector2 _myCenter;

    #endregion Instantiate Variables Used In Class('s)

    /// <summary>
    /// Instantiates local variables with current gameObjects, Colliders, and Bounds.
    /// </summary>
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
    ///     If the point is within the polygonal collider itself, return the coordinate.
    ///     Otherwise, recurse.
    /// </summary>
    public Vector2 GetRandomPoint()
    {
        #region Consts for Readability

        const string _destination = "DestinationArea";

        #endregion Consts for Readability

        #region Variable Instatiation

        bool isHitWithinBounds = false;
        Vector2 retVal = new Vector2();
        float x = Random.Range(_myCenter.x - _myBounds.extents.x, _myCenter.x + _myBounds.extents.x);
        float y = Random.Range(_myCenter.y - _myBounds.extents.y, _myCenter.y + _myBounds.extents.y);
        Vector2 foundCoordinate = new Vector2(x, y);

        #endregion Variable Instatiation

        RaycastHit2D[] foundHits = Physics2D.RaycastAll(foundCoordinate, Vector2.zero, Mathf.Infinity);

        foreach (RaycastHit2D hit in foundHits)
        {
            if (hit.collider.tag == _destination)
            {
                isHitWithinBounds = true;
                retVal = foundCoordinate;
            }
        }

        if (isHitWithinBounds == false)
        {
            retVal = GetRandomPoint();
        }

        return retVal;
    }
}