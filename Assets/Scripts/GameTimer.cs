using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Ayarlar")]
    public TextMeshProUGUI timerText; 
    private float timeElapsed;
    private bool timerIsRunning = true;

    void Start()
    {
        timeElapsed = 0f;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            timeElapsed += Time.deltaTime;
            DisplayTime(timeElapsed);
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        // Dakika hesabı
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        
        // Saniye hesabı
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        
        // Salise hesabı (Saniyenin 100'de 1'i)
        float milliseconds = (timeToDisplay % 1) * 100;

        // Format: 00:00:00 (Dakika : Saniye : Salise)
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }
}