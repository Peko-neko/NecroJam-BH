using System.Collections.Generic;
using UnityEngine;

namespace AlignedGames
{
    public class CelestialDirector : MonoBehaviour
    {
        public static CelestialDirector Instance;

        [Header("Spawn")]
        public Transform[] spawnPoints;

        [Header("Enemy Pool")]
        public List<EnemyEntry> enemyPool = new();

        [Header("Wave")]
        public int currentWave = 1;

        public int threatBudget = 6;

        public int threatIncrease = 2;

        public float delayBetweenWaves = 4f;

        [Header("Runtime")]
        public List<CelestialEnemyAI> aliveEnemies = new();

        bool waitingForNextWave;

        public int meleeKills;
        public int rangedKills;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            StartWave();
        }

        void Update()
        {
            aliveEnemies.RemoveAll(e => e == null);

            if (aliveEnemies.Count == 0 &&
                !waitingForNextWave)
            {
                waitingForNextWave = true;

                Invoke(nameof(StartWave),
                    delayBetweenWaves);
            }
        }

        void StartWave()
        {
            waitingForNextWave = false;

            Debug.Log("Wave " + currentWave);

            SpawnWave();

            currentWave++;

            threatBudget += threatIncrease;
        }

        void SpawnWave()
        {
            int budget = threatBudget;

            while (budget > 0)
            {
                EnemyEntry enemy =
                    PickEnemy(budget);

                if (enemy == null)
                    break;

                Spawn(enemy);

                budget -= enemy.threatCost;
            }
        }

        EnemyEntry PickEnemy(int budget)
        {
            List<EnemyEntry> valid =
                new();

            foreach (EnemyEntry e in enemyPool)
            {
                if (e.minimumWave <= currentWave &&
                    e.threatCost <= budget)
                {
                    valid.Add(e);
                }
            }

            if (valid.Count == 0)
                return null;

            // Tiny adaptation

            if (meleeKills > rangedKills + 5)
            {
                List<EnemyEntry> ranged =
                    valid.FindAll(
                        x => x.enemyType ==
                        EnemyType.Witness);

                if (ranged.Count > 0)
                {
                    return ranged[
                        Random.Range(
                        0,
                        ranged.Count)];
                }
            }

            return valid[
                Random.Range(
                0,
                valid.Count)];
        }

        void Spawn(EnemyEntry entry)
        {
            Transform point =
                spawnPoints[
                    Random.Range(
                    0,
                    spawnPoints.Length)];

            GameObject enemy =
                Instantiate(
                    entry.prefab,
                    point.position,
                    Quaternion.identity);

            CelestialEnemyAI ai =
                enemy.GetComponent<CelestialEnemyAI>();

            if (ai != null)
                RegisterEnemy(ai);
        }

        public void RegisterEnemy(
            CelestialEnemyAI enemy)
        {
            if (!aliveEnemies.Contains(enemy))
                aliveEnemies.Add(enemy);
        }

        public void UnregisterEnemy(
            CelestialEnemyAI enemy)
        {
            aliveEnemies.Remove(enemy);
        }
    }
}