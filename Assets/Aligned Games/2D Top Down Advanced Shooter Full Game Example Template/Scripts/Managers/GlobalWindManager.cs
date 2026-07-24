using UnityEngine;

namespace AlignedGames
{
    public class GlobalWindManager : MonoBehaviour
    {
        public static GlobalWindManager Instance { get; private set; }

        [Header("Wind")]
        [Tooltip("Wind speed in world units per second.")]
        public float windSpeed = 2f;

        [Tooltip("Wind direction in degrees (0° = +X, 90° = +Y).")]
        public float windAngleDegrees = 0f;

        public System.Action OnWindChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnValidate()
        {
            OnWindChanged?.Invoke();
        }
        public Vector2 Direction
        {
            get
            {
                float rad = windAngleDegrees * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            }
        }
        public float Speed => Mathf.Max(0f, windSpeed);
        public Vector2 WindVector => Direction * Speed;
        public void SetWind(float angleDegrees, float speed)
        {
            windAngleDegrees = angleDegrees;
            windSpeed = Mathf.Max(0f, speed);
            OnWindChanged?.Invoke();
        }
        public void SetWind(Vector2 windVec)
        {
            windSpeed = windVec.magnitude;
            windAngleDegrees = Mathf.Atan2(windVec.y, windVec.x) * Mathf.Rad2Deg;
            OnWindChanged?.Invoke();
        }
    }
}
