using UnityEngine;

public class Reflection : MonoBehaviour
{
    [SerializeField] private float constraintRadius;
    [SerializeField] private new Rigidbody2D rigidbody2D;

    public Vector2 Center => rigidbody2D.position;
    public float Radius => constraintRadius;

    private void FixedUpdate()
    {
        if (Mirror.AnyContain(this)) return;

        var mirror = Mirror.ClosestTo(Center);

        var max = mirror.Bounds.max;
        var min = mirror.Bounds.min;

        var constrainedPosition = new Vector2(
            Mathf.Clamp(rigidbody2D.position.x, min.x + constraintRadius, max.x - constraintRadius),
            Mathf.Clamp(rigidbody2D.position.y, min.y + constraintRadius, max.y - constraintRadius)
        );

        rigidbody2D.velocity += (constrainedPosition - rigidbody2D.position) / Time.deltaTime;
    }
}