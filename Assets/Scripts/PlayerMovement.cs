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

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public TrailRenderer tr; 

    [Header("Rope Climbing Settings (YENİ)")]
    public float climbSpeed = 3f; // İpte yukarı/aşağı kayma hızı
    public float swingPushForce = 50f; // İpi sallamak için uygulanan güç

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
    private Animator animator;
    
    // --- İP SİSTEMİ DEĞİŞKENLERİ ---
    private bool isAttachedToRope = false;
    private Rigidbody2D currentRopeRB; // Tutunduğumuz ipin fiziği
    private GameObject currentRopeObj; // Tutunduğumuz ip objesi
    private bool canAttach = false; // İpe yakın mıyız?
    private GameObject potentialRope; // Yakındaki ip
    // -------------------------------

    // State Variables
    private bool isGrounded;
    private bool isTouchingWall;
    private int wallDirection;
    private float wallJumpTimer;

    // Action States
    private bool isDashing;
    private bool canDash = true;

    // Input System
    private PlayerControls inputActions;

    private void Awake()
    {
        inputActions = new PlayerControls();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Dash izi
        if (tr == null) tr = GetComponent<TrailRenderer>();

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

        // --- DASH GİRİŞİ ---
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && canDash && !isAttachedToRope)
        {
            StartCoroutine(Dash());
        }

        // --- İPE TUTUNMA GİRİŞİ (E TUŞU) ---
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isAttachedToRope)
            {
                DetachFromRope(); // Zaten tutunuyorsak bırak
            }
            else if (canAttach && potentialRope != null)
            {
                AttachToRope(potentialRope); // Yakındaysak tutun
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // --- EĞER İPE TUTUNUYORSAK ---
        if (isAttachedToRope)
        {
            HandleRopeMovement();
            return; // Normal hareket kodlarını çalıştırma
        }

        // --- NORMAL HAREKET ---
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, 0.2f, groundLayer);

        if (wallCheckPoint != null)
        {
            isTouchingWall = Physics2D.OverlapCircle(wallCheckPoint.position, 0.2f, wallLayer);
            if (isTouchingWall)
            {
                wallDirection = (wallCheckPoint.position.x > transform.position.x) ? 1 : -1;
            }
        }

        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;

        float horizontalInput = inputActions.Player.Move.ReadValue<Vector2>().x;

        if (wallJumpTimer > 0) horizontalInput = 0f;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Animasyon
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        // Karakteri Çevir
        if (spriteRenderer != null && horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }

    // --- İP MANTIĞI ---

    void AttachToRope(GameObject rope)
    {
        isAttachedToRope = true;
        currentRopeObj = rope;
        currentRopeRB = rope.GetComponent<Rigidbody2D>();

        // Fiziği kapat (İpin hareketine uyacağız)
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;

        // Karakteri ipin içine (Child) al
        transform.SetParent(rope.transform);
        
        // Karakteri ipin ortasına hizala (X ekseninde)
        Vector3 localPos = transform.localPosition;
        localPos.x = 0; 
        transform.localPosition = localPos;
    }

    void DetachFromRope()
    {
        isAttachedToRope = false;
        
        // Parent'lıktan çık
        transform.SetParent(null);
        
        // Fiziği geri aç
        rb.isKinematic = false;

        // İpten ayrılırken ipin hızıyla fırlat (Momentum)
        if (currentRopeRB != null)
        {
            rb.linearVelocity = currentRopeRB.linearVelocity;
        }

        currentRopeObj = null;
        currentRopeRB = null;
    }

    void HandleRopeMovement()
    {
        // Girdileri al
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        // A ve D ile İpi Salla (Gücü ipe uyguluyoruz)
        if (input.x != 0 && currentRopeRB != null)
        {
            currentRopeRB.AddForce(new Vector2(input.x * swingPushForce, 0));
        }

        // W ve S ile İpte Kay (Tırmanma)
        if (input.y != 0)
        {
            transform.Translate(new Vector3(0, input.y * climbSpeed * Time.deltaTime, 0));
        }
        
        // Karakterin rotasyonunu düzelt
        transform.rotation = Quaternion.identity;
    }

    // --- ZIPLAMA ---
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isDashing) return;

        // 1. İpten Atlayarak Ayrılma
        if (isAttachedToRope)
        {
            DetachFromRope();
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            return;
        }

        // 2. Normal Zıplama
        if (isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        // 3. Wall Jump
        else if (isTouchingWall && !isGrounded && wallJumpTimer <= 0)
        {
            wallJumpTimer = wallJumpCooldown;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            Vector2 jumpVector = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
            rb.AddForce(jumpVector, ForceMode2D.Impulse);
        }
    }

    // --- TRIGGER (İpi Algılama) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Rope" tag'ine sahip bir objeye girdik mi?
        if (other.CompareTag("Rope"))
        {
            canAttach = true;
            potentialRope = other.gameObject;
        }

        if (((1 << other.gameObject.layer) & goalLayer) != 0) GameWin();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Rope"))
        {
            canAttach = false;
            potentialRope = null;
        }
    }

    // --- DASH (Aynı Kaldı) ---
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float direction = spriteRenderer.flipX ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        if (tr != null) tr.emitting = true;

        yield return new WaitForSeconds(dashDuration);

        if (tr != null) tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        RestartGameInstant();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & hunterLayer) != 0) GameOver();
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