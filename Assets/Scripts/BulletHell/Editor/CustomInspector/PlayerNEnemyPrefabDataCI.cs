using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerNEnemyPrefabData))]
public class PlayerNEnemyPrefabDataCI : Editor 
{
    PlayerNEnemyPrefabData mSelf;

    void OnEnable () 
    {
        mSelf = (PlayerNEnemyPrefabData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerTransList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyBossTransList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyMinionTransList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("s1EnemyMinionMoveList"), true);

        for (int i = 0; i < mSelf.s1EnemyMinionMoveList.Count; i++)
        {
            Transform wpParent = mSelf.s1EnemyMinionMoveList[i].movement.wayPointParent;
            if (wpParent != null)
            {
                for (int j = 0; j < wpParent.childCount; j++)
                {
                    int count = mSelf.s1EnemyMinionMoveList[i].movement.wayPointList.Count;
                    EnemyMovement.Movement enemyMove = mSelf.s1EnemyMinionMoveList[i].movement;

                    if (count - 1 < j) enemyMove.wayPointList.Add(new EnemyMovement.Movement.WayPoint());
                    enemyMove.wayPointList[j].targetTrans = wpParent.GetChild(j);

                    if (enemyMove.isConstantSpeed) enemyMove.wayPointList[j].speed = enemyMove.constantSpeed;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(target); 
    }
}
