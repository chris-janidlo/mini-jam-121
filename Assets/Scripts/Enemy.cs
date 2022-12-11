using UnityAtoms.BaseAtoms;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [SerializeField] private new Rigidbody2D rigidbody2D;
    [SerializeField] private Vector2Variable playerPosition;
    [SerializeField] private SceneLoader gameOver;

    private void FixedUpdate()
    {
        rigidbody2D.velocity = (playerPosition.Value - rigidbody2D.position).normalized * followSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.PLAYER_TAG)) gameOver.Load();
    }
}