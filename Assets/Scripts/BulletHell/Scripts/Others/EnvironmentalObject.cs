using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalObject : MonoBehaviour 
{
    public enum Type
    {
        GIVE_VALUE = 0,
        DAMAGE_PLAYER
    }
    public Type type = Type.GIVE_VALUE;

    public enum State
    {
        FREE_FALL = 0,
        MOVE_TOWARDS_PLAYER,
        FLYING_OUT
    }
    public State state = State.FREE_FALL;

    public float speedFreeFall = 1;
    public float speedToPlayer = 3;
    public float speedFlyOut = 3;
    public float speedFlyOutSlowDown = 1;
    public float value;

    public bool isDestructable = false;
    public bool isCrashable = false;
    public float currHitPoint = 100;
    public float maxHitPoint = 100;
    public float impactMultiplier = 1.0f;
    public float extraLinkMultiplier = 1.2f;
    public float scoreMultiplier = 1.0f;
    public float defeatedMult = 0.05f;

    Vector2 mDirection = Vector2.zero;
    float mDefaultSpdToPlayer, mDefaultSpdFlyOut, mRotate, mColliderBottomY;
    Transform mActivePlyHitBoxTrans;

	SpriteRenderer sr;
	bool mIsChangeColor = false, mIsDead = false;

    PlayerController mPlayer1Controller, mPlayer2Controller;

    void Awake()
    {
        mDefaultSpdToPlayer = speedToPlayer;
        mDefaultSpdFlyOut = speedFlyOut;
    }

    void Start()
    {
        Initialize();

        if (GetComponent<SpriteRenderer>() != null) sr = GetComponent<SpriteRenderer> ();
        else sr = GetComponentInChildren<SpriteRenderer> ();
            
        if (GetComponent<PolygonCollider2D>() != null) mColliderBottomY = GetComponent<PolygonCollider2D>().bounds.size.y / 2;
    }

    void OnEnable()
    {
        if (transform.tag == TagManager.sSingleton.ENV_OBJ_RockTag) EnvObjManager.sSingleton.AddToList(this.transform);
        state = State.FLYING_OUT;
        speedToPlayer = mDefaultSpdToPlayer;
        currHitPoint = maxHitPoint;
//        speedFlyOut = mDefaultSpdFlyOut;
    }

	void Update () 
    {
        if (UIManager.sSingleton.IsPauseGameOverMenu || UIManager.sSingleton.IsShowScoreRankNameInput) return;

        if (state == State.FREE_FALL)
        {
            Vector3 pos = transform.position;
            pos.y -= speedFreeFall * Time.deltaTime;
            transform.position = pos;
        }
        else if (state == State.MOVE_TOWARDS_PLAYER)
        {
            if (!mActivePlyHitBoxTrans.gameObject.activeSelf)
            {
                state = State.FREE_FALL;
                return;
            }

            float deltaTime = 0;
            if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
            else deltaTime = Time.deltaTime;

            float step = speedToPlayer * deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, mActivePlyHitBoxTrans.position, step);
        }
        else if (state == State.FLYING_OUT)
        {
            transform.Translate(mDirection * speedFlyOut * Time.unscaledDeltaTime, Space.World);
            transform.Rotate(new Vector3(0, 0, mRotate));

            speedFlyOut -= Time.unscaledDeltaTime * speedFlyOutSlowDown;
            if (speedFlyOut < 0)
            {
                transform.rotation = Quaternion.identity;
                speedFlyOut = mDefaultSpdFlyOut;
                state = State.FREE_FALL;
            }
        }
	}

    public void SetPlayer(Transform playerHitBox) 
    { 
        if (state == State.MOVE_TOWARDS_PLAYER) return;

        state = State.MOVE_TOWARDS_PLAYER;
        mActivePlyHitBoxTrans = playerHitBox; 
    }

	public void SetPlayerOverrideSpd(Transform playerHitBox) 
	{ 
		SetPlayer (playerHitBox);
		speedToPlayer = GameManager.sSingleton.autoCollectPU_SpeedToPly;
	}

	public void SetTowardsRandomPlayer(float speedMultiplier)
    {
        if (state == State.MOVE_TOWARDS_PLAYER) return;
        if (mPlayer1Controller == null || mPlayer2Controller == null) Initialize();

        state = State.MOVE_TOWARDS_PLAYER;
        speedToPlayer = GameManager.sSingleton.pointPU_SpeedToPly * speedMultiplier;

        int rand = -1;
		if (GameManager.sSingleton.TotalNumOfPlayer () == 1) 
		{
			if (GameManager.sSingleton.IsThisPlayerActive(1)) rand = 0;
			else rand = 1;
		}
        else 
		{
			rand = Random.Range(0, 2);
			if (rand == 0 && (mPlayer1Controller.state == PlayerController.State.DEAD || mPlayer1Controller.state == PlayerController.State.SOUL))
				rand = 1;
			else if (rand == 1 && (mPlayer2Controller.state == PlayerController.State.DEAD || mPlayer2Controller.state == PlayerController.State.SOUL))
				rand = 0;

//            Debug.Log(mPlayer1Controller.state + "  " + rand);
		}
	    
        mActivePlyHitBoxTrans = EnvObjManager.sSingleton.GetPlyHitBoxTransList[rand];
    }

    public void SetToFlyOut(Vector2 dir)
    {
        state = State.FLYING_OUT;
        mDirection = dir;
        speedFlyOut = GameManager.sSingleton.powerDropSpeed;
        speedFlyOutSlowDown = GameManager.sSingleton.powerDropSlowDown;
        mRotate = GameManager.sSingleton.powerDropRotate;
    }

    public void SetToFlyOut(Vector2 dir, float speedFlyOut, float speedSlowDown)
    {
        state = State.FLYING_OUT;
        mDirection = dir;
        this.speedFlyOut = speedFlyOut;
        speedFlyOutSlowDown = speedSlowDown;
        mRotate = GameManager.sSingleton.powerDropRotate * 0.2f;
    }

    void Initialize()
    {
        mPlayer1Controller = GameManager.sSingleton.player1.GetComponent<PlayerController>();

        if (GameManager.sSingleton.player2 != null)
            mPlayer2Controller = GameManager.sSingleton.player2.GetComponent<PlayerController>();
    }
        
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isDestructable) return;

        float damage = 0;
        string otherLayerName = LayerMask.LayerToName(other.gameObject.layer);

        BulletMove bulletMove = other.GetComponent<BulletMove>();

        // Particle play.
		if (otherLayerName == TagManager.sSingleton.playerBulletLayer || otherLayerName == TagManager.sSingleton.playerBulletNoDestroyLayer) 
		{
            if (other.tag != TagManager.sSingleton.bombShieldTag && other.tag != TagManager.sSingleton.bulletWipeTag)
            {
                if (!mIsChangeColor) StartCoroutine (GetDamagedColorChange());

                ParticleSystem ps = EnvObjManager.sSingleton.GetEnemyHitImpactPS();
                Vector3 pos = other.gameObject.GetComponent<Collider2D>().bounds.ClosestPoint(transform.position);

                if ((bulletMove != null && bulletMove.GetIsPiercing()) || other.GetComponent<Laser>() != null) PlayEnvObjGetHitImpactPS(ps, pos, true);
                else PlayEnvObjGetHitImpactPS(ps, pos, false);
            }
		}

        // Damage and impact.
        if (otherLayerName == TagManager.sSingleton.playerBulletLayer)
        {
            if (other.tag == TagManager.sSingleton.player1BulletTag || other.tag == TagManager.sSingleton.player2BulletTag)
            {
                damage = bulletMove.GetBulletDamage;

                float deltaTime = 0;
                if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
                else deltaTime = Time.deltaTime;

                // Move object up slightly.
                Vector3 pos = transform.position;
                pos.y += deltaTime * impactMultiplier;
                transform.position = pos;

                // Update player's score.
                if (other.tag == TagManager.sSingleton.player1BulletTag)
                {
                    if (bulletMove.BulletType == BulletManager.GroupIndex.PLAYER_MAIN)
                        mPlayer1Controller.UpdateLinkBar(extraLinkMultiplier);
                    else if (bulletMove.BulletType == BulletManager.GroupIndex.PLAYER_SECONDARY)
                        mPlayer1Controller.UpdateLinkBarForSecondary(extraLinkMultiplier);

                    mPlayer1Controller.UpdateScore((int)(damage * scoreMultiplier));
                }
                else if (other.tag == TagManager.sSingleton.player2BulletTag)
                {
                    if (bulletMove.BulletType == BulletManager.GroupIndex.PLAYER_MAIN)
                        mPlayer2Controller.UpdateLinkBar(extraLinkMultiplier);
                    else if (bulletMove.BulletType == BulletManager.GroupIndex.PLAYER_SECONDARY)
                        mPlayer2Controller.UpdateLinkBarForSecondary(extraLinkMultiplier);
                    
                    mPlayer2Controller.UpdateScore((int)(damage * scoreMultiplier));
                }

                if (!bulletMove.GetIsPiercing()) 
                    other.gameObject.SetActive(false);
            }
        }
        else if (otherLayerName == TagManager.sSingleton.playerBulletNoDestroyLayer)
        {
            if (other.GetComponent<Laser>() != null)
            {
                Laser laser = other.GetComponent<Laser>();
                damage = laser.GetDmgPerFrame;

                if (other.tag == TagManager.sSingleton.player1BulletTag)
                {
                    mPlayer1Controller.UpdateLinkBarForSecondary(extraLinkMultiplier);
                    mPlayer1Controller.UpdateScore((int)(damage * scoreMultiplier));
                }
                else if (other.tag == TagManager.sSingleton.player2BulletTag)
                {
                    mPlayer2Controller.UpdateLinkBarForSecondary(extraLinkMultiplier);
                    mPlayer2Controller.UpdateScore((int)(damage * scoreMultiplier));
                }
            }
        }

        // Death.
        currHitPoint -= damage;
        if (currHitPoint <= 0 && !mIsDead)
        {
            ParticleSystem ps = EnvObjManager.sSingleton.GetRockDestroyPS();
            ps.transform.position = transform.position;
            ps.Play();

            gameObject.SetActive(false);
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    transform.GetChild(i).GetComponent<ItemDropController>().ItemDropFunc();
                    gameObject.SetActive(false);
                }
            }

            if (other.tag == TagManager.sSingleton.player1BulletTag) mPlayer1Controller.UpdateMultiplier(defeatedMult);
            else if (other.tag == TagManager.sSingleton.player2BulletTag) mPlayer2Controller.UpdateMultiplier(defeatedMult);

            mIsDead = true;
        }
    }

    void PlayEnvObjGetHitImpactPS(ParticleSystem ps, Vector3 pos, bool isPierce)
    {
        Vector3 newPos = pos;
        if (!isPierce) newPos = new Vector3(pos.x, transform.position.y - mColliderBottomY, 0);

        ps.transform.position = newPos;
        ps.Play ();
    }

	IEnumerator GetDamagedColorChange()
	{
		mIsChangeColor = true;
		Color defaultColor = sr.color;

		sr.color = GameManager.sSingleton.rockDmgColor;
		yield return new WaitForSeconds(GameManager.sSingleton.enemyDmgColorDur);
		sr.color = defaultColor;
		mIsChangeColor = false;
	}

	void OnDisable()
	{
		state = State.FREE_FALL;
		speedToPlayer = mDefaultSpdToPlayer;
        mIsDead = false;

        if (transform.tag == TagManager.sSingleton.ENV_OBJ_RockTag) EnvObjManager.sSingleton.RemoveFromList(this.transform);
	}
}
