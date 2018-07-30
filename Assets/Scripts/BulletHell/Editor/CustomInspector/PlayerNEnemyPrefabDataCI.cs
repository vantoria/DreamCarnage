using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerNEnemyPrefabData))]
public class PlayerNEnemyPrefabDataCI : Editor 
{
    PlayerNEnemyPrefabData mSelf;
    EnemyManager.EnemyInfo copiedInfo;

    void OnEnable () 
    {
        mSelf = (PlayerNEnemyPrefabData)target;
        Refresh();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerTransList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyBossTransList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyMinionTransList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startStageDelay"), true);
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("s1EnemyMinionMoveList"), true);

        EditorGUILayout.BeginHorizontal ();
        mSelf.isShowEnemyList = EditorGUILayout.Foldout(mSelf.isShowEnemyList, "S1EnemyMinionMoveList", true);
		if (GUILayout.Button("Add", GUILayout.Width(60))) Add();
        if (GUILayout.Button("Refresh", GUILayout.Width(60))) Refresh();
        EditorGUILayout.EndHorizontal ();

        if (mSelf.isShowEnemyList)
        {
            for (int i = 0; i < mSelf.s1EnemyMinionMoveList.Count; i++)
            {
                EditorGUI.indentLevel++;
                mSelf.isShowEnemyFoldoutList[i] = EditorGUILayout.Foldout(mSelf.isShowEnemyFoldoutList[i], "Element " + i, true);

                if (mSelf.isShowEnemyFoldoutList[i])
                {
                    EditorGUI.indentLevel++;
                    EnemyManager.EnemyInfo currInfo = mSelf.s1EnemyMinionMoveList[i];

                    currInfo.groupIndex = (EnemyManager.GroupIndex) EditorGUILayout.EnumPopup ("Group Index" ,currInfo.groupIndex);
                    currInfo.attackPatternTrans = (Transform) EditorGUILayout.ObjectField("Attack Pattern", currInfo.attackPatternTrans, typeof(Transform), false);
                    currInfo.movePattern = (EnemyMovement) EditorGUILayout.ObjectField("Move Pattern", currInfo.movePattern, typeof(EnemyMovement), false);
                    currInfo.spawnPosition = (Transform) EditorGUILayout.ObjectField("Spawn Position", currInfo.spawnPosition, typeof(Transform), false);
                    currInfo.spawnTime = EditorGUILayout.FloatField("Spawn Time", currInfo.spawnTime);

                    EditorGUILayout.BeginHorizontal ();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Copy", GUILayout.Width(50))) Copy(currInfo);
                    else if (GUILayout.Button("Paste", GUILayout.Width(50))) Paste(i);
                    else if (GUILayout.Button("CP", GUILayout.Width(50))) CopyPaste(currInfo, i);
                    else if (GUILayout.Button("Delete", GUILayout.Width(50))) Delete(i);
                    else if (GUILayout.Button("Sort", GUILayout.Width(50))) Sort();
                    EditorGUILayout.EndHorizontal ();

                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("s1RockSpawnList"), true);

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(target); 
    }

	void Add()
	{
		mSelf.AddToList();
		mSelf.isShowEnemyFoldoutList.Add(true);
	}

    void Copy(EnemyManager.EnemyInfo info)
    {
        copiedInfo = info;
    }

    void Paste(int index)
    {
        mSelf.AddToList(index, copiedInfo);
        mSelf.isShowEnemyFoldoutList.Add(true);
    }

    void CopyPaste(EnemyManager.EnemyInfo info, int index)
    {
        Copy(info);
        Paste(index);
    }

    void Delete(int index)
    {
        mSelf.Delete(index);
    }

    void Sort()
    {
        mSelf.Sort();
    }

    void Refresh()
    {
//        mSelf.isShowEnemyList = false;
        mSelf.isShowEnemyFoldoutList.Clear();
        for (int i = 0; i < mSelf.s1EnemyMinionMoveList.Count; i++)
        { 
            mSelf.isShowEnemyFoldoutList.Add(false);
        }
    }
}
