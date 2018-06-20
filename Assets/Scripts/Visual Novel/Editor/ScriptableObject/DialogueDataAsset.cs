using UnityEngine;
using UnityEditor;

public class DialogueDataAsset
{
    [MenuItem("Assets/Create/DialogueData")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<DialogueData> ();
    }
}