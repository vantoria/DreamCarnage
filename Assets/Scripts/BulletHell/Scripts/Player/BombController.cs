using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public float dualBombAppearSpeed;
	public Transform dualLinkLaserTrans;

    float duration = 3.0f;
    float returnDefaultSpdDur = 1.0f;

    float mTimeScale = 0.05f;
    float mFixedDeltaTime = 0.0005f, mSavedFixedDT;
    bool mIsUsingBomb = false;

    float mPotraitMaxAlpha, mPotraitMoveSpeed, mPotraitFadeSpeed, mPotraitStopTime, mPotraitDuration, mPotraitWaitDuration, mPotraitWaitMoveSpeed;

    ParticleSystem mCurrPS, mSavedPS;
    Image mEyeshotImage, mPotraitImage, mBlankEyeshotImage;

    PlayerController mPlayerController;
    BulletWipe mBulletWipe;

    void Start()
    {
        mPlayerController = GetComponent<PlayerController>();

        if (mPlayerController.playerID == 1)
        {
            mEyeshotImage = BombManager.sSingleton.leftEyeshotImage;
            mBlankEyeshotImage = BombManager.sSingleton.leftBlankEyeshotImage;
            mPotraitImage = BombManager.sSingleton.leftPotraitImage;
        }
        else if (mPlayerController.playerID == 2)
        {
            mEyeshotImage = BombManager.sSingleton.rightEyeshotImage;
            mBlankEyeshotImage = BombManager.sSingleton.rightBlankEyeshotImage;
            mPotraitImage = BombManager.sSingleton.rightPotraitImage;
        }
        BombManager.sSingleton.SetPlayerBombController(mPlayerController.playerID, this); 

        mPotraitMaxAlpha = BombManager.sSingleton.potraitMaxAlpha;
        mPotraitMoveSpeed = BombManager.sSingleton.potraitMoveSpeed;
        mPotraitFadeSpeed = BombManager.sSingleton.potraitFadeSpeed;
        mPotraitStopTime = BombManager.sSingleton.potraitStopTime;
        mPotraitDuration = BombManager.sSingleton.potraitDuration;
        mPotraitWaitDuration = BombManager.sSingleton.potraitWaitDuration;
        mPotraitWaitMoveSpeed = BombManager.sSingleton.potraitWaitMoveSpeed;

        mSavedFixedDT = Time.fixedDeltaTime;

		if (type == Type.TIME_STOP) 
		{
            mSavedPS = BombManager.sSingleton.timeStopPE;
            mSavedPS.gameObject.SetActive(false);

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

    void Update()
    {
        if (UIManager.sSingleton.IsPauseGameOverMenu || UIManager.sSingleton.IsShowScoreRankNameInput) return;

        if (mIsUsingBomb && !BombManager.sSingleton.IsPause && !BombManager.sSingleton.IsShooting)
        {
            if (type == Type.TIME_STOP)
            {
                mCurrPS.Simulate(Time.unscaledDeltaTime, true, false);
                Transform bombTrans = mCurrPS.transform;
                bombTrans.position = transform.position;
                bombTrans.gameObject.SetActive(true);
            }
        }
    }

    public void ActivateBomb()
    {
		if (mIsUsingBomb) return;

		mIsUsingBomb = true;

        // Potrait moving up or down after activated bomb.
        if (mPlayerController.playerID == 1) StartCoroutine(MoveUp(true, mPotraitImage, mPotraitMoveSpeed, mPotraitFadeSpeed, mPotraitStopTime, mPotraitDuration, mPotraitWaitDuration));
        else StartCoroutine(MoveUp(false, mPotraitImage, mPotraitMoveSpeed, mPotraitFadeSpeed, mPotraitStopTime, mPotraitDuration, mPotraitWaitDuration));

        if (type == Type.TIME_STOP)
        {
            mCurrPS = Instantiate(mSavedPS);
			BombManager.sSingleton.isTimeStopBomb = true;

            Time.timeScale = mTimeScale;
            Time.fixedDeltaTime = mFixedDeltaTime;
            StartCoroutine(TimeStopSequence(duration, returnDefaultSpdDur));
            AudioManager.sSingleton.SetMinBGM_Pitch();
        }
        else if(type == Type.BULLET_WIPE)
		{
            Transform bombTrans = BombManager.sSingleton.bomb_BulletWipeTrans;
            bombTrans.gameObject.SetActive(true);
            mBulletWipe.Activate();
		}
		else if(type == Type.SHIELD_AREA)
		{
            ParticleSystem ps = BombManager.sSingleton.bombShieldPE;
            Transform bombTrans = ps.transform;
            bombTrans.position = transform.position;
            bombTrans.GetComponent<ConvertEnemyBullet>().PlayerID = mPlayerController.playerID;
            bombTrans.gameObject.SetActive(true);
            ps.Play();
            StartCoroutine(StartBombDuration(duration, DeactivateShieldArea));
		}
    }

    public void ActivateDualLinkBomb()
    {
        mIsUsingBomb = true;

        // Potrait moving up or down after activated bomb.
        if (mPlayerController.playerID == 1) StartCoroutine(MoveUp(true, mPotraitImage, mPotraitMoveSpeed, mPotraitFadeSpeed, mPotraitStopTime, mPotraitDuration, mPotraitWaitDuration));
        else StartCoroutine(MoveUp(false, mPotraitImage, mPotraitMoveSpeed, mPotraitFadeSpeed, mPotraitStopTime, mPotraitDuration, mPotraitWaitDuration));

        // Deactivation of eyeshot image.
        StartCoroutine(IEAlphaSequence(mEyeshotImage, 0, () => { }));
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
        StartCoroutine(IEAlphaSequence(mEyeshotImage, 1, () => { }));
        BombManager.sSingleton.ActivateBlankPotrait(mPlayerController.playerID);
    }

    public void ResetDualLinkVal()
    {
        StartCoroutine(IEAlphaSequence(mEyeshotImage, 0, () => { }));
        BombManager.sSingleton.DeactivateBlankPotrait(mPlayerController.playerID);
        BombManager.sSingleton.dualLinkState = BombManager.DualLinkState.NONE;
    }

    public void DeactivateBulletWipe()
    {
        Transform bombTrans = BombManager.sSingleton.bomb_BulletWipeTrans;
        bombTrans.gameObject.SetActive(false);
        mIsUsingBomb = false;
    }

    public bool IsUsingBomb 
    { 
        get { return mIsUsingBomb; } 
        set { mIsUsingBomb = value; }
    }

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------- Private Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void DeactivateShieldArea() 
    { 
        Debug.Log("Deactivate shield area");
        ParticleSystem ps = BombManager.sSingleton.bombShieldPE;
        Transform bombTrans = ps.transform;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        bombTrans.gameObject.SetActive(false);
        mIsUsingBomb = false;
    }

    IEnumerator StartBombDuration (float dur, Action doLast)
    {
        yield return new WaitForSeconds(dur);//StartCoroutine(CoroutineUtil.WaitForRealSeconds(dur));
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

        mCurrPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Destroy(mCurrPS.gameObject);
    }

    IEnumerator IEAlphaSequence (Image image, float toAlpha, Action doLast)
    {
        Color color = Color.white;
        if (image.color.a < toAlpha)
        {
            while (image.color.a < toAlpha)
            {
                color = image.color;
                color.a += Time.unscaledDeltaTime * dualBombAppearSpeed;

                if (color.a > toAlpha) color.a = toAlpha;
                image.color = color;

                yield return null;
            }
        }
        else
        {
            while (image.color.a > toAlpha)
            {
                color = image.color;
                color.a -= Time.unscaledDeltaTime * dualBombAppearSpeed;

                if (color.a < toAlpha) color.a = toAlpha;
                image.color = color;

                yield return null;
            }
        }
        doLast();
    }

    IEnumerator MoveUp(bool isMoveUp, Image image, float moveSpeed, float fadeSpeed, float stopTime, float duration, float waitDur)
    {
        float timer = 0, waitTimer = 0;
        Vector3 pos = image.transform.position;
        Vector3 defaultPos = pos;
        bool isDelay = true;

        while (timer < duration)
        {
            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            float moveVal = Time.unscaledDeltaTime * moveSpeed;
            if (!isMoveUp) moveVal = -moveVal;

            pos.y += moveVal;
            image.transform.position = pos;

            if (timer < stopTime)
            {
                Color color = image.color;
                color.a += Time.unscaledDeltaTime * fadeSpeed;
                if (color.a > mPotraitMaxAlpha) color.a = mPotraitMaxAlpha;
                image.color = color;
            }
            else if (timer > stopTime)
            {
                while (isDelay)
                {
                    while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
                    {
                        yield return null;
                    }

                    waitTimer += Time.unscaledDeltaTime;
                    if (waitTimer > waitDur) isDelay = false;

                    moveVal = Time.unscaledDeltaTime * mPotraitWaitMoveSpeed;
                    if (!isMoveUp) moveVal = -moveVal;

                    pos.y += moveVal;
                    image.transform.position = pos;
                    yield return null;
                }

                Color color = image.color;
                color.a -= Time.unscaledDeltaTime * fadeSpeed;
                if (color.a < 0) color.a = 0;
                image.color = color;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        image.transform.position = defaultPos;

        Color endColor = image.color;
        endColor.a = 0;
        image.color = endColor;
    }
}
