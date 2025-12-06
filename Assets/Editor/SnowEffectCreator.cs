using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SnowEffectCreator : EditorWindow
{
    [MenuItem("Tools/Add Snow Effect to Scene")]
    public static void AddSnowEffect()
    {
        // Mevcut sahneyi al
        var activeScene = EditorSceneManager.GetActiveScene();
        
        // Kar efekti objesi oluştur
        GameObject snowObj = new GameObject("SnowEffect");
        
        // Particle System ekle
        ParticleSystem particleSystem = snowObj.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        var colorOverLifetime = particleSystem.colorOverLifetime;
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        
        // Main Module Ayarları
        main.startLifetime = 15f; // Kar taneleri 15 saniye yaşar
        main.startSpeed = 2f; // Düşüş hızı
        main.startSize = 0.2f; // Kar tanesi boyutu (daha görünür)
        main.startColor = Color.white;
        main.maxParticles = 1000; // Maksimum parçacık sayısı
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.3f; // Yerçekimi (yavaş düşüş)
        main.startRotation = 0f;
        main.startRotation3D = false;
        main.playOnAwake = true; // Otomatik başlat
        main.loop = true; // Sürekli döngü
        
        // Emission Module
        emission.rateOverTime = 50f; // Saniyede 50 kar tanesi
        emission.enabled = true;
        
        // Shape Module (Kameranın üstünden yağsın)
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 1f, 1f); // Daha geniş alan
        shape.position = new Vector3(0, 0f, 0f); // Shape'in kendi pozisyonu
        
        // Velocity Over Lifetime (Rüzgar efekti)
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        var velocityX = velocityOverLifetime.x;
        velocityX.constantMin = -0.5f;
        velocityX.constantMax = 0.5f;
        velocityOverLifetime.x = velocityX;
        
        // Color Over Lifetime (Hafif şeffaflık)
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // Size Over Lifetime (Büyüme efekti)
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f); // Başlangıçta küçük
        sizeCurve.AddKey(0.5f, 1f); // Ortada normal
        sizeCurve.AddKey(1f, 1.2f); // Sonda biraz büyük
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Renderer Ayarları
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = -5; // Arka planda görünsün (negatif değer)
        renderer.sortingLayerName = "Default"; // Default sorting layer
        
        // 2D için önemli: Sorting layer ve order
        if (renderer.sortingLayerName == "")
        {
            renderer.sortingLayerName = "Default";
        }
        
        // Material oluştur (eğer yoksa)
        Material snowMaterial = CreateSnowMaterial();
        if (snowMaterial != null)
        {
            renderer.material = snowMaterial;
        }
        else
        {
            // Default material kullan
            Debug.LogWarning("Snow material oluşturulamadı, default material kullanılıyor.");
        }
        
        // Objeyi kameraya göre konumlandır (eğer kamera varsa)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Kameranın görüş alanının üstünde
            float cameraHeight = 15f;
            if (mainCam.orthographic)
            {
                cameraHeight = mainCam.orthographicSize * 2f + 5f; // 2D için
            }
            else
            {
                cameraHeight = 20f; // 3D için varsayılan
            }
            snowObj.transform.position = mainCam.transform.position + new Vector3(0, cameraHeight, 0);
            
            // Z pozisyonunu kamerayla aynı yap (2D için önemli)
            Vector3 camPos = mainCam.transform.position;
            snowObj.transform.position = new Vector3(camPos.x, camPos.y + cameraHeight, camPos.z);
        }
        else
        {
            snowObj.transform.position = new Vector3(0, 15f, 0);
        }
        
        // Particle System'i başlat
        particleSystem.Play();
        
        // SnowEffect script'ini ekle (kamerayı takip etmesi için)
        snowObj.AddComponent<SnowEffect>();
        
        // Scene'e kaydet
        EditorSceneManager.MarkSceneDirty(activeScene);
        
        Debug.Log("✅ Kar efekti eklendi! SnowEffect objesini seçerek ayarları değiştirebilirsiniz.");
        
        // Objeyi seç
        Selection.activeGameObject = snowObj;
    }
    
    static Material CreateSnowMaterial()
    {
        // Unity'nin built-in particle shader'ını kullan
        Shader particleShader = Shader.Find("Sprites/Default");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Unlit/Transparent");
        }
        if (particleShader == null)
        {
            particleShader = Shader.Find("Standard");
        }
        
        if (particleShader != null)
        {
            Material snowMaterial = new Material(particleShader);
            snowMaterial.color = Color.white;
            return snowMaterial;
        }
        
        return null; // Shader bulunamazsa null döndür
    }
}

