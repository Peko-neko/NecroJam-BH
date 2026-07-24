using UnityEngine;

namespace AlignedGames
{

    public class ScreenshakeTrigger : MonoBehaviour
    {
        // Reference to the main camera GameObject
        public GameObject MainCamera;

        // Magnitude of the screen shake effect
        public float TriggerMagnitude;

        // Duration of the screen shake effect
        public float TriggerDuration;

        // Called when the script instance is being loaded
        void Start()
        {
            // Find the GameObject tagged as "MainCamera"
            MainCamera = GameObject.FindWithTag("MainCamera");

            // Trigger the screen shake effect on the ScreenShakeManager component
            MainCamera.GetComponentInChildren<ScreenShakeManager>().TriggerShake(TriggerDuration, TriggerMagnitude);
        }
    }

}
