using UnityEngine;

namespace AlignedGames
{
    public class ArenaTrigger : MonoBehaviour
    {
        public int arenaIndex;
        bool used;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used)
                return;

            if (!other.CompareTag("Player"))
                return;

            used = true;

            GameManagerEndless.Instance.StartArena(arenaIndex);

            gameObject.SetActive(false);
        }
    }
}