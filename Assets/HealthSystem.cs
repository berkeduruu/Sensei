using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Ölüm Ayarları")]
    public bool destroyOnDeath = true; // Player için false yap, Boss için true
    public float destroyDelay = 3f; // Ölme animasyonu için yeterli süre (3 saniye)
    
    [Header("Events")]
    public UnityEvent<float> OnHealthChanged; // (currentHealth)
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken; // (damage)
    
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        float healthBefore = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Negatif olamaz
        
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"{gameObject.name} hasar aldı: {damage} (Kalan can: {currentHealth}/{maxHealth})");
        
        // Sadece can 0'a düştüyse öl (hemen ölme, önce hasar al)
        if (currentHealth <= 0 && healthBefore > 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // Max'i aşamaz
        
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"{gameObject.name} öldü!");
        
        OnDeath?.Invoke();
        
        // destroyOnDeath true ise objeyi yok et
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}

