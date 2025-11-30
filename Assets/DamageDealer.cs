using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Hasar Ayarları")]
    public float damage = 10f;
    public string[] targetTags = { "Player", "Boss" }; // Hangi tag'lere hasar verir
    public float attackCooldown = 0.5f; // Aynı hedefe tekrar hasar verme süresi
    
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
        Debug.Log($"[DamageDealer] {gameObject.name} trigger'a girdi: {other.gameObject.name} (Tag: {other.tag})");
        
        // Cooldown kontrolü
        if (Time.time < lastHitTime + attackCooldown && other.gameObject == lastHitTarget)
        {
            Debug.Log($"[DamageDealer] Cooldown aktif, hasar verilemiyor.");
            return;
        }
        
        // Tag kontrolü
        bool canDamage = false;
        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                canDamage = true;
                Debug.Log($"[DamageDealer] Tag eşleşti: {tag}");
                break;
            }
        }
        
        if (!canDamage)
        {
            Debug.Log($"[DamageDealer] Tag eşleşmedi. Hedef tag: {other.tag}, İstenen tag'ler: {string.Join(", ", targetTags)}");
            return;
        }
        
        // HealthSystem'i bul ve hasar ver
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health == null)
        {
            Debug.LogWarning($"[DamageDealer] {other.gameObject.name} üzerinde HealthSystem bulunamadı!");
            return;
        }
        
        if (health.IsDead())
        {
            Debug.Log($"[DamageDealer] {other.gameObject.name} zaten ölü, hasar verilemiyor.");
            return;
        }
        
        health.TakeDamage(damage);
        lastHitTime = Time.time;
        lastHitTarget = other.gameObject;
        
        Debug.Log($"✅ {gameObject.name} {other.gameObject.name}'e {damage} hasar verdi! (Kalan can: {health.currentHealth})");
        
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

