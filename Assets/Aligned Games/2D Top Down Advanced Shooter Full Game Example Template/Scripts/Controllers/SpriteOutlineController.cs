#if UNITY_EDITOR
using UnityEditor; 
#endif
using UnityEngine;

namespace AlignedGames
{

    public class SpriteOutlineController : MonoBehaviour
    {

        [Header("Outline Settings")]
        public Color outlineColor = Color.black; // Color of the outline
        [Range(0f, 100f)]
        public float outlineThickness = 1f; // Thickness of the outline in pixels
        [Tooltip("Smoothness applies a blur. High values are VERY performance intensive. 0 = no smoothness.")]
        [Range(0f, 2f)] // Limit smoothness to keep performance manageable
        public float smoothness = 0f; // How blurred the outline looks; higher = more blur
        public bool enableFlashing = false; // Should the outline flash on/off
        [Range(0.1f, 5f)]
        public float flashSpeed = 1f; // Speed of flashing effect

        // Private references and variables for internal use
        private SpriteRenderer spriteRenderer; // The main sprite's renderer
        private GameObject outlineObject; // Child GameObject used to display outline
        private SpriteRenderer outlineRenderer; // Renderer for the outline sprite

        private float flashTimer; // Timer used for flashing logic
        private bool _hasLoggedError = false; // Prevents repeated error logs

        // Variables to track changes in settings to know when to regenerate the outline
        private Sprite _lastSprite;
        private float _lastThickness;
        private float _lastSmoothness;
        private Color _lastOutlineColor;
        private bool _lastEnableFlashing;

        private bool _isDirty = true; // True when outline needs updating; start as true to initialize

#if UNITY_EDITOR
        // Editor-only code for updating the outline inside the Unity Editor outside Play mode
#endif

        // Called automatically by Unity when a value in the Inspector changes
        private void OnValidate()
        {
            _isDirty = true; // Mark for update since something changed

#if UNITY_EDITOR
            // Subscribe to Unity Editor's update event to process updates continuously
            // First unsubscribe to avoid adding multiple subscriptions
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }

        // Called when this component is enabled or the GameObject becomes active
        private void OnEnable()
        {
            // Cache the SpriteRenderer component reference
            spriteRenderer = GetComponent<SpriteRenderer>();
            _isDirty = true; // Mark dirty to regenerate outline

#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate; // Clean subscription in editor
            EditorApplication.update += EditorUpdate;
#endif
        }

        // Called when the scene starts or object first activates during Play mode
        private void Start()
        {
            // Ensure spriteRenderer reference is set in Play mode
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            _isDirty = true; // Mark dirty to generate outline on first frame
        }

#if UNITY_EDITOR
        // This runs repeatedly inside the Unity Editor (not Play mode) to update outline when needed
        private void EditorUpdate()
        {
            if (this == null || !enabled) // If component destroyed or disabled, unsubscribe and exit
            {
                EditorApplication.update -= EditorUpdate;
                return;
            }

            if (!gameObject.activeInHierarchy) // If GameObject inactive (e.g., prefab view), skip update
            {
                return;
            }

            if (_isDirty) // If marked dirty, process outline updates
            {
                ProcessUpdates();
            }
        }
#endif

        // Called every frame during Play mode or in Editor if ExecuteAlways attribute is used
        private void Update()
        {
            if (_isDirty) // If any relevant property changed, update outline
            {
                ProcessUpdates();
            }

            // Handle flashing effect during Play mode if enabled and outline is visible
            if (Application.isPlaying && enableFlashing && outlineRenderer != null && outlineObject != null && outlineObject.activeSelf)
            {
                FlashOutline();
            }
        }

        // Handles all logic for creating/updating the outline texture and related properties
        private void ProcessUpdates()
        {
            if (spriteRenderer == null) // Make sure we have the main sprite renderer
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    // Log an error once if missing SpriteRenderer since the outline cannot work without it
                    LogErrorOnce("SpriteRenderer component is missing. Outline cannot function.");

                    if (Application.isPlaying)
                        enabled = false; // Disable this component if critical dependency missing in Play mode

                    _isDirty = false; // No further processing needed
                    return;
                }
            }

            InitializeOutlineObjects(); // Prepare the GameObject and SpriteRenderer for the outline

            if (spriteRenderer.sprite == null) // If no sprite assigned to main renderer
            {
                if (outlineObject != null)
                    outlineObject.SetActive(false); // Hide the outline object

                _lastSprite = null; // Clear last sprite so outline regenerates if new sprite is assigned
                _isDirty = false; // Done updating for now
                return;
            }

            if (outlineObject != null && !outlineObject.activeSelf)
                outlineObject.SetActive(true); // Show the outline object if hidden

            // Check if any relevant properties changed that require regenerating the outline
            bool spriteChanged = _lastSprite != spriteRenderer.sprite;
            bool thicknessChanged = Mathf.Abs(_lastThickness - outlineThickness) > 0.001f;
            bool smoothnessChanged = Mathf.Abs(_lastSmoothness - smoothness) > 0.001f;
            bool outlineNeedsGeneration = spriteChanged || thicknessChanged || smoothnessChanged || (outlineRenderer != null && outlineRenderer.sprite == null);

            if (outlineNeedsGeneration)
            {
                GenerateAndApplyOutlineTexture(); // Create new outline texture and assign it

                // Update the "last known" values so changes can be detected later
                _lastSprite = spriteRenderer.sprite;
                _lastThickness = outlineThickness;
                _lastSmoothness = smoothness;
            }

            ApplyVisualProperties(); // Set colors and other visual effects on the outline

            _isDirty = false; // Mark as clean since update is done
        }

        // Create or get the outline GameObject and SpriteRenderer needed to render the outline
        private void InitializeOutlineObjects()
        {
            if (outlineObject == null)
            {
                // Try to find an existing child GameObject named "SpriteOutline"
                Transform existingOutline = transform.Find("SpriteOutline");
                if (existingOutline != null && existingOutline.parent == transform)
                {
                    outlineObject = existingOutline.gameObject;
                    outlineRenderer = outlineObject.GetComponent<SpriteRenderer>();
                }
                else
                {
                    // If no existing child found, create a new GameObject for the outline
                    outlineObject = new GameObject("SpriteOutline");
                    outlineObject.transform.SetParent(transform); // Make it a child of this object
                }
                // Hide the outline object from hierarchy and prevent saving it with the scene
                outlineObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            }

            // Ensure the outline object's transform is reset relative to parent
            if (outlineObject.transform.parent != transform)
                outlineObject.transform.SetParent(transform);

            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one;

            // Setup or add a SpriteRenderer on the outline object if needed
            if (outlineRenderer == null)
            {
                outlineRenderer = outlineObject.GetComponent<SpriteRenderer>();
                if (outlineRenderer == null)
                {
                    outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
                }
            }

            // Set sorting so outline renders behind the main sprite
            outlineRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        }

        // Generate the outline texture and apply it to the outline sprite renderer
        private void GenerateAndApplyOutlineTexture()
        {
            // Early exit if any critical references are missing
            if (spriteRenderer == null || spriteRenderer.sprite == null || outlineRenderer == null)
            {
                if (outlineRenderer != null)
                    outlineRenderer.sprite = null;
                return;
            }

            // Create a readable texture from the sprite for outline generation
            Texture2D sourceTextureForOutline = GetReadableTextureFromSprite(spriteRenderer.sprite);

            if (sourceTextureForOutline == null)
            {
                if (outlineRenderer != null)
                    outlineRenderer.sprite = null;
                return;
            }

            // Create the outline texture based on the source texture and outline settings
            Texture2D generatedTexture = CreateOutlineTexture(sourceTextureForOutline);

            // If source texture was a temporary cropped copy, destroy it to free memory
            if (sourceTextureForOutline != spriteRenderer.sprite.texture && sourceTextureForOutline != null)
            {
                SafeDestroy(sourceTextureForOutline);
            }

            if (generatedTexture == null)
            {
                if (outlineRenderer != null)
                    outlineRenderer.sprite = null;
                return;
            }

            // If outlineRenderer already has a sprite with a temporary texture, destroy those textures to prevent leaks
            if (outlineRenderer.sprite != null)
            {
                if (outlineRenderer.sprite.texture != null && (outlineRenderer.sprite.texture.hideFlags & HideFlags.DontSave) != 0)
                {
                    SafeDestroy(outlineRenderer.sprite.texture);
                }
                SafeDestroy(outlineRenderer.sprite);
            }

            // Create a new sprite from the generated outline texture and assign it to the outlineRenderer
            Rect spriteRect = new Rect(0, 0, generatedTexture.width, generatedTexture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            outlineRenderer.sprite = Sprite.Create(generatedTexture, spriteRect, pivot, spriteRenderer.sprite.pixelsPerUnit);
        }

        private void ApplyVisualProperties()
        {
            // Skip if no outlineRenderer or outlineObject is inactive
            if (outlineRenderer == null || (outlineObject != null && !outlineObject.activeSelf)) return;

            // Determine if flashing effect should be active during play mode and if enabled
            bool currentFlashingState = Application.isPlaying && enableFlashing;

            // If not flashing, ensure static outline color is applied
            if (!currentFlashingState)
            {
                // Update color if changed or if flashing was just disabled
                if (outlineRenderer.color != outlineColor || _lastEnableFlashing)
                {
                    outlineRenderer.color = outlineColor;
                }
            }
            // Note: Flashing effect is updated in Update() when flashing is active

            // Remember current color and flashing state for next update
            _lastOutlineColor = outlineColor;
            _lastEnableFlashing = currentFlashingState;
        }

        private Texture2D GetReadableTextureFromSprite(Sprite sprite)
        {
            // Return null and log if sprite or its texture is missing
            if (sprite == null || sprite.texture == null)
            {
                LogErrorOnce("GetReadableTextureFromSprite: Sprite or its texture is null.");
                return null;
            }

            Texture2D sourceTex = sprite.texture;
            Rect spriteRect = sprite.textureRect;

            // In the editor, check if the texture is readable and sprite uses full texture
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(sourceTex);
            if (!string.IsNullOrEmpty(path))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.isReadable &&
                    Mathf.Approximately(spriteRect.x, 0) && Mathf.Approximately(spriteRect.y, 0) &&
                    Mathf.Approximately(spriteRect.width, sourceTex.width) && Mathf.Approximately(spriteRect.height, sourceTex.height))
                {
                    // Use original texture directly if conditions met
                    return sourceTex;
                }
            }
#endif

            // If not readable directly or not in editor, create a readable copy of the sprite's portion
            try
            {
                // Confirm source texture is not null
                if (sourceTex == null)
                {
                    LogErrorOnce($"GetReadableTextureFromSprite: Source texture became null for sprite '{sprite.name}'.");
                    return null;
                }

                // Create a new Texture2D to hold the cropped portion
                Texture2D croppedTex = new Texture2D((int)spriteRect.width, (int)spriteRect.height, TextureFormat.ARGB32, false);
                croppedTex.filterMode = sourceTex.filterMode;
                croppedTex.wrapMode = TextureWrapMode.Clamp;

                // Copy pixels from the sprite's rectangle area
                Color[] pixels = sourceTex.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);
                croppedTex.SetPixels(pixels);
                croppedTex.Apply(true, false);

                // Prevent this texture from being saved to disk
                croppedTex.hideFlags = HideFlags.DontSave;

                return croppedTex;
            }
            catch (UnityException e)
            {
                // Log error if texture is not readable (common cause: Read/Write not enabled in import settings)
                LogErrorOnce($"Failed to GetPixels from sprite '{sprite.name}'. Ensure its texture '{sourceTex.name}' is Read/Write enabled in Import Settings. Error: {e.Message}");
                return null;
            }
        }

        private Texture2D CreateOutlineTexture(Texture2D originalTexture) // Expects already cropped, readable texture
        {
            if (originalTexture == null) return null;

            int width = originalTexture.width;
            int height = originalTexture.height;

            // Create new texture to store the outline pixels
            Texture2D texOutline = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texOutline.filterMode = originalTexture.filterMode;
            texOutline.wrapMode = TextureWrapMode.Clamp;
            texOutline.hideFlags = HideFlags.DontSave;

            Color32[] originalPixels;
            try
            {
                // Get pixel colors of original texture
                originalPixels = originalTexture.GetPixels32();
            }
            catch (UnityException e)
            {
                // Log error and safely destroy outline texture if pixel data retrieval fails
                LogErrorOnce($"CreateOutlineTexture: Failed to GetPixels32 from '{originalTexture.name}'. Error: {e.Message}");
                SafeDestroy(texOutline);
                return null;
            }

            // Create array for outline pixels, default transparent
            Color32[] outlinePixels = new Color32[width * height];

            // Iterate all pixels to detect edges and draw outlines
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // If pixel is on edge, draw outline around it
                    if (IsEdgePixel(originalPixels, x, y, width, height))
                    {
                        DrawOutlineAtPixel(outlinePixels, x, y, width, height, originalPixels);
                    }
                }
            }

            // Optionally apply smoothing if enabled
            if (smoothness > 0.01f)
            {
                outlinePixels = ApplySmoothness(outlinePixels, width, height);
            }

            // Apply the outline pixels to the texture and update it
            texOutline.SetPixels32(outlinePixels);
            texOutline.Apply(true, false);

            return texOutline;
        }

        private bool IsEdgePixel(Color32[] pixels, int x, int y, int width, int height)
        {
            int index = x + y * width;

            // Pixel must be mostly opaque to be considered for outline
            if (pixels[index].a < 10) return false;

            // Check all surrounding pixels (including diagonals)
            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    if (ox == 0 && oy == 0) continue; // Skip the pixel itself

                    int nx = x + ox;
                    int ny = y + oy;

                    // If adjacent pixel is inside texture bounds
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    {
                        // If adjacent pixel is mostly transparent, current pixel is an edge
                        if (pixels[nx + ny * width].a < 10) return true;
                    }
                    else
                    {
                        // Adjacent to texture border counts as edge
                        return true;
                    }
                }
            }

            // No adjacent transparent pixels or border, so not an edge
            return false;
        }


        private void DrawOutlineAtPixel(Color32[] targetOutlinePixels, int edgeX, int edgeY, int width, int height, Color32[] originalSpritePixels)
        {
            // Calculate outline drawing range, making sure it is zero or positive
            int range = Mathf.Max(0, Mathf.CeilToInt(outlineThickness));
            // Set the outline color to white (full opacity)
            Color32 colorForOutlineStamp = new Color32(255, 255, 255, 255);

            // Loop over the square area around the edge pixel based on the range
            for (int oy = -range; oy <= range; oy++)
            {
                for (int ox = -range; ox <= range; ox++)
                {
                    // Calculate the current pixel's position within the texture
                    int currentX = edgeX + ox;
                    int currentY = edgeY + oy;

                    // Check if the pixel is inside the texture boundaries
                    if (currentX >= 0 && currentY >= 0 && currentX < width && currentY < height)
                    {
                        int currentIndex = currentX + currentY * width;

                        // Draw outline only if the original sprite pixel here is mostly transparent
                        if (originalSpritePixels[currentIndex].a < 10)
                        {
                            // Draw the outline pixel only if it's more opaque than the current one
                            if (colorForOutlineStamp.a > targetOutlinePixels[currentIndex].a)
                            {
                                targetOutlinePixels[currentIndex] = colorForOutlineStamp;
                            }
                        }
                    }
                }
            }
        }

        private Color32[] ApplySmoothness(Color32[] pixels, int width, int height)
        {
            // Skip smoothing if smoothness value is very low
            if (smoothness <= 0.01f) return pixels;

            Color32[] blurredPixels = new Color32[pixels.Length];

            // Calculate blur radius from smoothness value
            int radius = Mathf.Max(1, Mathf.FloorToInt(smoothness));

            // For each pixel in the texture
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate average color of surrounding pixels and assign it
                    blurredPixels[x + y * width] = AverageSurrounding(pixels, x, y, width, height, radius);
                }
            }

            return blurredPixels;
        }

        private Color32 AverageSurrounding(Color32[] sourcePixels, int x, int y, int width, int height, int radius)
        {
            // Accumulate color channel sums for averaging
            float rSum = 0, gSum = 0, bSum = 0, aSum = 0;
            int count = 0;

            // Iterate over surrounding pixels within radius
            for (int oy = -radius; oy <= radius; oy++)
            {
                for (int ox = -radius; ox <= radius; ox++)
                {
                    int nx = x + ox;
                    int ny = y + oy;

                    // Check bounds
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    {
                        Color32 px = sourcePixels[nx + ny * width];
                        rSum += px.r;
                        gSum += px.g;
                        bSum += px.b;
                        aSum += px.a;
                        count++;
                    }
                }
            }

            // If no surrounding pixels found (unlikely), return original pixel color
            if (count == 0) return sourcePixels[x + y * width];

            // Return averaged color values as new pixel color
            return new Color32(
                (byte)(rSum / count),
                (byte)(gSum / count),
                (byte)(bSum / count),
                (byte)(aSum / count));
        }

        private void FlashOutline()
        {
            // Do nothing if outline renderer is missing
            if (outlineRenderer == null) return;

            // Increment timer based on flash speed and delta time
            flashTimer += Time.deltaTime * flashSpeed;

            // Use current outline color for base
            Color baseCol = outlineColor;

            // Calculate new alpha based on a sine wave for flashing effect
            float newAlpha = Mathf.Abs(Mathf.Sin(flashTimer)) * baseCol.a;

            // Set outline renderer color with updated alpha
            outlineRenderer.color = new Color(baseCol.r, baseCol.g, baseCol.b, newAlpha);
        }

        private void LogErrorOnce(string message)
        {
            // Log warning only once to avoid flooding the console
            if (!_hasLoggedError)
            {
                Debug.LogWarning($"[SpriteOutlineController] {gameObject.name}: {message}", this);
                _hasLoggedError = true;
            }
        }

        private void SafeDestroy(Object obj)
        {
            // Do nothing if object is already null
            if (obj == null) return;

#if UNITY_EDITOR
            // In editor: destroy immediately if not playing, else normal destroy
            if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
#else
    // In builds: normal destroy
    Destroy(obj);
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            // Unsubscribe from editor update callback when disabled
            EditorApplication.update -= EditorUpdate;
#endif

            // Clean up outline object and renderer when this component is disabled
            if (outlineObject != null)
            {
                SafeDestroy(outlineObject);
                outlineObject = null;
                outlineRenderer = null; // Prevent accidental access after destruction
            }

            // Note: any temporary textures used by the outline renderer will be cleaned up with it
        }

    }

}