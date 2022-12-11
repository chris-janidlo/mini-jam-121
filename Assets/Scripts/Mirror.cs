using System.Collections.Generic;
using System.Linq;
using crass;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class Mirror : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private static readonly List<Mirror> ActiveMirrors = new();

    [SerializeField] private new BoxCollider2D collider;

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

    private static Vector3 MousePosition2D(PointerEventData eventData)
    {
        var pos = CameraCache.Main.ScreenToWorldPoint(eventData.position);
        pos.z = 0;
        return pos;
    }

    private bool FullyContains(Reflection reflection)
    {
        var min = (Vector2)Bounds.min + Vector2.one * reflection.Radius;
        var max = (Vector2)Bounds.max - Vector2.one * reflection.Radius;
        var point = reflection.Center;

        return
            min.x <= point.x && point.x <= max.x &&
            min.y <= point.y && point.y <= max.y;
    }

    public static bool AnyContain(Reflection reflection)
    {
        return ActiveMirrors.Any(w => w.FullyContains(reflection));
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