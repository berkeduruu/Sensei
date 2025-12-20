using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("Loading Ayarları")]
    public string hedefSahneAdi; // Yüklenecek sahne adı (PlayerPrefs'ten alınacak)
    public float minimumLoadingSuresi = 2f; // Minimum loading süresi (saniye)
    
    [Header("UI Elementleri (Opsiyonel)")]
    public Slider loadingBar; // Loading bar (varsa)
    public Text loadingText; // Loading yazısı (varsa)
    public Image loadingImage; // Loading görseli (varsa)
    
    [Header("Loading Animasyonu (Opsiyonel)")]
    public bool useLoadingAnimation = false; // Loading animasyonu kullanılsın mı?
    public float animationSpeed = 1f; // Animasyon hızı
    
    private AsyncOperation asyncOperation;
    private float loadingProgress = 0f;
    private float loadingTimer = 0f;
    
    void Start()
    {
        // Hedef sahne adını veya index'ini PlayerPrefs'ten al
        if (string.IsNullOrEmpty(hedefSahneAdi))
        {
            // Önce string olarak sahne adını kontrol et
            hedefSahneAdi = PlayerPrefs.GetString("NextScene", "");
            
            // Eğer string boşsa, build index'i kontrol et
            if (string.IsNullOrEmpty(hedefSahneAdi))
            {
                int sahneIndex = PlayerPrefs.GetInt("NextSceneIndex", -1);
                if (sahneIndex >= 0 && sahneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    // Build index'ten sahne adını al
                    string path = SceneUtility.GetScenePathByBuildIndex(sahneIndex);
                    hedefSahneAdi = System.IO.Path.GetFileNameWithoutExtension(path);
                }
                else
                {
                    hedefSahneAdi = "Level1"; // Varsayılan
                }
            }
        }
        
        // Loading bar'ı başlangıçta 0 yap ve ayarları kontrol et
        if (loadingBar != null)
        {
            loadingBar.minValue = 0f;
            loadingBar.maxValue = 1f;
            loadingBar.value = 0f;
            Debug.Log("✅ Loading bar hazırlandı");
        }
        else
        {
            Debug.LogWarning("⚠️ Loading bar referansı atanmamış! Inspector'da Loading Bar alanına Slider'ı sürükleyin.");
        }
        
        // Loading text'i güncelle
        if (loadingText != null)
        {
            loadingText.text = "Yükleniyor...";
        }
        
        // Sahneyi asenkron olarak yüklemeye başla
        StartCoroutine(LoadSceneAsync());
    }
    
    IEnumerator LoadSceneAsync()
    {
        // Sahneyi asenkron olarak yükle (ama henüz aktif etme)
        asyncOperation = SceneManager.LoadSceneAsync(hedefSahneAdi);
        asyncOperation.allowSceneActivation = false; // Sahneyi hemen aktif etme
        
        // Minimum süre ve loading tamamlanana kadar bekle
        while (loadingTimer < minimumLoadingSuresi || asyncOperation.progress < 0.9f)
        {
            loadingTimer += Time.deltaTime;
            
            // Loading progress'i hesapla (0-1 arası)
            // asyncOperation.progress 0-0.9 arası gider, 0.9'da durur
            // allowSceneActivation false olduğu için 1.0'a gitmez
            loadingProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            
            // Minimum süre dolmadan progress'i yavaşça artır
            if (loadingTimer < minimumLoadingSuresi)
            {
                float timeProgress = loadingTimer / minimumLoadingSuresi;
                // Gerçek loading progress ile zaman progress'ini birleştir
                loadingProgress = Mathf.Max(loadingProgress, timeProgress);
            }
            
            // Loading bar'ı güncelle
            if (loadingBar != null)
            {
                loadingBar.value = loadingProgress;
                // Debug (her 0.1 saniyede bir log)
                if (Time.frameCount % 10 == 0)
                {
                    Debug.Log($"Loading Progress: {loadingProgress * 100f:F1}% (Timer: {loadingTimer:F2}s)");
                }
            }
            
            // Loading text'i güncelle
            if (loadingText != null)
            {
                int percentage = Mathf.RoundToInt(loadingProgress * 100f);
                loadingText.text = $"Yükleniyor... %{percentage}";
            }
            
            // Loading animasyonu (opsiyonel)
            if (useLoadingAnimation && loadingImage != null)
            {
                loadingImage.transform.Rotate(0, 0, -animationSpeed * Time.deltaTime * 360f);
            }
            
            yield return null;
        }
        
        // Minimum süre ve loading tamamlandı, loading bar'ı %100 yap
        if (loadingBar != null)
        {
            loadingBar.value = 1f;
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Yükleniyor... %100";
        }
        
        // Kısa bir bekleme (smooth geçiş için)
        yield return new WaitForSeconds(0.2f);
        
        // Sahneyi aktif et
        asyncOperation.allowSceneActivation = true;
    }
    
    // Bu metot diğer scriptlerden çağrılacak (sahne geçişi için)
    public static void LoadScene(string sahneAdi)
    {
        // Hedef sahne adını PlayerPrefs'e kaydet
        PlayerPrefs.SetString("NextScene", sahneAdi);
        PlayerPrefs.Save();
        
        // Loading scene'e geç (Loading scene'in adını buraya yazın)
        SceneManager.LoadScene("LoadingScene");
    }
    
    // Build index ile sahne yükleme
    public static void LoadScene(int sahneIndex)
    {
        // Build index'i PlayerPrefs'e kaydet (LoadingSceneManager bunu okuyacak)
        PlayerPrefs.SetInt("NextSceneIndex", sahneIndex);
        PlayerPrefs.SetString("NextScene", ""); // String'i temizle
        PlayerPrefs.Save();
        
        // Loading scene'e geç
        SceneManager.LoadScene("LoadingScene");
    }
}

