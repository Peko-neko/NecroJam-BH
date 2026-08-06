using UnityEngine;

    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifeTime = 10f;

        private Vector2 direction;

        public void Fire(Vector2 dir)
        {
            direction = dir.normalized;

            Destroy(gameObject, lifeTime);
        }

    private void Update()
        {
            transform.position +=
                (Vector3)(direction * speed * Time.deltaTime);
        }
    }