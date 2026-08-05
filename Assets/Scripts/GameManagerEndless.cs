using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace AlignedGames
{
    public class GameManagerEndless : MonoBehaviour
    {
        public static GameManagerEndless Instance;

        [Header("UI")]
        public GameObject deathMenu;
        public GameObject playerPrefab;
        public Transform spawnPoint;
        public bool IsAnyMenuOpen;

        [System.Serializable]
        public class Arena
        {
            public GameObject arenaWalls;          // Enable while fighting
            public Transform[] spawnPoints;
            public GameObject[] enemies;
        }

        [Header("Stage")]
        public Arena[] arenas;

        private int currentArena = -1;
        private readonly List<GameObject> aliveEnemies = new();

        [Header("Scene Progression")]
        public string nextSceneName = "Stage2";
        public DialogueManager dialogueManager;
        public DialogueSequence outroDialogue;
        public float transitionDelay = 2f;

        private void Awake()
        {
            Instance = this;

            Time.timeScale = 1;
            Cursor.visible = false;

            foreach (Arena arena in arenas)
            {
                if (arena.arenaWalls != null)
                    arena.arenaWalls.SetActive(false);
            }
        }

        public void StartArena(int arenaIndex)
        {
            if (arenaIndex != currentArena + 1)
                return;

            currentArena = arenaIndex;

            Arena arena = arenas[currentArena];

            if (arena.arenaWalls != null)
                arena.arenaWalls.SetActive(true);

            aliveEnemies.Clear();

            for (int i = 0; i < arena.enemies.Length; i++)
            {
                Transform point = arena.spawnPoints[i % arena.spawnPoints.Length];

                GameObject enemy = Instantiate(
                    arena.enemies[i],
                    point.position,
                    Quaternion.identity);

                aliveEnemies.Add(enemy);
            }

            StartCoroutine(CheckArenaClear());
        }

        IEnumerator CheckArenaClear()
        {
            while (true)
            {
                aliveEnemies.RemoveAll(e => e == null);

                if (aliveEnemies.Count == 0)
                    break;

                yield return null;
            }

            Arena arena = arenas[currentArena];

            if (arena.arenaWalls != null)
                arena.arenaWalls.SetActive(false);

            if (currentArena >= arenas.Length - 1)
            {
                StageClear();
            }
        }

        void StageClear()
        {
            SaveSystem.SaveProgress(nextSceneName);

            if (outroDialogue != null && dialogueManager != null)
            {
                dialogueManager.StartDialogue(outroDialogue, LoadNextScene);
            }
            else
            {
                Invoke(nameof(LoadNextScene), transitionDelay);
            }
        }

        void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }

        public void OnPlayerDeath()
        {
            Invoke(nameof(Death), 3f);
        }

        void Death()
        {
            IsAnyMenuOpen = true;

            deathMenu.SetActive(true);

            Cursor.visible = true;
            Time.timeScale = 0;
        }

        public void RespawnPlayer()
        {
            Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

            deathMenu.SetActive(false);

            Cursor.visible = false;
            Time.timeScale = 1;
        }

        public void RestartScene()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}