using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement;
using TMPro;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10.0f;
    public float jumpForce = 500f;

    [Header("Ground Detection")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;

    [Header("Wall Jump Settings")]
    public float wallJumpForceX = 700f;
    public float wallJumpForceY = 500f;
    public Transform wallCheckPoint;
    public LayerMask wallLayer;
    public float wallJumpCooldown = 0.2f;

    [Header("Game Elements")]
    public LayerMask hunterLayer;
    public LayerMask goalLayer;
    public GameObject loseTextUI;
    public GameObject winTextUI;
    public float fallLimitY = -10f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // YENİ: Animatör bileşeni
    
    // State Variables
    private bool isGrounded;
    private bool isTouchingWall;
    private int wallDirection;
    private float wallJumpTimer;

    // Input System
    private PlayerControls inputActions;

    private void Awake()
    {
        inputActions = new PlayerControls();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // YENİ: Bileşeni alıyoruz
        wallJumpTimer = 0f;

        if (groundCheckPoint == null) Debug.LogError("Ground Check Point atanmamış!");
        if (wallCheckPoint == null) Debug.LogError("Wall Check Point atanmamış!");

        if (loseTextUI != null) loseTextUI.SetActive(false);
        if (winTextUI != null) winTextUI.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Restart.performed += OnRestartPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Restart.performed -= OnRestartPerformed;
        inputActions.Player.Disable();
    }

    void Update()
    {
        if (transform.position.y < fallLimitY)
        {
            RestartGameInstant();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, 0.2f, groundLayer);

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

        // --- HAREKET ---
        float horizontalInput = inputActions.Player.Move.ReadValue<Vector2>().x;

        if (wallJumpTimer > 0)
        {
            horizontalInput = 0f;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // --- YENİ: ANIMASYON KODU ---
        // Hızımızı (pozitif olarak) Animator'daki "Speed" parametresine gönderiyoruz
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        // Karakteri çevir
        if (spriteRenderer != null && horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Debug.Log("Jump Tuşuna Basıldı! -- Yerde mi: " + isGrounded); 

        if (isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        else if (isTouchingWall && !isGrounded && wallJumpTimer <= 0)
        {
            wallJumpTimer = wallJumpCooldown;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            Vector2 jumpVector = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
            rb.AddForce(jumpVector, ForceMode2D.Impulse);
        }
    }

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        RestartGameInstant();
    }

    // --- Diğer Fonksiyonlar (Aynı) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & hunterLayer) != 0) GameOver();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & goalLayer) != 0) GameWin();
    }

    void GameOver()
    {
        if (loseTextUI != null) loseTextUI.SetActive(true);
        Time.timeScale = 0f;
        StartCoroutine(RestartGameAfterDelay(2f));
    }

    void GameWin()
    {
        if (winTextUI != null) winTextUI.SetActive(true);
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