using System.Collections;
using UnityAtoms.BaseAtoms;
using UnityAtoms.SceneMgmt;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float floatAmplitude, floatPeriod;

    [SerializeField] private new Rigidbody2D rigidbody2D;
    [SerializeField] private Vector2Variable playerPosition;
    [SerializeField] private Transform cloudTransform, reflectionTransform;
    [SerializeField] private GameObject shadow;
    [SerializeField] private Animator cloudAnimator, reflectionAnimator;
    [SerializeField] private string deathAnimationStateName;
    [SerializeField] private float gameOverSceneLoadDelay;
    [SerializeField] private SceneField gameOver;
    [SerializeField] private BoolVariable playerDying;

    private Vector2 _cloudLocalStartPosition, _reflectionLocalStartPosition;
    private Vector2 _input;

    private void Start()
    {
        Assert.IsTrue(rigidbody2D.isKinematic);

        playerDying.Value = false;

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
        if (playerDying.Value) return;

        rigidbody2D.velocity = _input * speed;
        playerPosition.Value = rigidbody2D.position;
    }

    public void Kill()
    {
        IEnumerator DeathRoutine()
        {
            playerDying.Value = true;

            cloudAnimator.Play(deathAnimationStateName);
            reflectionAnimator.Play(deathAnimationStateName);
            Destroy(shadow);

            yield return new WaitForSeconds(gameOverSceneLoadDelay);

            SceneManager.LoadScene(gameOver);
        }

        StartCoroutine(DeathRoutine());
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