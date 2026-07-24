using UnityEngine;

namespace AlignedGames
{

    [ExecuteAlways] // This makes the script run both in edit mode and play mode
    [RequireComponent(typeof(SpriteRenderer))] // Requires the GameObject to have a SpriteRenderer component
    public class SpriteShadowController : MonoBehaviour
    {
        [Header("Shadow Settings")]
        public Color shadowColor = Color.black; // Color of the shadow
        [Range(0.0f, 100.0f)]
        public float shadowThickness = 1.0f; // How thick the shadow appears (percentage)
        public Vector2 shadowOffset = new Vector2(0.1f, -0.1f); // Offset position of the shadow relative to the sprite

        private SpriteRenderer spriteRenderer; // Reference to the main sprite renderer
        private SpriteRenderer shadowRenderer; // Reference to the shadow's sprite renderer
        private GameObject shadowObject; // The GameObject that holds the shadow sprite

        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer on this GameObject
            CreateOrGetShadow(); // Create shadow object or get existing one
            UpdateShadow(); // Update shadow's appearance
            SyncShadowTransform(); // Sync shadow's position and rotation with the main sprite
        }

        private void OnDestroy()
        {
            if (shadowObject)
                DestroyImmediate(shadowObject); // Remove the shadow GameObject immediately when this component is disabled
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                // While editing in the editor (not playing), keep shadow updated and synced
                UpdateShadow();
                SyncShadowTransform();
            }
        }

        private void CreateOrGetShadow()
        {
            // Try to find an existing child GameObject named "SpriteShadow"
            if (shadowObject == null)
            {
                shadowObject = transform.Find("SpriteShadow")?.gameObject;
            }

            // If no shadow object found, create a new one
            if (shadowObject == null)
            {
                shadowObject = new GameObject("SpriteShadow"); // Create new GameObject
                shadowObject.transform.SetParent(transform); // Set it as a child of this object
                shadowObject.transform.localPosition = Vector3.zero; // Reset local position
                shadowObject.transform.localRotation = Quaternion.identity; // Reset local rotation
                shadowObject.transform.localScale = Vector3.one; // Reset local scale

                shadowRenderer = shadowObject.AddComponent<SpriteRenderer>(); // Add a SpriteRenderer to the shadow object
                shadowRenderer.material = new Material(Shader.Find("Sprites/Default")); // Assign default sprite shader
            }
            else
            {
                // If shadow object exists, get its SpriteRenderer
                shadowRenderer = shadowObject.GetComponent<SpriteRenderer>();
            }

            // Ensure shadowRenderer is not null, add component if missing
            if (shadowRenderer == null)
                shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        }

        private void UpdateShadow()
        {
            if (shadowRenderer == null || spriteRenderer == null) return; // Safety check

            // Set shadow's color and sprite to match the main sprite
            shadowRenderer.color = shadowColor;
            shadowRenderer.sprite = spriteRenderer.sprite;

            // Scale the shadow based on thickness setting (adds a small scale increase)
            Vector3 scale = Vector3.one * (1 + shadowThickness / 100f);
            shadowObject.transform.localScale = scale;
        }

        private void SyncShadowTransform()
        {
            if (shadowObject == null || shadowRenderer == null) return; // Safety check

            // Position shadow offset from main sprite's position
            shadowObject.transform.position = transform.position + (Vector3)shadowOffset;

            // Match rotation of main sprite
            shadowObject.transform.rotation = transform.rotation;

            // Scale shadow relative to main sprite scale and thickness
            shadowObject.transform.localScale = transform.localScale * (1 + shadowThickness / 100f);

            // Match sorting layer but put shadow behind the main sprite by lowering sorting order
            shadowRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            shadowRenderer.sortingOrder = spriteRenderer.sortingOrder - 2;
        }
    }

}
