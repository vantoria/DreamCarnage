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

    public int plyMaxLife = 8;
    public int plyStartLife = 2;
    public int plyMaxBomb = 5;
    public int plyStartBomb = 3;
    public float plySoulTime = 11;
    public float plyRevPressNum = 10;

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

	public float pointPU_SpeedToPly = 10;
    public float autoCollectPU_SpeedToPly = 20;

    public int plyPrimaryBulletsTotal = 50;
    public int plySecondaryBulletsTotal = 50;
    public int enemyMinionTotal = 100;
    public int enemyBulletsTotal = 1000;
    public int scorePickUpTotal = 1000;
    public int pickUpsTotal = 50;
    public int hazardsTotal = 10;

    public enum State
    {
        NONE = 0,
        DIALOGUE,
        BATTLE,
        RESULT
    }
    public State currState = State.BATTLE;
    public PlayerNEnemyPrefabData playerAndEnemyPrefabData;

    PlayerController mP1Controller, mP2Controller;

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

            List<int> mSelectedCharList = MainMenuManager.sSingleton.GetSelectedIndexList;
            for (int i = 0; i < mSelectedCharList.Count; i++)
            {
                Transform trans = playerAndEnemyPrefabData.playerTransList[mSelectedCharList[i]];
                if (i == 0)
                {
                    Transform char1 = Instantiate(trans, p1DefaultPos.position, Quaternion.identity);
                    player1 = char1;
                }
                else
                {
                    Transform char2 = Instantiate(trans, p2DefaultPos.position, Quaternion.identity);
                    player2 = char2;
                }
            }
        }

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

        // Enemy minion movement add.
        EnemyManager.sSingleton.AddMovementToMinion(playerAndEnemyPrefabData);

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

        mP1Controller = player1.GetComponent<PlayerController>();
        mP2Controller = player2.GetComponent<PlayerController>();
	}

    void Update()
    {
        if (currState == State.DIALOGUE)
        {
            DialogueManager.sSingleton.HandleDialogue();
        }
        else if (currState == State.BATTLE)
        {
            if (mP1Controller.state == PlayerController.State.SOUL && mP2Controller.state == PlayerController.State.SOUL)
            {
                if (mP1Controller.life > 0)
                {
                    mP1Controller.MinusLife();
                    mP1Controller.ReviveSelf();
                }
                if (mP2Controller.life > 0)
                {
                    mP2Controller.MinusLife();
                    mP2Controller.ReviveSelf();
                }
            }
        }
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

	public bool IsTheOtherPlayerAlive(int currPlayerNum)
	{
		if ( (currPlayerNum == 1 && player2 != null && player2.gameObject.activeSelf) ||
            (currPlayerNum == 2 && player1 != null && player1.gameObject.activeSelf) ) return true;
		return false;
	}
}
