using UnityEngine;

public class CameraFollowHorizontal : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform target; // Takip edilecek obje (Player)
    public float smoothSpeed = 0.15f; // Takip hızı (0-1 arası, düşük = yavaş)
    
    [Header("Sahne Sınırları")]
    public float minX = -10f; // Sol sınır
    public float maxX = 30f;   // Sağ sınır
    public float cameraY = 0f; // Sabit Y pozisyonu (sahneye göre ayarla)
    public float cameraZ = -10f; // Sabit Z pozisyonu
    
    [Header("Dead Zone (Opsiyonel)")]
    public bool useDeadZone = false; // Dead zone kullanılsın mı?
    public float deadZoneLeft = -2f;  // Player bu sınırın soluna geçerse kamera hareket eder
    public float deadZoneRight = 2f;  // Player bu sınırın sağına geçerse kamera hareket eder
    
    private float currentCameraX;
    
    void Start()
    {
        // Başlangıç pozisyonunu ayarla
        currentCameraX = transform.position.x;
        
        // Eğer target varsa, başlangıçta hedefe göre konumlandır
        if (target != null)
        {
            currentCameraX = target.position.x;
        }
        
        transform.position = new Vector3(currentCameraX, cameraY, cameraZ);
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        float targetX = target.position.x;
        float desiredCameraX = currentCameraX;
        
        // Dead zone kontrolü
        if (useDeadZone)
        {
            float playerOffsetFromCamera = targetX - currentCameraX;
            
            // Eğer player dead zone'un dışındaysa, kamerayı hareket ettir
            if (playerOffsetFromCamera < deadZoneLeft)
            {
                // Player sol tarafta, kamerayı sola kaydır
                desiredCameraX = targetX - deadZoneLeft;
            }
            else if (playerOffsetFromCamera > deadZoneRight)
            {
                // Player sağ tarafta, kamerayı sağa kaydır
                desiredCameraX = targetX - deadZoneRight;
            }
            // Dead zone içindeyse, kamerayı hareket ettirme
        }
        else
        {
            // Dead zone yoksa, direkt player'ı takip et
            desiredCameraX = targetX;
        }
        
        // Yumuşak takip (smooth follow)
        currentCameraX = Mathf.Lerp(currentCameraX, desiredCameraX, smoothSpeed);
        
        // Kamerayı sadece X ekseninde hareket ettir, Y ve Z sabit kalır
        transform.position = new Vector3(currentCameraX, cameraY, cameraZ);
    }
    
    // Inspector'dan sahne sınırlarını kolayca ayarlamak için (opsiyonel)
    void OnDrawGizmosSelected()
    {
        // Sahne sınırlarını görselleştir
        Gizmos.color = Color.yellow;
        Vector3 leftBound = new Vector3(minX, cameraY, 0);
        Vector3 rightBound = new Vector3(maxX, cameraY, 0);
        Gizmos.DrawLine(leftBound + Vector3.up * 5, leftBound + Vector3.down * 5);
        Gizmos.DrawLine(rightBound + Vector3.up * 5, rightBound + Vector3.down * 5);
        
        // Dead zone'u görselleştir (eğer aktifse)
        if (useDeadZone && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 camPos = transform.position;
            Vector3 leftDeadZone = camPos + Vector3.right * deadZoneLeft;
            Vector3 rightDeadZone = camPos + Vector3.right * deadZoneRight;
            Gizmos.DrawLine(leftDeadZone + Vector3.up * 3, leftDeadZone + Vector3.down * 3);
            Gizmos.DrawLine(rightDeadZone + Vector3.up * 3, rightDeadZone + Vector3.down * 3);
        }
    }
}





