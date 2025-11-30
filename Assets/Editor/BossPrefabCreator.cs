using UnityEngine;
using UnityEditor;
using System.IO;

public class BossPrefabCreator : EditorWindow
{
    [MenuItem("Tools/Create Boss Kagami Prefab")]
    public static void CreateBossPrefab()
    {
        // 1. Boss GameObject oluştur
        GameObject bossObj = new GameObject("Boss_Kagami");
        
        // 2. SpriteRenderer ekle ve IDLE_0 sprite'ını yükle
        SpriteRenderer spriteRenderer = bossObj.AddComponent<SpriteRenderer>();
        
        // IDLE_0 sprite'ını yükle (sprite sheet'ten)
        Object[] idleSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/KAGAMI/IDLE.png");
        Sprite idleSprite = null;
        
        foreach (Object obj in idleSprites)
        {
            if (obj is Sprite && obj.name == "IDLE_0")
            {
                idleSprite = obj as Sprite;
                break;
            }
        }
        
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
            Debug.Log("✅ IDLE_0 sprite'ı bulundu ve eklendi!");
        }
        else
        {
            Debug.LogWarning("⚠️ IDLE_0 sprite'ı bulunamadı! Sprite sheet'ten yüklenemedi.");
        }
        
        spriteRenderer.sortingOrder = 1; // Player'ın üstünde görünsün
        
        // 3. Animator ekle
        Animator animator = bossObj.AddComponent<Animator>();
        // Animator Controller'ı daha sonra manuel olarak ekleyebilirsin
        // animator.runtimeAnimatorController = ...;
        
        // 4. Rigidbody2D ekle (fizik için)
        Rigidbody2D rb = bossObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Boss fiziksel olarak etkilenmesin
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Rotasyonu dondur
        
        // 5. Collider ekle (opsiyonel - saldırı/hasar algılama için)
        CapsuleCollider2D collider = bossObj.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.8f, 1.5f); // Boss boyutuna göre ayarla
        collider.offset = new Vector2(0, 0.75f); // Sprite'ın merkezine göre ayarla
        
        // 6. BossKagami script'ini ekle
        BossKagami bossScript = bossObj.AddComponent<BossKagami>();
        
        // 7. Tag ekle (eğer yoksa)
        if (!TagExists("Boss"))
        {
            CreateTag("Boss");
        }
        bossObj.tag = "Boss";
        
        // 8. Layer ekle (eğer yoksa)
        if (!LayerExists("Boss"))
        {
            CreateLayer("Boss");
        }
        bossObj.layer = LayerMask.NameToLayer("Boss");
        
        // 9. Prefab olarak kaydet
        string prefabPath = "Assets/Prefabs/Boss_Kagami.prefab";
        string prefabDirectory = Path.GetDirectoryName(prefabPath);
        
        if (!Directory.Exists(prefabDirectory))
        {
            Directory.CreateDirectory(prefabDirectory);
        }
        
        // Mevcut prefab varsa sil
        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bossObj, prefabPath);
        DestroyImmediate(bossObj); // Scene'deki geçici objeyi sil
        
        Debug.Log($"✅ Boss Kagami prefab'ı oluşturuldu: {prefabPath}");
        
        // Prefab'ı seç
        Selection.activeObject = prefab;
        EditorUtility.FocusProjectWindow();
        
        EditorUtility.DisplayDialog("Başarılı!", 
            "Boss Kagami prefab'ı oluşturuldu!\n\n" +
            "Şimdi yapmanız gerekenler:\n" +
            "1. Prefab'ı aç ve Animator Controller ekle\n" +
            "2. KAGAMI animasyonlarını Animator'a ekle\n" +
            "3. Boss'un pozisyonunu ve ayarlarını düzenle", 
            "Tamam");
    }
    
    static bool TagExists(string tag)
    {
        try
        {
            GameObject.FindGameObjectWithTag(tag);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    static void CreateTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) return;
        }
        
        tagsProp.InsertArrayElementAtIndex(0);
        tagsProp.GetArrayElementAtIndex(0).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
    
    static bool LayerExists(string layerName)
    {
        return LayerMask.NameToLayer(layerName) != -1;
    }
    
    static void CreateLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        
        for (int i = 8; i < 32; i++)
        {
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Layer '{layerName}' oluşturuldu: Layer {i}");
                break;
            }
        }
    }
}

