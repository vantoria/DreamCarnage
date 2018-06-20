using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrapLaser : MonoBehaviour 
{
    public Transform leftTarget;
    public Transform rightTarget;
    public bool isRotateLeft = true;

    enum State
    {
        NONE = 0,
        EXPAND,
        MOVE_TO_TARGET,
        SHRINK
    }
    State state = State.NONE;

    Transform target;
    List<Transform> mChildList = new List<Transform>();

    float mExpandDelay = 0, mDuration = 5, mExpandSpeed = 2, mRotateSpeed, mMoveSpd, mShrinkDelay, mShrinkSpeed, mTrapFadeSpeed;
    bool mIsRotate = true, mIsTerminate = false, mIsFade = false;

	void Start () 
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            mChildList.Add(transform.GetChild(i));
        }
	}
	
	void Update () 
    {
        if (BulletManager.sSingleton.IsDisableSpawnBullet && !mIsFade) FadeTrap ();

        if (state != State.NONE && mIsRotate)
        {
            Vector3 dir = Vector3.forward;
            if (!isRotateLeft) dir = -dir;

            float speed = 0;
            if (state == State.EXPAND || state == State.MOVE_TO_TARGET)
                speed = mRotateSpeed * mExpandSpeed;
            else if (state == State.SHRINK)
                speed = mRotateSpeed * mShrinkSpeed;
            
            transform.Rotate(dir * Time.deltaTime * speed);
        }
	}

    public void InitializeTrapLaser(int playerID, AttackPattern.Properties prop) 
    { 
        if (playerID == 1) target = GameManager.sSingleton.player1;
        else target = GameManager.sSingleton.player2;

        mExpandDelay = prop.trapDelayExpand;
        mDuration = prop.trapExpandDur; 
        mExpandSpeed = prop.trapExpandSpd;
        mRotateSpeed = prop.trapRotateSpd;
        mMoveSpd = prop.trapMoveSpd;
        mShrinkDelay = prop.trapDelayShrink;
        mShrinkSpeed = prop.trapShrinkSpd;
		mTrapFadeSpeed = prop.trapFadeSpd;

        StartCoroutine(ExpandSequence(MoveToNearestTarget));
    }

    void MoveToNearestTarget() { StartCoroutine(MoveToNearestSequence(Shrink)); }
    void Shrink() { StartCoroutine(ShrinkSequence()); }

    void ResizeChild(int index, float speed)
    {
        Vector3 scale = mChildList[index].localScale;
        scale.x += Time.deltaTime * 2 * speed;
        mChildList[index].localScale = scale;

        float val = 0;
        if (index == 0 || index == 3) val = Time.deltaTime * speed;
        else if (index == 1 || index == 2) val = -Time.deltaTime * speed;

        if (index < 2)
        {
            Vector3 pos = mChildList[index].localPosition;
            pos.y += val;
            mChildList[index].localPosition = pos;
        }
        else
        {
            Vector3 pos = mChildList[index].localPosition;
            pos.x += val;
            mChildList[index].localPosition = pos;
        }
    }

	void FadeTrap()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform currChild = mChildList[i];
			currChild.GetComponent<BoxCollider2D>().enabled = false;

			Renderer rend = currChild.GetComponent<Renderer>();
			StartCoroutine(Fade(rend, 0, mTrapFadeSpeed, ResetTrap));
		}
	}

    void ResetTrap(Renderer rend)
    {
        Color color = rend.material.color;
        color.a = 1;
        rend.material.color = color;

        Vector3 scale = rend.transform.localScale;
        scale.x = 0;
        rend.transform.localScale = scale;
    }

    IEnumerator ExpandSequence(Action doLast)
    {
        state = State.EXPAND;
        yield return new WaitForSeconds(mExpandDelay);

        transform.position = target.position;

        float timer = 0;
        bool isActivateCol = false;

        while (timer < mDuration)
        {
            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                if (!isActivateCol && timer > 0.25f)
                    mChildList[i].GetComponent<BoxCollider2D>().enabled = true;

                ResizeChild(i, mExpandSpeed);
            }

            if (timer > 0.25f) isActivateCol = true;
            timer += Time.deltaTime;
            yield return null;
        }
        doLast();
    }

    IEnumerator MoveToNearestSequence(Action doLast)
    {
        state = State.MOVE_TO_TARGET;

        float minSqr = (transform.position - leftTarget.position).sqrMagnitude;
        Transform target = leftTarget;

        if((transform.position - rightTarget.position).sqrMagnitude < minSqr) target = rightTarget;

        while (transform.position != target.position)
        {
            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * mMoveSpd);
            yield return null;
        }
        doLast();
    }

    IEnumerator ShrinkSequence()
    {
        state = State.SHRINK;
        mIsRotate = false;

        yield return new WaitForSeconds(mShrinkDelay);

        mIsRotate = true;
        isRotateLeft = !isRotateLeft;

        while (mChildList[0].localPosition.y > 0.5f)
        {
            if (mIsTerminate) break;

            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                ResizeChild(i, -mShrinkSpeed);
            }
            yield return null;
        }

		yield return new WaitForSeconds(mShrinkDelay);

		FadeTrap ();
//        for (int i = 0; i < transform.childCount; i++)
//        {
//            Vector3 scale = mChildList[i].localScale;
//            scale.x = 0;
//            mChildList[i].localScale = scale;
//
//            Vector3 pos = mChildList[i].localPosition;
//            pos = Vector3.zero;
//            mChildList[i].localPosition = pos;
//        }
    }

    IEnumerator Fade(Renderer rend, float val, float speed, Action<Renderer> doLast)
    {
        mIsFade = true;
        Color color = rend.material.color;
        while (color.a > val)
        {
            color.a -= Time.deltaTime * speed;
            rend.material.color = color;
            yield return null;
        }
        doLast(rend);
        mIsFade = false;
    }
}
