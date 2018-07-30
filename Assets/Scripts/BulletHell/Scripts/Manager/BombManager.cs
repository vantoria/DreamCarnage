using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombManager : MonoBehaviour 
{
	public static BombManager sSingleton { get { return _sSingleton; } }
	static BombManager _sSingleton;

    public BombController p1BombCtrl;
    public BombController p2BombCtrl;

    public Image leftEyeshotImage;
    public Image leftPotraitImage;
    public Image leftBlankEyeshotImage;
    public Image rightEyeshotImage;
    public Image rightPotraitImage;
    public Image rightBlankEyeshotImage;

    // Bomb potrait.
    public float potraitMaxAlpha = 0.55f;
    public float potraitMoveSpeed = 1;
    public float potraitFadeSpeed = 1;
    public float potraitStopTime = 0.8f; 
    public float potraitDuration = 2; 
    public float potraitWaitDuration = 1; 
    public float potraitWaitMoveSpeed = 0.2f; 

    public ParticleSystem timeStopPE;
	public float bombTimeStopDur = 2.0f;
	public float bombReturnSpdDur = 2.0f;

    public ParticleSystem bombShieldPE;
    public float bombShieldAreaDur = 2.0f;
    public int bombShieldReturnDmg = 2;
	public float bombShieldReturnSpd = 15.0f;

    public Transform bomb_BulletWipeTrans;

    public ParticleSystem shinyLightningPS;
    public Transform dualLinkLightning;
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

    Text engLeftInstruction, engRightInstruction, jpLeftInstruction, jpRightInstruction;
    bool mIsCoroutine = false, mIsPauseLightning = false, mIsDeactivate;

	void Awake()
	{
		if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
		else _sSingleton = this;
	}

    void Start()
    {
        engLeftInstruction = leftBlankEyeshotImage.transform.GetChild(0).GetComponent<Text>();
        engRightInstruction = rightBlankEyeshotImage.transform.GetChild(0).GetComponent<Text>();
        jpLeftInstruction = leftBlankEyeshotImage.transform.GetChild(1).GetComponent<Text>();
        jpRightInstruction = rightBlankEyeshotImage.transform.GetChild(1).GetComponent<Text>();
    }

    void Update()
    {
        if (dualLinkState == DualLinkState.BOTH_PLAYER_INPUTTED)
        {
            if (UIManager.sSingleton.IsPause && !mIsPauseLightning)
            {
                PauseLightingPS(true);
                mIsPauseLightning = true;
            }
            else if (!UIManager.sSingleton.IsPause && mIsPauseLightning)
            {
                PauseLightingPS(false);
                mIsPauseLightning = false;
            }

            if (!mIsPauseLightning)
            {
                shinyLightningPS.Play();
                shinyLightningPS.Simulate(Time.unscaledDeltaTime, true, false);
            }
        }

        if (!mIsCoroutine && dualLinkState == DualLinkState.BOTH_PLAYER_INPUTTED)
            StartCoroutine(DualLinkSequence(potraitTime));
    }

    public void SetPlayerBombController(int playerNum, BombController bc)
    {
        if (playerNum == 1) p1BombCtrl = bc;
        else if (playerNum == 2) p2BombCtrl = bc;
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

    public void BothPlayerInputted()
    {
        BombManager.sSingleton.dualLinkState = BombManager.DualLinkState.BOTH_PLAYER_INPUTTED;
        BombManager.sSingleton.PlayLightningPS(true);

        DeactivateBlankPotrait(1);
        DeactivateBlankPotrait(2);
    }

    public void OnePlayerInputted()
    {
        BombManager.sSingleton.dualLinkState = BombManager.DualLinkState.PLAYER_INPUTTED;
    }

    public void ActivateBlankPotrait (int currPlayerID)
    {
        if (dualLinkState == BombManager.DualLinkState.BOTH_PLAYER_INPUTTED) return;

        if (currPlayerID == 1)
        {
            StartCoroutine(FadeInFollowingEyeShotImage(rightBlankEyeshotImage, leftEyeshotImage, 0.784f));
            StartCoroutine(FadeInFollowingEyeShotImage(engRightInstruction, leftEyeshotImage, 1));
        }
        else if (currPlayerID == 2)
        {
            StartCoroutine(FadeInFollowingEyeShotImage(leftBlankEyeshotImage, rightEyeshotImage, 0.784f));
            StartCoroutine(FadeInFollowingEyeShotImage(engLeftInstruction, rightEyeshotImage, 1));
        }
    }

    public void DeactivateBlankPotrait (int currPlayerID)
    {
        if (currPlayerID == 1)
        {
            StartCoroutine(FadeInFollowingEyeShotImage(rightBlankEyeshotImage, leftEyeshotImage, 0));
            StartCoroutine(FadeInFollowingEyeShotImage(engRightInstruction, leftEyeshotImage, 0));
        }
        else if (currPlayerID == 2)
        {
            StartCoroutine(FadeInFollowingEyeShotImage(leftBlankEyeshotImage, rightEyeshotImage, 0));
            StartCoroutine(FadeInFollowingEyeShotImage(engLeftInstruction, rightEyeshotImage, 0));
        }
    }

    void PlayLightningPS (bool isPLay)
    {
        for (int i = 0; i < dualLinkLightning.childCount; i++) 
        {
            dualLinkLightning.GetChild (i).gameObject.SetActive (isPLay);
        }
    }

    void PauseLightingPS (bool isPause)
    {
        for (int i = 0; i < dualLinkLightning.childCount; i++) 
        {
            dualLinkLightning.GetChild(i).GetComponent<DigitalRuby.LightningBolt.LightningBoltScript>().enabled = !isPause;
        }
    }

    IEnumerator DualLinkSequence (float stopDur)
    {
        mIsCoroutine = true;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(stopDur));
        shinyLightningPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        PlayLightningPS(false);

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

    IEnumerator FadeInFollowingEyeShotImage(Image currImage, Image followImage, float toAlpha)
    {
        while (currImage.color.a != toAlpha)
        {
            Color toFollowColor = followImage.color;
            Color currColor = currImage.color;

            currColor.a = toFollowColor.a * toAlpha;
            if (currColor.a > toAlpha) currColor.a = toAlpha;
            currImage.color = currColor;

            yield return null;
        }
    }

    IEnumerator FadeInFollowingEyeShotImage(Text text, Image followImage, float toAlpha)
    {
        while (text.color.a != toAlpha)
        {
            Color toFollowColor = followImage.color;
            Color currColor = text.color;

            currColor.a = toFollowColor.a * toAlpha;
            if (currColor.a > toAlpha) currColor.a = toAlpha;
            text.color = currColor;

            yield return null;
        }
    }
}
