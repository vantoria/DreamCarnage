using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour 
{
	public static BombManager sSingleton { get { return _sSingleton; } }
	static BombManager _sSingleton;

    public BombController p1BombCtrl;
    public BombController p2BombCtrl;

	public float bombTimeStopDur = 2.0f;
	public float bombReturnSpdDur = 2.0f;

    public Transform bombShieldTrans;
    public float bombShieldAreaDur = 2.0f;
    public int bombShieldReturnDmg = 2;
	public float bombShieldReturnSpd = 15.0f;

    public Transform bomb_BulletWipeTrans;

    public float potraitTime = 1.5f;
	public float bombDualLinkInputDur = 1.0f;
	public float bombDualLinkLaserDur = 2.5f;

	[HideInInspector] public bool isTimeStopBomb = false;

    public enum DualLinkState
    {
        NONE = 0,
        PLAYER_INPUTTED,
        BOTH_PLAYER_INPUTTED,
        SHOOTING
    }
    public DualLinkState dualLinkState = DualLinkState.NONE;

    bool mIsCoroutine = false;

	void Awake()
	{
		if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
		else _sSingleton = this;
	}

    void Update()
    {
        if (!mIsCoroutine && BombManager.sSingleton.dualLinkState == DualLinkState.BOTH_PLAYER_INPUTTED)
        {
            StartCoroutine(DualLinkSequence(potraitTime));
        }
    }

    public bool IsPause 
    { 
        get 
        { 
            if (dualLinkState == DualLinkState.PLAYER_INPUTTED || dualLinkState == DualLinkState.BOTH_PLAYER_INPUTTED)
                return true;
            return false;
        } 
    }

    public bool IsShooting
    { 
        get 
        { 
            if (dualLinkState == DualLinkState.SHOOTING) return true;
            return false;
        } 
    }

    IEnumerator DualLinkSequence (float stopDur)
    {
        mIsCoroutine = true;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(stopDur));

        dualLinkState = DualLinkState.SHOOTING;
        Time.timeScale = 1;
        p1BombCtrl.ActivateDualLinkBomb();
        p2BombCtrl.ActivateDualLinkBomb();

        float timer = 0;
        while(timer < bombDualLinkLaserDur)
        {
            timer += Time.deltaTime;

            if (timer > bombDualLinkLaserDur) timer = bombDualLinkLaserDur;
            float val = 1 - (timer/bombDualLinkLaserDur);
            UIManager.sSingleton.UpdateLinkBar(1, val);
            UIManager.sSingleton.UpdateLinkBar(2, val);

            yield return null;
        }

        p1BombCtrl.DeactivateDualLinkBomb();
        p2BombCtrl.DeactivateDualLinkBomb();
        dualLinkState = DualLinkState.NONE;
        mIsCoroutine = false;
    }
}
