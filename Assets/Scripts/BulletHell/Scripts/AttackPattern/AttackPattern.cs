using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackPattern : MonoBehaviour 
{
    // Base stats.
    public enum OwnerType
    {
        PLAYER = 0,
        ENEMY,
    }
    public OwnerType ownerType = OwnerType.ENEMY;

    public Transform owner;
    public float mainBulletDamage = 1, secondaryBulletDamage = 1, hpPercentSkipAtk = 100;
    public float mainBulletSpeed = 5, secondaryBulletSpeed = 5, shootDelay = 0.2f;
    public float pauseEverySec = 8, pauseDur = 1;
    public bool isMainPiercing = false, isSecondaryPiercing = false, isAlternateFire = false, isMagnum = false;
    public float alternateFireOffset = 1;
	public float magnumMarkedDuration = 1.5f;

    // Base laser stats.
    public float damagePerFrame = 0.1f, expandSpeed = 7, expandTillXScale = 3.5f;
    public bool isDestroyBullets = false;

    // Bullet stats.
    public BulletPrefabData bulletPrefabData;
    public int mainBulletIndex, secondaryBulletIndex;

    // Player stats.
    public Vector2 bulletDirection, mainBulletOffset, secondaryBulletOffset;

    // Enemy stats.
    public enum Target
    {
        PLAYER_1 = 0,
        PLAYER_2,
        RANDOM
    }
    public Target target = Target.PLAYER_1;
    public float duration = 10, onceStartDelay;
    public bool isShowDuration = false, isPotraitShow = false, isShootPlayer = true, isFollowPlayer = true, isDirectionFromOwner = false;
    public Sprite charSprite, spellCardSprite;
    public Vector2 shootDirection;
    //    public bool isDisBulletAftDone = false;

    public enum BulletType
    {
        PROJECTILE = 0,
		PROJECTILE_LOCK_ON,
        LASER
    }
    public BulletType bulletType = BulletType.PROJECTILE;

    // Pattern template.
    public enum Template
    {
        SINGLE_SHOT = 0,
        ANGLE_SHOT,
        SHOOT_AROUND_IN_CIRCLE,
        DOUBLE_SINE_WAVE,
        SHOCK_REPEL_AND_TRAP_LASER
    }
    public Template template = Template.SINGLE_SHOT;

    // Shoot at player values.
    public float initialSpacing;
    public int viewAngle = 90, segments;

    // Shoot around in circle values.
    public bool isClockwise = true;
    public float distance, startTurnDelay, turningRate, increaseTR, increaseTRTime, maxTR;
    public float xOffset, yOffset;

    [System.Serializable]
    public class UpdateSpeed
    {
        public float changeSpeedTime, toSpeed, toSpeedQuickness;
        public UpdateSpeed() { changeSpeedTime = toSpeed = toSpeedQuickness = 0; }
    }
    public List<UpdateSpeed> speedChangeList = new List<UpdateSpeed>();

    // Sine wave values.
    public Vector2 offsetPosition;
    public float frequency, magnitude, magExpandMult, sineWaveBullets, cooldown;

    // Shock repel values.
    public float giveStatRepelDur = 2.5f, slowValue = 1.0f, repelValue = 90.0f;
	public float trapDelayExpand = 2.0f, trapExpandDur = 5.0f, trapExpandSpd = 1.5f;
	public float trapRotateSpd = 3, trapMoveSpd = 1.5f, trapDelayShrink, trapShrinkSpd = 1, trapFadeSpd = 2;

    // Values that are to be sent over to individual bullets.
    public class Properties
    {
        // Fixed value.
        public OwnerType ownerType;
        public Template template;
        public float damage, speed, frequency, magnitude, magExpandMult;
        public float giveStatRepelDur, slowValue, repelValue;
		public float trapDelayExpand, trapExpandDur, trapExpandSpd, trapRotateSpd, trapMoveSpd, trapDelayShrink, trapShrinkSpd, trapFadeSpd;
        public bool isAlternateFire, isMagnum;

        // Values that will be changed real-time.
        public Vector2 direction;
        public Vector2 curveAxis;

        public Properties()
        {
            ownerType = OwnerType.ENEMY;
            template = Template.SINGLE_SHOT;
            isAlternateFire = isMagnum = false;
            damage = speed = frequency = magnitude = magExpandMult = 0;
			giveStatRepelDur = slowValue = repelValue = trapDelayExpand = trapExpandDur = trapExpandSpd = trapMoveSpd = trapDelayShrink = trapShrinkSpd = trapFadeSpd = 0;
            direction = curveAxis = Vector2.zero;
        }
    }
    Properties properties = new Properties();

    public SecondaryAttackType secondaryAttackType;
    public WithinRange withinRange;

	public enum AlternateFire
	{
		LEFT = 0,
		RIGHT
	}
	AlternateFire alternateFire = AlternateFire.LEFT;

    Vector2 mSavedDir = Vector2.zero;
    float mPauseTimer, mTimer, mAngle, mIncreaseTRTimer, mSlowDownTimer;
    bool mIsCoroutine = false;

    List<IEnumerator> mAttackCoList = new List<IEnumerator>();
    PlayerController mPlayerController;

    // Used for testing purposes in editor-mode.
    Template mSavedTemplateList;
    int savedMainBulletIndex, savedSecondaryBulletIndex;

    void Start()
    {
        GetNewestSameBulletList();
        UpdateProperties();
        mSavedTemplateList = template;

        if (owner == null) owner = transform.parent;
        GameManager.sSingleton.MagnumMarkedDuration = magnumMarkedDuration;
    }

    void GetNewestSameBulletList()
    {
        savedMainBulletIndex = mainBulletIndex;

        if (ownerType == OwnerType.PLAYER)
        {
            savedSecondaryBulletIndex = secondaryBulletIndex;

            mPlayerController = GetComponent<PlayerController>();

            string p1Tag = TagManager.sSingleton.player1Tag;
            string p2Tag = TagManager.sSingleton.player2Tag;

            if (transform.tag == p1Tag)
            {
                BulletManager.sSingleton.SetBulletTag(BulletManager.GroupIndex.PLAYER_MAIN, mainBulletIndex, TagManager.sSingleton.player1BulletTag);
                BulletManager.sSingleton.SetBulletTag(BulletManager.GroupIndex.PLAYER_SECONDARY, secondaryBulletIndex, TagManager.sSingleton.player1BulletTag);
            }
            else if (transform.tag == p2Tag)
            {
                BulletManager.sSingleton.SetBulletTag(BulletManager.GroupIndex.PLAYER_MAIN, mainBulletIndex, TagManager.sSingleton.player2BulletTag);
                BulletManager.sSingleton.SetBulletTag(BulletManager.GroupIndex.PLAYER_SECONDARY, secondaryBulletIndex, TagManager.sSingleton.player2BulletTag);
            }

            if (bulletType == BulletType.LASER)
            {
                Transform prefab = bulletPrefabData.plyLaserTransNoCacheList[secondaryBulletIndex];
                secondaryAttackType.SetIsPiercing(isSecondaryPiercing);
                secondaryAttackType.SetLaserProperties(prefab, damagePerFrame, expandSpeed, expandTillXScale, isDestroyBullets);
            }
        }
    }

    void Update()
    {
        if(Application.isEditor)
        {
            // Will only enter when user change the attack pattern in editor during play-mode.
            if (mSavedTemplateList != template)
            {
                for (int j = 0; j < mAttackCoList.Count; j++)
                {
                    StopCoroutine(mAttackCoList[j]);
                }
                mAttackCoList.Clear();
                mIsCoroutine = false;
                mTimer = 0;
                mSavedTemplateList = template;
            }
            if (savedMainBulletIndex != mainBulletIndex || savedSecondaryBulletIndex != secondaryBulletIndex)
                GetNewestSameBulletList();
            
            UpdateProperties();
        }
    }

	public void SetAttackPattern(AttackPattern ap)
	{
        ownerType = ap.ownerType;
		mainBulletDamage = ap.mainBulletDamage; secondaryBulletDamage = ap.secondaryBulletDamage; hpPercentSkipAtk = ap.hpPercentSkipAtk;
		mainBulletSpeed = ap.mainBulletSpeed; secondaryBulletSpeed = ap.secondaryBulletSpeed; shootDelay = ap.shootDelay;
		isMainPiercing = ap.isMainPiercing; isSecondaryPiercing = ap.isSecondaryPiercing;
        pauseEverySec = ap.pauseEverySec; pauseDur = ap.pauseDur;

        mainBulletIndex = ap.mainBulletIndex; secondaryBulletIndex = ap.secondaryBulletIndex;

		// Base laser stats.
		damagePerFrame = ap.damagePerFrame; expandSpeed = ap.expandSpeed; expandTillXScale = ap.expandTillXScale;
		isDestroyBullets = ap.isDestroyBullets;

		// Enemy stats.
		target = ap.target;
		duration = ap.duration; onceStartDelay = ap.onceStartDelay;
        isShowDuration = ap.isShowDuration; isPotraitShow = ap.isPotraitShow; isShootPlayer = ap.isShootPlayer; isFollowPlayer = ap.isFollowPlayer;

		bulletType = ap.bulletType;
		template = ap.template;

		// Shoot at player values.
		initialSpacing = ap.initialSpacing;
        viewAngle = ap.viewAngle; segments = ap.segments;

		// Shoot around in circle values.
		isClockwise = ap.isClockwise;
		distance = ap.distance; startTurnDelay = ap.startTurnDelay; turningRate = ap.turningRate; increaseTR = ap.increaseTR; increaseTRTime = ap.increaseTRTime; maxTR = ap.maxTR;
		xOffset = ap.xOffset; yOffset = ap.yOffset;

		speedChangeList = ap.speedChangeList;

		// Sine wave values.
		offsetPosition = ap.offsetPosition;
		frequency = ap.frequency; magnitude = ap.magnitude; magExpandMult = ap.magExpandMult; sineWaveBullets = ap.sineWaveBullets; cooldown = ap.cooldown;

		// Shock repel values.
		giveStatRepelDur = ap.giveStatRepelDur; slowValue = ap.slowValue; repelValue = ap.repelValue;
		trapDelayExpand = ap.trapDelayExpand; trapExpandDur = ap.trapExpandDur; trapExpandSpd = ap.trapExpandSpd;
		trapRotateSpd = ap.trapRotateSpd; trapMoveSpd = ap.trapMoveSpd; trapDelayShrink = ap.trapDelayShrink; trapShrinkSpd = ap.trapShrinkSpd; trapFadeSpd = ap.trapFadeSpd;
	}

    public AlternateFire GetCurrAlternateFire { get { return alternateFire; } }

    public void StartAttack(Action doLast)
    {
        if (!mIsCoroutine)
        {
            if (isPotraitShow)
            {
                if (charSprite != null) owner.GetComponent<EnemyBase>().EnableMagicCircle();
                if (spellCardSprite != null) PotraitShowManager.sSingleton.RunShowPotrait(charSprite, spellCardSprite);
            }
            if (isShowDuration) UIManager.sSingleton.ActivateBossTimer(duration);

            Transform targetTrans = null;
            if (isShootPlayer)
            {
				if (target == Target.PLAYER_1 && GameManager.sSingleton.IsThisPlayerActive(1)) targetTrans = GameManager.sSingleton.player1;
				else if (target == Target.PLAYER_2 && GameManager.sSingleton.IsThisPlayerActive(2)) targetTrans = GameManager.sSingleton.player2;
				else if (GameManager.sSingleton.IsThisPlayerActive(1) && !GameManager.sSingleton.IsThisPlayerActive(2)) targetTrans = GameManager.sSingleton.player1;
				else if (GameManager.sSingleton.IsThisPlayerActive(2) && !GameManager.sSingleton.IsThisPlayerActive(1)) targetTrans = GameManager.sSingleton.player2;
                else targetTrans = GameManager.sSingleton.GetRandomPlayer();
            }

            if (template == AttackPattern.Template.SINGLE_SHOT || template == AttackPattern.Template.ANGLE_SHOT ||
                template == AttackPattern.Template.SHOOT_AROUND_IN_CIRCLE)
                mAttackCoList.Add(Shoot(targetTrans, doLast));
            else if (template == AttackPattern.Template.DOUBLE_SINE_WAVE)
                mAttackCoList = DoubleSineWaveShot(targetTrans, doLast);
            else if (template == Template.SHOCK_REPEL_AND_TRAP_LASER) 
                mAttackCoList.Add(ShootOnceOnly(doLast));

            for (int i = 0; i < mAttackCoList.Count; i++)
            {
                StartCoroutine(mAttackCoList[i]);
            }
        }
    }

    public void PrimaryWeaponShoot()
    {
        if (template == Template.SINGLE_SHOT) ShootSingleShot(null);
        else if (template == AttackPattern.Template.ANGLE_SHOT) ShootAngleShot(null);
        else if (template == AttackPattern.Template.SHOOT_AROUND_IN_CIRCLE) ShootAroundInCirlce();
        else if (template == AttackPattern.Template.DOUBLE_SINE_WAVE)
        {
            SineWaveShoot(true);
            SineWaveShoot(false);
        }
    }

    public void SecondaryWeaponShoot()
    {
        int numOfMissle = Mathf.FloorToInt(mPlayerController.powerLevel);
        List<Vector3> fairyPosList = secondaryAttackType.GetAllFairyPos();

        for (int i = 0; i < numOfMissle; i++)
        {
            if (bulletType == BulletType.PROJECTILE || bulletType == BulletType.PROJECTILE_LOCK_ON)
            {
                Transform currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.PLAYER_SECONDARY, secondaryBulletIndex);
                Vector3 pos = fairyPosList[i];
                pos.x += secondaryBulletOffset.x;
                pos.y += secondaryBulletOffset.y;
                currBullet.position = pos;
                currBullet.gameObject.SetActive(true);

                BulletMove bulletMove = currBullet.GetComponent<BulletMove>();
                bulletMove.SetBaseProperties(properties);
                bulletMove.SetIsPiercing(isSecondaryPiercing);
                bulletMove.SetProperties(Template.SINGLE_SHOT, secondaryBulletDamage, secondaryBulletSpeed);
                bulletMove.BulletType = BulletManager.GroupIndex.PLAYER_SECONDARY;

                if (bulletType == BulletType.PROJECTILE)
                    bulletMove.SetDirection(bulletDirection);
                else if (bulletType == BulletType.PROJECTILE_LOCK_ON)
                {
                    Transform enemy = null;

                    if (!mPlayerController.IsShiftPressed) enemy = withinRange.GetClosestEnemy();
                    else if (mPlayerController.IsShiftPressed) enemy = withinRange.GetFurthestEnemy();

                    if (enemy == null) bulletMove.SetDirection(bulletDirection);
                    else
                    {
                        Vector2 dir = enemy.position - fairyPosList[i];
                        bulletMove.SetDirection(dir.normalized);
                    }
                }
            }
            else if (bulletType == BulletType.LASER)
                secondaryAttackType.ActivateSmallLaser(mPlayerController.playerID);
        }
    }

    public void StopCoroutine()
    {
        for (int j = 0; j < mAttackCoList.Count; j++)
        {
            StopCoroutine(mAttackCoList[j]);
        }
        mIsCoroutine = false;
        mAttackCoList.Clear();
    }

    IEnumerator ShootOnceOnly(Action doLast)
    {
        bool isShot = false;
        mIsCoroutine = true;

        int totalPlayers = GameManager.sSingleton.TotalNumOfPlayer();

        Transform targetTrans = null, nextTrans = null;
        if (totalPlayers > 1)
        {
            targetTrans = GameManager.sSingleton.player1;// GameManager.sSingleton.GetRandomPlayer();

            if (targetTrans.tag == TagManager.sSingleton.player1Tag) nextTrans = GameManager.sSingleton.player2;
            else nextTrans = GameManager.sSingleton.player1;
        }

        while (mTimer < duration)
        {
            while (onceStartDelay > 0)
            {
                if (!BombManager.sSingleton.isTimeStopBomb)
                {
                    mTimer += Time.deltaTime;
                    onceStartDelay -= Time.deltaTime;
                    if (onceStartDelay < 0) onceStartDelay = 0;
                }
                yield return null;
            }

            if (!BulletManager.sSingleton.IsDisableSpawnBullet)
            {
                while (pauseEverySec != 0 && mPauseTimer >= pauseEverySec)
                {
                    mPauseTimer = 0;
                    yield return new WaitForSeconds(pauseDur);
                }

                if (template == Template.SHOCK_REPEL_AND_TRAP_LASER && !isShot)
                {
                    ShootSingleShot(targetTrans);
                    isShot = true;
                }
            }
            else
            {
                // Set remaining player as target.
                isShot = false;
                targetTrans = nextTrans;

                if (nextTrans.tag == TagManager.sSingleton.player1Tag) nextTrans = GameManager.sSingleton.player2;
                else nextTrans = GameManager.sSingleton.player1;
            }

            mTimer += shootDelay + Time.deltaTime;
            mPauseTimer += shootDelay + Time.deltaTime;

            yield return new WaitForSeconds(shootDelay);
        }
        mIsCoroutine = false;
    }

    IEnumerator Shoot(Transform targetTrans, Action doLast)
    {
        mIsCoroutine = true;
        while (mTimer < duration)
        {
            while (onceStartDelay > 0)
            {
                if (!BombManager.sSingleton.isTimeStopBomb)
                {
                    mTimer += Time.deltaTime;
                    onceStartDelay -= Time.deltaTime;
                }
                yield return null;
            }

            if (!BulletManager.sSingleton.IsDisableSpawnBullet)
            {
                while (pauseEverySec != 0 && mPauseTimer >= pauseEverySec)
                {
                    mPauseTimer = 0;
                    mTimer += pauseDur;
                    yield return new WaitForSeconds(pauseDur);
                }

                if(template == Template.SINGLE_SHOT) ShootSingleShot(targetTrans);
                else if(template == Template.ANGLE_SHOT) ShootAngleShot(targetTrans);
                else if(template == Template.SHOOT_AROUND_IN_CIRCLE) ShootAroundInCirlce();
            }

            mTimer += shootDelay + Time.deltaTime;
            mPauseTimer += shootDelay + Time.deltaTime;

            yield return new WaitForSeconds(shootDelay);
        }
        doLast();
        mIsCoroutine = false;
    }

    List<IEnumerator> DoubleSineWaveShot(Transform targetTrans, Action doLast)
    {
        List<IEnumerator> ienumeratorList = new List<IEnumerator>();
        ienumeratorList.Add(SineWaveShootRoutine(targetTrans, true, () => {} )); 
        ienumeratorList.Add(SineWaveShootRoutine(targetTrans, false, doLast)); 
        return ienumeratorList;
    }

    IEnumerator SineWaveShootRoutine(Transform targetTrans, bool isStartLeft, Action doLast)
    {
        mIsCoroutine = true;
        float timer = 0;

        while (mTimer < duration)
        {
            while (onceStartDelay > 0)
            {
              if (!BombManager.sSingleton.isTimeStopBomb)
                {
                    mTimer += Time.deltaTime;
                    onceStartDelay -= Time.deltaTime;
                }
                yield return null;
            }

            while (pauseEverySec != 0 && mPauseTimer >= pauseEverySec)
            {
                mPauseTimer = 0;
                yield return new WaitForSeconds(pauseDur);
            }

            for (int i = 0; i < sineWaveBullets; i++)
            {
                if (!BulletManager.sSingleton.IsDisableSpawnBullet)
                {
                    SineWaveShoot(targetTrans.position, isStartLeft);
                }

                timer += shootDelay;
                yield return new WaitForSeconds(shootDelay);
            }
            yield return new WaitForSeconds(cooldown);

            mSavedDir = Vector2.zero;
            timer += cooldown + Time.deltaTime;
            mPauseTimer += timer;
            mTimer = timer;
        }
        doLast();
        mIsCoroutine = false;
    }

    void UpdateProperties()
    {
        properties.ownerType = ownerType;
        properties.template = template;
        properties.isAlternateFire = isAlternateFire;
        properties.isMagnum = isMagnum;
        properties.damage = mainBulletDamage;
        properties.speed = mainBulletSpeed;
        properties.frequency = frequency;
        properties.magnitude = magnitude;
        properties.magExpandMult = magExpandMult;

        properties.giveStatRepelDur = giveStatRepelDur;
        properties.slowValue = slowValue;
        properties.repelValue = repelValue;
        properties.trapDelayExpand = trapDelayExpand;
        properties.trapExpandDur = trapExpandDur;
        properties.trapExpandSpd = trapExpandSpd;
        properties.trapRotateSpd = trapRotateSpd;
        properties.trapMoveSpd = trapMoveSpd;
        properties.trapDelayShrink = trapDelayShrink;
		properties.trapShrinkSpd = trapShrinkSpd;
		properties.trapFadeSpd = trapFadeSpd;
    }

    void ShootSingleShot(Transform targetTrans)
    {
        Transform bulletTrans = null;
        Vector2 dir = Vector2.zero;
        Vector2 startPos = (Vector2) owner.position;

        if (ownerType == OwnerType.PLAYER)
        {
            bulletTrans = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.PLAYER_MAIN, mainBulletIndex);

			startPos.x += mainBulletOffset.x;
			startPos.y += (mPlayerController.PlayerSize.y / 2) + mainBulletOffset.y;
			dir = bulletDirection;

			if (isAlternateFire) 
			{
				if (alternateFire == AlternateFire.LEFT)
				{
					startPos.x -= alternateFireOffset;
					alternateFire = AlternateFire.RIGHT;
				}
				else if (alternateFire == AlternateFire.RIGHT)
				{
					startPos.x += alternateFireOffset;
					alternateFire = AlternateFire.LEFT;
				}
                AudioManager.sSingleton.PlayHandgunSfx();
			}
        }
        else
        {
            bulletTrans = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.ENEMY, mainBulletIndex);

            if (isShootPlayer)
            {
                if (isFollowPlayer) dir = (Vector2)(targetTrans.position - owner.position).normalized;
                else if (!isFollowPlayer)
                {
                    if (mSavedDir == Vector2.zero) mSavedDir = (Vector2)(targetTrans.position - owner.transform.position).normalized;
                    dir = mSavedDir;
                }
            }
            else
            {
                if (isDirectionFromOwner)
                {
                    Vector2 nextPos = (Vector2)owner.position + shootDirection;
                    dir = (nextPos - (Vector2)owner.position).normalized;
                }
                else dir = (Vector2)((Vector3)shootDirection - owner.position).normalized;
            }

            startPos += dir * initialSpacing;
        }

        Transform currBullet = bulletTrans;
        currBullet.position = (Vector3)startPos;
        currBullet.gameObject.SetActive(true);

        BulletMove bulletMove = currBullet.GetComponent<BulletMove>();
        SetProperties(ref bulletMove, dir);

        if (template == Template.SHOCK_REPEL_AND_TRAP_LASER) bulletMove.SetShockRepelTarget(targetTrans);
    }

    void ShootAngleShot(Transform targetTrans)
    {
        Vector2 targetDir = Vector2.zero;
        if (ownerType == OwnerType.PLAYER) 
        {
            Vector3 temp = transform.position;
            temp.y = bulletDirection.y;
            targetDir = (Vector2)(temp - owner.transform.position).normalized;

            if (targetDir.y < 0) targetDir.y = Mathf.Abs(targetDir.y);
        }
        else 
        {
            if (isShootPlayer)
            {
                if (isFollowPlayer) targetDir = (Vector2)(targetTrans.position - owner.transform.position).normalized;
                else if (!isFollowPlayer)
                {
                    if (mSavedDir == Vector2.zero) mSavedDir = (Vector2)(targetTrans.position - owner.transform.position).normalized;
                    targetDir = mSavedDir;
                }
            }
            else
            {
                if (isDirectionFromOwner)
                {
                    Vector2 nextPos = (Vector2)owner.position + shootDirection;
                    targetDir = (nextPos - (Vector2)owner.position).normalized;
                }
                else targetDir = (Vector2)((Vector3)shootDirection - owner.position).normalized;
            }
        }

        mAngle = Vector2.Angle(targetDir, transform.up) * Mathf.Deg2Rad;
        if (targetDir.x < 0)
            mAngle = -mAngle;

        float halfViewAngle = ((viewAngle * Mathf.Deg2Rad) / 2);
        float startAngle = mAngle - halfViewAngle;
        float endAngle = mAngle + halfViewAngle;

        //------------------------------Testing purposes---------------------------------
        //        float x = Mathf.Sin(startAngle);
        //        float y = Mathf.Cos(startAngle);
        //        Vector2 target = new Vector3(transform.position.x + x, transform.position.y + y);
        //        Debug.DrawLine (transform.position, target, Color.green);
        //
        //        x = Mathf.Sin(endAngle);
        //        y = Mathf.Cos(endAngle);
        //        target = new Vector3(transform.position.x + x, transform.position.y + y);
        //        Debug.DrawLine (transform.position, target, Color.green);
        //--------------------------------------------------------------------------------

        float inc = (viewAngle * Mathf.Deg2Rad) / segments;
        float total = startAngle;
        for (float i = 0; i < segments + 1; i++)
        {
            float angle = 0;
            if (i == 0) angle = startAngle;
            else if (i == segments) angle = endAngle;
            else 
            {
                total += inc;
                angle = total;
            }

            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            Vector2 target = new Vector3(owner.position.x + x, owner.position.y + y);
//            Debug.DrawLine(transform.position, target, Color.red);

            Transform currBullet = null;
            if (ownerType == OwnerType.PLAYER) currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.PLAYER_MAIN, mainBulletIndex);
            else currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.ENEMY, mainBulletIndex);

            Vector3 dir = (target - (Vector2)transform.position).normalized;
            currBullet.position = (Vector3)target + (Vector3)dir * initialSpacing;
            currBullet.gameObject.SetActive(true);

            BulletMove bulletMove = currBullet.GetComponent<BulletMove>();
            SetProperties(ref bulletMove, dir);
        }
    }

    void ShootAroundInCirlce()
    {
        if (startTurnDelay <= 0)
        {
            mIncreaseTRTimer += Time.deltaTime;
            if (turningRate < maxTR && mIncreaseTRTimer >= increaseTRTime)
            {
                turningRate += increaseTR;
                if (turningRate > maxTR) turningRate = maxTR;
                mIncreaseTRTimer = 0;
            }
        }
        else startTurnDelay -= shootDelay - Time.deltaTime;

        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin (mAngle + (xOffset * Mathf.Deg2Rad)) * distance;
            float y = Mathf.Cos (mAngle + (yOffset * Mathf.Deg2Rad)) * distance;
            mAngle += (Mathf.PI * 2 / segments);

            Vector2 dir = Vector2.zero;
            if (isClockwise) { dir.x += x;  dir.y += y; }
            else { dir.x += y; dir.y += x; }

            Transform currBullet = null;
            if (ownerType == OwnerType.PLAYER) currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.PLAYER_MAIN, mainBulletIndex);
            else currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.ENEMY, mainBulletIndex);

            currBullet.position = owner.position;
            currBullet.gameObject.SetActive(true);

            BulletMove bulletMove = currBullet.GetComponent<BulletMove>();
            SetProperties(ref bulletMove, dir);
        }

        mAngle += (turningRate * Mathf.Deg2Rad);
    }

    void SineWaveShoot(bool isStartLeft) { SineWaveShoot(Vector3.zero, isStartLeft); }
    void SineWaveShoot(Vector3 targetPos, bool isStartLeft)
    {
        Transform currBullet = null;
        if (ownerType == OwnerType.PLAYER) currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.PLAYER_MAIN, mainBulletIndex);
        else currBullet = BulletManager.sSingleton.GetBulletTrans(BulletManager.GroupIndex.ENEMY, mainBulletIndex);

        Vector3 temp = owner.position;
        temp.x += offsetPosition.x;
        temp.y += offsetPosition.y;
        currBullet.position = temp;
        currBullet.gameObject.SetActive(true);

        Vector2 dir = Vector2.zero;
        if (ownerType == OwnerType.PLAYER) dir = bulletDirection;
        else 
        {
            if (isShootPlayer)
            {
                if (isFollowPlayer) dir = (Vector2)(targetPos - owner.transform.position).normalized;
                else if (!isFollowPlayer)
                {
                    if (mSavedDir == Vector2.zero) mSavedDir = (Vector2)(targetPos - owner.transform.position).normalized;
                    dir = mSavedDir;
                }
            }
            else
            {
                if (isDirectionFromOwner)
                {
                    Vector2 nextPos = (Vector2)owner.position + shootDirection;
                    dir = (nextPos - (Vector2)owner.position).normalized;
                }
                else dir = (Vector2)((Vector3)shootDirection - owner.position).normalized;
            }
        }

        BulletMove bulletMove = currBullet.GetComponent<BulletMove>();
        SetProperties(ref bulletMove, dir);
        bulletMove.SetCurveAxis(GetCurveAxis(dir, isStartLeft));
    }

    void SetProperties(ref BulletMove bulletMove, Vector2 dir)
    {
        bulletMove.SetIsPiercing(isMainPiercing);
        bulletMove.SetBaseProperties(properties);
        bulletMove.SetDirection(dir);
        bulletMove.SetNewBulletSpeed(speedChangeList);

        bulletMove.BulletType = BulletManager.GroupIndex.PLAYER_MAIN;
    }

    Vector2 GetCurveAxis(Vector2 dir, bool isStartLeft)
    {
        Vector2 curveAxis = Vector2.zero;
        float xVal = 0, yVal = 0;

        if (dir.y < 0) xVal = -Mathf.Abs(dir.y);
        else xVal = Mathf.Abs(dir.y);

        if (dir.x < 0) yVal = Mathf.Abs(dir.x);
        else yVal = -Mathf.Abs(dir.x);

        if (isStartLeft) curveAxis = new Vector2(xVal, yVal);
        else curveAxis = new Vector2(-xVal, -yVal);

        return curveAxis;
    }

    void OnDisable()
    {
        mSavedDir = Vector2.zero;
    }
}
