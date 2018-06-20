using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy1 : EnemyBase 
{
    List<List<AttackPattern>> mFullAtkList = new List<List<AttackPattern>>();
    IEnumerator mMovementCo;

    EnemyBase mEnemyBase;

    public override void Start()
    {
        base.Start();
        mEnemyBase = gameObject.GetComponentInParent<EnemyBase>();

        for (int i = 0; i < attackTransList.Count; i++)
        {
            List<AttackPattern> attackTypeList = new List<AttackPattern>();

            AttackPattern[] atkTypeArray = attackTransList[i].GetComponents<AttackPattern>();

            for (int j = 0; j < atkTypeArray.Length; j++)
            {
                attackTypeList.Add(atkTypeArray[j]);
            }
            mFullAtkList.Add(attackTypeList);
        }
    }

	public override void Update () 
    {
        base.Update();

        if (currActionNum < attackTransList.Count)
        {
            // Handle movement.
            if (mEnemyMovement.enabled == true && currActionNum < mEnemyMovement.movementList.Count && !mEnemyMovement.movementList[currActionNum].isCoroutine)
            {
                mMovementCo = mEnemyMovement.MoveToWayPoint(currActionNum);
                StartCoroutine(mMovementCo);
            }

            List<AttackPattern> currAtkSequence = mFullAtkList[currActionNum];
            for (int i = 0; i < currAtkSequence.Count; i++)
            {
                AttackPattern currAtkType = currAtkSequence[i];
                currAtkType.StartAttack(UpdateAttack);
            }

            float hpToMinus = currAtkSequence[0].hpPercentSkipAtk / 100 * totalHitPoint;
            float hpThresholdToSkip = totalHitPoint - hpToMinus;

            if (currHitPoint <= hpThresholdToSkip)
            {
                for (int i = 0; i < currAtkSequence.Count; i++)
                {
                    currAtkSequence[i].StopCoroutine();
                }
                UpdateAttack();
            }
        }
	}

    public void UpdateAttack()
    {
        mEnemyMovement.StopCurrMovement(mMovementCo);
        UIManager.sSingleton.DeactivateBossTimer();
        currActionNum++;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.player1BulletTag || other.tag == TagManager.sSingleton.player2BulletTag)
        {
            mEnemyBase.PullTrigger(other);
            if (mEnemyHealth != null) mEnemyHealth.UpdateHpBarUI(currHitPoint, totalHitPoint);
        }
    }
}
