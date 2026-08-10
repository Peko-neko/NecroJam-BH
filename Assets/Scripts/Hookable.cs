using UnityEngine;

public class Hookable : MonoBehaviour
{
    [SerializeField] private Transform hookPoint;

    public Transform GetHookPoint()
    {
        return hookPoint != null
            ? hookPoint
            : transform;
    }
}