using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyMovement))]
public class EnemyMovementCI : Editor 
{
	EnemyMovement mSelf;

	void OnEnable () 
	{
		mSelf = (EnemyMovement)target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnPosTrans"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("moveInfo"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("isDrawGizmo"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("movementList"), true);

		for (int i = 0; i < mSelf.movementList.Count; i++)
        {
            EnemyMovement.Movement currMovement = mSelf.movementList[0];
            if (currMovement.type == EnemyMovement.Movement.Type.WAY_POINT)
            {
                Transform wpParent = mSelf.movementList [i].wayPointParent;
                if (wpParent != null)
                {
                    for (int j = 0; j < wpParent.childCount; j++)
                    {
                        EnemyMovement.Movement enemyMovement = mSelf.movementList [i];

                        if (enemyMovement.wayPointList.Count - 1 < j) enemyMovement.wayPointList.Add (new EnemyMovement.Movement.WayPoint ());
                        enemyMovement.wayPointList [j].targetTrans = wpParent.GetChild(j);

                        if (enemyMovement.isConstantSpeed) enemyMovement.wayPointList [j].speed = enemyMovement.constantSpeed;
                    }
                }
            }
        }

		serializedObject.ApplyModifiedProperties();
		if (GUI.changed) EditorUtility.SetDirty(target); 
	}
}
