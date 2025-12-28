using UnityEngine;
using TMPro; // TextMeshPro için

public class Coin : MonoBehaviour
{
    [Header("Ayarlar")]
    public int scoreValue = 10; // Her altın kaç puan?

    // Statik değişken: Tüm coinler aynı skoru artırır
    // Bu basit bir yöntemdir, karmaşık GameManager gerektirmez.
    public static int totalScore = 0; 
    public static TextMeshProUGUI scoreText; // Skoru yazdıracağımız yer

    private void Start()
    {
        // Eğer sahnede "ScoreText" adında bir UI bulursa ona bağlanır
        if (scoreText == null)
        {
            GameObject textObj = GameObject.Find("ScoreText");
            if (textObj != null)
                scoreText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncu çarptıysa
        if (other.gameObject.CompareTag("Player"))
        {
            AddScore();
            Destroy(gameObject); // Altını yok et
        }
    }

    void AddScore()
    {
        totalScore += scoreValue; // Puan ekle
        Debug.Log("Skor: " + totalScore);

        // UI Güncelle
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore.ToString();
        }
    }
}