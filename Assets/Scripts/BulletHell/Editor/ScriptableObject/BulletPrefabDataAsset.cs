using UnityEngine;
using UnityEditor;

public class BulletPrefabDataAsset
{
    [MenuItem("Assets/Create/BulletPrefabData")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<BulletPrefabData> ();
    }
}