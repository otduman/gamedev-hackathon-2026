using UnityEditor;

// Automatically disables camera and light import for every FBX file.
// Prevents modelling-software cameras (e.g. inside Yurt.fbx) from becoming
// active Unity Camera components that steal the Game view.
public class FBXImportSettings : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (importer == null) return;
        if (!assetPath.ToLower().EndsWith(".fbx")) return;

        importer.importCameras = false;
        importer.importLights  = false;
    }
}
