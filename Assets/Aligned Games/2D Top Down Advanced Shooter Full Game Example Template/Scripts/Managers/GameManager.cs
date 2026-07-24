using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlignedGames
{
    public class GameManager : MonoBehaviour
    {
        // Singleton instance for global access
        public static GameManager Instance;

        [Header("UI References")]
        // Reference to the death menu UI
        public GameObject deathMenu;

        // Player prefab to respawn
        public GameObject playerPrefab;

        // Position where the player should respawn
        public GameObject spawnPoint;

        // Keeps track of whether any menu is open
        public bool IsAnyMenuOpen;

        // Called when this script is first loaded
        private void Awake()
        {
            // Ensure time is running at normal speed
            Time.timeScale = 1f;

            // Handle singleton logic (only one instance allowed)
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            // Hide the cursor at start
            Cursor.visible = false;

            IsAnyMenuOpen = false;
        }

        // Called when player dies
        public void OnPlayerDeath()
        {
            // Wait 3 seconds, then trigger death logic
            Invoke(nameof(Death), 3f);
        }

        // Activates the death screen and pauses the game
        public void Death()
        {
            IsAnyMenuOpen = true;

            // Show death menu if assigned
            if (deathMenu != null)
                deathMenu.SetActive(true);

            // Pause the game
            Time.timeScale = 0f;

            // Show the mouse cursor
            Cursor.visible = true;
        }

        // Instantiates a new player at the spawn point
        public void RespawnPlayer()
        {
            Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
            Time.timeScale = 1f;

            // Hide death menu after respawn
            if (deathMenu != null)
                deathMenu.SetActive(false);
        }

        // Reloads the current scene
        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Quits the application
        public void QuitGame()
        {
            Application.Quit();
        }

        // Loads the "WorldScene"
        public void OpenWorldScene()
        {
            SceneManager.LoadScene("WorldScene", LoadSceneMode.Single);
        }

        // Loads the "PrototypeScene"
        public void OpenPrototypeScene()
        {
            SceneManager.LoadScene("PrototypeScene", LoadSceneMode.Single);
        }
    }
}
