using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Hasar Ayarları")]
    public float damage = 10f;
    public string[] targetTags = { "Player", "Boss" }; // Hangi tag'lere hasar verir
    public float attackCooldown = 0.3f; // Aynı hedefe tekrar hasar verme süresi (daha kısa)
    
    [Header("Hitbox Ayarları")]
    public bool destroyOnHit = false; // Vurduktan sonra yok olsun mu (projecile için)
    public bool isTrigger = true; // Collider trigger mı?
    
    private float lastHitTime;
    private GameObject lastHitTarget;
    
    void Start()
    {
        // Collider'ı trigger yap (eğer yoksa ekle)
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            // CapsuleCollider2D ekle
            col = gameObject.AddComponent<CapsuleCollider2D>();
        }
        col.isTrigger = isTrigger;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Sürekli temas halinde de hasar vermeyi dene (daha güvenilir)
        TryDealDamage(other);
    }
    
    void TryDealDamage(Collider2D other)
    {
        // Tag kontrolü önce (hızlı çıkış)
        bool canDamage = false;
        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                canDamage = true;
                break;
            }
        }
        
        if (!canDamage) return;
        
        // HealthSystem'i bul
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health == null || health.IsDead())
        {
            return;
        }
        
        // Cooldown kontrolü (hedef bazlı)
        string targetID = other.gameObject.GetInstanceID().ToString();
        if (lastHitTarget != null && lastHitTarget == other.gameObject)
        {
            if (Time.time < lastHitTime + attackCooldown)
            {
                return; // Cooldown aktif
            }
        }
        
        // Hasar ver
        health.TakeDamage(damage);
        lastHitTime = Time.time;
        lastHitTarget = other.gameObject;
        
        Debug.Log($"✅ {gameObject.name} {other.gameObject.name}'e {damage} hasar verdi! (Kalan: {health.currentHealth}/{health.maxHealth})");
        
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
    
    // Manuel hasar verme (script'ten çağrılabilir)
    public void DealDamage(GameObject target)
    {
        HealthSystem health = target.GetComponent<HealthSystem>();
        if (health != null && !health.IsDead())
        {
            health.TakeDamage(damage);
        }
    }
}

