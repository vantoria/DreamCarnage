using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RepelStatus : MonoBehaviour 
{
    public Transform trapLaser;

    int mPlayerID;
    float mStatRepelDur, mRepelValue;

    PlayerController mPlayerController;
    TrapLaser mTrapLaser;

    void Start()
    {
        mPlayerController = GetComponentInParent<PlayerController>();
        mTrapLaser = trapLaser.GetComponent<TrapLaser>();

        mPlayerID = mPlayerController.playerID;
    }

    public void SetStatRepel(AttackPattern.Properties prop)
    {
        mPlayerController.moveSpeed = prop.slowValue;
        mStatRepelDur = prop.giveStatRepelDur;
        mRepelValue = prop.repelValue;
        StartCoroutine(HandleTimerSequence(mStatRepelDur, true, ResetStatRepel));

        mTrapLaser.InitializeTrapLaser(mPlayerController.playerID, prop);
    }

    IEnumerator HandleTimerSequence(float duration, bool isUnsDeltaTime, Action doLast)
    {
        while (duration > 0)
        {
            if (isUnsDeltaTime) duration -= Time.unscaledDeltaTime;
            else duration -= Time.deltaTime;
            yield return null;
        }
        doLast();
    }

    void ResetStatRepel() 
    { 
        mStatRepelDur = 0; 
        mPlayerController.SlowlyReturnDefaultSpeed(true);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        //TODO: Something to do here?
//        if(other.tag == TagManager.sSingleton.
        if (mStatRepelDur > 0 && other.tag == TagManager.sSingleton.repelStat)
        {
            Transform p1 = GameManager.sSingleton.player1;
            Transform p2 = GameManager.sSingleton.player2;

            Vector2 dir = Vector2.zero;

            if (mPlayerID == 1) dir = (Vector2)(p2.position - p1.position).normalized;
            else dir = (Vector2)(p1.position - p2.position).normalized;
                
            other.transform.parent.GetComponent<Rigidbody2D>().AddForce (dir * mRepelValue, ForceMode2D.Impulse);
        }
    }
}
