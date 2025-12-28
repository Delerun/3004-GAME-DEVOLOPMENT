using UnityEngine;
using TMPro;

public class HunterAI : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float followSpeed = 5.0f;
    public Transform targetMonster; // Oyuncu (Transform)
    public float stoppingDistance = 1.0f;
    public float jumpForce = 300f;

    [Header("Geri Sayım Ayarı")]
    public float startDelay = 3.0f;
    public TextMeshProUGUI countdownText;

    [Header("Takılma Kurtarıcı (Anti-Stuck)")]
    public float stuckThreshold = 2.0f; // Kaç saniye takılırsa ışınlansın?
    public float teleportOffset = 3.0f; // Oyuncunun ne kadar arkasına ışınlansın?

    private Rigidbody2D rb; // Hunter'ın RB'si
    private Rigidbody2D targetRb; // Oyuncunun RB'si (Hızını okumak için lazım)
    private SpriteRenderer spriteRenderer;
    
    // Değişkenler
    private float currentDelayTimer;
    private float stuckTimer;
    private Vector2 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentDelayTimer = startDelay;
        lastPosition = transform.position;

        if (targetMonster == null) 
        {
            Debug.LogError("HunterAI: Target Monster atanmamış!");
        }
        else
        {
            // DÜZELTME BURADA: Oyuncunun Rigidbody'sini alıyoruz
            targetRb = targetMonster.GetComponent<Rigidbody2D>();
        }
    }

    void FixedUpdate()
    {
        if (targetMonster == null || rb == null) return;

        // --- 1. GERİ SAYIM BEKLEMESİ ---
        if (currentDelayTimer > 0)
        {
            currentDelayTimer -= Time.deltaTime;

            if (countdownText != null)
                countdownText.text = Mathf.Ceil(currentDelayTimer).ToString("0");

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        else
        {
            if (countdownText != null && countdownText.text != "") countdownText.text = "";
        }

        // --- 2. TAKILMA KONTROLÜ ---
        // Hunter çok az hareket ettiyse (0.05 birimden az)
        if (Vector2.Distance(transform.position, lastPosition) < 0.05f)
        {
            stuckTimer += Time.deltaTime; 

            if (stuckTimer >= stuckThreshold)
            {
                TeleportBehindPlayer(); 
                stuckTimer = 0; 
            }
        }
        else
        {
            stuckTimer = 0;
            lastPosition = transform.position;
        }

        // --- 3. KOVALAMA HAREKETİ ---
        float distanceToPlayer = targetMonster.position.x - transform.position.x;
        float horizontalSpeed = 0f;

        if (Mathf.Abs(distanceToPlayer) > stoppingDistance)
        {
            int direction = distanceToPlayer > 0 ? 1 : -1;
            horizontalSpeed = direction * followSpeed;

            if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
        }

        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);

        // Zıplama Kontrolü
        if (targetMonster.position.y - transform.position.y > stoppingDistance * 2 && rb.linearVelocity.y < 0.1f)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    // --- IŞINLANMA FONKSİYONU ---
    void TeleportBehindPlayer()
    {
        // Eğer oyuncunun Rigidbody'si bulunamadıysa ışınlanma çalışmasın (Hata vermesin)
        if (targetRb == null) return;

        // DÜZELTME BURADA: Artık Transform değil, yukarıda tanımladığımız targetRb'yi kullanıyoruz
        float offsetDir = (targetRb.linearVelocity.x > 0) ? -1 : 1;
        
        // Eğer oyuncu duruyorsa (Hızı neredeyse 0 ise)
        if (Mathf.Abs(targetRb.linearVelocity.x) < 0.1f)
        {
            // Oyuncunun sağına mı soluna mı geçeceğini pozisyona göre belirle
            offsetDir = (targetMonster.position.x > transform.position.x) ? -1 : 1;
        }

        // Yeni pozisyonu hesapla
        Vector2 newPos = new Vector2(
            targetMonster.position.x + (offsetDir * teleportOffset), 
            targetMonster.position.y + 1f 
        );

        transform.position = newPos;
        Debug.Log("Hunter takıldı ve oyuncunun arkasına ışınlandı!");
    }
}