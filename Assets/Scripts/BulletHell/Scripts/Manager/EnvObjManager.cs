using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvObjManager : MonoBehaviour 
{
    public static EnvObjManager sSingleton { get { return _sSingleton; } }
    static EnvObjManager _sSingleton;

    public EnvObjPrefabData envObjPrefabData;
	public bool isAppearRocks = true;

    [System.Serializable]
    public class Rocks
    {
        public enum Rock
        {
            Rock1 = 0, Rock2, Rock3, Rock4, Rock5, Rock6
        }
        public Rock rock = Rock.Rock1;

        public Transform spawnPosition;
        public float spawnTime;
    }

	int mSmallPowerUpIndex, mBigPowerUpIndex, mScoreIndex, mLifeIndex, mRockIndex, mCrateIndex, mKillScoreIndex, mMagnumRadiusIndex;
	int mDeathPSIndex, mGetHitImpactPSIndex, mGetHitBigImpactPSIndex, mRockDestroyIndex, mRifleIndex, mSniperIndex, mMagnumIndex, mPowerUpTextIndex;

    List<Transform> mSmallPowerUpList = new List<Transform>();
    List<Transform> mBigPowerUpList = new List<Transform>();
    List<Transform> mSmallScoreList = new List<Transform>();
    List<Transform> mBigScoreList = new List<Transform>();
    List<Transform> mLifeList = new List<Transform>();

    List<List<Transform>> mRockList = new List<List<Transform>>();
    List<Transform> mCrateList = new List<Transform>();

    List<Transform> mMagnumRadiusList = new List<Transform>();
    List<Transform> mKillScoreList = new List<Transform>();
	List<Transform> mPowerUpTextList = new List<Transform>();

	List<ParticleSystem> mDeathPSList = new List<ParticleSystem>();
	List<ParticleSystem> mGetHitImpactPSList = new List<ParticleSystem>();
    List<ParticleSystem> mGetHitBigImpactPSList = new List<ParticleSystem>();
    List<ParticleSystem> mRifleShotSparksPSList = new List<ParticleSystem>();
    List<ParticleSystem> mSniperShotSparksPSList = new List<ParticleSystem>();
    List<ParticleSystem> mMagnumShotSparksPSList = new List<ParticleSystem>();
	List<ParticleSystem> mRockDestroyPSList = new List<ParticleSystem>();

    List<Transform> mPlyHitBoxTransList = new List<Transform>();

    List<Rocks> mAllRockList = new List<Rocks>();
    int mAppearIndex = 0;
    float mTimer, mDelay;

    List<Transform> mAllDestroyableObj = new List<Transform>();

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Start()
    {
        Transform hitbox1 = GameManager.sSingleton.player1.Find(TagManager.sSingleton.hitboxTag);
        Transform hitbox2 = GameManager.sSingleton.player2.Find(TagManager.sSingleton.hitboxTag);

        mPlyHitBoxTransList.Add(hitbox1);
        mPlyHitBoxTransList.Add(hitbox2);
    }

    void Update()
    {
		if (!isAppearRocks || GameManager.sSingleton.currState != GameManager.State.BATTLE) return;

        if (!UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
        {
            mTimer += Time.deltaTime;
            for (int i = mAppearIndex; i < mAllRockList.Count; i++)
            {
                if (mTimer >= mAllRockList[i].spawnTime + mDelay)
                {
                    mAppearIndex = i + 1;
                    Transform currRock = GetRock((int)mAllRockList[i].rock);
                    currRock.position = mAllRockList[i].spawnPosition.position;
                    currRock.gameObject.SetActive(true);
                }
            }
        }
    }

    public void AddToList(Transform trans) { mAllDestroyableObj.Add(trans); }
    public void RemoveFromList(Transform trans) { mAllDestroyableObj.Remove(trans); }
    public void DisableAllDestroyableObj()
    {
        int count = mAllDestroyableObj.Count;
        for (int i = 0; i < count; i++)
        {
            mAllDestroyableObj[0].gameObject.SetActive(false);
        }
    }

    public List<Transform> GetPlyHitBoxTransList { get { return mPlyHitBoxTransList; } }

    //----------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------- Pickable objects --------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------

    public Transform GetSmallPowerUp()
    {
        Transform currTrans = mSmallPowerUpList[mSmallPowerUpIndex];

        if (mSmallPowerUpIndex + 1 > mSmallPowerUpList.Count - 1) mSmallPowerUpIndex = 0;
        else mSmallPowerUpIndex++;

        return currTrans;
    }

    public Transform GetBigPowerUp()
    {
        Transform currTrans = mBigPowerUpList[mBigPowerUpIndex];

        if (mBigPowerUpIndex + 1 > mBigPowerUpList.Count - 1) mBigPowerUpIndex = 0;
        else mBigPowerUpIndex++;

        return currTrans;
    }

    public Transform GetScorePickUp()
    {
		Transform currTrans = mSmallScoreList[mSmallPowerUpIndex];

		if (mSmallPowerUpIndex + 1 > mSmallScoreList.Count - 1) mSmallPowerUpIndex = 0;
		else mSmallPowerUpIndex++;

        return currTrans;
    }
	
    public Transform GetBigScorePickUp()
    {
		Transform currTrans = mBigScoreList[mBigPowerUpIndex];

		if (mBigPowerUpIndex + 1 > mBigScoreList.Count - 1) mBigPowerUpIndex = 0;
		else mBigPowerUpIndex++;

        return currTrans;
    }

    public Transform GetLifePickUp()
    {
        Transform currTrans = mLifeList[mLifeIndex];

        if (mLifeIndex + 1 > mLifeList.Count - 1) mLifeIndex = 0;
        else mLifeIndex++;

        return currTrans;
    }

    //----------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------ Destroyable objects -------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------

    public Transform GetRock(int index)
    {
        Transform currTrans = mRockList[index][mRockIndex];

        if (mRockIndex + 1 > mRockList[index].Count - 1) mRockIndex = 0;
        else mRockIndex++;

        return currTrans;
    }

    public Transform GetCrate()
    {
        Transform currTrans = mCrateList[mCrateIndex];

        if (mCrateIndex + 1 > mCrateList.Count - 1) mCrateIndex = 0;
        else mCrateIndex++;

        return currTrans;
    }

    //----------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------- UI ----------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------

	public Transform GetKillScore()
	{
		Transform currTrans = mKillScoreList[mKillScoreIndex];

		if (mKillScoreIndex + 1 > mKillScoreList.Count - 1) mKillScoreIndex = 0;
		else mKillScoreIndex++;

		return currTrans;
	}

    public Transform GetPowerUpText()
    {
        Transform currTrans = mPowerUpTextList[mPowerUpTextIndex];

        if (mPowerUpTextIndex + 1 > mPowerUpTextList.Count - 1) mPowerUpTextIndex = 0;
        else mPowerUpTextIndex++;

        return currTrans;
    }

    //----------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------- Particle Effects --------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------

	public ParticleSystem GetEnemyDeathPS()
	{
		ParticleSystem currPS = mDeathPSList[mDeathPSIndex];

		if (mDeathPSIndex + 1 > mDeathPSList.Count - 1) mDeathPSIndex = 0;
		else mDeathPSIndex++;

		return currPS;
	}

	public ParticleSystem GetEnemyHitImpactPS()
	{
		ParticleSystem currPS = mGetHitImpactPSList[mGetHitImpactPSIndex];

		if (mGetHitImpactPSIndex + 1 > mGetHitImpactPSList.Count - 1) mGetHitImpactPSIndex = 0;
		else mGetHitImpactPSIndex++;

		return currPS;
	}

	public ParticleSystem GetEnemyHitBigImpactPS()
	{
		ParticleSystem currPS = mGetHitBigImpactPSList[mGetHitBigImpactPSIndex];

		if (mGetHitBigImpactPSIndex + 1 > mGetHitBigImpactPSList.Count - 1) mGetHitBigImpactPSIndex = 0;
		else mGetHitBigImpactPSIndex++;

		return currPS;
	}

    public ParticleSystem GetRifleShotSparksPS()
    {
        ParticleSystem currPS = mRifleShotSparksPSList[mRifleIndex];

        if (mRifleIndex + 1 > mRifleShotSparksPSList.Count - 1) mRifleIndex = 0;
        else mRifleIndex++;

        return currPS;
    }

    public ParticleSystem GetSniperShotSparksPS()
    {
        ParticleSystem currPS = mSniperShotSparksPSList[mSniperIndex];

        if (mSniperIndex + 1 > mSniperShotSparksPSList.Count - 1) mSniperIndex = 0;
        else mSniperIndex++;

        return currPS;
    }

    public ParticleSystem GetMagnumShotSparksPS()
    {
        ParticleSystem currPS = mMagnumShotSparksPSList[mMagnumIndex];

        if (mMagnumIndex + 1 > mMagnumShotSparksPSList.Count - 1) mMagnumIndex = 0;
        else mMagnumIndex++;

        return currPS;
    }

    public ParticleSystem GetRockDestroyPS()
    {
        ParticleSystem currPS = mRockDestroyPSList[mRockDestroyIndex];

        if (mRockDestroyIndex + 1 > mRockDestroyPSList.Count - 1) mRockDestroyIndex = 0;
        else mRockDestroyIndex++;

        return currPS;
    }

    //----------------------------------------------------------------------------------------------------------------------

    public Transform GetMagnumRadius()
    {
        Transform currTrans = mMagnumRadiusList[mMagnumRadiusIndex];

        if (mMagnumRadiusIndex + 1 > mMagnumRadiusList.Count - 1) mMagnumRadiusIndex = 0;
        else mMagnumRadiusIndex++;

        return currTrans;
    }

    public void SortRockSpawnTime(PlayerNEnemyPrefabData data)
    {
        // Put all rocks into a List, regardless of rock type.
        for (int i = 0; i < data.s1RockSpawnList.Count; i++)
        {
            Rocks currRock = data.s1RockSpawnList[i];
            mAllRockList.Add(currRock);
        }

        // Put earliest-latest spawn time rock into new List.
        List<Rocks> spawnFirstList = new List<Rocks>();
        while (mAllRockList.Count != 0)
        {
            Rocks minSpawnTimeRock = mAllRockList[0];

            // TODO: Hard-coded start stage delay. To be changed.
            if (data.startStageDelay.Count != 0) mDelay = data.startStageDelay[0];

            float minSpwTime = minSpawnTimeRock.spawnTime + mDelay;
            int index = 0;

            for (int i = 1; i < mAllRockList.Count; i++)
            {
                Rocks currRock = mAllRockList[i];
                if (currRock.spawnTime + mDelay < minSpwTime)
                {
                    index = i;
                    minSpwTime = currRock.spawnTime + mDelay;
                    minSpawnTimeRock = currRock;
                }
            }

            mAllRockList.RemoveAt(index);
            spawnFirstList.Add(minSpawnTimeRock);
        }

        // Put spawnFirstList back into mAllRockList.
        mAllRockList = spawnFirstList;

        // Testing purposes.
//        for (int i = 0; i < mAllRockList.Count; i++)
//        {
//            Debug.Log(mAllRockList[i].spawnTime);
//        }
    }

	public void MoveAllPUToRandPlayer()
	{
		int rand = -1;
		if (GameManager.sSingleton.TotalNumOfPlayer() == 1) rand = 1;
		else rand = Random.Range(1, 3);

		MoveAllPUToPlayer (rand);
	}

	public void MoveAllPUToPlayer(int playerID)
	{
		Transform hitbox = mPlyHitBoxTransList[playerID - 1];
		for (int i = 0; i < mSmallPowerUpList.Count; i++) 
		{
			Transform currPickUp = mSmallPowerUpList[i];
			if (currPickUp.gameObject.activeSelf) currPickUp.GetComponent<EnvironmentalObject> ().SetPlayerOverrideSpd (hitbox);
		}

		for (int i = 0; i < mBigPowerUpList.Count; i++) 
		{
			Transform currPickUp = mBigPowerUpList[i];
			if (currPickUp.gameObject.activeSelf) currPickUp.GetComponent<EnvironmentalObject> ().SetPlayerOverrideSpd (hitbox);
		}

		for (int i = 0; i < mSmallScoreList.Count; i++) 
		{
			Transform currPickUp = mSmallScoreList[i];
			if (currPickUp.gameObject.activeSelf) currPickUp.GetComponent<EnvironmentalObject> ().SetPlayerOverrideSpd (hitbox);
		}

		for (int i = 0; i < mBigScoreList.Count; i++) 
		{
			Transform currPickUp = mBigScoreList[i];
			if (currPickUp.gameObject.activeSelf) currPickUp.GetComponent<EnvironmentalObject> ().SetPlayerOverrideSpd (hitbox);
		}

		for (int i = 0; i < mLifeList.Count; i++) 
		{
			Transform currPickUp = mLifeList[i];
			if (currPickUp.gameObject.activeSelf) currPickUp.GetComponent<EnvironmentalObject> ().SetPlayerOverrideSpd (hitbox);
		}
	}

    public void InstantiateAndCacheEnvObj(Transform currEnvObj, int total)
    {
        // Group name.
        GameObject go = new GameObject();
        go.name = currEnvObj.name;   

        string groupList = "";
        if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_PowerUp1Tag) groupList = "SmallPowerUp";
        else if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_PowerUp2Tag) groupList = "BigPowerUp";
        else if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp1Tag) groupList = "SmallScore";
        else if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp2Tag) groupList = "BigScore";
        else if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_LifePickUpTag) groupList = "Life";
        else if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_RockTag) groupList = "Rock";
        else if (currEnvObj.tag == TagManager.sSingleton.ENV_OBJ_CrateTag) groupList = "Crate";

        List<Transform> rockList = new List<Transform>();
        for (int i = 0; i < total; i++)
        {
            Transform trans = Instantiate(currEnvObj, Vector3.zero, Quaternion.identity);
            trans.name = currEnvObj.name;
            trans.SetParent(go.transform);
//			trans.GetComponent<EnvironmentalObject> ().Initialize ();
            trans.gameObject.SetActive(false);

            if (groupList == "SmallPowerUp") mSmallPowerUpList.Add(trans);
            else if (groupList == "BigPowerUp") mBigPowerUpList.Add(trans);
            else if (groupList == "SmallScore") mSmallScoreList.Add(trans);
            else if (groupList == "BigScore") mBigScoreList.Add(trans);
            else if (groupList == "Life") mLifeList.Add(trans);
            else if (groupList == "Rock") rockList.Add(trans);
            else if (groupList == "Crate") mCrateList.Add(trans);
        }

        if (groupList == "Rock") mRockList.Add(rockList);
    }

    public void InstantiateAndCacheUI(Transform objTrans, int total)
	{
		Transform canvasTrans = GameObject.FindGameObjectWithTag (TagManager.sSingleton.canvasTag).transform;

		// Group name.
		GameObject go = new GameObject();
        go.name = objTrans.name;
		go.transform.SetParent (canvasTrans);

		for (int i = 0; i < total; i++) 
		{
            Transform trans = Instantiate (objTrans, Vector3.zero, Quaternion.identity);
            trans.name = objTrans.name;
			trans.SetParent (go.transform);
			trans.gameObject.SetActive (false);

            if (trans.name == TagManager.sSingleton.UI_KillScoreName) mKillScoreList.Add (trans);
            else if (trans.name == TagManager.sSingleton.UI_PowerUpTextName) mPowerUpTextList.Add (trans);
		}
	}

    public void InstantiateAndCache(Transform objTrans, int total)
    {
        // Group name.
        GameObject go = new GameObject();
        go.name = objTrans.name;

        for (int i = 0; i < total; i++) 
        {
            Transform trans = Instantiate (objTrans, Vector3.zero, Quaternion.identity);
            trans.name = objTrans.name;
            trans.SetParent (go.transform);
            trans.gameObject.SetActive (false);

			if (trans.name == TagManager.sSingleton.magnumRadTag) 
			{
				trans.GetComponent<DamageWithinRadius> ().playerID = GameManager.sSingleton.MagnumPlayerID;
				mMagnumRadiusList.Add (trans);
			}
        }
    }

	public void InstantiateAndCachePS(Transform ps, int total)
	{
		// Group name.
		GameObject go = new GameObject();
		go.name = ps.name;

		for (int i = 0; i < total; i++) 
		{
            Transform trans = Instantiate (ps, Vector3.zero, ps.rotation);
			trans.name = ps.name;
			trans.SetParent (go.transform);

			if (ps.name == TagManager.sSingleton.UI_DeathWaveName) mDeathPSList.Add (trans.GetComponent<ParticleSystem>());
			else if (ps.name == TagManager.sSingleton.UI_GetHitImpactName) mGetHitImpactPSList.Add (trans.GetComponent<ParticleSystem>());
            else if (ps.name == TagManager.sSingleton.UI_GetHitImpactBigName) mGetHitBigImpactPSList.Add (trans.GetComponent<ParticleSystem>());
            else if (ps.name == TagManager.sSingleton.UI_RifleShotSparksName) mRifleShotSparksPSList.Add (trans.GetComponent<ParticleSystem>());
            else if (ps.name == TagManager.sSingleton.UI_SniperShotSparksName) mSniperShotSparksPSList.Add (trans.GetComponent<ParticleSystem>());
            else if (ps.name == TagManager.sSingleton.UI_MagnumShotSparksName) mMagnumShotSparksPSList.Add (trans.GetComponent<ParticleSystem>());
            else if (ps.name == TagManager.sSingleton.UI_RockDestroyName) mRockDestroyPSList.Add (trans.GetComponent<ParticleSystem>());
		}
	}

    public void TransformBulletIntoScorePU(Vector3 pos)
    {
		Transform currPoint = mSmallScoreList[mSmallPowerUpIndex];
        currPoint.position = pos;
        currPoint.gameObject.SetActive(true);

        EnvironmentalObject currObj = currPoint.GetComponent<EnvironmentalObject>();
        currObj.SetTowardsRandomPlayer(3.0f);
		mSmallPowerUpIndex++;
        if (mSmallPowerUpIndex + 1 > mSmallPowerUpList.Count - 1) mSmallPowerUpIndex = 0;
    }
}
