using UnityAtoms.BaseAtoms;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [SerializeField] private new Rigidbody2D rigidbody2D;
    [SerializeField] private Vector2Variable playerPosition;

    private void FixedUpdate()
    {
        rigidbody2D.velocity = (playerPosition.Value - rigidbody2D.position).normalized * followSpeed;
    }
}