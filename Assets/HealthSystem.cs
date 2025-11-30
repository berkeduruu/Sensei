using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Ölüm Ayarları")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 2f;
    
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
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Negatif olamaz
        
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"{gameObject.name} hasar aldı: {damage} (Kalan can: {currentHealth}/{maxHealth})");
        
        if (currentHealth <= 0)
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

