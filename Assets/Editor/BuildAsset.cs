using UnityEditor;
using UnityEngine;

public class BuildAsset : MonoBehaviour {

	[MenuItem("Assets/Build Asset Bundles/Build Android")]
    static void BuildAllAssetBundleForAndroid()
    {
        BuildPipeline.BuildAssetBundles("DLC", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }

    [MenuItem("Assets/Build Asset Bundles/Build iOS")]
    static void BuildAllAssetBundleForIOS()
    {
        BuildPipeline.BuildAssetBundles("DLC", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
    }
}
