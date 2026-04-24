using UnityEditor;
using UnityEngine;
using TMPro;
using System.IO;

public class HackathonAssetSetup : EditorWindow
{
    [MenuItem("Hackathon/Auto-Setup Assets")]
    public static void SetupAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 1. Find the models
        GameObject grassModel = FindModel("hex_grass");
        GameObject waterModel = FindModel("hex_water");
        GameObject mountainModel = FindModel("mountain_A_grass");
        GameObject yurtModel = FindModel("Yurt");

        if (grassModel == null || waterModel == null || mountainModel == null || yurtModel == null)
        {
            Debug.LogError("Could not find all required models! Make sure KayKit Hexagons and Yurt are in the Assets folder.");
            return;
        }

        // 2. Create the Yurt Prefab (just needs to be a standard prefab, no Tile logic)
        GameObject yurtPrefab = CreateSimplePrefab(yurtModel, "YurtPrefab");

        // 3. Create the Tile Prefabs
        GameObject grassPrefab = CreateTilePrefab(grassModel, "Tile_Grass", yurtPrefab);
        GameObject waterPrefab = CreateTilePrefab(waterModel, "Tile_Water", yurtPrefab);
        GameObject mountainPrefab = CreateTilePrefab(mountainModel, "Tile_Mountain", yurtPrefab);

        // 4. Assign to HexGridManager in the scene
        HexGridManager gridManager = FindAnyObjectByType<HexGridManager>();
        
        if (gridManager == null)
        {
            Debug.Log("Creating Game Controllers in the scene...");
            GameObject controllersObj = new GameObject("Game Controllers");
            gridManager = controllersObj.AddComponent<HexGridManager>();
            controllersObj.AddComponent<GameManager>();
            controllersObj.AddComponent<CombatManager>();
        }

        // 5. Setup Camera
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<CameraController>() == null)
        {
            Debug.Log("Attaching CameraController to Main Camera...");
            mainCam.gameObject.AddComponent<CameraController>();
            
            // Center it at 0,0 for a good starting view
            Vector3 camPos = mainCam.transform.position;
            camPos.x = 0;
            camPos.z = 0;
            mainCam.transform.position = camPos;
        }

        SerializedObject so = new SerializedObject(gridManager);
        
        SetArray(so, "plainPrefabs", grassPrefab);
        SetArray(so, "pasturePrefabs", grassPrefab); // Use grass for pasture for now
        SetArray(so, "waterPrefabs", waterPrefab);
        SetArray(so, "mountainPrefabs", mountainPrefab);
        
        so.ApplyModifiedProperties();
        Debug.Log("<color=green>Successfully assigned prefabs to HexGridManager!</color>");

        AssetDatabase.SaveAssets();
        Debug.Log("<color=cyan>Hackathon Asset Setup Complete!</color>");
    }

    private static GameObject FindModel(string modelName)
    {
        string[] guids = AssetDatabase.FindAssets(modelName + " t:Model");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        return null;
    }

    private static GameObject CreateSimplePrefab(GameObject model, string prefabName)
    {
        string path = $"Assets/Prefabs/{prefabName}.prefab";
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
        return prefab;
    }

    private static GameObject CreateTilePrefab(GameObject model, string prefabName, GameObject yurtPrefab)
    {
        string path = $"Assets/Prefabs/{prefabName}.prefab";
        
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        
        // Add Collider
        if (instance.GetComponent<Collider>() == null)
        {
            MeshCollider collider = instance.AddComponent<MeshCollider>();
            collider.convex = true; // Convex ensures clicking works cleanly
        }

        // Add Tile script
        Tile tile = instance.GetComponent<Tile>();
        if (tile == null)
        {
            tile = instance.AddComponent<Tile>();
        }

        // We need to set private fields on Tile via SerializedObject
        SerializedObject tileSo = new SerializedObject(tile);

        // Try to find the Renderer (usually in children for imported FBX)
        Renderer rend = instance.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            tileSo.FindProperty("meshRenderer").objectReferenceValue = rend;
        }

        // Set the Yurt Prefab
        if (yurtPrefab != null)
        {
            tileSo.FindProperty("yurtPrefab").objectReferenceValue = yurtPrefab;
        }

        // Add UI TextMeshPro
        GameObject textObj = new GameObject("CubeCountText");
        textObj.transform.SetParent(instance.transform);
        textObj.transform.localPosition = new Vector3(0, 1.2f, 0); // Float above tile
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0); // Lay flat facing camera
        
        TextMeshPro tmpro = textObj.AddComponent<TextMeshPro>();
        tmpro.text = "0";
        tmpro.alignment = TextAlignmentOptions.Center;
        tmpro.fontSize = 5;
        tmpro.color = Color.white;
        
        tileSo.FindProperty("cubeCountText").objectReferenceValue = tmpro;

        tileSo.ApplyModifiedProperties();

        // Save and cleanup
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
        return prefab;
    }

    private static void SetArray(SerializedObject so, string propertyName, GameObject prefab)
    {
        SerializedProperty arrayProp = so.FindProperty(propertyName);
        if (arrayProp != null)
        {
            arrayProp.arraySize = 1;
            arrayProp.GetArrayElementAtIndex(0).objectReferenceValue = prefab;
        }
    }
}
