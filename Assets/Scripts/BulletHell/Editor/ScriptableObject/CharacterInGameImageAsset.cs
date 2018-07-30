using UnityEngine;
using UnityEditor;

public class CharacterInGameImageAsset
{
    [MenuItem("Assets/Create/CharacterInGameImageData")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<CharacterInGameImageData> ();
    }
}
