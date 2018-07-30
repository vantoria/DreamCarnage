using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour 
{
    // Status for boss.
    public bool isBoss = false;
    public float pingPongSpeed = 0.01f;
    public float pingPongVal = 0.005f;
    public bool isEndShakeScreen = false;
    public float delayBeforeAttack;

    // For minions.
//    public EnemyManager.GroupIndex type;

    public float currHitPoint = 100;
    public float totalHitPoint = 100;

    public float scoreMultiplier = 1.0f;
    public int defeatedScore = 1000;
    public float defeatedMult = 0.01f;

    // Animation that is being used.
    public Animator anim;
	[ReadOnly] public bool isHitByMagnumRadius = false;

    [ReadOnly] public int currActionNum = 0;
    public List<Transform> attackTransList = new List<Transform>();

    protected Transform mPlayer1, mPlayer2;
    protected List<List<Transform>> mBulletList = new List<List<Transform>>();
    protected SpriteRenderer sr;
    protected MagicCirlce mMagicCircle;

    protected List<List<Action>> mListOfActionList = new List<List<Action>>();
    protected EnemyHealth mEnemyHealth;
    protected EnemyMovement mEnemyMovement;

    float mTotal = 0, mImageBottomY, mMagnumTimer, mMagnumMarkedDuration;
	bool mIsChangeColor = false, mIsPPUp = true, mIsMarkedByMagnum = false;

    PlayerController mPlayer1Controller, mPlayer2Controller;
    ItemDropController mItemDropController;

    public virtual void Start()
    {
        mPlayer1 = GameManager.sSingleton.player1;
        mPlayer1Controller = mPlayer1.GetComponent<PlayerController>();
            
        if (GameManager.sSingleton.player2 != null)
        {
            mPlayer2 = GameManager.sSingleton.player2;
            mPlayer2Controller = mPlayer2.GetComponent<PlayerController>();
        }

		sr = GetComponentInChildren<SpriteRenderer>();
        mImageBottomY = GetComponentInChildren<Renderer>().bounds.size.y / 2;

        mMagicCircle = gameObject.GetComponentInChildren<MagicCirlce>();
        mItemDropController = GetComponent<ItemDropController>();

        if (isBoss)
        {
            EnemyHealth enemyHealth = EnemyManager.sSingleton.bossEnemyHealthBar;
            enemyHealth.SetOwner(gameObject.transform);
            mEnemyHealth = enemyHealth;
        }
		if (GetComponentInChildren<EnemyMovement>() != null) mEnemyMovement = GetComponent<EnemyMovement>();

        mMagnumMarkedDuration = GameManager.sSingleton.MagnumMarkedDuration;
    }

    public virtual void Update()
    {
        if (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause) return;

        if (mIsMarkedByMagnum)
        {
            mMagnumTimer += Time.deltaTime;
            if (mMagnumTimer >= mMagnumMarkedDuration)
            {
                mMagnumTimer = 0;
                mIsMarkedByMagnum = false;
            }
        }

		if (isBoss)
        {
            Vector3 currPos = transform.position;
            float val = pingPongSpeed * Time.deltaTime;

            if (!mIsPPUp) val = -val;

            mTotal += Mathf.Abs(val);
            if (mTotal >= pingPongVal)
            {
                val = mTotal - pingPongVal;
                if (!mIsPPUp) val = -val;

                mTotal = 0;
                mIsPPUp = !mIsPPUp;
            }

            currPos.y += val;
            transform.position = currPos;
        }
    }

    public void EnableMagicCircle()
    {
        if(mMagicCircle != null)
            mMagicCircle.enabled = true;
    }

    public void PullTrigger(Collider2D other)
    {
        if (currHitPoint <= 0) return;

        float damage = 0;
        string otherLayer = LayerMask.LayerToName(other.gameObject.layer);

        if (otherLayer == TagManager.sSingleton.playerBulletLayer)
        {
            if (other.GetComponent<BulletMove>() != null)
            {
                BulletMove bulletMove = other.GetComponent<BulletMove>();
                damage = bulletMove.GetBulletDamage;

                if (bulletMove.GetIsMagnum)
                    mIsMarkedByMagnum = true;

                if (!bulletMove.GetIsPiercing()) 
                    other.gameObject.SetActive(false);

                GetDamaged(damage, other, bulletMove.BulletType);
            }
			else if (other.GetComponent<DamageWithinRadius>() != null && !isHitByMagnumRadius)
            {
				mIsMarkedByMagnum = true;
				isHitByMagnumRadius = true;

                DamageWithinRadius dmgRad = other.GetComponent<DamageWithinRadius>();
                GetDamaged(dmgRad.damage, other, BulletManager.GroupIndex.PLAYER_MAIN);
            }
        }
        else if (otherLayer == TagManager.sSingleton.playerBulletNoDestroyLayer)
        {
            Laser laser = other.GetComponent<Laser>();
            damage = laser.GetDmgPerFrame;
            GetDamaged(damage, other, BulletManager.GroupIndex.PLAYER_SECONDARY);
        }
    }

    public void StartHpBarSequence()
    {
        EnemyManager.sSingleton.bossEnemyHealthBar.StartHpBarSequence();
    }

    public void PlayEnemyDeathPS()
    {
        ParticleSystem ps = EnvObjManager.sSingleton.GetEnemyDeathPS ();
        ps.transform.position = transform.position;
        ps.Play ();
    }

    public void EnemyDiedByTime()
    {
        if (isBoss)
        {
            BulletManager.sSingleton.DisableEnemyBullets(false);
            UIManager.sSingleton.DeactivateBossTimer();
            EnemyManager.sSingleton.isBossDead = true;

            if (isEndShakeScreen)
                CameraShake.sSingleton.ShakeCamera();

            PlayEnemyDeathPS();
            gameObject.SetActive (false);
        }
    }

    void GetDamaged(float damagedValue, Collider2D other, BulletManager.GroupIndex groupType)
    {
        float scoreGet = damagedValue * scoreMultiplier;
        currHitPoint -= damagedValue;

        ParticleSystem ps = EnvObjManager.sSingleton.GetEnemyHitImpactPS ();
        Vector3 pos = other.gameObject.GetComponent<Collider2D>().bounds.ClosestPoint(transform.position);

        BulletMove bulletMove = other.GetComponent<BulletMove>();
        if ((bulletMove != null && bulletMove.GetIsPiercing()) || other.GetComponent<Laser>() != null) PlayEnemyGetHitImpactPS(ps, pos, true);
        else PlayEnemyGetHitImpactPS(ps, pos, false);

		if (!mIsChangeColor) StartCoroutine(GetDamagedColorChange());
        if (currHitPoint <= 0)
        {
            if (isBoss)
            {
                BulletManager.sSingleton.TransformEnemyBulsIntoScorePU();
                BulletManager.sSingleton.DisableEnemyBullets(false);
                UIManager.sSingleton.DeactivateBossTimer();
                EnemyManager.sSingleton.isBossDead = true;

                if (isEndShakeScreen) CameraShake.sSingleton.ShakeCamera();
                if (AudioManager.sSingleton != null) AudioManager.sSingleton.PlayBossExplodeSfx();
            }
            else if (mItemDropController != null)
            {
                mItemDropController.ItemDropFunc();
            }

            if (mIsMarkedByMagnum)
            {
                Transform magnumRad = EnvObjManager.sSingleton.GetMagnumRadius();
                magnumRad.position = transform.position;
                magnumRad.gameObject.SetActive(true);

                mMagnumTimer = 0;
                mIsMarkedByMagnum = false;
            }

            if (other.tag == TagManager.sSingleton.player1BulletTag) mPlayer1Controller.UpdateMultiplier(defeatedMult);
            else if (other.tag == TagManager.sSingleton.player2BulletTag) mPlayer2Controller.UpdateMultiplier(defeatedMult);
            else if (other.tag == TagManager.sSingleton.magnumRadTag) 
            {
                int playerID = other.GetComponent<DamageWithinRadius> ().playerID;
                if (playerID == 1) mPlayer1Controller.UpdateMultiplier(defeatedMult);
                else if (playerID == 2) mPlayer2Controller.UpdateMultiplier(defeatedMult);
            }
                
            scoreGet = (currHitPoint + damagedValue) * scoreMultiplier;
			scoreGet += defeatedScore;

			Transform trans = EnvObjManager.sSingleton.GetKillScore ();
			trans.GetComponent<Text> ().text = defeatedScore.ToString();
			trans.position = transform.position;
			trans.gameObject.SetActive (true);

            PlayEnemyDeathPS();
			gameObject.SetActive (false);
        }

		PlayerGainScore((int)scoreGet, other, groupType);
    }

    void PlayEnemyGetHitImpactPS(ParticleSystem ps, Vector3 pos, bool isPierce)
    {
        Vector3 newPos = pos;
        if (!isPierce) newPos = new Vector3(pos.x, transform.position.y - mImageBottomY, 0);

        ps.transform.position = newPos;
        ps.Play ();
    }

	void PlayerGainScore(int val, Collider2D other, BulletManager.GroupIndex groupType)
    {
		int finalValue = 0;
        if (other.tag == TagManager.sSingleton.player1BulletTag)
        {
            if (groupType == BulletManager.GroupIndex.PLAYER_MAIN) mPlayer1Controller.UpdateLinkBar();
            else mPlayer1Controller.UpdateLinkBarForSecondary();

            finalValue = (int)(val * mPlayer1Controller.scoreMult);
			mPlayer1Controller.UpdateScore(finalValue);
        }
        else if (other.tag == TagManager.sSingleton.player2BulletTag)
        {
            if (groupType == BulletManager.GroupIndex.PLAYER_MAIN) mPlayer2Controller.UpdateLinkBar();
            else mPlayer2Controller.UpdateLinkBarForSecondary();

            finalValue = (int)(val * mPlayer2Controller.scoreMult);
			mPlayer2Controller.UpdateScore(finalValue);
        }
		else if (other.tag == TagManager.sSingleton.magnumRadTag) 
		{
			int playerID = other.GetComponent<DamageWithinRadius> ().playerID;

			if (playerID == 1) 
			{
				mPlayer1Controller.UpdateLinkBar ();

                finalValue = (int)(val * mPlayer1Controller.scoreMult);
				mPlayer1Controller.UpdateScore (finalValue);
			} 
			else if (playerID == 2) 
			{
				mPlayer2Controller.UpdateLinkBar ();

                finalValue = (int)(val * mPlayer2Controller.scoreMult);
				mPlayer2Controller.UpdateScore(finalValue);
			}
		}
    }

    IEnumerator GetDamagedColorChange()
    {
        mIsChangeColor = true;
        Color defaultColor = sr.color;

        sr.color = GameManager.sSingleton.enemyDmgColor;
        yield return new WaitForSeconds(GameManager.sSingleton.enemyDmgColorDur);
        sr.color = defaultColor;
        mIsChangeColor = false;
    }
}
