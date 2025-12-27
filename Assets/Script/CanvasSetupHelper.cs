using UnityEngine;
using UnityEngine.UI;

// Bu script Canvas ayarlarını kontrol eder ve düzeltir
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class CanvasSetupHelper : MonoBehaviour
{
    void Start()
    {
        SetupCanvas();
    }
    
    [ContextMenu("Setup Canvas")]
    void SetupCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        
        if (canvas != null)
        {
            // Render Mode'u Screen Space - Overlay yap
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            Debug.Log("✅ Canvas Render Mode ayarlandı: Screen Space - Overlay");
        }
        
        if (scaler != null)
        {
            // Canvas Scaler ayarlarını yap
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            Debug.Log("✅ Canvas Scaler ayarlandı: 1920x1080, Match: 0.5");
        }
        
        // Tüm UI elementlerinin RectTransform'larını kontrol et
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rt in rectTransforms)
        {
            if (rt != GetComponent<RectTransform>()) // Canvas'ın kendisini atla
            {
                // Eğer pozisyon çok dışarıdaysa, merkeze al
                if (Mathf.Abs(rt.anchoredPosition.x) > 10000 || Mathf.Abs(rt.anchoredPosition.y) > 10000)
                {
                    rt.anchoredPosition = Vector2.zero;
                    Debug.Log($"⚠️ {rt.name} pozisyonu düzeltildi");
                }
            }
        }
        
        Debug.Log("✅ Canvas setup tamamlandı!");
    }
}

