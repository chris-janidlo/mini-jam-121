using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityEvent onDeath;

    [SerializeField] private float followSpeed;

    [Header("References")] [SerializeField]
    private new Rigidbody2D rigidbody2D;

    [SerializeField] private Reflection reflection;
    [SerializeField] private Vector2Variable playerPosition;
    [SerializeField] private IntVariable killCount;

    public float ReflectionRadius => reflection.Radius;

    private void FixedUpdate()
    {
        rigidbody2D.velocity = (playerPosition.Value - rigidbody2D.position).normalized * followSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(Constants.PLAYER_TAG)) return;

        other.GetComponent<Player>()!.Kill();
    }

    public void OnPlayerDyingChanged(bool value)
    {
        if (!value) return;

        rigidbody2D.isKinematic = false;
        rigidbody2D.gravityScale = 0;
        rigidbody2D.drag = 10;
        enabled = false;
    }

    public void Kill()
    {
        Debug.Log("ahh I'm dying");
        Destroy(gameObject);

        killCount.Value++;
        onDeath.Invoke();
    }
}