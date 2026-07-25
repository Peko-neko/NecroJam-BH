using UnityEngine;

namespace AlignedGames
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CelestialEnemyAI : MonoBehaviour
    {
        [Header("Identity")]
        public EnemyType enemyType;

        [Header("Movement")]
        public MovementType movement;

        public float moveSpeed = 3f;

        [Tooltip("How quickly the enemy reaches its desired velocity.")]
        public float acceleration = 8f;

        public float preferredDistance = 4f;

        public float orbitSpeed = 60f;

        [Header("Aggro")]
        public float aggroDistance = 12f;

        [Header("Steering")]
        public LayerMask enemyLayer;

        public float separationRadius = 1.5f;

        public float separationWeight = 2.5f;

        public float movementWeight = 1f;

        Rigidbody2D rb;

        Transform player;

        float orbitAngle;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;

            orbitAngle = Random.Range(0f, 360f);
        }

        void FixedUpdate()
        {
            if (player == null)
                return;

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            if (distance > aggroDistance)
            {
                rb.linearVelocity = Vector2.Lerp(
                    rb.linearVelocity,
                    Vector2.zero,
                    acceleration * Time.fixedDeltaTime);

                return;
            }

            Vector2 target =
                GetMovementTarget();

            Vector2 desiredDirection =
                (target - rb.position);

            if (desiredDirection.sqrMagnitude > 0.001f)
                desiredDirection.Normalize();

            Vector2 separation =
                GetSeparation();

            Vector2 steering =
                desiredDirection * movementWeight +
                separation * separationWeight;

            if (steering.sqrMagnitude > 0.001f)
                steering.Normalize();

            Vector2 desiredVelocity =
                steering * moveSpeed;

            rb.linearVelocity =
                Vector2.Lerp(
                    rb.linearVelocity,
                    desiredVelocity,
                    acceleration * Time.fixedDeltaTime);

            RotateTowardsPlayer();
        }

        Vector2 GetMovementTarget()
        {
            switch (movement)
            {
                case MovementType.Chase:

                    return player.position;

                case MovementType.Orbit:

                    orbitAngle +=
                        orbitSpeed * Time.fixedDeltaTime;

                    Vector2 orbit =
                        new Vector2(
                            Mathf.Cos(orbitAngle * Mathf.Deg2Rad),
                            Mathf.Sin(orbitAngle * Mathf.Deg2Rad));

                    return (Vector2)player.position +
                           orbit * preferredDistance;

                case MovementType.KeepDistance:

                    Vector2 away =
                        ((Vector2)transform.position -
                        (Vector2)player.position).normalized;

                    return (Vector2)player.position +
                           away * preferredDistance;

                case MovementType.Random:

                    return rb.position +
                           Random.insideUnitCircle * 2f;

                default:

                    return rb.position;
            }
        }

        Vector2 GetSeparation()
        {
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    transform.position,
                    separationRadius,
                    enemyLayer);

            Vector2 force = Vector2.zero;

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;

                Vector2 offset =
                    rb.position -
                    (Vector2)hit.transform.position;

                float distance =
                    offset.magnitude;

                if (distance < 0.001f)
                    continue;

                float strength =
                    (separationRadius - distance) /
                    separationRadius;

                force +=
                    offset.normalized *
                    strength;
            }

            return force;
        }

        void RotateTowardsPlayer()
        {
            Vector2 dir =
                player.position -
                transform.position;

            float angle =
                Mathf.Atan2(
                    dir.y,
                    dir.x) *
                Mathf.Rad2Deg;

            rb.rotation = angle;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                transform.position,
                aggroDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                transform.position,
                separationRadius);
        }
    }
}