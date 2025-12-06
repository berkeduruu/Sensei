using UnityEngine;
using UnityEngine.UI; // UI elementlerini kontrol etmek için şart
using UnityEngine.SceneManagement; // Sahne değiştirmek için şart

public class StoryManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Image ekrandakiGorsel; // Canvas'taki "HikayeGorseli"miz
    public string sonrakiSahneAdi; // Hikaye bitince hangi level açılacak?

    [Header("Hikaye Resimleri")]
    public Sprite[] hikayeKareleri; // Çizdiğin resimleri buraya dizeceğiz

    private int _aktifResimSirasi = 0;

    void Start()
    {
        // Oyun başlar başlamaz ilk resmi gösterelim
        GorseliGuncelle();
    }

    void Update()
    {
        // Fareye tıklandığında veya SPACE tuşuna basıldığında
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            SonrakiResmeGec();
        }
    }

    void SonrakiResmeGec()
    {
        _aktifResimSirasi++; // Sırayı bir artır

        // Eğer daha gösterilecek resim varsa
        if (_aktifResimSirasi < hikayeKareleri.Length)
        {
            GorseliGuncelle();
        }
        else
        {
            // Resimler bitti, oyuna (diğer sahneye) geç
            Debug.Log("Hikaye bitti, oyun sahnesi yükleniyor...");
            SceneManager.LoadScene(sonrakiSahneAdi);
        }
    }

    void GorseliGuncelle()
    {
        ekrandakiGorsel.sprite = hikayeKareleri[_aktifResimSirasi];
    }
}