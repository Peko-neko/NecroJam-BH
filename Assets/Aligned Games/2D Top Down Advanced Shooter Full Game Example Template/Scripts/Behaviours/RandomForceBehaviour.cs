using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AlignedGames
{
    public class RandomForceBehaviour : MonoBehaviour
    {
        // Movement speed in units per second
        public float moveSpeed;

        // Lifetime in seconds before this script destroys itself
        public float lifetime;

        // Direction of movement (randomized)
        private Vector2 moveDirection;

        void Start()
        {
            // Choose a random normalized direction inside a circle
            moveDirection = Random.insideUnitCircle.normalized;

            // Start coroutine to remove this script after 'lifetime' seconds
            StartCoroutine(RemoveScriptAfterTime());
        }

        void Update()
        {
            // Move the transform in the chosen direction at moveSpeed
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        }

        // Coroutine to destroy this component after lifetime seconds
        System.Collections.IEnumerator RemoveScriptAfterTime()
        {
            yield return new WaitForSeconds(lifetime);
            Destroy(this);
        }
    }
}
