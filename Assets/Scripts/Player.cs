using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float floatAmplitude, floatPeriod;

    [SerializeField] private new Rigidbody2D rigidbody2D;
    [SerializeField] private Transform cloudTransform, reflectionTransform;
    [SerializeField] private Vector2Variable playerPosition;

    private Vector2 _cloudLocalStartPosition, _reflectionLocalStartPosition;
    private Vector2 _input;

    private void Start()
    {
        Assert.IsTrue(rigidbody2D.isKinematic);

        _cloudLocalStartPosition = cloudTransform.localPosition;
        _reflectionLocalStartPosition = reflectionTransform.localPosition;
    }

    private void Update()
    {
        ReadInput();
        Float();
    }

    private void FixedUpdate()
    {
        rigidbody2D.velocity = _input * speed;
        playerPosition.Value = rigidbody2D.position;
    }

    private void ReadInput()
    {
        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void Float()
    {
        var floatOffset = floatAmplitude * Mathf.Sin(2f * Mathf.PI / floatPeriod * Time.time);

        cloudTransform.localPosition = _cloudLocalStartPosition + floatOffset * Vector2.up;
        reflectionTransform.localPosition = _reflectionLocalStartPosition + floatOffset * Vector2.up;
    }
}