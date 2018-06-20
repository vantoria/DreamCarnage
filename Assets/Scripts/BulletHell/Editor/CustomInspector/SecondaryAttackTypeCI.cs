using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SecondaryAttackType))]
public class SecondaryAttackTypeCI : Editor 
{
	SecondaryAttackType mSelf;

	void OnEnable () 
	{
		mSelf = (SecondaryAttackType)target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fairyPosition"));

		if (mSelf.fairyPosition == SecondaryAttackType.FairyPosition.ROTATE_AROUND_TARGET) 
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("center"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("axis"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isClockwise"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("focusedRadius"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("focusedChangeSpeed"));

		}
		else if (mSelf.fairyPosition == SecondaryAttackType.FairyPosition.FROM_THE_BACK) 
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetVec"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetFromCenter"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetBetwBullet"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetFocusedVec1"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetFocusedVec2"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("focusedChangeSpeed"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("pingPongVal"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pingPongSpeed"));
		}
			
		serializedObject.ApplyModifiedProperties();
		if (GUI.changed) EditorUtility.SetDirty(target); 
	}
}