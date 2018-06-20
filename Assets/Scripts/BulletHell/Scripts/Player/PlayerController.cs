using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour 
{
    public int playerID = 1;
    public float primaryShootDelay = 0.05f;
    public float secondaryShootDelay = 0.15f;
    public float moveSpeed = 1.0f;
    public float returnDefaultSpdQuickness = 1.0f;
    public float respawnXPos = 0.3f;

    public int score = 0;
    public int life = 2;
    public int bomb = 2;
    public float powerLevel = 0;
    public float maxPowerLevel = 4;
    public float linkValue = 0;
    public float linkMultiplier = 0.5f;

    public Transform hitBoxTrans;
    public Transform soulTrans;
    public Transform spriteBoxTrans;

    public enum State
    {
        NORMAL = 0,
        DISABLE_CONTROL,
		SOUL,
        DEAD
    };
    [ReadOnlyAttribute]public State state = State.NORMAL;

    class Border
    {
        public float top, bottom, left, right, autoCollect;

        public Border()
        {
            this.top = 0;
            this.bottom = 0;
            this.left = 0;
            this.right = 0;
			this.autoCollect = 0;
        }
    }
    static Border border = new Border();

	JoystickManager.JoystickInput mJoystick;

	Transform mOtherPlayerTrans;
    Vector3 mPlayerSize, mResetPos, mP1DefaultPos, mP2DefaultPos;
    float mDefaultMoveSpeed, mDisableCtrlTimer, mInvinsibilityTimer;
	bool mIsShiftPressed = false, mIsChangeAlpha = false, mIsInvinsible = false, mIsWaitOtherInput = false, mIsP1KeybInput = true;

    int totalCoroutine = 2;
    List<bool> mIsCoroutineList = new List<bool>();

    SpriteRenderer sr;
    AttackPattern mAttackPattern;
    SecondaryAttackType mFairy;
    BombController mBombController;
    PlayerSoul mPlayerSoul;
	PlayerController mOtherPlayerController;

    void Start () 
    {
		if (JoystickManager.sSingleton.connectedGamepadCount == 1)
            mIsP1KeybInput = false;

		if (playerID == 1) mJoystick = JoystickManager.sSingleton.p1_joystick;
		else if (playerID == 2) mJoystick = JoystickManager.sSingleton.p2_joystick;

        if (GameManager.sSingleton.TotalNumOfPlayer() == 2)
        {
			if (playerID == 1) mOtherPlayerTrans = GameManager.sSingleton.player2;
			else mOtherPlayerTrans = GameManager.sSingleton.player1;

			mOtherPlayerController = mOtherPlayerTrans.GetComponent<PlayerController> ();
        }

        mDefaultMoveSpeed = moveSpeed;
        mPlayerSize = GetComponent<Renderer>().bounds.size;

        // Here is the definition of the boundary in world point
        float distance = (transform.position - Camera.main.transform.position).z;

        border.left = Camera.main.ViewportToWorldPoint (new Vector3 (0.18f, 0, distance)).x + (mPlayerSize.x/2);
        border.right = Camera.main.ViewportToWorldPoint (new Vector3 (0.82f, 0, distance)).x - (mPlayerSize.x/2);
        border.top = Camera.main.ViewportToWorldPoint (new Vector3 (0, 1, distance)).y - (mPlayerSize.y/2);
        border.bottom = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, distance)).y + (mPlayerSize.y/2);

		float autoCollect_Y = GameManager.sSingleton.autoCollectPickUp_Y;
		border.autoCollect = Camera.main.ViewportToWorldPoint (new Vector3 (0, autoCollect_Y, distance)).y - (mPlayerSize.y/2);

        mResetPos.x = Camera.main.ViewportToWorldPoint (new Vector3 (respawnXPos, 0, distance)).x;
        mResetPos.y = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, distance)).y - (mPlayerSize.y/2);

        mP1DefaultPos = GameManager.sSingleton.p1DefaultPos.position;
        mP2DefaultPos = GameManager.sSingleton.p2DefaultPos.position;

        sr = GetComponent<SpriteRenderer>();
        mAttackPattern = GetComponent<AttackPattern>();
        mBombController = GetComponent<BombController>();

        // TODO : Delete null.
        if(soulTrans != null)
            mPlayerSoul = soulTrans.GetComponent<PlayerSoul>();

        for (int i = 0; i < totalCoroutine; i++)
        { mIsCoroutineList.Add(false); }

        life = GameManager.sSingleton.plyStartLife;
        bomb = GameManager.sSingleton.plyStartBomb;

        UIManager.sSingleton.UpdatePower(playerID, powerLevel, maxPowerLevel);
        UIManager.sSingleton.UpdateLinkBar(playerID, linkValue);

        mFairy = mAttackPattern.secondaryAttackType;
	}
	
	void Update () 
    {
        if (state == State.DEAD || GameManager.sSingleton.currState == GameManager.State.DIALOGUE) return;

		if ( (playerID == 1 &&  ( (mIsP1KeybInput && Input.GetKeyDown(KeyCode.Escape)) || Input.GetKeyDown(mJoystick.pauseKey))) ||
			( playerID == 2 && (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(mJoystick.pauseKey))) )
        {
            if ((playerID == 1 && !UIManager.sSingleton.isPlayer2Pause) ||
               (playerID == 2 && !UIManager.sSingleton.isPlayer1Pause))
            {
				bool isPauseMenu = UIManager.sSingleton.IsPauseGameOverMenu;

                if (!isPauseMenu) UIManager.sSingleton.EnablePauseScreen(playerID);
                else UIManager.sSingleton.DisablePauseScreen();
            }
        }

		if (UIManager.sSingleton.IsPauseGameOverMenu) return;

		if (transform.position.y > border.autoCollect) 
		{
			if (mOtherPlayerTrans != null && mOtherPlayerTrans.position.y > border.autoCollect) EnvObjManager.sSingleton.MoveAllPUToRandPlayer ();
			else EnvObjManager.sSingleton.MoveAllPUToPlayer (playerID);
		}

        if (mIsInvinsible && !mIsChangeAlpha) StartCoroutine(GetDamagedAlphaChange());
        if (state == State.NORMAL)
        {
            if (mIsInvinsible)
            {
                mInvinsibilityTimer += Time.deltaTime;
                if (mInvinsibilityTimer >= GameManager.sSingleton.plyInvinsibilityTime)
                {
                    mIsInvinsible = false;
                    mInvinsibilityTimer = 0;
                }
            }

            HandleMovement();
            HandleAttack();
        }
		else if (state == State.SOUL)
        {
			if (playerID == 1 && ((mIsP1KeybInput && Input.GetKey(KeyCode.Return)) || Input.GetKeyDown(mJoystick.reviveKey)) ||
				playerID == 2 && (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKeyDown(mJoystick.reviveKey)))
            {
                // Self-Revive
				if (life > 0) 
				{
					MinusLife();
					ReviveSelf();
					
				}
            }
        }
        else if (state == State.DISABLE_CONTROL)
        {
            Vector3 pos = transform.position;
            pos.y += Time.deltaTime * GameManager.sSingleton.plyRespawnYSpd;
            transform.position = pos;

            mDisableCtrlTimer += Time.deltaTime;
            if (mDisableCtrlTimer >= GameManager.sSingleton.plyDisabledCtrlTime)
            {
                mDisableCtrlTimer = 0;
                state = State.NORMAL;
            }
        }
	}

    public Vector3 PlayerSize { get { return this.mPlayerSize; } }
    public bool IsInvinsible { get { return this.mIsInvinsible; } }
    public bool IsContinueShoot
    {
        get 
        {
            if (state != State.NORMAL)
                return false;
            else return true;
        }
    }

    public void SlowlyReturnDefaultSpeed(bool isUnsDeltaTime)
    {
        StartCoroutine(SlowlyReturnToDefaultSpeed(isUnsDeltaTime));
    }

    public void UpdateScore(int addScore)
    {
		this.score += addScore;
        UIManager.sSingleton.UpdateScore(playerID, this.score);
    }

    public void UpdateLinkBar()
    {
        // TODO : Value to be changed later.
        linkValue += 0.01f * linkMultiplier;
        if (linkValue > 1) linkValue = 1;
        UIManager.sSingleton.UpdateLinkBar(playerID, linkValue);
    }

    public void ResetLinkBar()
    {
        linkValue = 0;
        UIManager.sSingleton.UpdateLinkBar(playerID, linkValue);
    }

    public void GetPowerUp(float val)
    {
        if (powerLevel < maxPowerLevel)
        {
            powerLevel += val;
            mFairy.UpdateSprite(Mathf.FloorToInt(powerLevel));

            if (powerLevel > maxPowerLevel) powerLevel = maxPowerLevel;
            UIManager.sSingleton.UpdatePower(playerID, powerLevel, maxPowerLevel);
        }
    }

    public void GetDamaged()
    {
        if (mIsInvinsible) return;

//        Debug.Log("Die");
// TODO: Player destroyed animation..
//        Destroy(gameObject);

		state = State.SOUL;
        DropPower();

        // Reset the values to default.
        powerLevel = 0;
        ResetDefaultSpeed();
        mIsInvinsible = true;
        mFairy.UpdateSprite(Mathf.FloorToInt(powerLevel));

        // Disable current sprite and activate soul transform.
        sr.enabled = false;
        hitBoxTrans.gameObject.SetActive(false);
        spriteBoxTrans.gameObject.SetActive(false);
        mPlayerSoul.Activate();

        UIManager.sSingleton.UpdatePower(playerID, powerLevel, maxPowerLevel);
        BulletManager.sSingleton.DisableEnemyBullets(true);

        if ( (life == 0 && !GameManager.sSingleton.IsTheOtherPlayerAlive (playerID) ||
            IsFinalSave() && mOtherPlayerController.IsFinalSave()) ) UIManager.sSingleton.EnableGameOverScreen ();
    }

    public bool IsFinalSave()
    {
        if (life == 0 && state == State.SOUL) return true;
        return false;
    }

    public void PlusLife()
    {
        if (life < GameManager.sSingleton.plyMaxLife)
        {
            life += 1;
            UIManager.sSingleton.UpdateLife(playerID, life);
        }
    }

	public void MinusLife()
	{
		life -= 1;
		UIManager.sSingleton.UpdateLife(playerID, life);

		if (life < 0) 
		{
			state = State.DEAD;
			GameManager.sSingleton.DisablePlayer (playerID);
		}
	}

    public void ReviveSelf()
    {
        state = State.DISABLE_CONTROL;
        transform.position = mResetPos;

        sr.enabled = true;
        hitBoxTrans.gameObject.SetActive(true);
        spriteBoxTrans.gameObject.SetActive(true);
        mPlayerSoul.Deactivate();
    }

    void HandleMovement()
    {
        if (BombManager.sSingleton.IsPause) return;

        if (playerID == 1)
        {
            //            // Player 1 only movement.
            //            if (Input.GetKey(KeyCode.UpArrow)) transform.Translate(Vector3.up * moveSpeed * Time.unscaledDeltaTime);
            //            if (Input.GetKey(KeyCode.LeftArrow)) transform.Translate(Vector3.left * moveSpeed * Time.unscaledDeltaTime);
            //            if (Input.GetKey(KeyCode.DownArrow)) transform.Translate(Vector3.down * moveSpeed * Time.unscaledDeltaTime);
            //            if (Input.GetKey(KeyCode.RightArrow)) transform.Translate(Vector3.right * moveSpeed * Time.unscaledDeltaTime);

            // Basic wasd movement.
			if (mIsP1KeybInput && Input.GetKey(KeyCode.T)) transform.Translate(Vector3.up * moveSpeed * Time.unscaledDeltaTime);
			if (mIsP1KeybInput && Input.GetKey(KeyCode.F)) transform.Translate(Vector3.left * moveSpeed * Time.unscaledDeltaTime);
			if (mIsP1KeybInput && Input.GetKey(KeyCode.G)) transform.Translate(Vector3.down * moveSpeed * Time.unscaledDeltaTime);
			if (mIsP1KeybInput && Input.GetKey(KeyCode.H)) transform.Translate(Vector3.right * moveSpeed * Time.unscaledDeltaTime);
        }
        else if(playerID == 2)
        {
            // Basic wasd movement.
            if (Input.GetKey(KeyCode.UpArrow)) transform.Translate(Vector3.up * moveSpeed * Time.unscaledDeltaTime);
            if (Input.GetKey(KeyCode.LeftArrow)) transform.Translate(Vector3.left * moveSpeed * Time.unscaledDeltaTime);
            if (Input.GetKey(KeyCode.DownArrow)) transform.Translate(Vector3.down * moveSpeed * Time.unscaledDeltaTime);
            if (Input.GetKey(KeyCode.RightArrow)) transform.Translate(Vector3.right * moveSpeed * Time.unscaledDeltaTime);
        }

		if (!mIsShiftPressed && ( (playerID == 1 && ((mIsP1KeybInput && Input.GetKey(KeyCode.LeftShift)) || Input.GetKey(mJoystick.slowMoveKey)))
			|| (playerID == 2 && (Input.GetKey(KeyCode.Comma) || Input.GetKey(mJoystick.slowMoveKey))) )) moveSpeed *= 0.5f; 
            
        // Prevent player from moving out of screen.
        transform.position = (new Vector3 (
            Mathf.Clamp (transform.position.x, border.left, border.right),
            Mathf.Clamp (transform.position.y, border.bottom, border.top),
            transform.position.z)
        );
    }

    void HandleAttack()
    {
		if ( ((playerID == 1 && ((mIsP1KeybInput && Input.GetKey(KeyCode.LeftShift)) || Input.GetKey(mJoystick.slowMoveKey))) || 
			(playerID == 2 && Input.GetKey(KeyCode.Comma)) || Input.GetKey(mJoystick.slowMoveKey) ) && !mIsShiftPressed)
        {
            mFairy.FocusedStance();
            mIsShiftPressed = true;
        }
		else if ( (playerID == 1 && ((mIsP1KeybInput && Input.GetKeyUp(KeyCode.LeftShift)) || Input.GetKeyUp(mJoystick.slowMoveKey))) ||
			(playerID == 2 && (Input.GetKeyUp(KeyCode.Comma) || Input.GetKeyUp(mJoystick.slowMoveKey))) ) ResetDefaultSpeed();

        if (BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.NONE)
        {
            // Primary attack.
			if ( (playerID == 1 && (mIsP1KeybInput && Input.GetKey(KeyCode.Z)) || Input.GetKeyUp(mJoystick.fireKey)) || 
				(playerID == 2 && (Input.GetKey(KeyCode.Period) || Input.GetKeyUp(mJoystick.fireKey))) )
            {
                if(!mIsCoroutineList[0]) StartCoroutine(DoFirstThenDelay(0, () => mAttackPattern.PrimaryWeaponShoot(), primaryShootDelay));
                if(powerLevel > 0 && !mIsCoroutineList[1]) StartCoroutine(DoFirstThenDelay(1, () => mAttackPattern.SecondaryWeaponShoot(), secondaryShootDelay));
            }
        }

        // Bomb.
		if (( (playerID == 1 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.X) || Input.GetKey(mJoystick.bombKey))) ) || 
			(playerID == 2 && (Input.GetKeyDown(KeyCode.Slash) || Input.GetKey(mJoystick.bombKey))) ) && 
			!mBombController.IsUsingBomb && !mIsWaitOtherInput )
        {
			if (bomb > 0) 
			{
                bomb -= 1;
                UIManager.sSingleton.UpdateBomb(playerID, bomb);

				if (GameManager.sSingleton.TotalNumOfPlayer() == 2 && mOtherPlayerController.state != State.SOUL)
                {
                    // The other player receiver during dual link ultimate.
                    if(BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.PLAYER_INPUTTED)
                    {
                        Debug.Log("Activate pause before shooting.");
                        BombManager.sSingleton.dualLinkState = BombManager.DualLinkState.BOTH_PLAYER_INPUTTED;
                        mBombController.ActivatePotrait();
                    }

                    // If both players gauge are full, stop time for input of second player.
                    if (linkValue >= 1 && mOtherPlayerController.linkValue >= 1 && BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.NONE) 
                    {
                        Debug.Log("Player started dual link bomb");
                        Time.timeScale = 0;
                        mBombController.ActivatePotrait();

                        mIsWaitOtherInput = true;
                        BombManager.sSingleton.dualLinkState = BombManager.DualLinkState.PLAYER_INPUTTED;
                        StartCoroutine (WaitOtherResponseSequence (BombManager.sSingleton.bombDualLinkInputDur));
                    }
                }

                if(BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.NONE) mBombController.ActivateBomb();
			}
        }
    }

    void ResetDefaultSpeed() 
    { 
        moveSpeed = mDefaultMoveSpeed; 
        mIsShiftPressed = false;
    }

    // Drop power down on screen when died.
    void DropPower()
    {
        Vector2 targetDir = Vector2.zero;
        Vector2 currPos = transform.position;

        float xVal = 0;
        if (currPos.x < mP1DefaultPos.x) xVal = currPos.x / border.left;
        else if (currPos.x > mP2DefaultPos.x) xVal = -(currPos.x / border.right);

        targetDir = new Vector2(xVal, 1);
        float angle = Vector2.Angle(targetDir, transform.up) * Mathf.Deg2Rad;

        if (targetDir.x < 0)
            angle = -angle;
        
        float dropAngle = GameManager.sSingleton.powerDropAngle;
        float halfViewAngle = ((dropAngle * Mathf.Deg2Rad) / 2);
        float startAngle = angle - halfViewAngle;
        float endAngle = angle + halfViewAngle;

        int segments = GameManager.sSingleton.totalPowerDrop;
        float inc = (dropAngle * Mathf.Deg2Rad) / segments;

        float totalAngle = startAngle;

        for (float i = 0; i < segments + 1; i++)
        {
            float newAngle = 0;
            if (i == 0) newAngle = startAngle;
            else if (i == segments) newAngle = endAngle;
            else 
            {
                totalAngle += inc;
                newAngle = totalAngle;
            }

            float x = Mathf.Sin(newAngle);
            float y = Mathf.Cos(newAngle);

            Vector2 target = new Vector3(transform.position.x + x, transform.position.y + y);
            Vector3 dir = (target - (Vector2)transform.position).normalized;

            Transform currPowerUp = EnvObjManager.sSingleton.GetBigPowerUp();
            currPowerUp.position = transform.position;
            currPowerUp.GetComponent<EnvironmentalObject>().SetToFlyOut(dir);
            currPowerUp.gameObject.SetActive(true);
        }
    }

    IEnumerator DoFirstThenDelay(int index, Action doFirst, float time)
    {
        mIsCoroutineList[index] = true;
        doFirst();
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(time)); 
        mIsCoroutineList[index] = false;
    }

    IEnumerator GetDamagedAlphaChange()
    {
        mIsChangeAlpha = true;

        Color temp = sr.color;
        temp.a = GameManager.sSingleton.plyRespawnAlpha;
        sr.color = temp;

        yield return new WaitForSeconds(GameManager.sSingleton.plyAlphaBlinkDelay);

        temp.a = 1;
        sr.color = temp;

        yield return new WaitForSeconds(GameManager.sSingleton.plyAlphaBlinkDelay);
        mIsChangeAlpha = false;
    }

	IEnumerator WaitOtherResponseSequence (float stopDur)
	{
		float timer = 0;
		while(timer < stopDur)
		{
			// The real dual link activation happens in the bomb manager.
            if (BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.BOTH_PLAYER_INPUTTED) 
			{
				mIsWaitOtherInput = false;
				yield break;
			}

			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		Time.timeScale = 1;
		mIsWaitOtherInput = false;
        mBombController.ResetDualLinkVal();

        mBombController.ActivateBomb();
		Debug.Log ("Time ended.");
	}

    IEnumerator SlowlyReturnToDefaultSpeed(bool isUnsDeltaTime)
    {
        float val = 0;
        while (moveSpeed != mDefaultMoveSpeed)
        {
            if (isUnsDeltaTime) val = Time.unscaledDeltaTime;
            else val = Time.deltaTime;

            if (moveSpeed > mDefaultMoveSpeed) val = -val;

            moveSpeed += val;
            if ( (val > 0 && moveSpeed > mDefaultMoveSpeed) || (val < 0 && moveSpeed < mDefaultMoveSpeed) )
                moveSpeed = mDefaultMoveSpeed;
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
		if (state != State.SOUL)
        {
            if (other.tag == TagManager.sSingleton.ENV_OBJ_PowerUp1Tag || other.tag == TagManager.sSingleton.ENV_OBJ_PowerUp2Tag ||
                other.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp1Tag || other.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp2Tag ||
                other.tag == TagManager.sSingleton.ENV_OBJ_LifePickUpTag)
                other.GetComponent<EnvironmentalObject>().SetPlayer(transform);
        }
    }
}
