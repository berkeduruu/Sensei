using UnityEngine;
using UnityEngine.SceneManagement; // Sahne kontrolü için gerekli

public class MenuMusic : MonoBehaviour
{
    public static MenuMusic instance;
    private AudioSource audioSource;

    [Header("Ses Ayarları")]
    public float mainMenuVolume = 0.6f;   // Ana menüdeki ses seviyesi
    public float otherScenesVolume = 0.2f; // Diğer tüm sahnelerdeki (kısık) ses seviyesi
    public float fadeSpeed = 0.5f;        // Geçiş hızı

    private float targetVolume;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        
        targetVolume = mainMenuVolume; // Başlangıç sesi
    }

    // --- KRİTİK KISIM: Sahne Değişimini Takip Etme ---

    void OnEnable()
    {
        // Sahne her yüklendiğinde "OnSceneLoaded" fonksiyonunu çalıştır
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Obje yok olursa takibi bırak (Hata almamak için önemli)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Her yeni sahne açıldığında Unity bu fonksiyonu otomatik çağırır
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eğer yüklenen sahne "MainMenu" ise sesi yükselt, değilse kıs
        if (scene.name == "MainMenu")
        {
            targetVolume = mainMenuVolume;
        }
        else
        {
            targetVolume = otherScenesVolume;
        }
    }

    // --- SES GEÇİŞİ ---

    void Update()
    {
        if (audioSource != null && audioSource.volume != targetVolume)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
        }
    }
}