using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSoul : MonoBehaviour 
{
    public Transform revivalCircleTrans;

    Transform mTimerTextTrans;
    float mTimer;
    Text mTimerText;
    bool mStopTime = false;

    SpriteRenderer sr;
    PlayerController mPlayerController;

    void Start()
    {
        if (transform.parent.tag == TagManager.sSingleton.player1Tag)
        {
            mTimerTextTrans = UIManager.sSingleton.GetDeathTimerUI(0);
            mTimerText = mTimerTextTrans.GetComponent<Text>();
        }
        else
        {
            mTimerTextTrans = UIManager.sSingleton.GetDeathTimerUI(1);
            mTimerText = mTimerTextTrans.GetComponent<Text>();
        }
//        mTimerText = timerTextTrans.GetComponent<Text>();
        sr = GetComponent<SpriteRenderer>();
        mPlayerController = GetComponentInParent<PlayerController>();
    }
	
	void Update () 
    {
        if (sr.enabled && !mStopTime)
        {
            mTimer -= Time.deltaTime;
            mTimerText.text = ((int)mTimer).ToString();

            if (mTimer <= 0)
            {
				mPlayerController.MinusLife ();
                mPlayerController.ReviveSelf();
                Debug.Log("Ended");
            }
        }
	}

    public void Activate()
    {
        sr.enabled = true;
        ResetDeadTimeToDefault();
        revivalCircleTrans.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        sr.enabled = false;
        mTimerTextTrans.gameObject.SetActive(false);
        revivalCircleTrans.gameObject.SetActive(false);
    }

    public void StartTimer()
    {
        mStopTime = false;
    }

    public void StopTimer()
    {
        mStopTime = true;
    }

    void ResetDeadTimeToDefault()
    {
        mTimer = GameManager.sSingleton.plySoulTime;
        mTimerText.text = mTimer.ToString();
        mTimerTextTrans.position = transform.GetChild(0).position;
        mTimerTextTrans.gameObject.SetActive(true);
    }
}
