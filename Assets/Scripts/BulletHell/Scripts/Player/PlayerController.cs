using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour 
{
    public int playerID = 1;
    public int characterID = 1;
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
	public float scoreMult = 1;
    public float linkValue = 0;
    public float linkMultiplier = 0.5f;
    public float secondarylinkMultiplier = 0.2f;

    public SpriteRenderer charImageRenderer;
    public ParticleSystem hitboxPS;
    public List<ParticleSystem> shotSparksPSList;

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
	bool mIsShiftPressed = false, mIsChangeAlpha = false, mIsInvinsible = false, mIsWaitOtherInput = false;
    bool mIsP1KeybInput = true, mIsShownPoweredUp = true;

    int totalCoroutine = 2;
    List<bool> mIsCoroutineList = new List<bool>();

    Animator anim;
    ParticleSystem mBrokenHeartPS, mRevivedPS, mDeathPS, mLinkFlame;
    SpriteRenderer sr;
    AttackPattern mAttackPattern;
    SecondaryAttackType mFairy;
    BombController mBombController;
    PlayerSoul mPlayerSoul;
	PlayerController mOtherPlayerController;

    void Start () 
    {
        mIsP1KeybInput = JoystickManager.sSingleton.IsP1KeybInput;
            
		if (playerID == 1) mJoystick = JoystickManager.sSingleton.p1_joystick;
		else if (playerID == 2) mJoystick = JoystickManager.sSingleton.p2_joystick;

        if (GameManager.sSingleton.TotalNumOfPlayer() == 2)
        {
			if (playerID == 1) mOtherPlayerTrans = GameManager.sSingleton.player2;
			else mOtherPlayerTrans = GameManager.sSingleton.player1;

			mOtherPlayerController = mOtherPlayerTrans.GetComponent<PlayerController> ();
        }

        mDefaultMoveSpeed = moveSpeed;
        mPlayerSize = GetComponentInChildren<Renderer>().bounds.size;
        anim = GetComponentInChildren<Animator>();

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

        sr = charImageRenderer;
        mAttackPattern = GetComponent<AttackPattern>();
        mBombController = GetComponent<BombController>();

        if(soulTrans != null) mPlayerSoul = soulTrans.GetComponent<PlayerSoul>();

        for (int i = 0; i < totalCoroutine; i++)
        { mIsCoroutineList.Add(false); }

        life = GameManager.sSingleton.plyStartLife;
        bomb = GameManager.sSingleton.plyStartBomb;

        UIManager.sSingleton.UpdatePower(playerID, powerLevel, maxPowerLevel);
        if (GameManager.sSingleton.TotalNumOfPlayer() == 2) UIManager.sSingleton.UpdateLinkBar(playerID, linkValue);

        mFairy = mAttackPattern.secondaryAttackType;

        mBrokenHeartPS = Instantiate(GameManager.sSingleton.brokenHeartPS, Vector3.zero, GameManager.sSingleton.brokenHeartPS.transform.rotation);
        mRevivedPS = Instantiate(GameManager.sSingleton.revivedPS, Vector3.zero, Quaternion.identity);
		mDeathPS = Instantiate(GameManager.sSingleton.deathPS, Vector3.zero, Quaternion.identity);

		mLinkFlame = Instantiate(GameManager.sSingleton.linkFlamePS, Vector3.zero, Quaternion.identity);
		mLinkFlame.transform.position = UIManager.sSingleton.GetLinkBarPos (playerID - 1);
        mLinkFlame.gameObject.SetActive(false);
	}
	
	void Update () 
    {
        if (state == State.DEAD || !GameManager.sSingleton.IsMoveDuringDialogue() || !GameManager.sSingleton.IsPlayerInteractable()) return;

        if ( (playerID == 1 &&  ( (mIsP1KeybInput && Input.GetKeyDown(KeyCode.Escape)) || Input.GetKeyDown(mJoystick.startKey))) ||
            ( playerID == 2 && (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(mJoystick.startKey))) )
        {
            if ((playerID == 1 && !UIManager.sSingleton.isPlayer2Pause) ||
               (playerID == 2 && !UIManager.sSingleton.isPlayer1Pause))
            {
				bool isPauseMenu = UIManager.sSingleton.IsPause;

                if (!isPauseMenu) UIManager.sSingleton.EnablePauseScreen(playerID);
				else UIManager.sSingleton.DisablePauseScreen();
            }
        }

        if (UIManager.sSingleton.IsPauseGameOverMenu || UIManager.sSingleton.IsShowScoreRankNameInput) return;

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

            if (transform.position.y > border.autoCollect) 
            {
                if (mOtherPlayerTrans != null && mOtherPlayerTrans.position.y > border.autoCollect) EnvObjManager.sSingleton.MoveAllPUToRandPlayer ();
                else EnvObjManager.sSingleton.MoveAllPUToPlayer (playerID);
            }

            if (!BombManager.sSingleton.IsPause && !BombManager.sSingleton.IsShooting)
            {
                if (bomb == 0 || (mOtherPlayerController != null && mOtherPlayerController.bomb == 0))
                {
                    DisableFlamePS();
                    UIManager.sSingleton.DeactivateBothLinkBar();
                }
            }

            if (GameManager.sSingleton.IsMoveDuringDialogue()) HandleMovement();
			if (!GameManager.sSingleton.IsBossMakeEntrance()) HandleAttack();
            HandleFocusedMode();
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
					ReviveSelf(true);
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

    public JoystickManager.JoystickInput GetJoystickInput { get { return mJoystick; } }
    public Vector3 PlayerSize { get { return this.mPlayerSize; } }
    public bool IsShiftPressed { get { return this.mIsShiftPressed; } }
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
        if (!IsUpdateLinkBar()) return;

        linkValue += 0.01f * linkMultiplier;
        ShowFlameUpdateUI();
    }

    public void UpdateLinkBar(float multiplier)
    {
        if (!IsUpdateLinkBar()) return;

        linkValue += 0.01f * (linkMultiplier + multiplier);
        ShowFlameUpdateUI();
    }

    public void UpdateLinkBarForSecondary()
    {
        if (!IsUpdateLinkBar()) return;

        linkValue += 0.01f * secondarylinkMultiplier;
        ShowFlameUpdateUI();
    }

    public void UpdateLinkBarForSecondary(float multiplier)
    {
        if (!IsUpdateLinkBar()) return;

        linkValue += 0.01f * (secondarylinkMultiplier + multiplier);
        ShowFlameUpdateUI();
    }

    public void ResetLinkBar()
    {
        linkValue = 0;
        UIManager.sSingleton.UpdateLinkBar(playerID, linkValue);

        DisableFlamePS();
    }

    public void GetPowerUp(float val)
    {
        if (powerLevel < maxPowerLevel)
        {
            float prevPowerLevel = powerLevel;
            powerLevel += val;

            if (powerLevel > maxPowerLevel) powerLevel = maxPowerLevel;
            mFairy.UpdateSprite(Mathf.FloorToInt(powerLevel));

            if (Mathf.FloorToInt(powerLevel) >= Mathf.FloorToInt(prevPowerLevel) + 1) mIsShownPoweredUp = false;

            if (!mIsShownPoweredUp)
            {
                mIsShownPoweredUp = true;

                Transform powerUpTextTrans = EnvObjManager.sSingleton.GetPowerUpText();
                powerUpTextTrans.position = transform.position;
                powerUpTextTrans.gameObject.SetActive(true);

                if (AudioManager.sSingleton != null) AudioManager.sSingleton.PlayPowerLevelUpSfx();
            }

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

        // Check for game over status.
        if ( ((life == 0 && !GameManager.sSingleton.IsTheOtherPlayerActive(playerID)) ||
            (IsFinalSave() && mOtherPlayerController.IsFinalSave())) )
        {
            state = State.DEAD;
            mPlayerSoul.Deactivate();

            mDeathPS.transform.position = transform.position;
            mDeathPS.Play();

            if (mOtherPlayerController.state == State.SOUL) mOtherPlayerController.PlayDeathPS();

            UIManager.sSingleton.EnableGameOverScreen();
        }

        // If only 1 player is left, auto revive self. Do not go into soul mode.
        if (GameManager.sSingleton.TotalNumOfPlayer() == 1 && life != 0)
        {
            ReviveSelf(true);
            MinusLife();
        }
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
            PlayDeathPS();

            UIManager.sSingleton.GreyOutPlayerUI(playerID);
			GameManager.sSingleton.DisablePlayer (playerID);
		}
	}

    public void PlayDeathPS()
    {
        mDeathPS.transform.position = transform.position;
        mDeathPS.Play();
    }

    public void ReviveSelf(bool isSelfRevive)
    {
        state = State.DISABLE_CONTROL;

        if (isSelfRevive)
        {
            mBrokenHeartPS.transform.position = transform.position;
            mBrokenHeartPS.Play();
        }
        else if (!isSelfRevive)
        {
            scoreMult = GameManager.sSingleton.scoreMultAfterRes;
            UIManager.sSingleton.UpdateScoreMultiplier(playerID, scoreMult);
        }

        mRevivedPS.transform.position = transform.position;
        mRevivedPS.Play();
        transform.position = mResetPos;

        mPlayerSoul.Deactivate();

        sr.enabled = true;
        hitBoxTrans.gameObject.SetActive(true);
        spriteBoxTrans.gameObject.SetActive(true);
    }

    public void UpdateMultiplier(float mult)
    {
        scoreMult += mult;
        UIManager.sSingleton.UpdateScoreMultiplier(playerID, scoreMult);
    }

    public void DisableFlamePS()
    {
        mLinkFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        mLinkFlame.gameObject.SetActive(false);
    }

    void HandleMovement()
    {
        if (BombManager.sSingleton.IsPause) return;

        float deltaTime = GameManager.sSingleton.GetDeltaTime();;
        if (playerID == 1)
        {
            // Basic keyboard movement.
            if (mIsP1KeybInput)
            {
                if (Input.GetKey(KeyCode.T)) transform.Translate(Vector3.up * moveSpeed * deltaTime);
                if (Input.GetKey(KeyCode.F))  transform.Translate(Vector3.left * moveSpeed * deltaTime);
                if (Input.GetKey(KeyCode.G)) transform.Translate(Vector3.down * moveSpeed * deltaTime);
                if (Input.GetKey(KeyCode.H)) transform.Translate(Vector3.right * moveSpeed * deltaTime);
            }
            else
            {
                // Joystick movement.
                float xTranslation = 0, yTranslation = 0;

                float horizontal = Input.GetAxis("HorizontalP1");
                float vertical = Input.GetAxis("VerticalP1");
                float horizontalDpad = Input.GetAxis("HorizontalP1Dpad");
                float verticalDpad = Input.GetAxis("VerticalP1Dpad");

                if (horizontal != 0 || vertical != 0)
                {
                    xTranslation = horizontal * moveSpeed * deltaTime;
                    yTranslation = vertical * moveSpeed * deltaTime;
                }
                else if (horizontalDpad != 0 || verticalDpad != 0)
                {
                    xTranslation = horizontalDpad * moveSpeed * deltaTime;
                    yTranslation = -verticalDpad * moveSpeed * deltaTime;
                }

                // Set the animation.
                if (anim.GetInteger("State") == 0)
                {
                    // Left and right.
                    if (horizontal < 0 || horizontalDpad < 0) anim.SetInteger("State", 1);  
                    else if (horizontal > 0 || horizontalDpad > 0) anim.SetInteger("State", 2);  
                }
                // If it's in left animation, but player's movement is now right.
                else if (anim.GetInteger("State") == 1 && (horizontal > 0 || horizontalDpad > 0)) anim.SetInteger("State", 3);  
                // If it's in right animation, but player's movement is now left.
                else if (anim.GetInteger("State") == 2 && (horizontal < 0 || horizontalDpad < 0)) anim.SetInteger("State", 4);  
                else if (horizontal == 0 && horizontalDpad == 0) anim.SetInteger("State", 0);  

                transform.Translate(Vector3.right * xTranslation);
                transform.Translate(Vector3.up * yTranslation);
            }
        }
        else if(playerID == 2)
        {
            // Basic keyboard movement.
            if (Input.GetKey(KeyCode.UpArrow)) transform.Translate(Vector3.up * moveSpeed * deltaTime);
            if (Input.GetKey(KeyCode.LeftArrow)) transform.Translate(Vector3.left * moveSpeed * deltaTime);
            if (Input.GetKey(KeyCode.DownArrow)) transform.Translate(Vector3.down * moveSpeed * deltaTime);
            if (Input.GetKey(KeyCode.RightArrow)) transform.Translate(Vector3.right * moveSpeed * deltaTime);

            // Joystick movement.
            float xTranslation = 0, yTranslation = 0;

            float horizontal = Input.GetAxis("HorizontalP2");
            float vertical = Input.GetAxis("VerticalP2");
            float horizontalDpad = Input.GetAxis("HorizontalP2Dpad");
            float verticalDpad = Input.GetAxis("VerticalP2Dpad");

            if (horizontal != 0 || vertical != 0)
            {
                xTranslation = horizontal * moveSpeed * deltaTime;
                yTranslation = vertical * moveSpeed * deltaTime;
            }
            else if (horizontalDpad != 0 || verticalDpad != 0)
            {
                xTranslation = horizontalDpad * moveSpeed * deltaTime;
                yTranslation = -verticalDpad * moveSpeed * deltaTime;
            }

			// Set the animation.
//			if (anim.GetInteger("State") == 0)
//			{
//				// Left and right.
//				if (horizontal < 0 || horizontalDpad < 0) anim.SetInteger("State", 1);  
//				else if (horizontal > 0 || horizontalDpad > 0) anim.SetInteger("State", 2);  
//			}
//			// If it's in left animation, but player's movement is now right.
//			else if (anim.GetInteger("State") == 1 && (horizontal > 0 || horizontalDpad > 0)) anim.SetInteger("State", 3);  
//			// If it's in right animation, but player's movement is now left.
//			else if (anim.GetInteger("State") == 2 && (horizontal < 0 || horizontalDpad < 0)) anim.SetInteger("State", 4);  
//			else if (horizontal == 0 && horizontalDpad == 0) anim.SetInteger("State", 0);  

            transform.Translate(Vector3.right * xTranslation);
            transform.Translate(Vector3.up * yTranslation);
        }

        // Prevent player from moving out of screen.
        transform.position = (new Vector3 (
            Mathf.Clamp (transform.position.x, border.left, border.right),
            Mathf.Clamp (transform.position.y, border.bottom, border.top),
            transform.position.z)
        );
    }

    void HandleFocusedMode()
    {
        if ( ((playerID == 1 && ((mIsP1KeybInput && Input.GetKey(KeyCode.LeftShift)) || Input.GetKey(mJoystick.slowMoveKey))) || 
            (playerID == 2 && Input.GetKey(KeyCode.Comma)) || Input.GetKey(mJoystick.slowMoveKey) ) && !mIsShiftPressed)
        {
            moveSpeed *= 0.5f;
            hitboxPS.Play();
            mFairy.FocusedStance();
            mIsShiftPressed = true;
        }
        else if ( (playerID == 1 && ((mIsP1KeybInput && Input.GetKeyUp(KeyCode.LeftShift)) || Input.GetKeyUp(mJoystick.slowMoveKey))) ||
                  (playerID == 2 && (Input.GetKeyUp(KeyCode.Comma) || Input.GetKeyUp(mJoystick.slowMoveKey))) ) ResetDefaultSpeed();
    }

    void HandleAttack()
    {
        if (BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.NONE)
        {
            // Primary attack.
			if ( (playerID == 1 && (mIsP1KeybInput && Input.GetKey(KeyCode.Z)) || Input.GetKey(mJoystick.fireKey)) || 
				(playerID == 2 && (Input.GetKey(KeyCode.Period) || Input.GetKey(mJoystick.fireKey))) )
            {
                if (!mIsCoroutineList[0])
                {
                    BulletSparks();
                    StartCoroutine(DoFirstThenDelay(0, () => mAttackPattern.PrimaryWeaponShoot(), primaryShootDelay));
                }
                if(powerLevel > 0 && !mIsCoroutineList[1]) StartCoroutine(DoFirstThenDelay(1, () => mAttackPattern.SecondaryWeaponShoot(), secondaryShootDelay));
            }
            else if( (playerID == 1 && (mIsP1KeybInput && Input.GetKeyUp(KeyCode.Z)) || Input.GetKeyUp(mJoystick.fireKey)) || 
                (playerID == 2 && (Input.GetKeyUp(KeyCode.Period) || Input.GetKeyUp(mJoystick.fireKey))) )
            {
                for (int i = 0; i < shotSparksPSList.Count; i++)
                {
                    shotSparksPSList[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
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

                if (GameManager.sSingleton.TotalNumOfPlayer() == 2 && mOtherPlayerController.state != State.SOUL && mOtherPlayerController.state != State.DISABLE_CONTROL)
                {
                    // The other player receiver during dual link ultimate.
                    if(BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.PLAYER_INPUTTED)
                    {
                        mIsWaitOtherInput = true;
                        Debug.Log("Activate pause before shooting.");

                        mBombController.ActivatePotrait();
                        BombManager.sSingleton.BothPlayerInputted();
                    }

                    // If both players gauge are full, stop time for input of second player.
                    if (linkValue >= 1 && mOtherPlayerController.linkValue >= 1 && BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.NONE) 
                    {
                        Debug.Log("Player started dual link bomb");
                        Time.timeScale = 0;
                        mBombController.ActivatePotrait();

                        mIsWaitOtherInput = true;
                        BombManager.sSingleton.OnePlayerInputted();
                        StartCoroutine (WaitOtherResponseSequence (BombManager.sSingleton.bombDualLinkInputDur));
                    }
                }

                if(BombManager.sSingleton.dualLinkState == BombManager.DualLinkState.NONE) mBombController.ActivateBomb();
			}
        }
    }

    void BulletSparks()
    {
        if (mAttackPattern.isAlternateFire)
        {
            if (mAttackPattern.GetCurrAlternateFire == AttackPattern.AlternateFire.LEFT) shotSparksPSList[0].Play();
            else if (mAttackPattern.GetCurrAlternateFire == AttackPattern.AlternateFire.RIGHT) shotSparksPSList[1].Play();
        }
        else
        {
            for (int i = 0; i < shotSparksPSList.Count; i++)
            {
                shotSparksPSList[i].Play();
            }
        }
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

        int segments = GameManager.sSingleton.totalPowerDrop - 1;
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

    void ResetDefaultSpeed() 
    { 
        hitboxPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        moveSpeed = mDefaultMoveSpeed; 
        mIsShiftPressed = false;
    }

    bool IsUpdateLinkBar()
    {
        if (bomb == 0 || mOtherPlayerController == null ||
            (mOtherPlayerController != null && (mOtherPlayerController.bomb == 0 || mOtherPlayerController.state == State.DEAD))) 
            return false;
        return true;
    }

    void ShowFlameUpdateUI()
    {
        if (linkValue > 1) linkValue = 1;
        if (linkValue == 1)
        {
            mLinkFlame.gameObject.SetActive(true);
            mLinkFlame.Play();
        }

        UIManager.sSingleton.UpdateLinkBar(playerID, linkValue);
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
                mOtherPlayerController.mIsWaitOtherInput = false;

                mBombController.IsUsingBomb = true;
                mOtherPlayerController.mBombController.IsUsingBomb = true;

				yield break;
			}

            while (UIManager.sSingleton.IsPauseGameOverMenu)
            {
                yield return null;
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
        if (state != State.SOUL && state != State.DEAD)
        {
            if (other.tag == TagManager.sSingleton.ENV_OBJ_PowerUp1Tag || other.tag == TagManager.sSingleton.ENV_OBJ_PowerUp2Tag ||
                other.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp1Tag || other.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp2Tag ||
                other.tag == TagManager.sSingleton.ENV_OBJ_LifePickUpTag)
                other.GetComponent<EnvironmentalObject>().SetPlayer(transform);
        }
    }
}
