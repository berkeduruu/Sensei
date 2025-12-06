using UnityEngine;

public class SnowEffect : MonoBehaviour
{
    [Header("Kar Ayarları")]
    public float intensity = 50f; // Kar yoğunluğu (parçacık sayısı)
    public float windStrength = 0.5f; // Rüzgar gücü
    public float fallSpeed = 2f; // Düşüş hızı
    
    private ParticleSystem snowParticles;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.VelocityOverLifetimeModule velocity;
    private ParticleSystem.MainModule main;
    
    void Start()
    {
        snowParticles = GetComponent<ParticleSystem>();
        if (snowParticles == null)
        {
            Debug.LogError("SnowEffect: ParticleSystem bulunamadı!");
            return;
        }
        
        emission = snowParticles.emission;
        velocity = snowParticles.velocityOverLifetime;
        main = snowParticles.main;
        
        // Başlangıç ayarlarını uygula
        UpdateSnowSettings();
    }
    
    void UpdateSnowSettings()
    {
        if (snowParticles == null) return;
        
        // Yoğunluk
        emission.rateOverTime = intensity;
        
        // Rüzgar
        var velocityX = velocity.x;
        velocityX.constantMin = -windStrength;
        velocityX.constantMax = windStrength;
        velocity.x = velocityX;
        
        // Düşüş hızı
        main.startSpeed = fallSpeed;
    }
    
    // Inspector'dan değişiklik yapıldığında otomatik güncelle
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateSnowSettings();
        }
    }
    
    // Kamerayı takip et (opsiyonel)
    void LateUpdate()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Kameranın görüş alanının üstünde kal
            float cameraHeight = 15f;
            if (mainCam.orthographic)
            {
                cameraHeight = mainCam.orthographicSize * 2f + 5f;
            }
            
            transform.position = new Vector3(
                mainCam.transform.position.x,
                mainCam.transform.position.y + cameraHeight,
                mainCam.transform.position.z
            );
        }
    }
    
    void OnEnable()
    {
        // Obje aktif olduğunda particle system'i başlat
        if (snowParticles != null && !snowParticles.isPlaying)
        {
            snowParticles.Play();
        }
    }
}

