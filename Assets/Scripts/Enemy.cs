using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityEvent onDeath;

    [SerializeField] private float followSpeed;
    [SerializeField] private float avoidanceRadius, acceleration;
    [SerializeField] private LayerMask avoidanceMask;

    [SerializeField] private float spawnTime;
    [SerializeField] private Ease spawnEase;

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

    private readonly List<Collider2D> _avoidanceResults = new();
    private bool _dying, _spawning;

    public float ReflectionRadius => reflection.Radius;

    private IEnumerator Start()
    {
        _spawning = true;
        transform.localScale = Vector3.zero;
        yield return transform.DOScale(Vector3.one, spawnTime)
            .SetEase(spawnEase)
            .WaitForCompletion();
        _spawning = false;
    }

    private void FixedUpdate()
    {
        if (_dying || _spawning) return;

        var go = gameObject;
        var originalLayer = go.layer;
        go.layer = 0;

        _avoidanceResults.Clear();
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = avoidanceMask
        };
        Physics2D.OverlapCircle(rigidbody2D.position, avoidanceRadius, filter, _avoidanceResults);

        go.layer = originalLayer;

        Vector2 desiredVelocity;
        if (_avoidanceResults.Count == 0)
        {
            desiredVelocity = (playerPosition.Value - rigidbody2D.position).normalized * followSpeed;
        }
        else
        {
            var avoidanceVector = Vector2.zero;
            foreach (var col in _avoidanceResults)
                avoidanceVector -= (Vector2)col.transform.position - rigidbody2D.position;
            avoidanceVector /= _avoidanceResults.Count;

            desiredVelocity = avoidanceVector.normalized * followSpeed;
        }

        var velocity = rigidbody2D.velocity;
        velocity += (desiredVelocity - velocity) * (acceleration * Time.deltaTime);
        rigidbody2D.velocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_dying || _spawning) return;

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