using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSoul : MonoBehaviour 
{
	public ParticleSystem revivalCirclePS;
    public Transform saveUITrans;
    public Image UI_FillUpSoulImage;
    public List<Sprite> UI_FillUpSoulSprite;

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

            UI_FillUpSoulImage = UIManager.sSingleton.GetReviveUI(0);
        }
        else
        {
            mTimerTextTrans = UIManager.sSingleton.GetDeathTimerUI(1);
            mTimerText = mTimerTextTrans.GetComponent<Text>();

            UI_FillUpSoulImage = UIManager.sSingleton.GetReviveUI(1);
        }

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
                if (mPlayerController.life >= 0) mPlayerController.ReviveSelf(true);

                Deactivate();
                Debug.Log("Ended");
            }
        }
	}

    public void Activate()
    {
        sr.enabled = true;
        ResetDeadTimeToDefault();
		revivalCirclePS.gameObject.SetActive(true);
		revivalCirclePS.Play ();
        saveUITrans.gameObject.SetActive(true);

        UI_FillUpSoulImage.transform.position = transform.position;
        UI_FillUpSoulImage.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        sr.enabled = false;
        mStopTime = false;
        mTimerTextTrans.gameObject.SetActive(false);
        revivalCirclePS.gameObject.SetActive(false);
        revivalCirclePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        saveUITrans.gameObject.SetActive(false);

        UI_FillUpSoulImage.gameObject.SetActive(false);
    }

    public void StartTimer()  { mStopTime = false; }
    public void StopTimer() { mStopTime = true; }

    public SpriteRenderer GetSr { get { return sr; }  }
    public List<Sprite> GetFillUpSpriteList{ get { return UI_FillUpSoulSprite; }  }

    void ResetDeadTimeToDefault()
    {
        mTimer = GameManager.sSingleton.plySoulTime;
        mTimerText.text = mTimer.ToString();
        mTimerTextTrans.position = transform.GetChild(0).position;
        mTimerTextTrans.gameObject.SetActive(true);
    }
}
