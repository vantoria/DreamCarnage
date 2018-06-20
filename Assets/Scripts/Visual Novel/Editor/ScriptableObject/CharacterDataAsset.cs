using UnityEngine;
using UnityEditor;

public class CharacterDataAsset
{
    [MenuItem("Assets/Create/CharacterData")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<CharacterData> ();
    }
}
