using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Hasar Ayarları")]
    public float damage = 10f;
    public string[] targetTags = { "Boss" }; // Sadece düşman taglerini yaz
    public float attackCooldown = 0.3f; 
    
    [Header("Saldırı Kontrolü")]
    // Bu değişken false ise temas etse bile hasar vermez!
    private bool canDealDamage = false; 
    
    [Header("Hitbox Ayarları")]
    public bool isTrigger = true; 
    
    private float lastHitTime;
    private GameObject lastHitTarget;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider2D>();
        col.isTrigger = isTrigger;
        
        // Başlangıçta hasar vermeyi kapat
        canDealDamage = true; 
    }

    // --- KRİTİK FONKSİYONLAR ---

    // Saldırı başladığında çağır (Animasyonun başında)
    public void StartAttack()
    {
        canDealDamage = true;
        lastHitTarget = null; // Yeni saldırıda aynı hedefe tekrar vurabilsin
    }

    // Saldırı bittiğinde çağır (Animasyonun sonunda)
    public void EndAttack()
    {
        canDealDamage = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (canDealDamage) TryDealDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Vuruş anında hitbox hedefin içinde doğarsa hasarı kaçırmamak için Stay gereklidir
        if (canDealDamage) TryDealDamage(other);
    }

    void TryDealDamage(Collider2D other)
    {
        // 1. Saldırı aktif mi?
        if (!canDealDamage) return;

        // 2. Tag kontrolü
        bool validTag = false;
        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag)) { validTag = true; break; }
        }
        if (!validTag) return;

        // 3. Cooldown ve Hedef Kontrolü
        if (lastHitTarget == other.gameObject && Time.time < lastHitTime + attackCooldown) return;

        // 4. Can Sistemini Bul
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null && !health.IsDead())
        {
            health.TakeDamage(damage);
            lastHitTime = Time.time;
            lastHitTarget = other.gameObject;
            
            // Eğer istersen tek vuruşta birden fazla kişiye vurmamak için:
            // canDealDamage = false; 

            Debug.Log($"<color=green>HASAR:</color> {gameObject.name} -> {other.name} ({damage} HP)");
        }
    }
}
