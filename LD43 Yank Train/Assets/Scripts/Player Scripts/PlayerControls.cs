using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float moveSpeed = 100f;

    private Rigidbody2D _rb;

    /// <summary>
    /// Runs once when the script has been initialized.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
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

    }
}