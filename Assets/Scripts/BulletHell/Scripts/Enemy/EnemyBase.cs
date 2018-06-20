using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBase : MonoBehaviour 
{
    // Status.
    // For boss.
    public bool isBoss = false;
    public float pingPongSpeed = 0.01f;
    public float pingPongVal = 0.005f;
    public bool isEndShakeScreen = false;

    // For minions.
    public EnemyManager.GroupIndex type;

    public float currHitPoint = 100;
    public float totalHitPoint = 100;

    public float scoreMultiplier = 1.0f;
//    public int scoreGetPerBullet = 100;

    // Animation that is being used.
    public Animator anim;

    [ReadOnly] public int currActionNum = 0;
    public List<Transform> attackTransList = new List<Transform>();
    public Transform healthBarTrans;

    protected Transform mPlayer1, mPlayer2;
    protected List<List<Transform>> mBulletList = new List<List<Transform>>();
    protected SpriteRenderer sr;
    protected MagicCirlce mMagicCircle;

    protected List<List<Action>> mListOfActionList = new List<List<Action>>();
    protected EnemyHealth mEnemyHealth;
    protected EnemyMovement mEnemyMovement;

    float mTotal = 0;
    bool mIsChangeColor = false, mIsPPUp = true;

    PlayerController mPlayer1Controller, mPlayer2Controller;
    ItemDropController mItemDropController;

    void Awake()
    {
        EnemyManager.sSingleton.AddToList(transform);
    }

    public virtual void Start()
    {
        mPlayer1 = GameManager.sSingleton.player1;
        mPlayer1Controller = mPlayer1.GetComponent<PlayerController>();
            
        if (GameManager.sSingleton.player2 != null)
        {
            mPlayer2 = GameManager.sSingleton.player2;
            mPlayer2Controller = mPlayer2.GetComponent<PlayerController>();
        }

        sr = GetComponent<SpriteRenderer>();
        mMagicCircle = gameObject.GetComponentInChildren<MagicCirlce>();
        mItemDropController = GetComponent<ItemDropController>();

        if (healthBarTrans != null) mEnemyHealth = healthBarTrans.GetComponent<EnemyHealth>();
        if (GetComponent<EnemyMovement>() != null) mEnemyMovement = GetComponent<EnemyMovement>();
    }

    public virtual void Update()
    {
        if (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause) return;

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
            BulletMove bulletMove = other.GetComponent<BulletMove>();
            damage = bulletMove.GetBulletDamage;

            if (!bulletMove.GetIsPiercing()) 
                other.gameObject.SetActive(false);
        }
        else 
        {
            Laser laser = other.GetComponent<Laser>();
            damage = laser.GetDmgPerFrame;
        }

        GetDamaged(damage, other.tag);
    }

    void GetDamaged(float damagedValue, string otherTag)
    {
        float scoreGet = damagedValue * scoreMultiplier;

        currHitPoint -= damagedValue;

        if (currHitPoint <= 0)
        {
            if (isBoss)
            {
                BulletManager.sSingleton.TransformEnemyBulsIntoScorePU();
                BulletManager.sSingleton.DisableEnemyBullets(false);
                UIManager.sSingleton.DeactivateBossTimer();

                if (isEndShakeScreen)
                    CameraShake.sSingleton.ShakeCamera();
            }
            else if (mItemDropController != null)
            {
                mItemDropController.ItemDropFunc();
            }

            scoreGet = (currHitPoint + damagedValue) * scoreMultiplier;

            // TODO: Enemy destroyed animation..
            Destroy(gameObject);
        }

        PlayerGainScore((int)scoreGet, otherTag);
        if (!mIsChangeColor) StartCoroutine(GetDamagedColorChange());
    }

    void PlayerGainScore(int val, string otherTag)
    {
        if (otherTag == TagManager.sSingleton.player1BulletTag)
        {
            mPlayer1Controller.UpdateLinkBar();
            mPlayer1Controller.UpdateScore(val);
        }
        else if (otherTag == TagManager.sSingleton.player2BulletTag)
        {
            mPlayer2Controller.UpdateLinkBar();
            mPlayer2Controller.UpdateScore(val);
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
