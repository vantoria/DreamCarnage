using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReviveController : MonoBehaviour
{
    public ParticleSystem otherWithinCirclePS;
    Transform mOtherPlayerTrans;

    int mPlayerID, mCurrPressNum, mMaxPressNum;
    bool mIsInsideCircle = false, mIsRevived = false, mIsP1KeybInput = false;

    Image mOtherFillUpImage;
    List<Sprite> mOtherFillUpSpriteList;

    PlayerController mPlayerController, mOtherPlayerController;
    PlayerSoul mOtherPlayerSoul;
    SpriteRenderer mOtherSr;

    void Start()
    {
        mMaxPressNum = GameManager.sSingleton.plyRevPressNum;
        mIsP1KeybInput = JoystickManager.sSingleton.IsP1KeybInput;

        mPlayerController = GetComponentInParent<PlayerController>();
        mPlayerID = mPlayerController.playerID;

        if (GameManager.sSingleton.TotalNumOfPlayer() == 2)
        {
            if (mPlayerID == 1)
            {
                mOtherPlayerTrans = GameManager.sSingleton.player2;
                mOtherPlayerSoul = mOtherPlayerTrans.GetComponentInChildren<PlayerSoul>();

                // Sometimes ID 1 gets called first. Re-initialize it because the other player was null when first instantiated.
                mOtherPlayerTrans.GetComponentInChildren<ReviveController>().UpdateFillUpImage();
            }
            else if (mPlayerID == 2)
            {
                mOtherPlayerTrans = GameManager.sSingleton.player1;
                mOtherPlayerSoul = mOtherPlayerTrans.GetComponentInChildren<PlayerSoul>();

                // Sometimes ID 2 gets called first. Re-initialize it because the other player was null when first instantiated.
                mOtherPlayerTrans.GetComponentInChildren<ReviveController>().UpdateFillUpImage();
            }

            mOtherPlayerController = mOtherPlayerTrans.GetComponentInParent<PlayerController>();

            mOtherFillUpImage = mOtherPlayerSoul.UI_FillUpSoulImage;
            mOtherFillUpSpriteList = mOtherPlayerSoul.GetFillUpSpriteList;
            mOtherSr = mOtherPlayerSoul.GetSr;
        }
    }

    void Update()
    {
        if ((mOtherPlayerController != null && mOtherPlayerController.state != PlayerController.State.SOUL) || 
            mPlayerController.state == PlayerController.State.SOUL) return;
        
        if (mIsInsideCircle)
        {
            if (otherWithinCirclePS != null && !otherWithinCirclePS.isPlaying) otherWithinCirclePS.Play();

            if ((mPlayerID == 1 && (mIsP1KeybInput && Input.GetKeyDown(KeyCode.A)) || Input.GetKeyDown(mPlayerController.GetJoystickInput.reviveKey)) ||
               (mPlayerID == 2 && (Input.GetKeyDown(KeyCode.Semicolon) || Input.GetKeyDown(mPlayerController.GetJoystickInput.reviveKey))))
            {
                mCurrPressNum++;
                mOtherFillUpImage.fillAmount = mCurrPressNum / (float)mMaxPressNum;

                if (mCurrPressNum >= mMaxPressNum)
                {
                    ResetSoul();
                    mOtherPlayerSoul.Deactivate();
                    mOtherPlayerController.ReviveSelf(false);
                    otherWithinCirclePS.Stop();
                }
            }
        }
        else
        {
            if (otherWithinCirclePS != null && !otherWithinCirclePS.isPlaying) otherWithinCirclePS.Stop();
        }

        if (GameManager.sSingleton.TotalNumOfPlayer() != 1)
        {
            string spriteName = mOtherSr.sprite.name;
            string spriteNum = spriteName.Substring(spriteName.Length - 1);
            mOtherFillUpImage.sprite = mOtherFillUpSpriteList[int.Parse(spriteNum)];
        }
    }

    public void UpdateWithinCirclePs()
    {
        mPlayerController = GetComponentInParent<PlayerController>();
        mPlayerID = mPlayerController.playerID;

        if (mPlayerID == 1)
        {
            Transform trans = GameManager.sSingleton.player2;
            AssignOtherRevivingPS(trans);
        }
        else if (mPlayerID == 2)
        {
            Transform trans = GameManager.sSingleton.player1;
            AssignOtherRevivingPS(trans);
        }
    }

    public void ResetSoul()
    {
        mIsRevived = true;
        mCurrPressNum = 0;
        mOtherFillUpImage.fillAmount = 0;
    }

    void UpdateFillUpImage()
    {
        if (mOtherPlayerTrans != null)
        {
            mOtherPlayerSoul = mOtherPlayerTrans.GetComponentInChildren<PlayerSoul>();
            mOtherFillUpImage = mOtherPlayerSoul.UI_FillUpSoulImage;
            mOtherFillUpSpriteList = mOtherPlayerSoul.GetFillUpSpriteList;
            mOtherSr = mOtherPlayerSoul.GetSr;
        }
    }

    void AssignOtherRevivingPS(Transform trans)
    {
        int count = trans.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform currTrans = trans.GetChild(i);
            if (currTrans.name == TagManager.sSingleton.revivingPSName)
            {
                otherWithinCirclePS = currTrans.GetComponent<ParticleSystem>();
                break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.reviveCircleTag)
        {
            // Stop timer.
            mOtherPlayerSoul.StopTimer();
            mIsInsideCircle = true;
            Debug.Log("Step In");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.reviveCircleTag)
        {
            // Start timer.
            if(!mIsRevived) mOtherPlayerSoul.StartTimer();
            mIsRevived = false;
            mIsInsideCircle = false;
            Debug.Log("Step Out");
        }
    }
}
