using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Bayrağa dokunulduğunda çalışır
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NextLevel();
        }
    }

    public void NextLevel()
    {
        // Mevcut sahnenin index numarasını al ve 1 ekle
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Eğer sonraki sahne listede varsa yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Loading scene üzerinden sonraki sahneye geç
            LoadingSceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Oyun Bitti! Tebrikler.");
            // Oyun bittiyse menüye dön (loading scene ile)
            LoadingSceneManager.LoadScene("MainMenu");
        }
    }
} 