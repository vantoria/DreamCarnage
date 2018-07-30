using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy1 : EnemyBase 
{
    List<List<AttackPattern>> mFullAtkList = new List<List<AttackPattern>>();
    IEnumerator mMovementCo;

    float mTimer;
    EnemyBase mEnemyBase;

    public override void Start()
    {
        base.Start();
        mEnemyBase = gameObject.GetComponentInParent<EnemyBase>();
    }

    void OnEnable()
    {
        EnemyManager.sSingleton.AddToList(transform);
        UpdateAttackPattern();
    }

	public override void Update () 
    {
        base.Update();

		if (GameManager.sSingleton.IsBossMakeEntrance()) return;

        if (delayBeforeAttack != 0)
        {
            mTimer += Time.deltaTime;
            if (mTimer < delayBeforeAttack) return;
        }

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
                if (i == 0) currAtkType.StartAttack(UpdateAttack);
                else currAtkType.StartAttack(() => { });
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

				BulletManager.sSingleton.TransformEnemyBulsIntoScorePU ();
//                BulletManager.sSingleton.DisableEnemyBullets(false);
            }
        }
	}

    void UpdateAttack()
    {
        if (isBoss)
        {
            float hpToMinus = mFullAtkList[currActionNum][0].hpPercentSkipAtk / 100 * totalHitPoint;
            float hpThresholdToSkip = totalHitPoint - hpToMinus;

            if (mEnemyHealth != null) mEnemyHealth.ReduceHpBarUI(currHitPoint, hpThresholdToSkip, totalHitPoint);
            currHitPoint = hpThresholdToSkip;

            if (currHitPoint <= 0) EnemyDiedByTime();
        }

        mEnemyMovement.StopCurrMovement(mMovementCo);
        UIManager.sSingleton.DeactivateBossTimer();
        currActionNum++;
    }

    void UpdateAttackPattern()
    {
        List<List<AttackPattern>> atkList = new List<List<AttackPattern>>();

        for (int i = 0; i < attackTransList.Count; i++)
        {
            List<AttackPattern> attackTypeList = new List<AttackPattern>();

            AttackPattern[] atkTypeArray = attackTransList[i].GetComponents<AttackPattern>();
            if (atkTypeArray.Length == 0) return;

            for (int j = 0; j < atkTypeArray.Length; j++)
            {
                attackTypeList.Add(atkTypeArray[j]);
            }
            atkList.Add(attackTypeList);
        }
        mFullAtkList = atkList;
    }

    void OnTriggerStay2D(Collider2D other)
    {
		if (isBoss && GameManager.sSingleton.currState == GameManager.State.BOSS_MOVE_INTO_SCREEN) return;

        if (other.tag == TagManager.sSingleton.player1BulletTag || other.tag == TagManager.sSingleton.player2BulletTag ||
			other.tag == TagManager.sSingleton.magnumRadTag)
        {
            mEnemyBase.PullTrigger(other);
            if (mEnemyHealth != null) mEnemyHealth.UpdateHpBarUI(currHitPoint, totalHitPoint);
        }
    }

    void OnDisable()
    {
        mTimer = 0;
        EnemyManager.sSingleton.RemoveFromList(transform);
    }
}
