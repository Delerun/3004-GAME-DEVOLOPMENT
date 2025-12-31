using UnityEngine;

public class PlayerRopeMechanic : MonoBehaviour
{
    [Header("Ayarlar")]
    public string ropeTag = "Rope";      // İp parçalarının Tag'i
    public float jumpOffForce = 15f;     // İpten atlarken fırlatma gücü
    public float swingForce = 30f;       // İpi sallama gücü (YENİ)

    [Header("Referanslar")]
    public HingeJoint2D playerJoint;     // Karakterdeki HingeJoint2D
    private Rigidbody2D rb;
    private Rigidbody2D currentRopeSegment; // Tutunduğumuz ip parçası
    private bool isAttached = false;     // Şu an ipe tutunuyor muyuz?

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Eğer inspector'dan atanmadıysa otomatik bul
        if (playerJoint == null)
            playerJoint = GetComponent<HingeJoint2D>();

        // Başlangıçta joint kapalı olmalı
        playerJoint.enabled = false;
    }

    void Update()
    {
        if (isAttached)
        {
            // İpe asılıyken Zıplama tuşuna basılırsa bırak
            if (Input.GetButtonDown("Jump"))
            {
                DetachFromRope();
            }

            // İpe asılıyken Yön Tuşları ile sallanma gücü uygula (YENİ KISIM)
            SwingControl();
        }
    }

    void SwingControl()
    {
        // Yatay girdiyi al (A/D veya Sol/Sağ Ok)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Eğer tuşa basılıyorsa ve bir ipe bağlıysak
        if (Mathf.Abs(horizontalInput) > 0.1f && currentRopeSegment != null)
        {
            // İp parçasına yatay kuvvet uygula
            // ForceMode2D.Force sürekli itme sağlar, sallanma için idealdir
            currentRopeSegment.AddForce(Vector2.right * horizontalInput * swingForce * Time.deltaTime * 100f, ForceMode2D.Force);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Tutunmuyorsak ve çarptığımız şey ip ise
        if (!isAttached && collision.gameObject.CompareTag(ropeTag))
        {
            Rigidbody2D ropeSegmentRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (ropeSegmentRb != null)
            {
                AttachToRope(ropeSegmentRb);
            }
        }
    }

    void AttachToRope(Rigidbody2D ropeSegment)
    {
        // 1. Joint'i aktif et ve bağla
        playerJoint.enabled = true;
        playerJoint.connectedBody = ropeSegment;
        
        // 2. Referansları kaydet
        currentRopeSegment = ropeSegment;
        isAttached = true;
    }

    void DetachFromRope()
    {
        // 1. Joint'i kapat
        playerJoint.enabled = false;
        playerJoint.connectedBody = null;
        
        // 2. Referansları temizle
        currentRopeSegment = null;
        isAttached = false;

        // 3. İpten ayrılırken zıplama yönüne doğru fırlat
        // Karakterin baktığı yöne veya tuşa göre de ayarlanabilir ama şimdilik yukarı/ileri
        Vector2 jumpDirection = Vector2.up + (Vector2.right * Input.GetAxis("Horizontal") * 0.5f);
        rb.AddForce(jumpDirection.normalized * jumpOffForce, ForceMode2D.Impulse);
    }
}