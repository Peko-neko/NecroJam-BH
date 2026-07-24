using UnityEngine;

namespace AlignedGames
{
    public class RandomAudioBehaviour : MonoBehaviour
    {
        // Audio settings section with a list of sound clips and an audio source
        [Header("Audio Settings")]
        [Tooltip("List of attack sounds to choose from.")]
        public AudioClip[] RandomSound; // Array to hold different sound clips to play randomly

        public AudioSource audioSource; // Reference to the AudioSource component that will play the sounds

        // Start is called before the first frame update
        public void Start()
        {
            // Play a random attack sound when the game starts
            PlayRandomAttackSound();
        }

        // Method to play a random attack sound
        public void PlayRandomAttackSound()
        {
            // Check if the audio source is not already playing
            if (!audioSource.isPlaying)
            {
                // Stop the audio source if it's playing
                audioSource.Stop();

                // Check if the sound list is empty or the audio source is not assigned
                if (RandomSound.Length == 0 || audioSource == null)
                {
                    // Log a warning if there are no sounds or the audio source is missing
                    Debug.LogWarning("No attack sounds or AudioSource assigned in PlayerAudioManager.");
                    return;
                }

                // Select a random sound from the list
                int randomIndex = Random.Range(0, RandomSound.Length); // Random index within the bounds of the sound array
                AudioClip selectedClip = RandomSound[randomIndex]; // Get the selected random sound clip

                // Play the selected sound using PlayOneShot to play it once
                audioSource.PlayOneShot(selectedClip);
            }
        }
    }
}
