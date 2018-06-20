using UnityEngine;
using UnityEditor;

public class EnvObjPrefabDataAsset
{
    [MenuItem("Assets/Create/EnvObjPrefabData")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<EnvObjPrefabData> ();
    }
}