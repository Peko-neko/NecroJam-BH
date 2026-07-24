using UnityEngine;

public class SanityTester : MonoBehaviour
{
    [SerializeField] private SanitySystem sanity;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            sanity.Lose(10);

        if (Input.GetKeyDown(KeyCode.E))
            sanity.Gain(10);

        if (Input.GetKeyDown(KeyCode.Space))
            sanity.Spend(25);
    }
}