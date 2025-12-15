using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float jumpForce = 500f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public LayerMask hunterLayer;
    public GameObject loseTextUI;

    public float wallJumpForceX = 700f;
    public float wallJumpForceY = 500f;
    public Transform wallCheckPoint;
    public LayerMask wallLayer;
    public float wallJumpCooldown = 0.2f;

    public LayerMask goalLayer;
    public GameObject winTextUI;
    public float fallLimitY = -10f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool isTouchingWall;
    private int wallDirection;
    private float wallJumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        wallJumpTimer = 0f;

        if (groundCheckPoint == null)
        {
            Debug.LogError("Ground Check Point is not assigned!");
        }
        if (wallCheckPoint == null)
        {
            Debug.LogError("Wall Check Point is not assigned!");
        }

        if (loseTextUI != null)
        {
            loseTextUI.SetActive(false);
        }
        if (winTextUI != null)
        {
            winTextUI.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (wallCheckPoint != null)
        {
            isTouchingWall = Physics2D.OverlapCircle(wallCheckPoint.position, 0.2f, wallLayer);

            if (isTouchingWall)
            {
                wallDirection = (wallCheckPoint.position.x > transform.position.x) ? 1 : -1;
            }
        }

        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0)
        {
            horizontalInput = 0f;
        }

        Vector2 targetVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;

        if (spriteRenderer != null)
        {
            if (horizontalInput != 0)
            {
                spriteRenderer.flipX = horizontalInput < 0;
            }
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, 0.2f, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isTouchingWall && !isGrounded && wallJumpTimer <= 0)
        {
            wallJumpTimer = wallJumpCooldown;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            Vector2 jumpVector = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
            rb.AddForce(jumpVector, ForceMode2D.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            RestartGameInstant();
        }

        if (transform.position.y < fallLimitY)
        {
            RestartGameInstant();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & hunterLayer) != 0)
        {
            GameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & goalLayer) != 0)
        {
            GameWin();
        }
    }

    void GameOver()
    {
        if (loseTextUI != null)
        {
            loseTextUI.SetActive(true);
        }

        Time.timeScale = 0f;

        StartCoroutine(RestartGameAfterDelay(2f));
    }

    void GameWin()
    {
        if (winTextUI != null)
        {
            winTextUI.SetActive(true);
        }

        Time.timeScale = 0f;

        StartCoroutine(RestartGameAfterDelay(4f));
    }

    void RestartGameInstant()
    {
        if (loseTextUI != null) loseTextUI.SetActive(false);
        if (winTextUI != null) winTextUI.SetActive(false);
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}