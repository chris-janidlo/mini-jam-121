using UnityEngine;
using UnityEngine.Assertions;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private new Rigidbody2D rigidbody2D;

    private Vector2 _input;

    private void Start()
    {
        Assert.IsTrue(rigidbody2D.isKinematic);
    }

    private void Update()
    {
        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        rigidbody2D.velocity = _input * speed;
    }
}