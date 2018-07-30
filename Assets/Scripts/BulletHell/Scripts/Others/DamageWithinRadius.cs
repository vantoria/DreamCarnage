using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageWithinRadius : MonoBehaviour 
{
    public int damage;
    public float stayDuration = 0.25f;
    public bool isOneTimeDamage = true;
	public int playerID = 1;

	List<Transform> mHitList = new List<Transform>();
	float mTimer;
	
	void Update () 
	{
		mTimer += Time.deltaTime;
		if (mTimer >= stayDuration) 
		{
			mTimer = 0;
			gameObject.SetActive (false);
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		for (int i = 0; i < mHitList.Count; i++) 
		{
			if (mHitList [i].transform == other.transform) return;
		}
		mHitList.Add (other.transform);
	}

	void OnDisable()
	{
		for (int i = 0; i < mHitList.Count; i++) 
		{
			if (mHitList [i].GetComponent<EnemyBase> () != null) 
			{
				EnemyBase enemyBase = mHitList [i].GetComponent<EnemyBase> ();
				enemyBase.isHitByMagnumRadius = false;
			}
		}
	}
}
