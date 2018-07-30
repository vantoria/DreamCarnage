using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNEnemyPrefabData : ScriptableObject 
{
    public List<Transform> playerTransList = new List<Transform>();
    public List<Transform> enemyBossTransList = new List<Transform>();
    public List<Transform> enemyMinionTransList = new List<Transform>();

    public List<int> startStageDelay = new List<int>();
    public List<EnemyManager.EnemyInfo> s1EnemyMinionMoveList = new List<EnemyManager.EnemyInfo>();
    public List<EnvObjManager.Rocks> s1RockSpawnList = new List<EnvObjManager.Rocks>();

    public bool isShowEnemyList = false;
    public List<bool> isShowEnemyFoldoutList = new List<bool>();

	public void AddToList()
	{
		s1EnemyMinionMoveList.Add(new EnemyManager.EnemyInfo());
	}

    public void AddToList(int index, EnemyManager.EnemyInfo info)
    {
        EnemyManager.EnemyInfo newInfo = new EnemyManager.EnemyInfo();
        newInfo.groupIndex = info.groupIndex;
        newInfo.attackPatternTrans = info.attackPatternTrans;
        newInfo.movePattern = info.movePattern;
        newInfo.spawnPosition = info.spawnPosition;
        newInfo.spawnTime = info.spawnTime;

        s1EnemyMinionMoveList.Insert(index, newInfo);
    }

    public void Delete(int index)
    {
        s1EnemyMinionMoveList.RemoveAt(index);
    }

    public void Sort()
    {
        List<EnemyManager.EnemyInfo> spawnFirstList = new List<EnemyManager.EnemyInfo>();

        while (s1EnemyMinionMoveList.Count != 0)
        {
            EnemyManager.EnemyInfo minSpawnTimeInfo = s1EnemyMinionMoveList[0];
            float minSpwTime = minSpawnTimeInfo.spawnTime;
            int index = 0;

            for (int i = 1; i < s1EnemyMinionMoveList.Count; i++)
            {
                EnemyManager.EnemyInfo currInfo = s1EnemyMinionMoveList[i];
                if (currInfo.spawnTime < minSpwTime)
                {
                    index = i;
                    minSpwTime = currInfo.spawnTime;
                    minSpawnTimeInfo = currInfo;
                }
            }

            s1EnemyMinionMoveList.RemoveAt(index);
            spawnFirstList.Add(minSpawnTimeInfo);
        }

        s1EnemyMinionMoveList = spawnFirstList;

//        for (int i = 0; i < s1EnemyMinionMoveList.Count; i++)
//        {
////            s1EnemyMinionMoveList[i].attackPatternTrans.
////            s1EnemyMinionMoveList.Add(spawnFirstList[i]);
//        }
    }
}
