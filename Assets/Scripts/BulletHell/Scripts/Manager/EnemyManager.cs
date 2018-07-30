using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour 
{
    public static EnemyManager sSingleton { get { return _sSingleton; } }
    static EnemyManager _sSingleton;

	public bool isAppearEnemy = true;
    public bool isAppearBoss = true;
    public EnemyHealth bossEnemyHealthBar;

    [System.Serializable]
    public class EnemyInfo
    {
        public GroupIndex groupIndex;

        public Transform attackPatternTrans;
		public EnemyMovement movePattern;
        public Transform spawnPosition;
        public float spawnTime;
    }

    public enum GroupIndex
    {
        MINION_1 = 0,
        MINION_2,
        MINION_3
    }

	public List<float> meetBossTimeList = new List<float>();

    [HideInInspector] public List<Transform> enemyList = new List<Transform>();

    // Instantiated enemy minions.
    List<Transform> mEnemyMinion1List = new List<Transform>();
    List<Transform> mEnemyMinion2List = new List<Transform>();
    List<Transform> mEnemyMinion3List = new List<Transform>();
    List<Transform> mEnemyBossList = new List<Transform>();

    // Separate the minions moveInfo from scriptable PlayerNEnemyPrefabData.
	List<EnemyInfo> mEnemyMinion1InfoList = new List<EnemyInfo>();
	List<EnemyInfo> mEnemyMinion2InfoList = new List<EnemyInfo>();
	List<EnemyInfo> mEnemyMinion3InfoList = new List<EnemyInfo>();
    List<EnemyInfo> mAllEnemyMinionInfoList = new List<EnemyInfo>();

    float mTimer;
    [HideInInspector] public bool isBossAppeared = false, isBossDead = false;

    // The index for instantiated prefab to appear. (Used for mEnemyMinionMoveList)
    int mMinion1Index, mMinion2Index, mMinion3Index, mBossIndex;

    // The index for next in line for instantiated prefab to appear. (Used for mAllEnemyMinionMoveInfoList)
    int mAppearIndex = 0;

    // The index for next in line to get moveData placed into instantiated prefab. (Used for mEnemyMinionMoveList)
    int mMinion1SetIndex, mMinion2SetIndex, mMinion3SetIndex;

    // The saved index for Enemy moveData of each minion type. 
    // Ex: There might be 5 moveData for Minion_1 but with only 1 instantiated prefab for Minion_1. 
    // This var save the next index of moveData to be used by Minion_1. (Used for scriptableObj mEnemyMinionMoveInfoList)
    int mMinion1SavedSetIndex, mMinion2SavedSetIndex, mMinion3SavedSetIndex;

    int mBonusScore = 2000000;
    float mDelay = 0;
    bool mIsShowScoreBonus = true;

    // Used for adding bonus score after defeating stage boss.
    PlayerController mPlayerController1, mPlayerController2;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Start()
    {
        mPlayerController1 = GameManager.sSingleton.player1.GetComponent<PlayerController>();

        if (GameManager.sSingleton.IsThisPlayerActive(2))
            mPlayerController2 = GameManager.sSingleton.player2.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (GameManager.sSingleton.currState != GameManager.State.BATTLE) return;

        if (!UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause) 
		{
			mTimer += Time.deltaTime;
			if (isAppearBoss && !isBossAppeared)
			{
                if (mTimer > meetBossTimeList[GameManager.sSingleton.currStage - 1] && BombManager.sSingleton.dualLinkState != BombManager.DualLinkState.SHOOTING)
				{
					isBossAppeared = true;
					GameManager.sSingleton.currState = GameManager.State.BOSS_MOVE_INTO_SCREEN;

                    // Disable remaining enemies on screen.
                    int count = enemyList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        enemyList[0].GetComponent<EnemyBase>().PlayEnemyDeathPS();
                        enemyList[0].gameObject.SetActive(false);
                    }

                    EnvObjManager.sSingleton.DisableAllDestroyableObj();
				}
			}

			if (isAppearEnemy) AppearEnemyMinion ();
		}

        if (isBossDead && !CoroutineUtil.isCoroutine)
        {
            System.Action action = (() => ShowAndAddBonusScore());
            if (mIsShowScoreBonus) StartCoroutine(CoroutineUtil.WaitFor(2.0f, action));
        }
    }

	public void AddAttackAndMovementToMinion(PlayerNEnemyPrefabData data)
    {
        // Put all enemy move list into a List, regardless of group type.
        for (int i = 0; i < data.s1EnemyMinionMoveList.Count; i++)
        {
            EnemyInfo currInfo = data.s1EnemyMinionMoveList[i];
			mAllEnemyMinionInfoList.Add(currInfo);
        }

        // Put earliest-latest spawn time minion into new List.
        List<EnemyInfo> spawnFirstList = new List<EnemyInfo>();
		while (mAllEnemyMinionInfoList.Count != 0)
        {
			EnemyInfo minSpawnTimeInfo = mAllEnemyMinionInfoList[0];

            // TODO: Hard-coded start stage delay. To be changed.
            if (data.startStageDelay.Count != 0) mDelay = data.startStageDelay[0];

            float minSpwTime = minSpawnTimeInfo.spawnTime + mDelay;
            int index = 0;

			for (int i = 1; i < mAllEnemyMinionInfoList.Count; i++)
            {
				EnemyInfo currInfo = mAllEnemyMinionInfoList[i];
                if (currInfo.spawnTime + mDelay < minSpwTime)
                {
                    index = i;
                    minSpwTime = currInfo.spawnTime + mDelay;
                    minSpawnTimeInfo = currInfo;
                }
            }

			mAllEnemyMinionInfoList.RemoveAt(index);
            spawnFirstList.Add(minSpawnTimeInfo);
        }

        // Put spawnFirstList back into allEnemyMinionList.
		mAllEnemyMinionInfoList = spawnFirstList;

        // Testing purposes.
//        for (int i = 0; i < mAllEnemyMinionMoveInfoList.Count; i++)
//        {
//            Debug.Log(mAllEnemyMinionMoveInfoList[i].spawnTime);
//        }

        // Separate spawnFirstList into their respective groups.
        for (int i = 0; i < spawnFirstList.Count; i++)
        {
            EnemyInfo currInfo = spawnFirstList[i];

            if (currInfo.groupIndex == GroupIndex.MINION_1) mEnemyMinion1InfoList.Add(currInfo);
			else if (currInfo.groupIndex == GroupIndex.MINION_2) mEnemyMinion2InfoList.Add(currInfo);
			else if (currInfo.groupIndex == GroupIndex.MINION_3) mEnemyMinion3InfoList.Add(currInfo);
        }

        // Set attack and movement info into respective enemy.
        AddAttackAndMovementToSameInstantiatedPrefabs(mEnemyMinion1InfoList, mEnemyMinion1List, ref mMinion1SetIndex, ref mMinion1SavedSetIndex);
        AddAttackAndMovementToSameInstantiatedPrefabs(mEnemyMinion2InfoList, mEnemyMinion2List, ref mMinion2SetIndex, ref mMinion2SavedSetIndex);
        AddAttackAndMovementToSameInstantiatedPrefabs(mEnemyMinion3InfoList, mEnemyMinion3List, ref mMinion3SetIndex, ref mMinion3SavedSetIndex);
    }

    public void SetNextMinionMovement(GroupIndex groupIndex)
    {
        if (groupIndex == GroupIndex.MINION_1) 
            AddAttackAndMovementToNextPrefab(mEnemyMinion1InfoList, mEnemyMinion1List, ref mMinion1SetIndex, ref mMinion1SavedSetIndex);
        else if (groupIndex == GroupIndex.MINION_2) 
            AddAttackAndMovementToNextPrefab(mEnemyMinion2InfoList, mEnemyMinion2List, ref mMinion2SetIndex, ref mMinion2SavedSetIndex);
        else if (groupIndex == GroupIndex.MINION_3) 
            AddAttackAndMovementToNextPrefab(mEnemyMinion3InfoList, mEnemyMinion3List, ref mMinion3SetIndex, ref mMinion3SavedSetIndex);
    }

    public void InstantiateAndCacheEnemyBoss(Transform currEnemy)
    {
        Transform trans = Instantiate(currEnemy, currEnemy.position, Quaternion.identity);
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

			if (groupIndex == 0) mEnemyMinion1List.Add (trans);
			else if (groupIndex == 1) mEnemyMinion2List.Add (trans);
			else if (groupIndex == 2) mEnemyMinion3List.Add (trans);

            // Set sort order for enemy bullets.
//            SpriteRenderer transSr = trans.GetComponent<SpriteRenderer>();
//            if (transSr != null && transSr.sortingLayerName == TagManager.sSingleton.sortLayerTopG) 
//                transSr.sortingOrder = i;
        }
    }

    public Transform GetEnemyBossTrans()
    {
        Transform trans = mEnemyBossList[mBossIndex];
        if (mBossIndex + 1 > mEnemyBossList.Count - 1) mBossIndex = 0;
        else mBossIndex++;
        return trans;
    }
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

    public void AddToList(Transform trans) { enemyList.Add(trans); }
    public void RemoveFromList(Transform trans) { enemyList.Remove(trans); }

    public Vector2 GetClosestEnemyDir(Transform bullet)
    {
        if (enemyList.Count == 0) return Vector2.zero;
        
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

    void ShowAndAddBonusScore()
    {
        mIsShowScoreBonus = false;
        UIManager.sSingleton.ShowBonusScore(mBonusScore);

        if (GameManager.sSingleton.TotalNumOfPlayer() == 2)
        {
            int dividedScore = mBonusScore / 2;
            mPlayerController1.score += dividedScore;
            mPlayerController2.score += dividedScore;
        }
        else if (GameManager.sSingleton.IsThisPlayerActive(1)) mPlayerController1.score += mBonusScore;
        else if (GameManager.sSingleton.IsThisPlayerActive(2)) mPlayerController2.score += mBonusScore;

        if (GameManager.sSingleton.IsThisPlayerActive(1)) UIManager.sSingleton.UpdateScore(1, mPlayerController1.score);
        if (GameManager.sSingleton.IsThisPlayerActive(2)) UIManager.sSingleton.UpdateScore(2, mPlayerController2.score);

        StartCoroutine(CoroutineUtil.WaitFor(5.0f, AfterBossIsDead));
    }

    void AfterBossIsDead()
    {
        isBossDead = false;
        UIManager.sSingleton.EnableGameOverScreen();
    }

	void AppearEnemyMinion()
	{
		for (int i = mAppearIndex; i < mAllEnemyMinionInfoList.Count; i++)
		{
            if (mTimer >= mAllEnemyMinionInfoList[i].spawnTime + mDelay)
			{
				mAppearIndex = i + 1;
				if (mAllEnemyMinionInfoList[i].groupIndex == GroupIndex.MINION_1)
				{
					Transform currEnemy = mEnemyMinion1List[mMinion1Index].transform;
					currEnemy.position = mAllEnemyMinionInfoList[i].spawnPosition.position;
					currEnemy.gameObject.SetActive(true);

					if (mMinion1Index + 1 > mEnemyMinion1List.Count - 1) mMinion1Index = 0;
					else mMinion1Index++;
				}
                else if (mAllEnemyMinionInfoList[i].groupIndex == GroupIndex.MINION_2)
                {
                    Transform currEnemy = mEnemyMinion2List[mMinion2Index].transform;
                    currEnemy.position = mAllEnemyMinionInfoList[i].spawnPosition.position;
                    currEnemy.gameObject.SetActive(true);

                    if (mMinion2Index + 1 > mEnemyMinion2List.Count - 1) mMinion2Index = 0;
                    else mMinion2Index++;
                }
                else if (mAllEnemyMinionInfoList[i].groupIndex == GroupIndex.MINION_3)
                {
                    Transform currEnemy = mEnemyMinion3List[mMinion3Index].transform;
                    currEnemy.position = mAllEnemyMinionInfoList[i].spawnPosition.position;
                    currEnemy.gameObject.SetActive(true);

                    if (mMinion3Index + 1 > mEnemyMinion3List.Count - 1) mMinion3Index = 0;
                    else mMinion3Index++;
                }
			}
		}
	}

    // Set enemy attack and move pattern to everything in minionTransList.
    void AddAttackAndMovementToSameInstantiatedPrefabs(List<EnemyInfo> enemyInfoList, List<Transform> minionTransList, ref int currMinionIndex, ref int savedIndex)
    {
        bool isLoop = true;
        while(isLoop)
        {
            isLoop = AddAttackAndMovementToNextPrefab(enemyInfoList, minionTransList, ref currMinionIndex, ref savedIndex);
        }
    }

    // Set enemy attack and move pattern.
	bool AddAttackAndMovementToNextPrefab(List<EnemyInfo> enemyInfoList, List<Transform> minionTransList, ref int currMinionIndex, ref int savedIndex)
    {
        if (savedIndex + currMinionIndex > enemyInfoList.Count - 1) return false;
            
        EnemyInfo currInfo = enemyInfoList[savedIndex + currMinionIndex];

        // Get all attack pattern component from current enemy, then destroy all leaving only 1.
        AttackPattern[] currApArray = minionTransList[currMinionIndex].GetComponentsInChildren<AttackPattern>();
        for (int i = 1; i < currApArray.Length; i++)
        {
            Destroy(currApArray[i]);
        }

		// Set attack pattern.
        AttackPattern currAp = minionTransList[currMinionIndex].GetComponentInChildren<AttackPattern>();
        AttackPattern[] infoApArray = currInfo.attackPatternTrans.GetComponents<AttackPattern>();
        for (int i = 0; i < infoApArray.Length; i++)
        {
            if (i == 0)
                currAp.SetAttackPattern(infoApArray[0]);
            else
                minionTransList[currMinionIndex].transform.GetChild(1).gameObject.AddComponent<AttackPattern>().SetAttackPattern(infoApArray[i]);
        }

//        minionTransList[currMinionIndex].GetComponentInChildren<AttackPattern>().SetAttackPattern(currInfo.attackPattern);
//        minionTransList[currMinionIndex].GetComponent<Enemy1>().UpdateAttackPattern();

		// Set move pattern.
        minionTransList[currMinionIndex].GetComponent<EnemyMovement>().SetMovement(currInfo.spawnPosition, currInfo.movePattern.movementList[0]);

        // If the currMinionIndex is over the instantiated prefab count, reset it back to 0(use back the first prefab).
        if (currMinionIndex + 1 > minionTransList.Count - 1)
        {
            savedIndex += currMinionIndex + 1;
            currMinionIndex = 0;
            return false;
        }
        else currMinionIndex++;

        return true;
    }
}
