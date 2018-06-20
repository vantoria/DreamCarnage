using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TexturePickerEditor : EditorWindow
{
    public static string[] optionsArray;
    public static int index = 0;

    public int width = 200;
    public int height = 250;

    int mYOffsetValue = 10;

    static DialogueData.Dialogue.Sentence mCurrSentence;
    static List<Sprite> mCurrSpriteList;

//    [MenuItem("Examples/Editor GUILayout Popup usage")]
    public static void Init()
    {
        EditorWindow window = GetWindow(typeof(TexturePickerEditor));
        window.minSize = new Vector2(232.0f, 300.0f);
        window.maxSize = new Vector2(232.0f, 300.0f);
        window.maximized = false;
        window.Show();
    }

    void OnGUI()
    {
        if (optionsArray == null || mCurrSpriteList == null) return;

        Rect rect = new Rect(0, 0, width, height);
        rect.center = new Vector2(position.width * 0.5f, position.height * 0.5f - mYOffsetValue);
        EditorGUI.DrawTextureTransparent(rect, mCurrSpriteList[index].texture);

        GUILayout.Space(rect.y + rect.height + mYOffsetValue);
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace();
        index = EditorGUILayout.Popup(index, optionsArray, GUILayout.Width(150));
        mCurrSentence.sprite = mCurrSpriteList[index];
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal ();
    }

    public static void ListToEditorWindow(DialogueData.Dialogue.Sentence currSentence, 
        int activeTextureIndex, List<Sprite> spriteList)
    {
        mCurrSentence = currSentence;
        index = activeTextureIndex;
        mCurrSpriteList = spriteList;

        optionsArray = new string[spriteList.Count];

        for (int i = 0; i < spriteList.Count; i++)
        { optionsArray[i] = spriteList[i].name; }

        Init();
    }

    void OnLostFocus()
    {
        this.Close();
    }
}