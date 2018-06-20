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
    public float hitPoint = 100;
    public float impactMultiplier = 1.0f;
    public float scoreMultiplier = 1.0f;

    Vector2 mDirection = Vector2.zero;
    float mDefaultSpdToPlayer, mDefaultSpdFlyOut, mRotate;
    Transform mActivePlyHitBoxTrans;

    PlayerController mPlayer1Controller, mPlayer2Controller;

    void Awake()
    {
        mDefaultSpdToPlayer = speedToPlayer;
        mDefaultSpdFlyOut = speedFlyOut;
    }

    void Start()
    {
        mPlayer1Controller = GameManager.sSingleton.player1.GetComponent<PlayerController>();

        if (GameManager.sSingleton.player2 != null)
            mPlayer2Controller = GameManager.sSingleton.player2.GetComponent<PlayerController>();
    }

	void Update () 
    {
        if (state == State.FREE_FALL)
        {
            Vector3 pos = transform.position;
            pos.y -= speedFreeFall * Time.deltaTime;
            transform.position = pos;
        }
        else if (state == State.MOVE_TOWARDS_PLAYER)
        {
            float deltaTime = 0;
            if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
            else deltaTime = Time.deltaTime;

            float step = speedToPlayer * deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, mActivePlyHitBoxTrans.position, step);
        }
        else if(state == State.FLYING_OUT)
        {
            transform.Translate(mDirection * speedFlyOut * Time.deltaTime, Space.World);
            if (Time.deltaTime != 0) transform.Rotate(new Vector3(0, 0, mRotate));

            speedFlyOut -= Time.deltaTime * speedFlyOutSlowDown;
            if (speedFlyOut < 0)
            {
                transform.rotation = Quaternion.identity;
                speedFlyOut = mDefaultSpdFlyOut;
                state = State.FREE_FALL;
            }
        }
	}

    void OnDisable()
    {
        state = State.FREE_FALL;
        speedToPlayer = mDefaultSpdToPlayer;
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

    public void SetTowardsRandomPlayer()
    {
        if (state == State.MOVE_TOWARDS_PLAYER) return;

        state = State.MOVE_TOWARDS_PLAYER;
        speedToPlayer = GameManager.sSingleton.pointPU_SpeedToPly;

        int rand = -1;
        if (GameManager.sSingleton.TotalNumOfPlayer() == 1) rand = 0;
        else rand = Random.Range(0, 2);

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

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isDestructable) return;

        float damage = 0;
        string otherLayerName = LayerMask.LayerToName(other.gameObject.layer);

        if (otherLayerName == TagManager.sSingleton.playerBulletLayer)
        {
            if (other.tag == TagManager.sSingleton.player1BulletTag || other.tag == TagManager.sSingleton.player2BulletTag)
            {
                BulletMove bulletMove = other.GetComponent<BulletMove>();
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
                    mPlayer1Controller.UpdateLinkBar();
                    mPlayer1Controller.UpdateScore((int)(damage * scoreMultiplier));
                }
                else if (other.tag == TagManager.sSingleton.player2BulletTag)
                {
                    mPlayer2Controller.UpdateLinkBar();
                    mPlayer2Controller.UpdateScore((int)(damage * scoreMultiplier));
                }

                // TODO : Effect it does when contact.

                if (!bulletMove.GetIsPiercing()) 
                    other.gameObject.SetActive(false);
            }
        }
        else if (otherLayerName == TagManager.sSingleton.playerBulletNoDestroyLayer)
        {
            Laser laser = other.GetComponent<Laser>();
            damage = laser.GetDmgPerFrame;
        }

        hitPoint -= damage;
        if (hitPoint <= 0)
        {
            // TODO : Effect it does when it dies.
            gameObject.SetActive(false);
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    transform.GetChild(i).GetComponent<ItemDropController>().ItemDropFunc();
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
