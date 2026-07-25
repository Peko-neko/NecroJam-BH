using UnityEngine;

namespace AlignedGames
{
    [System.Serializable]
    public class EnemyEntry
    {
        public string enemyName;

        public EnemyType enemyType;

        public GameObject prefab;

        public int threatCost = 2;

        public int minimumWave = 1;
    }
}