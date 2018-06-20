using UnityEngine;
using UnityEditor;

public class PlayerNEnemyPrefabDataAsset
{
    [MenuItem("Assets/Create/EnemyPrefabData")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<PlayerNEnemyPrefabData> ();
    }
}