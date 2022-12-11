using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityEvent onDeath;

    [SerializeField] private float followSpeed;
    [SerializeField] private new Rigidbody2D rigidbody2D;
    [SerializeField] private Vector2Variable playerPosition;
    [SerializeField] private SceneLoader gameOver;
    [SerializeField] private Reflection reflection;

    [SerializeField] private IntVariable killCount;

    public float ReflectionRadius => reflection.Radius;

    private void FixedUpdate()
    {
        rigidbody2D.velocity = (playerPosition.Value - rigidbody2D.position).normalized * followSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.PLAYER_TAG)) gameOver.Load();
    }

    public void Kill()
    {
        Debug.Log("ahh I'm dying");
        Destroy(gameObject);

        killCount.Value++;
        onDeath.Invoke();
    }
}