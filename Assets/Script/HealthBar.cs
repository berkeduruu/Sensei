using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Referansları")]
    public Slider healthSlider;
    public Image fillImage;
    public Text healthText; // Opsiyonel: "100/100" gibi text
    
    [Header("Renk Ayarları")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f; // %30'un altında kırmızı
    
    [Header("Hedef")]
    public HealthSystem targetHealth; // Hangi HealthSystem'i takip edecek
    
    void Start()
    {
        if (targetHealth == null)
        {
            // Eğer atanmamışsa, parent'ta ara
            targetHealth = GetComponentInParent<HealthSystem>();
        }
        
        if (targetHealth != null)
        {
            // Event'lere abone ol
            targetHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            targetHealth.OnDeath.AddListener(OnTargetDeath);
            
            // İlk değeri ayarla
            UpdateHealthBar(targetHealth.currentHealth);
        }
        else
        {
            Debug.LogWarning("HealthBar: HealthSystem bulunamadı!");
        }
        
        // Slider ayarları
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = targetHealth != null ? targetHealth.maxHealth : 100;
        }
    }
    
    void UpdateHealthBar(float currentHealth)
    {
        if (targetHealth == null) return;
        
        float healthPercentage = targetHealth.GetHealthPercentage();
        
        // Slider'ı güncelle (hemen güncelle, animasyon yok)
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        // Renk değiştir
        if (fillImage != null)
        {
            if (healthPercentage <= lowHealthThreshold)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage / lowHealthThreshold);
            }
            else
            {
                fillImage.color = Color.Lerp(fullHealthColor, lowHealthColor, (1f - healthPercentage) / (1f - lowHealthThreshold));
            }
        }
        
        // Text güncelle
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(targetHealth.maxHealth)}";
        }
        
        Debug.Log($"[HealthBar] Can barı güncellendi: {currentHealth}/{targetHealth.maxHealth} ({healthPercentage * 100:F1}%)");
    }
    
    void OnTargetDeath()
    {
        // Ölüm animasyonu veya gizleme
        if (healthSlider != null)
        {
            healthSlider.value = 0;
        }
    }
    
    void OnDestroy()
    {
        // Event'lerden ayrıl
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
            targetHealth.OnDeath.RemoveListener(OnTargetDeath);
        }
    }
}

