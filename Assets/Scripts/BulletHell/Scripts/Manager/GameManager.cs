using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
    public static GameManager sSingleton { get { return _sSingleton; } }
    static GameManager _sSingleton;

    public Transform player1;
    public Transform player2;

    public int currStage = 1;
	public float autoCollectPickUp_Y = 0.75f;

    public float powerDropSpeed = 5;
    public float powerDropSlowDown = 5;
    public float powerDropRotate = -100;
    public float powerDropAngle = 90;
    public int totalPowerDrop = 1;

    public Transform p1DefaultPos;
    public Transform p2DefaultPos;
	public Transform p1StartPos;
	public Transform p2StartPos;
    public Transform bossDefaultPos;

    public float p1RespawnXPos = 0.3f;
    public float p2RespawnXPos = 0.7f;

    public int plyMaxLife = 8;
    public int plyStartLife = 2;
    public int plyMaxBomb = 5;
    public int plyStartBomb = 3;
    public float plySoulTime = 11;
    public int plyRevPressNum = 10;
    public float scoreMultAfterRes = 0.7f;

    public ParticleSystem brokenHeartPS;
    public ParticleSystem revivedPS;
	public ParticleSystem deathPS;
    public ParticleSystem linkFlamePS;

	public Transform powerUpTextTrans;

    public float plyDisabledCtrlTime = 1;
    public float plyInvinsibilityTime = 2;
    public float plyRespawnAlpha = 0.8f;
    public float plyRespawnYSpd = 1;
    public float plyAlphaBlinkDelay = 0.2f;

    public float bulletDisappearSpeed = 1;
    public float enemyDisBulletTime = 1;
    public Color enemyDmgColor;
    public float enemyDmgColorDur = 0.1f;
    public float gameOverWaitDur = 1.5f;
	public Color rockDmgColor;

	public float pointPU_SpeedToPly = 10;
    public float autoCollectPU_SpeedToPly = 20;

    public int plyPrimaryBulletsTotal = 50;
    public int plySecondaryBulletsTotal = 50;
    public int enemyMinionTotal = 100;
    public int enemyBulletsTotal = 1000;
    public int scorePickUpTotal = 1000;
    public int pickUpsTotal = 50;
	public int hazardsTotal = 10;
    public int killScoreTotal = 100;
	public int powerUpTextTotal = 8;
	public int deathPSTotal = 100;
    public int impactPSTotal = 200;
    public int rockDestroyPSTotal = 5;

    public float initalWaitAfterIntoScreen = 1.0f;

    public enum State
    {
        NONE = 0,
        PLAYER_MOVE_INTO_SCREEN,
        BOSS_MOVE_INTO_SCREEN,
        DIALOGUE,
        BATTLE,
        RESULT,
        TUTORIAL
    }
    public State currState = State.BATTLE;
    public PlayerNEnemyPrefabData playerAndEnemyPrefabData;
    public CharacterInGameImageData charInGameImageData;

    [HideInInspector]public bool isMoveDuringDialogue = false;

    Transform mCurrBoss;
    bool mIsScoreYellow = false;
	int mMagnumPlayerID = 0, mRankOneHighScore;
    float mTimer, mMagnumMarkedDuration;
    PlayerController mP1Controller, mP2Controller;
    ReviveController mP1ReviveController, mP2ReviveController;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

	void Start () 
    {
        if (MainMenuManager.sSingleton != null)
        {
            MainMenuManager.sSingleton.gameObject.SetActive(false);
            mRankOneHighScore = MainMenuManager.sSingleton.GetFirstRankTotalScore();

            List<int> selectedCharList = MainMenuManager.sSingleton.GetSelectedIndexList;
            List<ReviveController> reviveControllerList = new List<ReviveController>();
            int count = selectedCharList.Count;

            for (int i = 0; i < count; i++)
            {
                Transform trans = playerAndEnemyPrefabData.playerTransList[selectedCharList[i]];

                Sprite eyeshot = null;
                Sprite potrait = null;

                if (i == 0)
                {
					Transform p1 = Instantiate(trans, p1StartPos.position, Quaternion.identity);
                    p1.tag = TagManager.sSingleton.player1Tag;

                    PlayerController pc = p1.GetComponent<PlayerController>();
                    pc.playerID = 1;
                    pc.respawnXPos = p1RespawnXPos;
                    player1 = p1;

                    // Get the revive controller.
                    if (count == 2) reviveControllerList.Add(p1.GetComponentInChildren<ReviveController>());

                    // Set character's image for left side player.
                    if (selectedCharList[i] == 0)
                    {
                        eyeshot = charInGameImageData.char1EyeshotLeft;
                        potrait = charInGameImageData.char1Potrait;
                    }
                    else if (selectedCharList[i] == 1)
                    {
                        eyeshot = charInGameImageData.char2EyeshotLeft;
                        potrait = charInGameImageData.char2Potrait;
                    }
                    else if (selectedCharList[i] == 2)
                    {
						mMagnumPlayerID = 1;

                        eyeshot = charInGameImageData.char3EyeshotLeft;
                        potrait = charInGameImageData.char3Potrait;
                    }

                    BombManager.sSingleton.SetPlayerBombController(1, p1.GetComponent<BombController>());
                    BombManager.sSingleton.leftEyeshotImage.sprite = eyeshot;
                    BombManager.sSingleton.leftPotraitImage.sprite = potrait;
                }
                else 
                {
                    Transform p2 = Instantiate(trans, p2StartPos.position, Quaternion.identity);
                    p2.tag = TagManager.sSingleton.player2Tag;

                    PlayerController pc = p2.GetComponent<PlayerController>();
                    pc.playerID = 2;
                    pc.respawnXPos = p2RespawnXPos;
                    player2 = p2;

                    // Get the revive controller.
                    if (count == 2) reviveControllerList.Add(p2.GetComponentInChildren<ReviveController>());

                    // Set character's image for right side player.
                    if (selectedCharList[i] == 0)
                    {
                        eyeshot = charInGameImageData.char1EyeshotRight;
                        potrait = charInGameImageData.char1Potrait;
                    }
                    else if (selectedCharList[i] == 1)
                    {
                        eyeshot = charInGameImageData.char2EyeshotRight;
                        potrait = charInGameImageData.char2Potrait;
                    }
                    else if (selectedCharList[i] == 2)
                    {
						mMagnumPlayerID = 2;

                        eyeshot = charInGameImageData.char3EyeshotRight;
                        potrait = charInGameImageData.char3Potrait;
                    }

                    BombManager.sSingleton.SetPlayerBombController(2, p2.GetComponent<BombController>());
                    BombManager.sSingleton.rightEyeshotImage.sprite = eyeshot;
                    BombManager.sSingleton.rightPotraitImage.sprite = potrait;
                }
            }

            if (reviveControllerList.Count != 0)
            {
                for (int i = 0; i < reviveControllerList.Count; i++)
                {
                    reviveControllerList[i].UpdateWithinCirclePs();
                }
            }

            if (count == 1) mP2Controller = null;
        }
        else 
        {
            mP1Controller = player1.GetComponent<PlayerController>();
            mP2Controller = player2.GetComponent<PlayerController>();

            Sprite eyeshot = null;
            Sprite potrait = null;

            if (mP1Controller.characterID == 1)
            {
                eyeshot = charInGameImageData.char1EyeshotLeft;
                potrait = charInGameImageData.char1Potrait;
            }
            else if (mP1Controller.characterID == 2)
            {
                eyeshot = charInGameImageData.char2EyeshotLeft;
                potrait = charInGameImageData.char2Potrait;
            }
            else if (mP1Controller.characterID == 3)
            {
                eyeshot = charInGameImageData.char3EyeshotLeft;
                potrait = charInGameImageData.char3Potrait;
            }

            BombManager.sSingleton.leftEyeshotImage.sprite = eyeshot;
            BombManager.sSingleton.leftPotraitImage.sprite = potrait;

            if (mP2Controller.characterID == 1)
            {
                eyeshot = charInGameImageData.char1EyeshotRight;
                potrait = charInGameImageData.char1Potrait;
            }
            else if (mP2Controller.characterID == 2)
            {
                eyeshot = charInGameImageData.char2EyeshotRight;
                potrait = charInGameImageData.char2Potrait;
            }
            else if (mP2Controller.characterID == 3)
            {
                eyeshot = charInGameImageData.char3EyeshotRight;
                potrait = charInGameImageData.char3Potrait;
            }

            BombManager.sSingleton.rightEyeshotImage.sprite = eyeshot;
            BombManager.sSingleton.rightPotraitImage.sprite = potrait;
        }

        InstantiateEnemy();
        InstantiateBullets();
        InstantiateEnvObj();

        mP1Controller = player1.GetComponent<PlayerController>();
        mP2Controller = player2.GetComponent<PlayerController>();
        mP1ReviveController = player1.GetComponentInChildren<ReviveController>();
        mP2ReviveController = player2.GetComponentInChildren<ReviveController>();
	}

    void Update()
    {
        if (currState == State.PLAYER_MOVE_INTO_SCREEN)
        {
            if (AudioManager.sSingleton != null)
            {
                AudioManager.sSingleton.PlayInGameDialogueBGM();
                AudioManager.sSingleton.FadeInBGM();
            }

			isMoveDuringDialogue = false;

			Vector3 currPos = player1.transform.position;
            Vector3 toPos = p1DefaultPos.position;
            float step = plyRespawnYSpd * Time.deltaTime;

            player1.transform.position = Vector3.MoveTowards(currPos, toPos, step);
            if (TotalNumOfPlayer() == 2)
            {
                currPos = player2.transform.position;
                toPos = p2DefaultPos.position;
                player2.transform.position = Vector3.MoveTowards(currPos, toPos, step);
            }

            if (player1.transform.position == p1DefaultPos.position || player2.transform.position == p2DefaultPos.position)
            {
                if (!CoroutineUtil.isCoroutine) StartCoroutine(CoroutineUtil.WaitFor(initalWaitAfterIntoScreen, () => {currState = State.TUTORIAL;}));
            }
        }
        else if (currState == State.BOSS_MOVE_INTO_SCREEN)
        {
            Transform boss = EnemyManager.sSingleton.GetEnemyBossTrans();
            boss.gameObject.SetActive(true);
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, bossDefaultPos.position, plyRespawnYSpd * Time.deltaTime * 2.5f);
            mCurrBoss = boss;

			isMoveDuringDialogue = true;
            if (boss.transform.position == bossDefaultPos.position)
            {
                if (!CoroutineUtil.isCoroutine) StartCoroutine(CoroutineUtil.WaitFor(initalWaitAfterIntoScreen, () => {currState = State.DIALOGUE;}));
            }
        }
        else if (currState == State.DIALOGUE)
        {
            if (DialogueManager.sSingleton != null) DialogueManager.sSingleton.HandleDialogue();
            else if (DialogueManager.sSingleton == null && EnemyManager.sSingleton.isBossAppeared) SetToBattleState();
            else currState = State.BATTLE;
        }
        else if (currState == State.BATTLE)
        {
			UIManager.sSingleton.ShowAutoCollectZone ();
			UIManager.sSingleton.ShowStageName ();

            HandleScoreColor();

            if (mP1Controller.state == PlayerController.State.SOUL && mP2Controller != null && mP2Controller.state == PlayerController.State.SOUL)
            {
                if (mP1Controller.life > 0)
                {
                    mP1Controller.MinusLife();
                    mP1Controller.ReviveSelf(true);
                    mP1ReviveController.ResetSoul();
                }
                if (mP2Controller.life > 0)
                {
                    mP2Controller.MinusLife();
                    mP2Controller.ReviveSelf(true);
                    mP2ReviveController.ResetSoul();
                }
            }

            if (IsDeactivateLinkBar())
            {
                mP1Controller.DisableFlamePS();
                if (mP2Controller != null) mP2Controller.DisableFlamePS();
                UIManager.sSingleton.DeactivateBothLinkBar();
            }
        }
        else if (currState == State.RESULT)
        {
            // TODO: RESET BACK EVERYTHING.
			isMoveDuringDialogue = false;
        }
        else if (currState == State.TUTORIAL)
        {
            TutorialManager.sSingleton.SetEnableTutorial();
        }
    }

    public void SetToTutorialState()
    {
        currState = State.TUTORIAL;
    }

    public void SetToBattleState()
    {
        currState = State.BATTLE;
		isMoveDuringDialogue = true;

        if (AudioManager.sSingleton != null)
        {
            AudioManager.sSingleton.StopBGM();
            AudioManager.sSingleton.PlayInGameStage1BGM();
            AudioManager.sSingleton.FadeInBGM();
        }

        if (EnemyManager.sSingleton.isBossAppeared)
        {
            if (AudioManager.sSingleton != null)
            {
                AudioManager.sSingleton.StopBGM();
                AudioManager.sSingleton.PlayInGameStage1BossBGM();
            }
            mCurrBoss.GetComponent<EnemyBase>().StartHpBarSequence();
        }
    }

    public bool IsMoveDuringDialogue()
    {
        if (currState == State.DIALOGUE) return isMoveDuringDialogue;

        // If it's not dialogue now, allow the player to move.
        return true;
    }

    public bool IsPlayerInteractable()
    {
        if (currState == State.PLAYER_MOVE_INTO_SCREEN || currState == State.RESULT || currState == State.TUTORIAL) return false;
        return true;
    }

	public bool IsBossMakeEntrance()
	{
		if ((EnemyManager.sSingleton.isBossAppeared && currState == State.BOSS_MOVE_INTO_SCREEN) || currState == State.DIALOGUE) return true;
		return false;
	}

    public float GetDeltaTime()
    {
        float deltaTime = 0;
        if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
        else deltaTime = Time.deltaTime;

        return deltaTime;
    }

    public Transform GetRandomPlayer()
    {
        int rand = Random.Range(0, 2);
        if (rand == 0) return player1;
        else return player2;
    }

	public void DisablePlayer(int playerNum)
	{
		if (playerNum == 1 && player1 != null && player1.gameObject.activeSelf)
			player1.gameObject.SetActive (false);
		else if (playerNum == 2 && player2 != null && player2.gameObject.activeSelf)
			player2.gameObject.SetActive (false);

        if (!player1.gameObject.activeSelf && !player2.gameObject.activeSelf)
            UIManager.sSingleton.EnableGameOverScreen ();
	}

    public int TotalNumOfPlayer()
    {
        int total = 0;
        if (player1 != null && player1.gameObject.activeSelf) total++;
        if (player2 != null && player2.gameObject.activeSelf) total++;
        return total;
    }

    public bool IsThisPlayerActive(int playerNum)
    {
        if ( (playerNum == 1 && player1 != null && player1.gameObject.activeSelf) || 
            (playerNum == 2 && player2 != null && player2.gameObject.activeSelf) ) return true;
        return false;
    }

	public bool IsTheOtherPlayerActive(int currPlayerNum)
	{
		if ( (currPlayerNum == 1 && player2 != null && player2.gameObject.activeSelf) ||
            (currPlayerNum == 2 && player1 != null && player1.gameObject.activeSelf) ) return true;
		return false;
	}

    public float MagnumMarkedDuration
    {
        get { return mMagnumMarkedDuration; }
        set { mMagnumMarkedDuration = value; }
    }

	public int MagnumPlayerID { get { return mMagnumPlayerID; } }

    bool IsDeactivateLinkBar()
    {
        if (!BombManager.sSingleton.IsPause && !BombManager.sSingleton.IsShooting && (mP1Controller.bomb == 0 || mP2Controller.bomb == 0 ||
            mP1Controller.state == PlayerController.State.DEAD || (mP2Controller != null && mP2Controller.state == PlayerController.State.DEAD)))
            return true;
        return false;
    }

    // Change the score color when over Rank 1 score.
    void HandleScoreColor()
    {
        if (!mIsScoreYellow)
        {
            int totalScore = mP1Controller.score;
            if (mP2Controller != null) totalScore += mP2Controller.score;

            if (totalScore > mRankOneHighScore)
            {
                bool isP1Darken = false, isP2Darken = false;
                if (mP1Controller.state == PlayerController.State.DEAD) isP1Darken = true;
                if (mP2Controller != null && mP2Controller.state == PlayerController.State.DEAD) isP2Darken = true;

                UIManager.sSingleton.MakeScoreYellow(1, isP1Darken);
                UIManager.sSingleton.MakeScoreYellow(2, isP2Darken);
                mIsScoreYellow = true;
            }
        }
    }

    void InstantiateEnemy()
    {
        // Enemy instantiate.
        for (int i = 0; i < playerAndEnemyPrefabData.enemyBossTransList.Count; i++)
        {
            Transform currEnemy = playerAndEnemyPrefabData.enemyBossTransList[i];
            EnemyManager.sSingleton.InstantiateAndCacheEnemyBoss(currEnemy);
        }

        for (int i = 0; i < playerAndEnemyPrefabData.enemyMinionTransList.Count; i++)
        {
            Transform currEnemy = playerAndEnemyPrefabData.enemyMinionTransList[i];
            EnemyManager.sSingleton.InstantiateAndCacheEnemy(currEnemy, enemyMinionTotal, i);
        }
    }

    void InstantiateBullets()
    {
        // Enemy minion attack and movement add.
        EnemyManager.sSingleton.AddAttackAndMovementToMinion(playerAndEnemyPrefabData);

        BulletPrefabData bulletData = BulletManager.sSingleton.bulletPrefabData;
        // Player main bullet instantiate.
        for (int i = 0; i < bulletData.plyMainBulletTransList.Count; i++)
        {
            Transform currBullet = bulletData.plyMainBulletTransList[i];
            BulletManager.sSingleton.InstantiateAndCacheBullet(currBullet, plyPrimaryBulletsTotal, 0);
        }

        // Player secondary bullet instantiate.
        for (int i = 0; i < bulletData.plySecondaryBulletTransList.Count; i++)
        {
            Transform currBullet = bulletData.plySecondaryBulletTransList[i];
            BulletManager.sSingleton.InstantiateAndCacheBullet(currBullet, plySecondaryBulletsTotal, 1);
        }

        // Enemy bullet instantiate.
        for (int i = 0; i < bulletData.enemyBulletTransList.Count; i++)
        {
            Transform currBullet = bulletData.enemyBulletTransList[i];
            BulletManager.sSingleton.InstantiateAndCacheBullet(currBullet, enemyBulletsTotal, 2);
        }
    }

    void InstantiateEnvObj()
    {
        EnvObjPrefabData envObjData = EnvObjManager.sSingleton.envObjPrefabData;
        // Pickable environmental object instantiate.
        for (int i = 0; i < envObjData.pickableObjTransList.Count; i++)
        {
            Transform currPickableObj = envObjData.pickableObjTransList[i];

            int total = 0;
            if (currPickableObj.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp1Tag || currPickableObj.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp2Tag)
                total = scorePickUpTotal;
            else
                total = pickUpsTotal;

            EnvObjManager.sSingleton.InstantiateAndCacheEnvObj(currPickableObj, total);
        }

        // Damagable environmental object instantiate.
        for (int i = 0; i < envObjData.hazardTransList.Count; i++)
        {
            Transform currHazard = envObjData.hazardTransList[i];
            EnvObjManager.sSingleton.InstantiateAndCacheEnvObj(currHazard, hazardsTotal);
        }

        EnvObjManager.sSingleton.SortRockSpawnTime(playerAndEnemyPrefabData);

        Transform magnumRadTrans = envObjData.magnumRadius;
        EnvObjManager.sSingleton.InstantiateAndCache(magnumRadTrans, pickUpsTotal);

        // UI.
        Transform killScoreTrans = envObjData.killScoreTrans;
        EnvObjManager.sSingleton.InstantiateAndCacheUI(killScoreTrans, killScoreTotal);

        Transform powerUpTextTrans = envObjData.powerUpText;
        EnvObjManager.sSingleton.InstantiateAndCacheUI(powerUpTextTrans, powerUpTextTotal);

        // Particle effect.
        Transform deathPSTrans = envObjData.enemyDeathPS;
        EnvObjManager.sSingleton.InstantiateAndCachePS(deathPSTrans, deathPSTotal);

        Transform impactPSTrans = envObjData.enemyGetHitPS;
        EnvObjManager.sSingleton.InstantiateAndCachePS(impactPSTrans, impactPSTotal);

        Transform impactBigPSTrans = envObjData.enemyGetHitBigPS;
        EnvObjManager.sSingleton.InstantiateAndCachePS(impactBigPSTrans, impactPSTotal);

        Transform rockDestroyPSTrans = envObjData.rockDestroyPS;
        EnvObjManager.sSingleton.InstantiateAndCachePS(rockDestroyPSTrans, rockDestroyPSTotal);
    }
}
