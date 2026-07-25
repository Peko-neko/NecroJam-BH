using System.Collections;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Seconds of silence between the end of one play-through and the start of the next.")]
    [SerializeField] private float restartDelay = 10f;

    private void Start()
    {
        audioSource.clip = musicClip;
        audioSource.loop = false;
        StartCoroutine(PlayWithSilenceGap());
    }

    private IEnumerator PlayWithSilenceGap()
    {
        while (true)
        {
            audioSource.Play();
            yield return new WaitForSeconds(musicClip.length);
            yield return new WaitForSeconds(restartDelay);
        }
    }
}
