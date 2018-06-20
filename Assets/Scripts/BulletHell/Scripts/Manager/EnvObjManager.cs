using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvObjManager : MonoBehaviour 
{
    public static EnvObjManager sSingleton { get { return _sSingleton; } }
    static EnvObjManager _sSingleton;

    public EnvObjPrefabData envObjPrefabData;

    int smallPowerUpIndex, bigPowerUpIndex, scoreIndex, lifeIndex, rockIndex, crateIndex;

    List<Transform> mSmallPowerUpList = new List<Transform>();
    List<Transform> mBigPowerUpList = new List<Transform>();
    List<Transform> mSmallScoreList = new List<Transform>();
    List<Transform> mBigScoreList = new List<Transform>();
    List<Transform> mLifeList = new List<Transform>();

    List<Transform> mRockList = new List<Transform>();
    List<Transform> mCrateList = new List<Transform>();

    List<Transform> mPlyHitBoxTransList = new List<Transform>();

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Start()
    {
        GameObject[] hitboxArray = GameObject.FindGameObjectsWithTag(TagManager.sSingleton.hitboxTag);
        for (int i = 0; i < hitboxArray.Length; i++)
        {
            mPlyHitBoxTransList.Add(hitboxArray[i].transform);
        }
    }

    public List<Transform> GetPlyHitBoxTransList { get { return mPlyHitBoxTransList; } }

    public Transform GetSmallPowerUp()
    {
        Transform currTrans = mSmallPowerUpList[smallPowerUpIndex];

        if (smallPowerUpIndex + 1 > mSmallPowerUpList.Count - 1) smallPowerUpIndex = 0;
        else smallPowerUpIndex++;

        return currTrans;
    }

    public Transform GetBigPowerUp()
    {
        Transform currTrans = mBigPowerUpList[bigPowerUpIndex];

        if (bigPowerUpIndex + 1 > mBigPowerUpList.Count - 1) bigPowerUpIndex = 0;
        else bigPowerUpIndex++;

        return currTrans;
    }

    public Transform GetScorePickUp()
    {
        Transform currTrans = mSmallScoreList[scoreIndex];

        if (scoreIndex + 1 > mSmallScoreList.Count - 1) scoreIndex = 0;
        else scoreIndex++;

        return currTrans;
    }
	
    public Transform GetBigScorePickUp()
    {
        Transform currTrans = mBigScoreList[scoreIndex];

        if (scoreIndex + 1 > mBigScoreList.Count - 1) scoreIndex = 0;
        else scoreIndex++;

        return currTrans;
    }

    public Transform GetLifePickUp()
    {
        Transform currTrans = mLifeList[lifeIndex];

        if (lifeIndex + 1 > mLifeList.Count - 1) lifeIndex = 0;
        else lifeIndex++;

        return currTrans;
    }

    public Transform GetRock()
    {
        Transform currTrans = mRockList[rockIndex];

        if (rockIndex + 1 > mRockList.Count - 1) rockIndex = 0;
        else rockIndex++;

        return currTrans;
    }

    public Transform GetCrate()
    {
        Transform currTrans = mCrateList[crateIndex];

        if (crateIndex + 1 > mCrateList.Count - 1) crateIndex = 0;
        else crateIndex++;

        return currTrans;
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

        for (int i = 0; i < total; i++)
        {
            Transform trans = Instantiate(currEnvObj, Vector3.zero, Quaternion.identity);
            trans.name = currEnvObj.name;
            trans.SetParent(go.transform);
            trans.gameObject.SetActive(false);

            if (groupList == "SmallPowerUp") mSmallPowerUpList.Add(trans);
            else if (groupList == "BigPowerUp") mBigPowerUpList.Add(trans);
            else if (groupList == "SmallScore") mSmallScoreList.Add(trans);
            else if (groupList == "BigScore") mBigScoreList.Add(trans);
            else if (groupList == "Life") mLifeList.Add(trans);
            else if (groupList == "Rock") mRockList.Add(trans);
            else if (groupList == "Crate") mCrateList.Add(trans);
        }
    }

    public void TransformBulletIntoScorePU(Vector3 pos)
    {
        Transform currPoint = mSmallScoreList[scoreIndex];
        currPoint.position = pos;
        currPoint.gameObject.SetActive(true);

        EnvironmentalObject currObj = currPoint.GetComponent<EnvironmentalObject>();
        currObj.SetTowardsRandomPlayer();
        scoreIndex++;
        if (smallPowerUpIndex + 1 > mSmallPowerUpList.Count - 1) smallPowerUpIndex = 0;
    }
}
