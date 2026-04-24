using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HackathonAssetSetup : EditorWindow
{
    [MenuItem("Hackathon/Auto-Setup Assets")]
    public static void SetupAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Try multiple known name variants for each model type.
        // Different KayKit packs and import settings produce different asset names.
        GameObject grassModel    = FindModelMultipleNames(new[]{ "hex_grass", "hexagon_grass", "hex_plain", "hex_ground", "grass" });
        GameObject waterModel    = FindModelMultipleNames(new[]{ "hex_water", "hexagon_water", "hex_ocean", "hex_river", "water" });
        GameObject mountainModel = FindModelMultipleNames(new[]{ "mountain_A_grass", "hex_mountain", "hexagon_mountain", "mountain_grass", "mountain" });
        GameObject yurtModel     = FindModelMultipleNames(new[]{ "yurt", "Yurt", "structure_yurt", "building_yurt", "yurt_A", "yurt_B", "ger", "Ger" });

        if (grassModel == null)
        {
            Debug.LogError("[Setup] Could not find a grass/plain hex model. Import KayKit Hexagon assets first. Setup aborted.");
            return;
        }
        if (waterModel    == null) { Debug.LogWarning("[Setup] No water model found — using grass as fallback.");    waterModel    = grassModel; }
        if (mountainModel == null) { Debug.LogWarning("[Setup] No mountain model found — using grass as fallback."); mountainModel = grassModel; }

        GameObject yurtPrefab = null;
        if (yurtModel != null)
        {
            yurtPrefab = CreateYurtPrefab(yurtModel, "YurtPrefab");
            Debug.Log("[Setup] Yurt prefab created. The game will auto-scale it to fit the tile at runtime.");
        }
        else
        {
            Debug.LogWarning("[Setup] No yurt model found — yurt tiles will show a gold sphere placeholder. Import a yurt model and re-run setup to fix.");
        }

        GameObject grassPrefab    = CreateTilePrefab(grassModel,    "Tile_Grass",    yurtPrefab);
        GameObject waterPrefab    = CreateTilePrefab(waterModel,    "Tile_Water",    yurtPrefab);
        GameObject mountainPrefab = CreateTilePrefab(mountainModel, "Tile_Mountain", yurtPrefab);

        // Create or find Game Controllers object.
        HexGridManager gridManager = FindAnyObjectByType<HexGridManager>();
        if (gridManager == null)
        {
            GameObject controllersObj = new GameObject("Game Controllers");
            gridManager = controllersObj.AddComponent<HexGridManager>();
            controllersObj.AddComponent<GameManager>();
            controllersObj.AddComponent<CombatManager>();
            Undo.RegisterCreatedObjectUndo(controllersObj, "Create Game Controllers");
        }
        else
        {
            if (gridManager.GetComponent<GameManager>()   == null) gridManager.gameObject.AddComponent<GameManager>();
            if (gridManager.GetComponent<CombatManager>() == null) gridManager.gameObject.AddComponent<CombatManager>();
        }

        SerializedObject so = new SerializedObject(gridManager);
        SetArray(so, "plainPrefabs",    grassPrefab);
        SetArray(so, "pasturePrefabs",  grassPrefab);
        SetArray(so, "waterPrefabs",    waterPrefab);
        SetArray(so, "mountainPrefabs", mountainPrefab);
        so.ApplyModifiedProperties();
        Debug.Log("[Setup] Prefabs assigned to HexGridManager.");

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<CameraController>() == null)
        {
            mainCam.gameObject.AddComponent<CameraController>();
            Vector3 p = mainCam.transform.position;
            p.x = 0f; p.z = 0f;
            mainCam.transform.position = p;
        }

        if (FindAnyObjectByType<UIManager>() == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight  = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.AddComponent<UIManager>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create UI Canvas");
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(gridManager);
        Debug.Log("<color=cyan>[Setup] Complete! Press Play to test.</color>");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static GameObject FindModelMultipleNames(string[] candidates)
    {
        foreach (string name in candidates)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Model");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model != null)
                {
                    Debug.Log($"[Setup] Found model '{name}' at '{path}'.");
                    return model;
                }
            }
        }
        return null;
    }

    // Creates a yurt prefab wrapped in a scale-neutral root.
    // The game's Tile.cs then auto-scales the instantiated yurt at runtime using
    // Renderer.bounds so any KayKit model fits correctly regardless of its native scale.
    private static GameObject CreateYurtPrefab(GameObject model, string prefabName)
    {
        string path = $"Assets/Prefabs/{prefabName}.prefab";
        GameObject root          = new GameObject(prefabName);
        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        modelInstance.transform.SetParent(root.transform, false);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localScale    = Vector3.one;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateTilePrefab(GameObject model, string prefabName, GameObject yurtPrefab)
    {
        string path = $"Assets/Prefabs/{prefabName}.prefab";
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);

        if (instance.GetComponent<Collider>() == null)
        {
            MeshCollider col = instance.AddComponent<MeshCollider>();
            col.convex = true;
        }

        Tile tile = instance.GetComponent<Tile>() ?? instance.AddComponent<Tile>();
        SerializedObject tileSo = new SerializedObject(tile);

        Renderer rend = instance.GetComponentInChildren<Renderer>();
        if (rend != null)
            tileSo.FindProperty("meshRenderer").objectReferenceValue = rend;
        else
            Debug.LogWarning($"[Setup] No Renderer found on '{model.name}'. Tile tinting will be inactive.");

        if (yurtPrefab != null)
            tileSo.FindProperty("yurtPrefab").objectReferenceValue = yurtPrefab;

        // Floating cube-count label (world-space TMP).
        GameObject textObj = new GameObject("CubeCountText");
        textObj.transform.SetParent(instance.transform);
        textObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        TextMeshPro tmpro = textObj.AddComponent<TextMeshPro>();
        tmpro.text      = "";
        tmpro.alignment = TextAlignmentOptions.Center;
        tmpro.fontSize  = 6f;
        tmpro.color     = Color.white;
        tmpro.gameObject.SetActive(false);

        tileSo.FindProperty("cubeCountText").objectReferenceValue = tmpro;
        tileSo.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
        Debug.Log($"[Setup] Created prefab: {path}");
        return prefab;
    }

    private static void SetArray(SerializedObject so, string propertyName, GameObject prefab)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null) { Debug.LogWarning($"[Setup] Property '{propertyName}' not found on HexGridManager."); return; }
        prop.arraySize = 1;
        prop.GetArrayElementAtIndex(0).objectReferenceValue = prefab;
    }
}
