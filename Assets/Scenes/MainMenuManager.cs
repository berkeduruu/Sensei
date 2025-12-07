using UnityEngine;
using UnityEngine.SceneManagement; // Sahne iþlemleri için gerekli kütüphane

public class MainMenuManager : MonoBehaviour
{
    // OYNA (Start) Butonu buna basacak
    public void PlayGame()
    {
        // Sýradaki sahneye geç (Level 1'e)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ÇIKIÞ (Quit) Butonu buna basacak
    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýldý!"); // Editörde çalýþtýðýný görmek için mesaj
        Application.Quit();
    }
}