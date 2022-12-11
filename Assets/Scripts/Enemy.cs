using System.Collections;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityEvent onDeath;

    [SerializeField] private float followSpeed;

    [Header("References")] [SerializeField]
    private new Rigidbody2D rigidbody2D;

    [SerializeField] private new Collider2D collider;

    [SerializeField] private Reflection reflection;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Animator animator;
    [SerializeField] private string deathAnimationStateName;
    [SerializeField] private Material deathMaterial;

    [SerializeField] private Vector2Variable playerPosition;
    [SerializeField] private IntVariable killCount;

    private bool _dying;

    public float ReflectionRadius => reflection.Radius;

    private void FixedUpdate()
    {
        if (_dying) return;

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
        IEnumerator DeathRoutine()
        {
            rigidbody2D.simulated = false;
            collider.enabled = false;

            animator.Play(deathAnimationStateName);
            spriteRenderer.material = deathMaterial;

            // TODO: this is super gross, there has to be a better way
            yield return null;
            yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationStateName));

            Destroy(gameObject);

            killCount.Value++;
            onDeath.Invoke();
        }

        StartCoroutine(DeathRoutine());
    }
}