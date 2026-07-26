using System.Collections;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Play(Sprite[] frames, float frameTime)
    {
        StopAllCoroutines();
        StartCoroutine(Animate(frames, frameTime));
    }

    IEnumerator Animate(Sprite[] frames, float frameTime)
    {
        foreach (Sprite frame in frames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameTime);
        }

        spriteRenderer.sprite = null;   // Hide weapon afterwards
    }
}