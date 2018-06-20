using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour 
{
    public static EnemyManager sSingleton { get { return _sSingleton; } }
    static EnemyManager _sSingleton;

    public bool isAppearEnemy = true;

    [System.Serializable]
    public class EnemyInfo
    {
        public GroupIndex groupIndex;

        public AttackPattern.Target attackTarget;
        public Transform spawnPosition;
        public float spawnTime;
        public EnemyMovement.Movement movement;
    }

    public enum GroupIndex
    {
        MINION_1 = 0,
        MINION_2,
        MINION_3
    }

    [HideInInspector] public List<Transform> enemyList = new List<Transform>();

    // Instantiated enemy minions.
//    List<Transform> mEnemyMinion1List = new List<Transform>();
//    List<Transform> mEnemyMinion2List = new List<Transform>();
//    List<Transform> mEnemyMinion3List = new List<Transform>();
    List<Transform> mEnemyBossList = new List<Transform>();

    // The component attached to enemy minions in scene.
    List<AttackPattern> mEnemyMinion1APList = new List<AttackPattern>();
    List<AttackPattern> mEnemyMinion2APList = new List<AttackPattern>();
    List<AttackPattern> mEnemyMinion3APList = new List<AttackPattern>();

    List<EnemyMovement> mEnemyMinion1MoveList = new List<EnemyMovement>();
    List<EnemyMovement> mEnemyMinion2MoveList = new List<EnemyMovement>();
    List<EnemyMovement> mEnemyMinion3MoveList = new List<EnemyMovement>();

    // Separate the minions moveInfo from scriptable PlayerNEnemyPrefabData.
    List<EnemyInfo> mEnemyMinion1MoveInfoList = new List<EnemyInfo>();
    List<EnemyInfo> mEnemyMinion2MoveInfoList = new List<EnemyInfo>();
    List<EnemyInfo> mEnemyMinion3MoveInfoList = new List<EnemyInfo>();
    List<EnemyInfo> mAllEnemyMinionMoveInfoList = new List<EnemyInfo>();

    float mTimer;

    // The index for instantiated prefab to appear. (Used for mEnemyMinionMoveList)
    int mMinion1Index, mMinion2Index, mMinion3Index, mBossIndex;

    // The index for next in line for instantiated prefab to appear. (Used for mAllEnemyMinionMoveInfoList)
    int mAppearIndex = 0;

    // The index for next in line to get moveData placed into instantiated prefab. (Used for mEnemyMinionMoveList)
    int mMinion1MoveIndex, mMinion2MoveIndex, mMinion3MoveIndex;

    // The saved index for Enemy moveData of each minion type. 
    // Ex: There might be 5 moveData for Minion_1 but with only 1 instantiated prefab for Minion_1. 
    // This var save the next index of moveData to be used by Minion_1. (Used for scriptableObj mEnemyMinionMoveInfoList)
    int mMinion1SavedMoveIndex, mMinion2SavedMoveIndex, mMinion3SavedMoveIndex;

    PlayerNEnemyPrefabData mPlayerAndEnemyPrefabData;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Update()
    {
        if (!isAppearEnemy) return;

        mTimer += Time.deltaTime;
        for (int i = mAppearIndex; i < mAllEnemyMinionMoveInfoList.Count; i++)
        {
//            Debug.Log("i " + i + " spawn time " + mAllEnemyMinionMoveInfoList[i].spawnTime);

            if (mTimer >= mAllEnemyMinionMoveInfoList[i].spawnTime)
            {
                mAppearIndex = i + 1;
                if (mAllEnemyMinionMoveInfoList[i].groupIndex == GroupIndex.MINION_1)
                {
                    Transform currEnemy = mEnemyMinion1MoveList[mMinion1Index].transform;
                    currEnemy.position = mEnemyMinion1MoveList[mMinion1Index].spawnPosTrans.position;
                    mEnemyMinion1APList[mMinion1Index].target = mAllEnemyMinionMoveInfoList[i].attackTarget;
                    currEnemy.gameObject.SetActive(true);

                    if (mMinion1Index + 1 > mEnemyMinion1MoveList.Count - 1) mMinion1Index = 0;
                    else mMinion1Index++;
                }
//                else if (mAllEnemyMinionMoveInfoList[i].groupIndex == GroupIndex.MINION_2)
//                {
//                    mEnemyMinion2MoveList[mMinion2Index].gameObject.SetActive(true);
//
//                    if (mMinion2Index + 1 > mEnemyMinion2MoveList.Count - 1) mMinion2Index = 0;
//                    else mMinion2Index++;
//                }
//                else if (mAllEnemyMinionMoveInfoList[i].groupIndex == GroupIndex.MINION_3)
//                {
//                    mEnemyMinion3MoveList[mMinion3Index].gameObject.SetActive(true);
//
//                    if (mMinion3Index + 1 > mEnemyMinion3MoveList.Count - 1) mMinion3Index = 0;
//                    else mMinion3Index++;
//                }
            }

        }
//        if (Input.GetKeyDown(KeyCode.Return))
//        {
//            SetNextMinionMovement(GroupIndex.MINION_1);
//        }
    }

    public void AddMovementToMinion(PlayerNEnemyPrefabData data)
    {
        // Put all enemy move list into a List, regardless of group type.
        mPlayerAndEnemyPrefabData = data;
        for (int i = 0; i < data.s1EnemyMinionMoveList.Count; i++)
        {
            EnemyInfo currInfo = data.s1EnemyMinionMoveList[i];
            mAllEnemyMinionMoveInfoList.Add(currInfo);
        }

        // Put earliest-latest spawn time minion into new List.
        List<EnemyInfo> spawnFirstList = new List<EnemyInfo>();
        while (mAllEnemyMinionMoveInfoList.Count != 0)
        {
            EnemyInfo minSpawnTimeInfo = mAllEnemyMinionMoveInfoList[0];
            float minSpwTime = minSpawnTimeInfo.spawnTime;
            int index = 0;

            for (int i = 1; i < mAllEnemyMinionMoveInfoList.Count; i++)
            {
                EnemyInfo currInfo = mAllEnemyMinionMoveInfoList[i];
                if (currInfo.spawnTime < minSpwTime)
                {
                    index = i;
                    minSpwTime = currInfo.spawnTime;
                    minSpawnTimeInfo = currInfo;
                }
            }

            mAllEnemyMinionMoveInfoList.RemoveAt(index);
            spawnFirstList.Add(minSpawnTimeInfo);
        }

        // Put spawnFirstList back into allEnemyMinionList.
        mAllEnemyMinionMoveInfoList = spawnFirstList;

        // Testing purposes.
//        for (int i = 0; i < mAllEnemyMinionMoveInfoList.Count; i++)
//        {
//            Debug.Log(mAllEnemyMinionMoveInfoList[i].spawnTime);
//        }

        // Separate spawnFirstList into their respective groups.
        for (int i = 0; i < spawnFirstList.Count; i++)
        {
            EnemyInfo currInfo = spawnFirstList[i];

            if (currInfo.groupIndex == GroupIndex.MINION_1) mEnemyMinion1MoveInfoList.Add(currInfo);
            else if (currInfo.groupIndex == GroupIndex.MINION_2) mEnemyMinion2MoveInfoList.Add(currInfo);
            else if (currInfo.groupIndex == GroupIndex.MINION_3) mEnemyMinion3MoveInfoList.Add(currInfo);
        }

        // Set movement info to respective EnemyMovement script.
        AddMovementToSameInstantiatedPrefabs(mEnemyMinion1MoveInfoList, mEnemyMinion1MoveList, ref mMinion1MoveIndex, ref mMinion1SavedMoveIndex);
        AddMovementToSameInstantiatedPrefabs(mEnemyMinion2MoveInfoList, mEnemyMinion2MoveList, ref mMinion2MoveIndex, ref mMinion2SavedMoveIndex);
        AddMovementToSameInstantiatedPrefabs(mEnemyMinion3MoveInfoList, mEnemyMinion3MoveList, ref mMinion3MoveIndex, ref mMinion3SavedMoveIndex);
    }

    public void SetNextMinionMovement(GroupIndex groupIndex)
    {
        if (groupIndex == GroupIndex.MINION_1) 
            AddMovementToNextPrefab(mEnemyMinion1MoveInfoList, mEnemyMinion1MoveList, ref mMinion1MoveIndex, ref mMinion1SavedMoveIndex);
        else if (groupIndex == GroupIndex.MINION_2) 
            AddMovementToNextPrefab(mEnemyMinion2MoveInfoList, mEnemyMinion2MoveList, ref mMinion2MoveIndex, ref mMinion2SavedMoveIndex);
        else if (groupIndex == GroupIndex.MINION_3) 
            AddMovementToNextPrefab(mEnemyMinion3MoveInfoList, mEnemyMinion3MoveList, ref mMinion3MoveIndex, ref mMinion3SavedMoveIndex);
    }

    public void InstantiateAndCacheEnemyBoss(Transform currEnemy)
    {
        Transform trans = Instantiate(currEnemy, Vector3.zero, Quaternion.identity);
        trans.name = currEnemy.name;
        trans.gameObject.SetActive(false);
        mEnemyBossList.Add(trans);
    }

    public void InstantiateAndCacheEnemy(Transform currEnemy, int total, int groupIndex)
    {
        // Group name.
        GameObject go = new GameObject();
        go.name = currEnemy.name;   

        List<Transform> sameEnemyList = new List<Transform>();
        for (int i = 0; i < total; i++)
        {
            Transform trans = Instantiate(currEnemy, Vector3.zero, Quaternion.identity);
            trans.name = currEnemy.name;
            trans.SetParent(go.transform);
            trans.gameObject.SetActive(false);

            sameEnemyList.Add(trans);

            AttackPattern ap = trans.GetComponentInChildren<AttackPattern>();
            EnemyMovement enemyMovement = trans.GetComponent<EnemyMovement>();

            if (groupIndex == 0)
            {
                mEnemyMinion1APList.Add(ap);
                mEnemyMinion1MoveList.Add(enemyMovement);
            }
            else if (groupIndex == 1)
            {
                mEnemyMinion2APList.Add(ap);
                mEnemyMinion2MoveList.Add(enemyMovement);
            }
            else if (groupIndex == 2)
            {
                mEnemyMinion3APList.Add(ap);
                mEnemyMinion3MoveList.Add(enemyMovement);
            }

            // Set sort order for enemy bullets.
//            SpriteRenderer transSr = trans.GetComponent<SpriteRenderer>();
//            if (transSr != null && transSr.sortingLayerName == TagManager.sSingleton.sortLayerTopG) 
//                transSr.sortingOrder = i;
        }

//        if (groupIndex == 0) mEnemyMinion1List = sameEnemyList;
//        else if (groupIndex == 1) mEnemyMinion2List = sameEnemyList;
//        else if (groupIndex == 2) mEnemyMinion3List = sameEnemyList;
    }

//    public Transform GetEnemyBossTrans()
//    {
//        Transform trans = mEnemyBossList[mBossIndex];
//        if (mBossIndex + 1 > mEnemyBossList.Count - 1) mBossIndex = 0;
//        else mBossIndex++;
//        return trans;
//    }
//
//    public Transform GetEnemyMinionTrans(GroupIndex groupIndex)
//    {
//        if (groupIndex == GroupIndex.MINION_1)
//        {
//            Transform trans = mEnemyMinion1List[mMinion1Index];
//            if (mMinion1Index + 1 > mEnemyMinion1List.Count - 1) mMinion1Index = 0;
//            else mMinion1Index++;
//            return trans;
//        }
//        else if (groupIndex == GroupIndex.MINION_2)
//        {
//            Transform trans = mEnemyMinion2List[mMinion2Index];
//            if (mMinion2Index + 1 > mEnemyMinion2List.Count - 1) mMinion2Index = 0;
//            else mMinion2Index++;
//            return trans;
//        }
//        else if (groupIndex == GroupIndex.MINION_3)
//        {
//            Transform trans = mEnemyMinion3List[mMinion3Index];
//            if (mMinion3Index + 1 > mEnemyMinion3List.Count - 1) mMinion3Index = 0;
//            else mMinion3Index++;
//            return trans;
//        }
//        return null;
//    }

    public void AddToList(Transform trans)
    {
        enemyList.Add(trans);
    }

    public Vector2 GetClosestEnemyDir(Transform bullet)
    {
        Transform enemyTrans = enemyList[0];
        float minSqrLength = (enemyTrans.position - bullet.position).sqrMagnitude;

        for (int i = 1; i < enemyList.Count; i++)
        {
            Vector3 offset = enemyList[i].position - bullet.position;
            float sqrLen = offset.sqrMagnitude;
            if (sqrLen < minSqrLength)
            {
                enemyTrans = enemyList[i];
                minSqrLength = sqrLen;
            }
        }

        Vector2 dir = enemyTrans.position - bullet.position;
        return dir.normalized;
    }

    // Send enemyMoveInfo(from scriptable object) to EnemyMovement(component).
    void AddMovementToSameInstantiatedPrefabs(List<EnemyInfo> enemyMoveInfoList, List<EnemyMovement> enemyMovementList, ref int moveIndex, ref int savedIndex)
    {
        bool isLoop = true;
        while(isLoop)
        {
            isLoop = AddMovementToNextPrefab(enemyMoveInfoList, enemyMovementList, ref moveIndex, ref savedIndex);
        }
    }

    // Send enemyMoveInfo(from scriptable object) to EnemyMovement(component).
    bool AddMovementToNextPrefab(List<EnemyInfo> enemyMoveInfoList, List<EnemyMovement> enemyMovementList, ref int moveIndex, ref int savedIndex)
    {
        if (savedIndex + moveIndex > enemyMoveInfoList.Count - 1) return false;
            
        EnemyInfo currInfo = enemyMoveInfoList[savedIndex + moveIndex];
        enemyMovementList[moveIndex].SetMovement(currInfo.spawnPosition, currInfo.movement);
//        Debug.Log(currInfo.spawnPosition.name);

        // If the next index is over the instantiated prefab count, reset it back to 0(use back the first prefab).
        if (moveIndex + 1 > enemyMovementList.Count - 1)
        {
            savedIndex += moveIndex + 1;
            moveIndex = 0;
            return false;
        }
        else moveIndex++;

        return true;
    }
}
