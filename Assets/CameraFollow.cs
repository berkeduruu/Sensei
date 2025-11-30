using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform target; // Takip edilecek obje (Player)
    public float smoothSpeed = 0.125f; // Takip hızı (0-1 arası, düşük = yavaş)
    public Vector3 offset = new Vector3(0, 2, -10); // Kameranın hedefe göre konumu
    
    [Header("Sınırlar (Opsiyonel)")]
    public bool useBounds = false;
    public float minX = -10f;
    public float maxX = 30f;
    public float minY = 0f;
    public float maxY = 10f;
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Hedef pozisyonu hesapla
        Vector3 desiredPosition = target.position + offset;
        
        // Sınırları uygula (eğer aktifse)
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Yumuşak takip (smooth follow)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

