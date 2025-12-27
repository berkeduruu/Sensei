using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panelleri")]
    public GameObject deathPanel; // Hazırladığın DeathPanel prefabını buraya sürükle

    public void ShowGameOver()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true); // Paneli aç
            Time.timeScale = 0f; // Oyunu durdur
        }
    }

    // Butonlar için fonksiyonlar
    public void RestartGame()
    {
        Time.timeScale = 1f; // Zamanı geri al (Önemli!)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
