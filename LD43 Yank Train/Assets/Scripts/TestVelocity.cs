using System.Collections;
using UnityEngine;

public class TestVelocity : MonoBehaviour
{
    #region Instantiate Variables Used In Class('s)

    public float velocity = 1f;
    public float torque = 1f;
    public float drag = 1f;
    private Rigidbody2D _myRigidBody;

    #endregion Instantiate Variables Used In Class('s)

    // Use this for initialization
    private void Start()
    {
        #region consts for readability

        const string _player = "Player";

        #endregion consts for readability

        GameObject player = GameObject.FindGameObjectWithTag(_player);
        _myRigidBody = gameObject.GetComponent<Rigidbody2D>();

        Vector2 direction = new Vector2(
                                        player.transform.position.x - transform.position.x,
                                        player.transform.position.y - transform.position.y
                                            );

        transform.up = direction;

        _myRigidBody.AddForce(transform.up * velocity);

        Delay(2f);
        _myRigidBody.AddTorque(torque);

        _myRigidBody.drag = drag;
        _myRigidBody.angularDrag = (drag * 2);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
    }

    /// <summary>
    /// Causes a variable amount of "wait"/"pause" time based on variable fed in
    /// </summary>
    /// <param name="seconds"></param>
    /// <see cref="Start"/>
    private IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}