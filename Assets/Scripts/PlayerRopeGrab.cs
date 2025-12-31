using UnityEngine;

public class PlayerRopeGrab : MonoBehaviour
{
    [Header("Ayarlar")]
    public string ropeTag = "Rope"; // İp parçalarının Tag'i
    public float jumpOffForce = 10f; // İpten atlarken ne kadar zıplasın

    [Header("Referanslar")]
    public HingeJoint2D playerJoint; // Karakterdeki HingeJoint2D
    private Rigidbody2D rb;
    private bool isAttached = false; // Şu an ipe tutunuyor muyuz?

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Eğer inspector'dan atanmadıysa otomatik bulmaya çalış
        if (playerJoint == null)
            playerJoint = GetComponent<HingeJoint2D>();

        // Başlangıçta joint kapalı olmalı
        playerJoint.enabled = false;
    }

    void Update()
    {
        // Eğer tutunuyorsak ve Boşluk (Space) tuşuna basarsak bırakalım
        if (isAttached && Input.GetButtonDown("Jump"))
        {
            DetachFromRope();
        }
    }

    // İpe değdiğimizi algılayan fonksiyon
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Eğer zaten tutunmuyorsak VE değdiğimiz şey "Rope" ise
        if (!isAttached && collision.gameObject.CompareTag(ropeTag))
        {
            // İpin o parçasının Rigidbody'sini al
            Rigidbody2D ropeSegmentRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (ropeSegmentRb != null)
            {
                AttachToRope(ropeSegmentRb);
            }
        }
    }

    // İpe bağlanma işlemi
    void AttachToRope(Rigidbody2D ropeSegment)
    {
        // 1. Joint'i aktif et
        playerJoint.enabled = true;
        
        // 2. Joint'in bağlanacağı cismi (ipin parçasını) belirle
        playerJoint.connectedBody = ropeSegment;
        
        // 3. Durum değişkenini güncelle
        isAttached = true;

        // İsteğe bağlı: Tutunurken karakterin dönmesini engellemek istersen:
        // rb.freezeRotation = true; 
    }

    // İpten ayrılma işlemi
    void DetachFromRope()
    {
        // 1. Joint'i kapat
        playerJoint.enabled = false;
        
        // 2. Bağlantıyı sıfırla
        playerJoint.connectedBody = null;
        
        // 3. Durumu güncelle
        isAttached = false;

        // 4. İpten ayrılırken hafifçe yukarı/ileri fırlat (Daha iyi hissettirir)
        rb.AddForce(Vector2.up * jumpOffForce, ForceMode2D.Impulse);
    }
}