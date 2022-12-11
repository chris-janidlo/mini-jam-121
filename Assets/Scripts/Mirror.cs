using System.Collections.Generic;
using System.Linq;
using crass;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class Mirror : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
{
    private static readonly List<Mirror> ActiveMirrors = new();

    public UnityEvent onDeath;

    [SerializeField] private new BoxCollider2D collider;
    private readonly List<Collider2D> _overlappingColliders = new();

    private Vector3 _dragOffset;

    public Bounds Bounds => collider.bounds;

    private void OnEnable()
    {
        ActiveMirrors.Add(this);
    }

    private void OnDisable()
    {
        ActiveMirrors.Remove(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var mousePos = MousePosition2D(eventData);
        _dragOffset = transform.position - mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var mousePos = MousePosition2D(eventData);
        transform.position = _dragOffset + mousePos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        _overlappingColliders.Clear();
        collider.OverlapCollider(new ContactFilter2D { useTriggers = true }, _overlappingColliders);

        if (_overlappingColliders.Any(r => r.CompareTag(Constants.PLAYER_TAG)))
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
        Debug.Log("err");
    }

    private void Kill(IEnumerable<Enemy> enemies)
    {
        foreach (var enemy in enemies) enemy.Kill();

        Debug.Log("destroying");
        Destroy(gameObject);

        onDeath.Invoke();
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