using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BombController : MonoBehaviour 
{
    public enum Type
    {
        NONE = 0, 
        TIME_STOP,
		BULLET_WIPE,
		SHIELD_AREA
    }
    public Type type = Type.NONE;

    public SpriteRenderer potraitSR;

    public float dualBombAppearSpeed;
	public Transform dualLinkLaserTrans;

    float duration = 3.0f;
    float returnDefaultSpdDur = 1.0f;

    float mTimeScale = 0.05f;
    float mFixedDeltaTime = 0.0005f, mSavedFixedDT;
    bool mIsUsingBomb = false;

    PlayerController mPlayerController;
    BulletWipe mBulletWipe;

    void Start()
    {
        mPlayerController = GetComponent<PlayerController>();

        mSavedFixedDT = Time.fixedDeltaTime;

		if (type == Type.TIME_STOP) 
		{
			duration = BombManager.sSingleton.bombTimeStopDur;
			returnDefaultSpdDur = BombManager.sSingleton.bombReturnSpdDur;
		}
        else if(type == Type.BULLET_WIPE)
		{
            mBulletWipe = BombManager.sSingleton.bomb_BulletWipeTrans.GetComponent<BulletWipe>();
            mBulletWipe.SetOwnerBombCtrl(this);
		}
		else if(type == Type.SHIELD_AREA)
		{
			duration = BombManager.sSingleton.bombShieldAreaDur;
		}
    }

    public void ActivateBomb()
    {
		if (mIsUsingBomb) return;

		mIsUsingBomb = true;
        if (type == Type.TIME_STOP)
        {
			BombManager.sSingleton.isTimeStopBomb = true;

            Time.timeScale = mTimeScale;
            Time.fixedDeltaTime = mFixedDeltaTime;
            StartCoroutine(TimeStopSequence(duration, returnDefaultSpdDur));
        }
        else if(type == Type.BULLET_WIPE)
		{
            Transform bombTrans = BombManager.sSingleton.bomb_BulletWipeTrans;
            bombTrans.gameObject.SetActive(true);
            mBulletWipe.Activate();
		}
		else if(type == Type.SHIELD_AREA)
		{
            Transform bombTrans = BombManager.sSingleton.bombShieldTrans;
            bombTrans.position = transform.position;
            bombTrans.GetComponent<ConvertEnemyBullet>().PlayerID = mPlayerController.playerID;
            bombTrans.gameObject.SetActive(true);
            StartCoroutine(StartBombDuration(duration, DeactivateShieldArea));
		}
    }

    public void ActivateDualLinkBomb()
    {
        mIsUsingBomb = true;
        StartCoroutine(IEAlphaSequence(potraitSR, 0, () => { }));
        dualLinkLaserTrans.gameObject.SetActive (true);
    }

    public void DeactivateDualLinkBomb()
    {
        mIsUsingBomb = false;
        dualLinkLaserTrans.gameObject.SetActive (false);
        mPlayerController.ResetLinkBar();
    }

    public void ActivatePotrait()
    {
        StartCoroutine(IEAlphaSequence(potraitSR, 1, () => { }));
    }

    public void ResetDualLinkVal()
    {
        StartCoroutine(IEAlphaSequence(potraitSR, 0, () => { }));
        BombManager.sSingleton.dualLinkState = BombManager.DualLinkState.NONE;
    }

    public void DeactivateBulletWipe()
    {
        Transform bombTrans = BombManager.sSingleton.bomb_BulletWipeTrans;
        bombTrans.gameObject.SetActive(false);
        mIsUsingBomb = false;
    }

    public bool IsUsingBomb { get { return mIsUsingBomb; } }

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------- Private Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void DeactivateShieldArea() 
    { 
        Transform bombTrans = BombManager.sSingleton.bombShieldTrans;
        bombTrans.gameObject.SetActive(false);
        mIsUsingBomb = false;
    }

    IEnumerator StartBombDuration (float dur, Action doLast)
    {
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(dur));
        doLast();
    }
        
    IEnumerator TimeStopSequence (float stopDur, float returnSpdDur)
    {
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(stopDur));

        float currTime = 0;
        float maxVal = (1 - mTimeScale);

        while(currTime < returnSpdDur)
        {
			while (UIManager.sSingleton.IsPauseGameOverMenu)
            {
                yield return null;
            }

            currTime += Time.unscaledDeltaTime;

            float val = currTime / returnSpdDur * maxVal; 
            if (val > maxVal) val = maxVal;

            Time.timeScale = mTimeScale + val;
            Time.fixedDeltaTime = mFixedDeltaTime + (val / 100);
            yield return null;
        }

        Time.timeScale = 1;
        Time.fixedDeltaTime = mSavedFixedDT;
        mIsUsingBomb = false;
		BombManager.sSingleton.isTimeStopBomb = false;
    }

    IEnumerator IEAlphaSequence (SpriteRenderer sr, float toAlpha, Action doLast)
    {
        Color color = Color.white;
        if (sr.color.a < toAlpha)
        {
            while (sr.color.a < toAlpha)
            {
                color = sr.color;
                color.a += Time.unscaledDeltaTime * dualBombAppearSpeed;

                if (color.a > toAlpha) color.a = toAlpha;
                sr.color = color;

                yield return null;
            }
        }
        else
        {
            while (sr.color.a > toAlpha)
            {
                color = sr.color;
                color.a -= Time.unscaledDeltaTime * dualBombAppearSpeed;

                if (color.a < toAlpha) color.a = toAlpha;
                sr.color = color;

                yield return null;
            }
        }
        doLast();
    }
}
