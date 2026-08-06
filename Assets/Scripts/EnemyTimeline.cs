using UnityEngine;

    public class EnemyTimeline : MonoBehaviour
    {
        [Header("Timeline")]
        [SerializeField] private bool loop = true;
        [SerializeField] private EnemyAction[] actions;

        [Header("Settings")]
        [SerializeField] private float arriveDistance = 0.05f;

        private EnemyAttack enemyAttack;

        private int currentAction = 0;

        private float waitTimer;

    private Vector3 moveStartPosition;
    private float moveTimer;
    private bool startedMove;

    private void Awake()
        {
            enemyAttack = GetComponent<EnemyAttack>();
        }

        private void Update()
        {
            if (actions.Length == 0)
                return;

            EnemyAction action = actions[currentAction];

            switch (action.action)
            {
                case EnemyActionType.MoveTo:
                    UpdateMove(action);
                    break;

                case EnemyActionType.Wait:
                    UpdateWait(action);
                    break;

                case EnemyActionType.Fire:
                    enemyAttack.Fire();
                    NextAction();
                    break;
            }
        }

    private void UpdateMove(EnemyAction action)
    {
        if (action.target == null)
        {
            NextAction();
            return;
        }

        if (!startedMove)
        {
            startedMove = true;
            moveTimer = 0f;
            moveStartPosition = transform.position;
        }

        moveTimer += Time.deltaTime;

        float t = Mathf.Clamp01(moveTimer / action.moveDuration);
        float eased = action.moveCurve.Evaluate(t);

        transform.position = Vector3.Lerp(
            moveStartPosition,
            action.target.position,
            eased);

        if (t >= 1f)
        {
            transform.position = action.target.position;
            startedMove = false;
            NextAction();
        }
    }

    private void UpdateWait(EnemyAction action)
        {
            if (waitTimer <= 0f)
                waitTimer = action.waitTime;

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waitTimer = 0f;
                NextAction();
            }
        }

    private void NextAction()
    {
        startedMove = false;
        moveTimer = 0f;
        waitTimer = 0f;

        currentAction++;

        if (currentAction >= actions.Length)
        {
            if (loop)
                currentAction = 0;
            else
                enabled = false;
        }
    }
}