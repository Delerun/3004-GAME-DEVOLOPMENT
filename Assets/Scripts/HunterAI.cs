using UnityEngine;



public class HunterAI : MonoBehaviour
{
    public float followSpeed = 5.0f;
    public Transform targetMonster;
    public float stoppingDistance = 1.0f;
    public float jumpForce = 300f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetMonster == null)
        {
            Debug.LogError("HunterAI: Target Monster is not assigned!");
        }
    }

    void FixedUpdate()
    {
        if (targetMonster == null || rb == null) return;

        float distance = targetMonster.position.x - transform.position.x;
        float horizontalSpeed = 0f;

        if (Mathf.Abs(distance) > stoppingDistance)
        {
            int direction = distance > 0 ? 1 : -1;

            horizontalSpeed = direction * followSpeed;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction < 0;
            }
        }

        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);

        if (targetMonster.position.y - transform.position.y > stoppingDistance * 2 && rb.linearVelocity.y < 0.1f)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }
}