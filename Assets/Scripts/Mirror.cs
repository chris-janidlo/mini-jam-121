using System.Collections;
using System.Collections.Generic;
using System.Linq;
using crass;
using DG.Tweening;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class Mirror : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
{
    private static readonly List<Mirror> ActiveMirrors = new();

    public UnityEvent onDeath;

    [SerializeField] private Vector2 dimensions;

    [SerializeField] private Transform childrenContainer;

    [SerializeField] private Ease spawnEase, despawnEase;
    [SerializeField] private float spawnTime, despawmTime, errTime, errOffset;

    [SerializeField] private SpriteRenderer mainArea, border;
    [SerializeField] private new BoxCollider2D collider;
    [SerializeField] private BoolVariable playerDying;

    private readonly List<Collider2D> _overlappingColliders = new();
    private bool _alive; // false when spawning or despawning
    private Vector3 _dragOffset;

    public Bounds Bounds => collider.bounds;

    private IEnumerator Start()
    {
        collider.size = dimensions;
        mainArea.transform.localScale = dimensions;

        // mirror border has 3 additional pixels in each dimension - two to the left and bottom, one to the right and top
        border.size = dimensions + 3f / 32f * Vector2.one;

        transform.localScale = Vector3.zero;
        yield return transform.DOScale(Vector3.one, spawnTime)
            .SetEase(spawnEase)
            .WaitForCompletion();

        ActiveMirrors.Add(this);
        _alive = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (playerDying.Value || !_alive) return;

        var mousePos = MousePosition2D(eventData);
        _dragOffset = transform.position - mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (playerDying.Value || !_alive) return;

        var mousePos = MousePosition2D(eventData);
        transform.position = _dragOffset + mousePos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playerDying.Value || !_alive) return;

        if (eventData.button != PointerEventData.InputButton.Right) return;

        _overlappingColliders.Clear();
        collider.OverlapCollider(new ContactFilter2D { useTriggers = true }, _overlappingColliders);

        if (!CanKill())
            PlayCantCloseAnimation();
        else
            Kill(_overlappingColliders.Select(c => c.GetComponent<Enemy>()).Where(e => e is not null));
    }

    private static Vector3 MousePosition2D(PointerEventData eventData)
    {
        var pos = CameraCache.Main.ScreenToWorldPoint(eventData.position);
        pos.z = 0;
        return pos;
    }

    private bool FullyContains(Vector2 point, float radius)
    {
        var min = (Vector2)Bounds.min + Vector2.one * radius;
        var max = (Vector2)Bounds.max - Vector2.one * radius;

        return
            min.x <= point.x && point.x <= max.x &&
            min.y <= point.y && point.y <= max.y;
    }

    private void PlayCantCloseAnimation()
    {
        childrenContainer.localPosition = errOffset * (RandomExtra.Chance(.5f) ? 1 : -1) * Vector3.right;
        childrenContainer.DOLocalMoveX(0, errTime)
            .SetEase(Ease.OutElastic);
    }

    private void Kill(IEnumerable<Enemy> enemies)
    {
        IEnumerator DeathRoutine()
        {
            ActiveMirrors.Remove(this);
            _alive = false;
            foreach (var enemy in enemies) enemy.Kill();

            yield return transform
                .DOScale(Vector3.zero, despawmTime)
                .SetEase(despawnEase)
                .WaitForCompletion();

            Destroy(gameObject);
            onDeath.Invoke();
        }

        StartCoroutine(DeathRoutine());
    }

    private bool CanKill()
    {
        foreach (var other in _overlappingColliders)
        {
            if (!other.CompareTag(Constants.PLAYER_TAG)) continue;

            var reflection = other.GetComponent<Reflection>();

            var count = ActiveMirrors.Count(m => m.FullyContains(reflection.Center, reflection.Radius));
            return count > 1;
        }

        return true;
    }

    public static bool AnyContain(Reflection reflection)
    {
        return AnyContain(reflection.Center, reflection.Radius);
    }

    public static bool AnyContain(Vector2 point, float radius)
    {
        return ActiveMirrors.Any(w => w.FullyContains(point, radius));
    }

    public static Mirror ClosestTo(Vector2 point)
    {
        var minSquareDist = float.PositiveInfinity;
        Mirror closestMirror = null;

        foreach (var mirror in ActiveMirrors)
        {
            var squareDist = Vector2.SqrMagnitude(mirror.collider.ClosestPoint(point) - point);
            if (squareDist >= minSquareDist) continue;

            minSquareDist = squareDist;
            closestMirror = mirror;
        }

        Assert.IsNotNull(closestMirror);
        return closestMirror;
    }
}