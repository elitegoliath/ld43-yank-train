using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float moveSpeed = 100f;

    private Rigidbody2D _rb;
    private Camera _cam;
    private Transform _my;

    /// <summary>
    /// Runs once when the script has been initialized.
    /// </summary>
    private void Start()
    {
        _my = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
    }

    /// <summary>
    /// It's the update function. Called once per frame.
    /// </summary>
    private void Update()
    {
        FaceAvatar();
    }

    /// <summary>
    /// Fixed update is better used to calculate physics based events.
    /// </summary>
    private void FixedUpdate()
    {
        MoveAvatar();
    }

    /// <summary>
    /// Listens for user movement inputs and applies the proper physics.
    /// </summary>
    private void MoveAvatar()
    {
        // Get the combination of movement inputs from the player.
        float moveX = (Input.GetAxis("Horizontal") * Time.deltaTime) * moveSpeed;
        float moveY = (Input.GetAxis("Vertical") * Time.deltaTime) * moveSpeed;

        _rb.velocity = new Vector2(moveX, moveY);
    }

    /// <summary>
    /// Ensures the player avatar is always facing the mouse direction.
    /// </summary>
    private void FaceAvatar()
    {
        // Distance between player avatar and the camera so the angle can be properly derived.
        float camDis = _cam.transform.position.y - _my.position.y;

        // Get the mouse position in the world space, while using the camera distance for Z, for some reason.
        Vector3 mousePos = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDis));

        // Fuckin fancy ass math... I really don't get it, but it works, so yolo.
        float AngleRad = Mathf.Atan2(mousePos.y - _my.position.y, mousePos.x - _my.position.x);
        float angle = (180 / Mathf.PI) * AngleRad;

        _rb.rotation = angle;
    }
}