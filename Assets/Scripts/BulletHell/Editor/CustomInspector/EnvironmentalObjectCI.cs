using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(EnvironmentalObject))]
public class EnvironmentalObjectCI : Editor 
{
    EnvironmentalObject mSelf;

	void OnEnable () 
	{
        mSelf = (EnvironmentalObject)target;
	}

	public override void OnInspectorGUI()
	{
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("state"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("speedFreeFall"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speedToPlayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isDestructable"));

        if (mSelf.isDestructable)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currHitPoint"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHitPoint"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("impactMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("extraLinkMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defeatedMult"));
        }

        serializedObject.ApplyModifiedProperties();
		if (GUI.changed) EditorUtility.SetDirty(target); 
	}
}