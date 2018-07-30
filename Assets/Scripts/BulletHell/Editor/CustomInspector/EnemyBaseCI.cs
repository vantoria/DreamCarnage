using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyBase))]
public class EnemyBaseCI : Editor 
{
    EnemyBase mSelf;

    void OnEnable () 
    {
        mSelf = (EnemyBase)target;
    }
	
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("isBoss"));

        if (mSelf.isBoss)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pingPongSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pingPongVal"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isEndShakeScreen"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("delayBeforeAttack"));
        }
//        else
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
//        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("currHitPoint"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("totalHitPoint"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreMultiplier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defeatedScore"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defeatedMult"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("anim"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("currActionNum"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackTransList"), true);

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(target); 
    }
}

[CustomEditor(typeof(Enemy1))]
public class Enemy1Editor : EnemyBaseCI
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Additional code for the derived class...
    }
}
