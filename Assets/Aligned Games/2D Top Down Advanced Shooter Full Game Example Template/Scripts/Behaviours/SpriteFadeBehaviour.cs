using UnityEngine;
using System.Collections;

namespace AlignedGames
{
    public class SpriteFadeBehaviour : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public Sprite[] bloodDecalSprites; // Array of available sprites for blood decals

        private float lifetime;       // How long the decal stays fully visible
        private float fadeDuration;   // How long the fade-out effect lasts

        // Initialize the decal with sorting order, scale, lifetime and fade duration
        public void Initialize(int sortingOrder, float scale, float lifetime, float fadeDuration)
        {
            // Check if blood sprites are assigned; if none, log error and destroy this object
            if (bloodDecalSprites == null || bloodDecalSprites.Length == 0)
            {
                Debug.LogError("[SpriteFadeBehaviour] No blood sprites assigned.");
                Destroy(gameObject);
                return;
            }

            // If spriteRenderer reference is missing, try to get it from this GameObject
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError("[SpriteFadeBehaviour] Missing SpriteRenderer on decal object.");
                    Destroy(gameObject);
                    return;
                }
            }

            // Assign a random sprite from the array to the spriteRenderer
            spriteRenderer.sprite = bloodDecalSprites[Random.Range(0, bloodDecalSprites.Length)];
            // Set the sorting order to render the decal correctly
            spriteRenderer.sortingOrder = sortingOrder;
            // Set scale for the decal
            transform.localScale = new Vector3(scale, scale, 1f);

            this.lifetime = lifetime;
            this.fadeDuration = fadeDuration;

            // Start the fade-out coroutine
            StartCoroutine(FadeAndDestroy());
        }

        // Coroutine to wait for lifetime seconds, then fade out the decal over fadeDuration seconds, and finally destroy it
        private IEnumerator FadeAndDestroy()
        {
            // Wait while fully visible
            yield return new WaitForSeconds(lifetime);

            float elapsed = 0f;
            Color originalColor = spriteRenderer.color;

            // Gradually reduce alpha from 1 to 0 over fadeDuration time
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            // Destroy the decal GameObject after fade-out completes
            Destroy(gameObject);
        }
    }
}
