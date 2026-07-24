using UnityEngine;

namespace AlignedGames
{

    public class TimedDestroyBehaviour : MonoBehaviour
    {
        [Header("Destruction Settings")]
        [Tooltip("Time in seconds before the GameObject is destroyed.")]
        public float destroyAfterSeconds = 5f;

        void Start()
        {
            // Schedule the GameObject for destruction
            Destroy(gameObject, destroyAfterSeconds);
        }
    }

}