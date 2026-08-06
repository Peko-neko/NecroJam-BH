using UnityEngine;

    public enum EnemyActionType
    {
        MoveTo,
        Wait,
        Fire
    }

    [System.Serializable]
    public class EnemyAction
    {
        public EnemyActionType action;

    [Header("Move")]
    public Transform target;
    public float moveDuration = 1f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Wait")]
        public float waitTime = 1f;
    }