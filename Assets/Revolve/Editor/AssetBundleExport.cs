using UnityEditor;
using UnityEngine;

public class AssetBundleExport {
  [MenuItem ("Assets/Build AssetBundles")]
  private static void BuildAllAssetBundles() {
    Debug.Log("bulding assetBundles for " + EditorUserBuildSettings.activeBuildTarget);
    BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, 
    BuildAssetBundleOptions.None, 
    EditorUserBuildSettings.activeBuildTarget);
  }
  
  [MenuItem("Assets/Build AssetBundles From Selection")]
  public static void ExportAssetBundleIOSComplete() {
    string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "unity3d");
    if (path.Length != 0) {
      Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
//      BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
      Selection.objects = selection;
    }
  }
}
