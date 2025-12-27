using UnityEngine;
using UnityEngine.SceneManagement; // Sahne i�lemleri i�in gerekli k�t�phane

public class MainMenuManager : MonoBehaviour
{
    // OYNA (Start) Butonu buna basacak
    public void PlayGame()
    {
        // S�radaki sahneye ge� (Level 1'e)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        LoadingSceneManager.LoadScene(nextSceneIndex);
    }

    // �IKI� (Quit) Butonu buna basacak
    public void QuitGame()
    {
        Debug.Log("Oyundan ��k�ld�!"); // Edit�rde �al��t���n� g�rmek i�in mesaj
        Application.Quit();
    }
}