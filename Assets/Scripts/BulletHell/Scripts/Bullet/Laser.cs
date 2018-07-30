using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour 
{
    public float dmgPerFrame = 1, expandSpeed = 1, expandTillXScale = 3.5f;
    public bool isDestroyBullets = true;

    float mDefaultXScale = 0;
    bool mIsFiring = false, mIsPiercing = false;

    BoxCollider2D boxCollider;
    SpriteRenderer sr;
    IEnumerator mCurrCo;

    PlayerController mPlayerController;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        mDefaultXScale = transform.localScale.x;
    }

    public float GetDmgPerFrame { get { return dmgPerFrame; } }

    public void SetPlayerController(PlayerController playerController) { mPlayerController = playerController; }
    public void SetIsPiercing(bool isPierce) { mIsPiercing = isPierce; }
    public void SetLaserProperties(float dmgPerFrame, float expandSpeed, float expandTillXScale, bool isDestroyBullets)
    {
        this.dmgPerFrame = dmgPerFrame;
        this.expandSpeed = expandSpeed;
        this.expandTillXScale = expandTillXScale;
        this.isDestroyBullets = isDestroyBullets;
    }

    public void Expand(int playerID)
    {
        if (mIsFiring) return;
        if (mCurrCo != null) StopCoroutine(mCurrCo);

        boxCollider.enabled = true;
        sr.enabled = true;
        StartCoroutine(ExpandSequence(playerID));
    }

    IEnumerator ExpandSequence(int playerID)
    {
        mIsFiring = true;
        while ( ((playerID == 1 && (Input.GetKey(KeyCode.Z) || Input.GetKey(mPlayerController.GetJoystickInput.fireKey))) || 
            (playerID == 2 && (Input.GetKey(KeyCode.Period) || Input.GetKey(mPlayerController.GetJoystickInput.fireKey))) ) && 
			mPlayerController.IsContinueShoot && !BombManager.sSingleton.IsShooting)
        {
            if (UIManager.sSingleton.IsShowScoreRankNameInput) yield break;

			if (GameManager.sSingleton.IsBossMakeEntrance ()) 
			{
				mIsFiring = false;
				if (!mPlayerController.IsContinueShoot) boxCollider.enabled = false;
				mCurrCo = ShrinkSequence();
				StartCoroutine(mCurrCo);
				yield break;
			}

            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            if (transform.localScale.x != expandTillXScale)
            {
                Vector3 scale = transform.localScale;
				scale.x += Time.unscaledDeltaTime * expandSpeed;

                if (scale.x > expandTillXScale) scale.x = expandTillXScale;
                transform.localScale = scale;
            }

            yield return null;
        }

        mIsFiring = false;
        if (!mPlayerController.IsContinueShoot) boxCollider.enabled = false;

        mCurrCo = ShrinkSequence();
        StartCoroutine(mCurrCo);
    }

    IEnumerator ShrinkSequence()
    {
        while (transform.localScale.x != mDefaultXScale)
        {
            if (UIManager.sSingleton.IsShowScoreRankNameInput) yield break;

            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            Vector3 scale = transform.localScale;
			scale.x -= Time.unscaledDeltaTime * expandSpeed;

            if (scale.x < mDefaultXScale) scale.x = mDefaultXScale;
            transform.localScale = scale;

            yield return null;
        }
        boxCollider.enabled = false;
        sr.enabled = false;
    }

	void OnTriggerStay2D(Collider2D other)
	{
        if (isDestroyBullets && other.tag == TagManager.sSingleton.enemyBulletTag) 
		{
			other.gameObject.SetActive (false);
		}
	}
}
