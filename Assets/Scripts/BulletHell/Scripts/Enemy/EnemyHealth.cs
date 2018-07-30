using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour 
{
    public float refillBarDuration = 1.5f;
    public float reduceBarDuration = 0.5f;

    public Transform mEnemyTrans;

    bool mIsFillingUp = false;
    float mFillAmount = -1;
    Image mHpBar;

	void Start () 
    {
        mHpBar = GetComponent<Image>();
	}
	
	void Update () 
    {
        if (mEnemyTrans == null) return;
        transform.position = mEnemyTrans.position;
	}

    public void SetOwner(Transform trans) { mEnemyTrans = trans; }
    public void StartHpBarSequence()
    {
        if (mEnemyTrans.gameObject.activeSelf) RefillHpBarUI();
    }

    // Call this when getting shot by player.
    public void UpdateHpBarUI(float currHp, float totalHp)
    {
        float val = currHp / totalHp;

        if (!mIsFillingUp) mHpBar.fillAmount = val;
        else mFillAmount = val;
    }

    // Call this when attack timer is over to reduce it to supposed health.
    public void ReduceHpBarUI(float fromHp, float toHp, float totalHp)
    {
        StartCoroutine(ReduceBarSequence(reduceBarDuration, fromHp, toHp, totalHp));
    }

    public void RefillHpBarUI() { StartCoroutine(RefillBarSequence(refillBarDuration)); }
    public void RefillHpBarUI(float duration) { StartCoroutine(RefillBarSequence(duration)); }

    IEnumerator RefillBarSequence(float duration)
    {
        mIsFillingUp = true;
        float mRefillBarTimer = 0;
        while(mRefillBarTimer < duration)
        {
            float deltaTime = 0;
			if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
            else deltaTime = Time.deltaTime;

            mRefillBarTimer += deltaTime;
            float val = mRefillBarTimer / duration;

            if (val > 1) val = 1;
            if (mFillAmount != -1 && mFillAmount < val)
            {
                mHpBar.fillAmount = mFillAmount;
                mFillAmount = -1;
                mIsFillingUp = false;
                yield break;
            }

            mHpBar.fillAmount = val;
            yield return null;
        }
        mIsFillingUp = false;
    }

    // Higher val(fromhp) to lower val(toHP).
    IEnumerator ReduceBarSequence(float duration, float fromHp, float toHp, float totalHp)
    {
        float mReduceBarTimer = 0;
        float difference = fromHp - toHp;
        float defaultFromHp = fromHp;

        while(mReduceBarTimer < duration)
        {
            float deltaTime = 0;
            if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
            else deltaTime = Time.deltaTime;

            mReduceBarTimer += deltaTime;
            if (mReduceBarTimer > duration) mReduceBarTimer = duration;

            float valToMinus = mReduceBarTimer / duration * difference;
            fromHp = defaultFromHp;
            fromHp -= valToMinus;

            mHpBar.fillAmount = fromHp / totalHp;
            if (mHpBar.fillAmount > 1) mHpBar.fillAmount = 1;
            yield return null;
        }
    }
}
