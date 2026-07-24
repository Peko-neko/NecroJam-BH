using UnityEngine;

namespace AlignedGames
{
    public class WeatherManager : MonoBehaviour
    {
        [Header("References")]
        public Transform player;          // The player or camera the clouds will follow around
        public Sprite[] cloudSprites;     // A list of cloud images to choose from

        [Header("Cloud Pool")]
        [Tooltip("How many clouds to maintain.")]
        public int maxClouds = 24;        // How many clouds exist at once

        [Tooltip("Prefab used for clouds (optional). If null, a simple GameObject with SpriteRenderer is created.")]
        public GameObject cloudPrefab;    // You can assign a prefab. If none is given, a blank GameObject with a SpriteRenderer will be made

        [Header("Spawn & Despawn")]
        [Tooltip("Clouds spawn randomly within this radius from the player.")]
        public float spawnRadius = 30f;   // How far from the player clouds can spawn

        [Tooltip("When a cloud drifts farther than this radius from the player, it will be repositioned.")]
        public float despawnRadius = 40f; // If a cloud goes this far away, it will be moved back near the player

        [Header("Appearance")]
        [Tooltip("Uniform random scale range for clouds.")]
        public Vector2 uniformScaleRange = new Vector2(0.8f, 1.6f); // Random size range for clouds

        [Tooltip("Optional color tint applied to clouds.")]
        public Color cloudColor = Color.white; // Default color of the clouds

        [Header("Movement")]
        [Tooltip("Multiplies the global wind vector for clouds (1 = exactly wind speed).")]
        public float windSpeedMultiplier = 1f; // Controls how fast clouds move with the wind

        [Tooltip("Small random per-cloud speed variance (e.g., 0.15 = ±15%). Set 0 to disable.")]
        public float perCloudSpeedVariance = 0.15f; // Adds variation so each cloud moves a bit differently

        [Header("Rendering")]
        [Tooltip("Unity Layer index for the cloud GameObjects (camera culling, physics masks).")]
        public int unityLayer = 0; // Which Unity "Layer" clouds are on (used for cameras and physics filters)

        [Tooltip("Sorting Layer name for SpriteRenderer so clouds draw above other sprites.")]
        public string sortingLayerName = "Default"; // Sorting layer name (e.g., "Foreground")

        [Tooltip("Sorting order within the sorting layer. Higher draws on top.")]
        public int sortingOrder = 1000; // Order inside the sorting layer. Higher = in front

        [Header("Runtime (Read-Only)")]
        public GameObject[] clouds; // Stores all the spawned cloud GameObjects

        // Extra runtime data for movement/positioning
        float[] speedMul;        // Individual speed multipliers for each cloud
        Vector2[] initialOffsets; // The starting offset of each cloud from the player

        void Start()
        {
            // If no player assigned, use this GameObject as the center
            if (player == null)
            {
                Debug.LogWarning("[CloudManager] Player reference is missing. Using this transform as center.");
                player = transform;
            }

            // Make sure we always have at least 1 cloud
            if (maxClouds < 1) maxClouds = 1;

            // Create arrays to store clouds and their properties
            clouds = new GameObject[maxClouds];
            speedMul = new float[maxClouds];
            initialOffsets = new Vector2[maxClouds];

            // Spawn all the clouds
            for (int i = 0; i < maxClouds; i++)
            {
                SpawnOrRecycle(i, randomAroundPlayer: true);
            }
        }

        void Update()
        {
            // Default wind direction and speed
            var wind = Vector2.zero;
            float speed = 0f;

            // If a GlobalWindManager exists, get the wind direction and speed from it
            if (GlobalWindManager.Instance != null)
            {
                wind = GlobalWindManager.Instance.Direction;
                speed = GlobalWindManager.Instance.Speed * Mathf.Max(0f, windSpeedMultiplier);
            }

            // Center position (usually player)
            Vector3 center = player != null ? (Vector3)player.position : transform.position;

            // Loop through all clouds
            for (int i = 0; i < clouds.Length; i++)
            {
                var go = clouds[i];
                if (go == null) continue;

                // Each cloud has a speed multiplier and some randomness
                float mul = (1f + RandomSigned(perCloudSpeedVariance)) * speedMul[i];
                Vector2 delta = wind * speed * mul * Time.deltaTime;

                // Move the cloud
                go.transform.position += (Vector3)delta;

                // If cloud is too far away, reposition it
                float dist = Vector2.Distance(go.transform.position, center);
                if (dist > despawnRadius)
                {
                    RepositionUpwind(i);
                }
            }
        }

        // Creates a new cloud or reuses an old one
        void SpawnOrRecycle(int index, bool randomAroundPlayer)
        {
            GameObject go = clouds[index];
            if (go == null)
            {
                // If we have a prefab, use it. Otherwise make an empty object with a SpriteRenderer
                go = (cloudPrefab != null) ? Instantiate(cloudPrefab) : new GameObject("Cloud");
                clouds[index] = go;

                var sr = go.GetComponent<SpriteRenderer>();
                if (sr == null) sr = go.AddComponent<SpriteRenderer>();
                sr.color = cloudColor;
                sr.drawMode = SpriteDrawMode.Simple; // Normal 2D sprite mode

                // Apply layer and sorting settings
                ApplyRenderingSettings(go, sr);
            }
            else
            {
                // Make sure rendering stays consistent if cloud already existed
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) ApplyRenderingSettings(go, sr);
                else ApplyRenderingSettings(go, null);
            }

            // Pick a random sprite for the cloud
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            if (cloudSprites != null && cloudSprites.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
            }

            // Random scale (size)
            float s = Random.Range(uniformScaleRange.x, uniformScaleRange.y);
            go.transform.localScale = new Vector3(s, s, 1f);

            // Decide where to place the cloud
            Vector3 center = player != null ? (Vector3)player.position : transform.position;
            Vector2 offset = randomAroundPlayer
                ? Random.insideUnitCircle.normalized * Random.Range(spawnRadius * 0.3f, spawnRadius)
                : initialOffsets[index];

            go.transform.position = center + (Vector3)offset;

            initialOffsets[index] = offset;

            // Assign a random speed multiplier for movement
            speedMul[index] = Mathf.Clamp(1f + RandomSigned(perCloudSpeedVariance), 0.2f, 2.5f);
        }

        // Moves a cloud back into view when it drifts too far
        void RepositionUpwind(int index)
        {
            if (GlobalWindManager.Instance == null)
            {
                SpawnOrRecycle(index, randomAroundPlayer: true);
                return;
            }

            Vector3 center = player != null ? (Vector3)player.position : transform.position;

            Vector2 windDir = GlobalWindManager.Instance.Direction;
            if (windDir.sqrMagnitude < 0.0001f)
            {
                // No wind, just place randomly
                SpawnOrRecycle(index, randomAroundPlayer: true);
                return;
            }

            // Place cloud "upwind" so it will drift back toward the player
            Vector2 lateral = Vector2.Perpendicular(windDir).normalized;
            float lateralOffset = Random.Range(-spawnRadius, spawnRadius);
            Vector2 pos = (Vector2)center - windDir.normalized * spawnRadius + lateral * lateralOffset;

            var go = clouds[index];
            if (go == null) return;

            go.transform.position = pos;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && cloudSprites != null && cloudSprites.Length > 0)
                sr.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];

            // Reapply rendering settings
            if (sr != null) ApplyRenderingSettings(go, sr);
            else ApplyRenderingSettings(go, null);

            // Random scale again
            float s = Random.Range(uniformScaleRange.x, uniformScaleRange.y);
            go.transform.localScale = new Vector3(s, s, 1f);

            // Assign speed multiplier
            speedMul[index] = Mathf.Clamp(1f + RandomSigned(perCloudSpeedVariance), 0.2f, 2.5f);
        }

        // Makes sure clouds are drawn on the right layer and in front
        void ApplyRenderingSettings(GameObject go, SpriteRenderer sr)
        {
            // Unity layer (used for camera culling and physics filters)
            go.layer = unityLayer;

            // Sorting layer/order (used for rendering draw order)
            if (sr == null) sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (!string.IsNullOrEmpty(sortingLayerName))
                    sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;
            }
        }

        // Returns a random value between -amplitude and +amplitude
        static float RandomSigned(float amplitude)
        {
            if (amplitude <= 0f) return 0f;
            return Random.Range(-amplitude, amplitude);
        }

#if UNITY_EDITOR
        // Draw helpful circles in the Scene view when selected
        void OnDrawGizmosSelected()
        {
            var center = player != null ? player.position : transform.position;
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f);
            Gizmos.DrawWireSphere(center, spawnRadius);   // Where clouds can spawn
            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.DrawWireSphere(center, despawnRadius); // When clouds are recycled
        }
#endif
    }
}
