using UnityEngine;

public class SpeedCheck : MonoBehaviour
{
    public Rigidbody2D rb;

    [Range(-50f, 50f)]
    public float currentFallSpeed; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        currentFallSpeed = rb.velocity.y;
    }
}
