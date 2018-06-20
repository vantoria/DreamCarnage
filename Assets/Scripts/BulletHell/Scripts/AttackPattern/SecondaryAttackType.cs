using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryAttackType : MonoBehaviour 
{
    public enum FairyPosition
    {
        ROTATE_AROUND_TARGET = 0,
        FROM_THE_BACK,
    }
    public FairyPosition fairyPosition = FairyPosition.ROTATE_AROUND_TARGET;

    public Transform center;
    public Vector2 axis;
    public bool isClockwise = true;

    public float radius = 1.0f, rotationSpeed = 80.0f;
    public float focusedRadius = 0.85f, focusedChangeSpeed = 1;

	public Vector2 offsetVec;
    public float offsetFromCenter, offsetBetwBullet;
    public Vector2 offsetFocusedVec1, offsetFocusedVec2;

    public float pingPongVal, pingPongSpeed;

    class Fairy
    {
        public Transform fairyTrans;
        public Laser laser;
        public SpriteRenderer sr;

        public Fairy()
        {
            this.fairyTrans = null;
            this.laser = null;
            this.sr = null;
        }

        public Fairy(Transform fairyTrans, SpriteRenderer sr)
        {
            this.fairyTrans = fairyTrans;
            this.sr = sr;
        }

        public Fairy(Transform fairyTrans, Laser laser, SpriteRenderer sr)
        {
            this.fairyTrans = fairyTrans;
            this.laser = laser;
            this.sr = sr;
        }
    }
    List<Fairy> mFairyList = new List<Fairy>();

    float defaultRadius = 0;
    int mActivatedFairies = 0;

    // Laser properties.
    Transform laserPrefab = null;
    float mDmgPerFrame, mExpandSpeed, mExpandTillXScale;
    bool mIsDestroyBullets = false, mIsPiercing = false;

    // Ping pong.
    Vector3 mTarget;
    bool mIsGetPPTarget = false, mIsPPUp = false;

	List<Vector3> mDefaultFairyPos = new List<Vector3>();
    List<Vector3> mFromBackFocusedPos = new List<Vector3>();

    IEnumerator mCurrCo;
    PlayerController mPlayerController;
    AttackPattern mAttackPattern;

	void Start () 
    {
        defaultRadius = radius;
        mPlayerController = GetComponentInParent<PlayerController>();
        mAttackPattern = GetComponentInParent<AttackPattern>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SpriteRenderer tempSr = child.GetComponent<SpriteRenderer>();

            Fairy newFairy = new Fairy();
			if (mAttackPattern.bulletType == AttackPattern.BulletType.PROJECTILE || mAttackPattern.bulletType == AttackPattern.BulletType.PROJECTILE_LOCK_ON)
				newFairy = new Fairy(child, tempSr);
            else if(mAttackPattern.bulletType == AttackPattern.BulletType.LASER)
            {
                Transform trans = Instantiate(laserPrefab, transform.position, Quaternion.identity);
                trans.SetParent(child.transform);
                trans.localScale = laserPrefab.localScale;

                Laser laser = trans.GetComponent<Laser>();
                newFairy = new Fairy(child, laser, tempSr);
                newFairy.laser.SetPlayerController(mPlayerController);
                newFairy.laser.SetIsPiercing(mIsPiercing);
                newFairy.laser.SetLaserProperties(mDmgPerFrame, mExpandSpeed, mExpandTillXScale, mIsDestroyBullets);
            }
            mFairyList.Add(newFairy);
        }

        if (fairyPosition == FairyPosition.FROM_THE_BACK)
        {
            SetDefaultPos_FROM_BACK ();
			SetFocusedtPos_FROM_BACK ();

			int maxPower = (int)mPlayerController.maxPowerLevel;
			for (int i = 0; i < maxPower; i++) 
			{
				// Set the initial position to the fairies.
				mFairyList[i].fairyTrans.localPosition = mDefaultFairyPos[i];
			}
        }
        
        UpdateSprite(Mathf.FloorToInt(mPlayerController.powerLevel));
	}

    void Update () 
    {
        if (fairyPosition == FairyPosition.ROTATE_AROUND_TARGET)
        {
            for (int i = 0; i < mFairyList.Count; i++)
            {
                RotateAroundNoSelfRotate(i);
            }
        }
        else if (fairyPosition == FairyPosition.FROM_THE_BACK)
        {
            if (mIsGetPPTarget)
            {
                mTarget = transform.localPosition;
                mIsGetPPTarget = false;

                if (mIsPPUp) mTarget.y += pingPongVal;
                else mTarget.y -= pingPongVal;
            }

			transform.localPosition = Vector3.MoveTowards(transform.localPosition, mTarget, pingPongSpeed * Time.unscaledDeltaTime);
            if (transform.localPosition == mTarget)
            {
                mIsGetPPTarget = true;
                mIsPPUp = !mIsPPUp;
            }
        }
    }

    public void SetIsPiercing(bool isPierce) { mIsPiercing = isPierce; }
    public void SetLaserProperties(Transform prefab, float dmgPerFrame, float expandSpeed, float expandTillXScale, bool isDestroyBullets) 
    { 
        laserPrefab = prefab;
        mDmgPerFrame = dmgPerFrame;
        mExpandSpeed = expandSpeed;
        mExpandTillXScale = expandTillXScale;
        mIsDestroyBullets = isDestroyBullets;
    }

    public List<Vector3> GetAllFairyPos() { return GetFairiesPosition(); }

    public void FocusedStance()
    {
        if (fairyPosition == FairyPosition.ROTATE_AROUND_TARGET)
        {
            if (mCurrCo != null) StopCoroutine(mCurrCo);
            mCurrCo = MoveToFocusedStance_RA();
            StartCoroutine(mCurrCo);
        }
        else if (fairyPosition == FairyPosition.FROM_THE_BACK)
        {
            if (mCurrCo != null) StopCoroutine(mCurrCo);
            mCurrCo = MoveToFocusedStance_FB();
            StartCoroutine(mCurrCo);
        }
    }

    public void UpdateSprite(int powerLevel)
    {
        if (mActivatedFairies == powerLevel) return;

        for (int i = 0; i < mFairyList.Count; i++)
        {
            if (i < powerLevel) mFairyList[i].sr.enabled = true;
            else mFairyList[i].sr.enabled = false;
        }
        mActivatedFairies = powerLevel;
    }

    public void ActivateSmallLaser(int playerID) 
    { 
        for (int i = 0; i < mActivatedFairies; i++)
        {
            mFairyList[i].laser.Expand(playerID);
        }
    }

    List<Vector3> GetFairiesPosition()
    {
        List<Vector3> tempList = new List<Vector3>();
        for (int i = 0; i < mFairyList.Count; i++)
        {
            tempList.Add(mFairyList[i].fairyTrans.position);
        }
        return tempList;
    }

    void RotateAroundNoSelfRotate(int index)
    {
        float deltaTime = 0;
        if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
        else deltaTime = Time.deltaTime;

        mFairyList[index].fairyTrans.transform.position = (mFairyList[index].fairyTrans.transform.position - center.position).normalized * radius + center.position;

        Vector3 pos = mFairyList[index].fairyTrans.transform.position;
        float angle = rotationSpeed * deltaTime;

        float z = 0;
        if (isClockwise) z = -1;
        else z = 1;

        Vector3 newAxis = new Vector3(axis.x, axis.y, z);
        Quaternion rot = Quaternion.AngleAxis(angle, newAxis); // get the desired rotation
        Vector3 dir = pos - center.position; // find current direction relative to center
        dir = rot * dir; // rotate the direction
        mFairyList[index].fairyTrans.transform.position = center.position + dir; 
    }

    void SetDefaultPos_FROM_BACK()
    {
		int maxPower = (int)mPlayerController.maxPowerLevel;
		for (int i = 0; i < maxPower; i++)
        {
			Transform currChild = transform.GetChild (i);
			Vector2 pos = (Vector2)currChild.localPosition;

            pos.x += offsetVec.x;
            pos.y += offsetVec.y;

            float offset = offsetBetwBullet;
            if (i == 0) pos.x -= offset + offsetFromCenter;
            else if (i == 1) pos.x += offset + offsetFromCenter; 
            else if (i == 2) pos.x -= offset * 2 + offset;
            else if (i == 3) pos.x += offset * 2 + offset;

			mDefaultFairyPos.Add (pos);
        }
    }

	void SetFocusedtPos_FROM_BACK()
	{
		int maxPower = (int)mPlayerController.maxPowerLevel;
		for (int i = 0; i < maxPower; i++)
		{
			Transform currChild = transform.GetChild (i);
			Vector2 pos = (Vector2)currChild.localPosition;
			Vector3 size = currChild.GetComponent<Renderer>().bounds.size;

			if (i == 0)
			{
				pos.x += -offsetFocusedVec1.x;
				pos.y += size.y / 2 + mPlayerController.PlayerSize.y / 2 + offsetFocusedVec1.y;
			}
			else if (i == 1)
			{
				pos.x += offsetFocusedVec1.x;
				pos.y += size.y / 2 + mPlayerController.PlayerSize.y / 2 + offsetFocusedVec1.y;
			}
			else if (i == 2) pos.x += -(size.x / 2 + mPlayerController.PlayerSize.x / 2) - offsetFocusedVec2.x;
			else if (i == 3) pos.x += (size.x / 2 + mPlayerController.PlayerSize.x / 2) + offsetFocusedVec2.x;
			mFromBackFocusedPos.Add(pos);
		}
	}

    IEnumerator MoveToFocusedStance_RA()
    {
        while (((mPlayerController.playerID == 1 && Input.GetKey(KeyCode.LeftShift)) || (mPlayerController.playerID == 2 && Input.GetKey(KeyCode.Comma))) && 
            mPlayerController.IsContinueShoot)
        {
            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

            if (radius != focusedRadius)
            {
				float val = Time.unscaledDeltaTime * focusedChangeSpeed;
                if (radius > focusedRadius) val = -val;

                radius += val;

                if ((radius < defaultRadius && radius < focusedRadius) || (radius > defaultRadius && radius > focusedRadius)) radius = focusedRadius;
            }
            yield return null;
        }

        mCurrCo = MoveToDefaultStance_RA();
        StartCoroutine(mCurrCo);
    }

    IEnumerator MoveToDefaultStance_RA()
    {
        while (radius != defaultRadius)
        {
            while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
            {
                yield return null;
            }

			float val = Time.unscaledDeltaTime * focusedChangeSpeed;
            if (radius > defaultRadius) val = -val;

            radius += val;

            if ((radius > defaultRadius && radius > focusedRadius) || (radius < defaultRadius && radius < focusedRadius)) radius = defaultRadius;
            yield return null;
        }
    }

    IEnumerator MoveToFocusedStance_FB()
    {
		float startTime = Time.time;
		float journeyLength = (mFairyList[0].fairyTrans.localPosition - mFromBackFocusedPos [0]).sqrMagnitude;

        while(((mPlayerController.playerID == 1 && Input.GetKey(KeyCode.LeftShift)) || (mPlayerController.playerID == 2 && Input.GetKey(KeyCode.Comma))) &&
            mPlayerController.IsContinueShoot)
		{
			while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
			{
				yield return null;
			}

			// Multipy by 200 because putting it into local position.
			float distCovered = (Time.time - startTime) * focusedChangeSpeed * 200;
			float fracJourney = distCovered / journeyLength;

			if (fracJourney <= 1)
			{
				for (int i = 0; i < mFairyList.Count; i++) 
				{
					Transform fairy = mFairyList [i].fairyTrans;
					Vector3 target = mFromBackFocusedPos [i];

					fairy.localPosition = Vector3.Lerp(fairy.localPosition, target, fracJourney);
				}
			}
			yield return null;
		}

		mCurrCo = MoveToDefaultStance_FB();
		StartCoroutine(mCurrCo);
    }

	IEnumerator MoveToDefaultStance_FB()
	{
		float startTime = Time.time;
		float journeyLength = (mFairyList[0].fairyTrans.localPosition - mDefaultFairyPos[0]).sqrMagnitude;

		while (mFairyList[0].fairyTrans.localPosition != mDefaultFairyPos[0])
		{
			while (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)
			{
				yield return null;
			}

			// Multipy by 200 because putting it into local position.
			float distCovered = (Time.time - startTime) * focusedChangeSpeed * 200;
			float fracJourney = distCovered / journeyLength;

			if (fracJourney <= 1)
			{
				for (int i = 0; i < mFairyList.Count; i++) 
				{
					Transform fairy = mFairyList [i].fairyTrans;
					Vector3 target = mDefaultFairyPos [i];

					fairy.localPosition = Vector3.Lerp(fairy.localPosition, target, fracJourney);
				}
			}
			yield return null;
		}
	}

}
