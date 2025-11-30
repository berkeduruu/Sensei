using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneCreator : EditorWindow
{
    [MenuItem("Tools/Create 2D Platform Scene", false, 1)]
    public static void CreatePlatformScene()
    {
        // Önce mevcut scene'i kaydetmek isteyip istemediğini sor
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return; // Kullanıcı iptal etti
        }
        
        // Yeni scene oluştur
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Mevcut scene'i temizle ve yeni objeler ekle
        ClearScene();
        SetupScene();
        
        // Scene'i kaydet
        string scenePath = "Assets/Scenes/PlatformScene.unity";
        bool saved = EditorSceneManager.SaveScene(newScene, scenePath);
        
        if (saved)
        {
            Debug.Log("✅ 2D Platform Scene başarıyla oluşturuldu! Scene: " + scenePath);
            EditorUtility.DisplayDialog("Başarılı!", 
                "2D Platform Scene oluşturuldu!\n\n" +
                "Şimdi yapmanız gerekenler:\n" +
                "1. Tilemap penceresinden (Window > 2D > Tile Palette) tile'ları ekleyin\n" +
                "2. Player'ın Ground Layer ayarını kontrol edin", 
                "Tamam");
        }
        else
        {
            Debug.LogError("Scene kaydedilemedi!");
        }
    }
    
    static void ClearScene()
    {
        // Mevcut objeleri temizle (Main Camera ve Directional Light hariç)
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name != "Main Camera" && obj.name != "Directional Light")
            {
                DestroyImmediate(obj);
            }
        }
    }
    
    static void SetupScene()
    {
        // 1. Ground Layer oluştur (eğer yoksa)
        CreateLayerIfNotExists("Ground");
        
        // 2. Tilemap Grid oluştur
        GameObject gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        
        // 3. Ground Tilemap oluştur
        GameObject groundTilemapObj = new GameObject("Ground");
        groundTilemapObj.transform.SetParent(gridObj.transform);
        groundTilemapObj.layer = LayerMask.NameToLayer("Ground");
        
        Tilemap groundTilemap = groundTilemapObj.AddComponent<Tilemap>();
        TilemapRenderer groundRenderer = groundTilemapObj.AddComponent<TilemapRenderer>();
        TilemapCollider2D groundCollider = groundTilemapObj.AddComponent<TilemapCollider2D>();
        groundCollider.usedByComposite = false;
        
        // Composite Collider ekle (daha iyi performans için)
        Rigidbody2D groundRb = groundTilemapObj.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D compositeCollider = groundTilemapObj.AddComponent<CompositeCollider2D>();
        groundCollider.usedByComposite = true;
        
        // 4. Platform Tilemap oluştur (isteğe bağlı)
        GameObject platformTilemapObj = new GameObject("Platforms");
        platformTilemapObj.transform.SetParent(gridObj.transform);
        platformTilemapObj.layer = LayerMask.NameToLayer("Ground");
        
        Tilemap platformTilemap = platformTilemapObj.AddComponent<Tilemap>();
        TilemapRenderer platformRenderer = platformTilemapObj.AddComponent<TilemapRenderer>();
        TilemapCollider2D platformCollider = platformTilemapObj.AddComponent<TilemapCollider2D>();
        platformCollider.usedByComposite = false;
        
        Rigidbody2D platformRb = platformTilemapObj.AddComponent<Rigidbody2D>();
        platformRb.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D platformComposite = platformTilemapObj.AddComponent<CompositeCollider2D>();
        platformCollider.usedByComposite = true;
        
        // 5. Player Prefab'ını ekle (başlangıç platformunda)
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Sensei (Player).prefab");
        GameObject playerObj = null;
        if (playerPrefab != null)
        {
            playerObj = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            playerObj.transform.position = new Vector3(-12, 2.5f, 0); // Başlangıç platformunda
            playerObj.name = "Player";
        }
        else
        {
            Debug.LogWarning("Player prefab bulunamadı! Assets/Prefabs/Sensei (Player).prefab yolunu kontrol edin.");
        }
        
        // 6. Main Camera'yı 2D için ayarla ve takip scripti ekle
        Camera mainCam = Camera.main;
        
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6; // Biraz daha geniş görüş açısı
            // Kamera başlangıç pozisyonunu player'a göre ayarla
            if (playerObj != null)
            {
                mainCam.transform.position = playerObj.transform.position + new Vector3(0, 2, -10);
            }
            else
            {
                mainCam.transform.position = new Vector3(-12, 4.5f, -10);
            }
            mainCam.backgroundColor = new Color(0.4f, 0.6f, 0.9f); // Gökyüzü mavisi
            
            // Kamera takip scripti ekle
            CameraFollow cameraFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            if (playerObj != null)
            {
                cameraFollow.target = playerObj.transform;
                cameraFollow.offset = new Vector3(0, 2, -10);
                cameraFollow.smoothSpeed = 0.15f;
                cameraFollow.useBounds = true;
                cameraFollow.minX = -17f; // Zemin sınırlarına göre
                cameraFollow.maxX = 32f;
                cameraFollow.minY = 0f;
                cameraFollow.maxY = 12f;
            }
        }
        
        // 7. Directional Light'ı 2D için ayarla (eğer varsa)
        Light dirLight = Object.FindObjectOfType<Light>();
        if (dirLight != null)
        {
            dirLight.type = LightType.Directional;
            dirLight.intensity = 1f;
        }
        
        // 8. Background layer oluştur (dekoratif objeler için)
        GameObject decorativesObj = new GameObject("Decoratives");
        decorativesObj.transform.SetParent(gridObj.transform);
        
        // 9. Örnek platform seviyesi oluştur
        CreateExampleLevel(groundTilemap, platformTilemap, decorativesObj);
        
        Debug.Log("Scene hazır! Örnek platform seviyesi oluşturuldu.");
        Debug.Log("Ground layer'ı 6. layer olarak ayarlandı. Player'ın Ground Layer ayarını kontrol edin.");
    }
    
    static void CreateExampleLevel(Tilemap groundTilemap, Tilemap platformTilemap, GameObject decorativesParent)
    {
        // Tile asset'lerini yükle
        TileBase groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/Tileset_0.asset");
        TileBase platformTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/Tileset_1.asset");
        TileBase wallTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/Tileset_2.asset");
        
        // Eğer tile'lar bulunamazsa, farklı tile'ları dene
        if (groundTile == null)
        {
            // Tileset_0'dan Tileset_46'ya kadar dene
            for (int i = 0; i <= 46; i++)
            {
                string tilePath = $"Assets/Tiles/Tileset_{i}.asset";
                TileBase testTile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
                if (testTile != null)
                {
                    groundTile = testTile;
                    if (platformTile == null) platformTile = testTile;
                    if (wallTile == null && i < 45) wallTile = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/Tileset_{i+1}.asset");
                    break;
                }
            }
        }
        
        if (groundTile == null)
        {
            Debug.LogWarning("Tile asset'leri bulunamadı! Manuel olarak tile eklemeniz gerekecek.");
            return;
        }
        
        // Daha geniş ve oynanabilir zemin oluştur
        // Ana zemin (daha uzun)
        Vector3Int groundStart = new Vector3Int(-15, 0, 0);
        Vector3Int groundEnd = new Vector3Int(30, 0, 0);
        for (int x = groundStart.x; x <= groundEnd.x; x++)
        {
            groundTilemap.SetTile(new Vector3Int(x, groundStart.y, 0), groundTile);
        }
        
        // Sol tarafta başlangıç alanı (geniş platform)
        for (int x = -15; x <= -8; x++)
        {
            groundTilemap.SetTile(new Vector3Int(x, 1, 0), platformTile ?? groundTile);
        }
        
        // Orta platformlar (zıplama için, daha iyi yerleştirilmiş)
        // Platform 1: x=2, y=3 (orta seviye)
        for (int x = 0; x <= 4; x++)
        {
            platformTilemap.SetTile(new Vector3Int(x, 3, 0), platformTile ?? groundTile);
        }
        
        // Platform 2: x=8, y=5 (yüksek)
        for (int x = 6; x <= 10; x++)
        {
            platformTilemap.SetTile(new Vector3Int(x, 5, 0), platformTile ?? groundTile);
        }
        
        // Platform 3: x=15, y=4 (orta-yüksek)
        for (int x = 13; x <= 17; x++)
        {
            platformTilemap.SetTile(new Vector3Int(x, 4, 0), platformTile ?? groundTile);
        }
        
        // Platform 4: x=22, y=6 (en yüksek)
        for (int x = 20; x <= 24; x++)
        {
            platformTilemap.SetTile(new Vector3Int(x, 6, 0), platformTile ?? groundTile);
        }
        
        // Sol duvar (başlangıç noktası için)
        for (int y = 0; y <= 8; y++)
        {
            groundTilemap.SetTile(new Vector3Int(-15, y, 0), wallTile ?? groundTile);
        }
        
        // Sağ tarafta bitiş platformu (geniş)
        for (int x = 26; x <= 30; x++)
        {
            groundTilemap.SetTile(new Vector3Int(x, 0, 0), groundTile);
        }
        
        // Sağ tarafta yüksek hedef platformu
        for (int x = 27; x <= 30; x++)
        {
            platformTilemap.SetTile(new Vector3Int(x, 7, 0), platformTile ?? groundTile);
        }
        
        // Dekoratif objeler ekle (ağaçlar, props, vb.)
        AddDecoratives(decorativesParent);
        
        // Tilemap'i güncelle
        groundTilemap.RefreshAllTiles();
        platformTilemap.RefreshAllTiles();
        
        Debug.Log("✅ Örnek platform seviyesi oluşturuldu!");
        Debug.Log("   - Ana zemin: x=-15 ile x=30 arası (daha geniş)");
        Debug.Log("   - 4 farklı yükseklikte platform");
        Debug.Log("   - Dekoratif objeler eklendi");
    }
    
    static void AddDecoratives(GameObject parent)
    {
        // Sprite sheet'lerden tüm sprite'ları yükle
        Object[] treeSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/REAL WORLD/Trees.png");
        Object[] objectSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/REAL WORLD/Objects.png");
        Object[] propSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/REAL WORLD/Props.png");
        Object[] flagSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/REAL WORLD/Flag.png");
        
        // Ağaç sprite'larını filtrele (sadece Sprite tipindekiler)
        Sprite[] trees = System.Array.ConvertAll(
            System.Array.FindAll(treeSprites, obj => obj is Sprite),
            obj => obj as Sprite
        );
        
        Sprite[] objects = System.Array.ConvertAll(
            System.Array.FindAll(objectSprites, obj => obj is Sprite),
            obj => obj as Sprite
        );
        
        Sprite[] props = System.Array.ConvertAll(
            System.Array.FindAll(propSprites, obj => obj is Sprite),
            obj => obj as Sprite
        );
        
        Sprite[] flags = System.Array.ConvertAll(
            System.Array.FindAll(flagSprites, obj => obj is Sprite),
            obj => obj as Sprite
        );
        
        // Ağaçlar ekle (zemin üzerinde, farklı sprite'lar kullan)
        if (trees.Length > 0)
        {
            int treeIndex = 0;
            CreateDecorativeObject(parent, "Tree_1", trees[treeIndex % trees.Length], new Vector3(-12, 1.5f, 0), new Vector2(2, 2));
            treeIndex++;
            CreateDecorativeObject(parent, "Tree_2", trees[treeIndex % trees.Length], new Vector3(-5, 1.5f, 0), new Vector2(2, 2));
            treeIndex++;
            CreateDecorativeObject(parent, "Tree_3", trees[treeIndex % trees.Length], new Vector3(5, 1.5f, 0), new Vector2(2, 2));
            treeIndex++;
            CreateDecorativeObject(parent, "Tree_4", trees[treeIndex % trees.Length], new Vector3(15, 1.5f, 0), new Vector2(2, 2));
            treeIndex++;
            CreateDecorativeObject(parent, "Tree_5", trees[treeIndex % trees.Length], new Vector3(25, 1.5f, 0), new Vector2(2, 2));
        }
        
        // Objects ekle (büyük dekoratif objeler)
        if (objects.Length > 0)
        {
            int objIndex = 0;
            CreateDecorativeObject(parent, "Object_1", objects[objIndex % objects.Length], new Vector3(-8, 1.3f, 0), new Vector2(1.5f, 1.5f));
            objIndex++;
            if (objects.Length > 1)
            {
                CreateDecorativeObject(parent, "Object_2", objects[objIndex % objects.Length], new Vector3(12, 1.3f, 0), new Vector2(1.5f, 1.5f));
            }
        }
        
        // Props ekle (küçük dekoratif objeler)
        if (props.Length > 0)
        {
            int propIndex = 0;
            CreateDecorativeObject(parent, "Prop_1", props[propIndex % props.Length], new Vector3(-10, 1.1f, 0), new Vector2(1, 1));
            propIndex++;
            if (props.Length > 1)
            {
                CreateDecorativeObject(parent, "Prop_2", props[propIndex % props.Length], new Vector3(10, 1.1f, 0), new Vector2(1, 1));
                propIndex++;
            }
            if (props.Length > 2)
            {
                CreateDecorativeObject(parent, "Prop_3", props[propIndex % props.Length], new Vector3(20, 1.1f, 0), new Vector2(1, 1));
            }
        }
        
        // Bayrak ekle (hedef noktası)
        if (flags.Length > 0)
        {
            CreateDecorativeObject(parent, "Flag_Goal", flags[0], new Vector3(28.5f, 8f, 0), new Vector2(1.5f, 1.5f));
        }
        
        Debug.Log($"✅ Dekoratif objeler eklendi! (Ağaç: {trees.Length}, Obje: {objects.Length}, Prop: {props.Length}, Bayrak: {flags.Length})");
    }
    
    static void CreateDecorativeObject(GameObject parent, string name, Sprite sprite, Vector3 position, Vector2 scale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(scale.x, scale.y, 1);
        
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 0; // Zemin tilemap'i ile aynı seviyede (genellikle 0)
        renderer.sortingLayerName = "Default"; // Default sorting layer
        
        // Eğer sprite null ise, renkli bir placeholder oluştur
        if (sprite == null)
        {
            renderer.color = new Color(0.3f, 0.6f, 0.3f, 0.5f); // Yeşil placeholder
            Debug.LogWarning($"{name} için sprite bulunamadı, placeholder kullanılıyor.");
        }
    }
    
    static void CreateLayerIfNotExists(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        
        // Layer'ı bul veya boş bir slot bul
        bool layerExists = false;
        for (int i = 8; i < 32; i++) // Layer 8-31 kullanılabilir
        {
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
            if (layerProp.stringValue == layerName)
            {
                layerExists = true;
                break;
            }
        }
        
        if (!layerExists)
        {
            // Boş bir layer slotu bul
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
}

