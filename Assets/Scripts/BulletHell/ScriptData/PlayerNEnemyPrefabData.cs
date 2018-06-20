using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNEnemyPrefabData : ScriptableObject 
{
    public List<Transform> playerTransList = new List<Transform>();
    public List<Transform> enemyBossTransList = new List<Transform>();
    public List<Transform> enemyMinionTransList = new List<Transform>();
    public List<EnemyManager.EnemyInfo> s1EnemyMinionMoveList = new List<EnemyManager.EnemyInfo>();
}
