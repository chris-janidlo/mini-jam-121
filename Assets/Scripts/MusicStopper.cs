using DG.Tweening;
using UnityEngine;

public class MusicStopper : MonoBehaviour
{
    [SerializeField] private float fadeDuration;
    [SerializeField] private Ease fadeEase;

    [SerializeField] private AudioSource audioSource;

    public void OnPlayerDyingChanged(bool value)
    {
        if (!value) return;

        audioSource.DOFade(0, fadeDuration).SetEase(fadeEase);
    }
}