using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UIManager : MonoBehaviour 
{
    public static UIManager sSingleton { get { return _sSingleton; } }
    static UIManager _sSingleton;

	public Transform player1_UI, player2_UI, gameOver_UI, showScore_UI, showNameInput_UI, gameOverButton_UI, timerUI;

    public float nameInputTimer = 60, nameInputCountDownSpd = 1;
	public int maxPauseButton = 3, maxGameOverButton = 3;

    [HideInInspector] public bool isPlayer1Pause = false, isPlayer2Pause = false;

    // Score show and highscore input.
	public Transform charInputTrans, charEndTrans;
	public Transform p1CharInputTrans, p2CharInputTrans;
    public Text cumulativeScoreText, rankText, backWordText, scoreTimerText;
	public int startUnicode = 65, endUnicode = 90;
	public float totalCharNameInput = 3, xOffsetBetwChar = 1, alphaChangeSpeed = 1, alphaDelay = 0, charChangeDelay = 0.1f;

    // Score count to reach final number. For count-up animation showing.
	public float scoreCountTime;
    public float endScoreCountTime;
    public float multCountUpTime;
	public float multCountDownTime;

    // Auto-collect UI which appears at the start of the game.
	public bool isShowAutoCollect = true;
	public Transform autoCollectTrans;
	public int autoCollectNumTimes = 2;
	public float autoCollectFadeSpeed = 1;
	public float autoCollectDelay = 0.5f;

    // Show the stage name at the start of the game.
	public bool isShowStageName = true;
	public float waitTimeAftAutoCollect = 8;
	public Transform stageNameTrans;
	public float stageShowSpeed = 1;
	public float stageShowDuration = 2;

    public Transform bonusScoreTrans;
    public float bonusScoreFadeSpeed = 1;
    public float bonusScoreStayDuration = 2;

    public SlideInMoveOut BGM_Appear;

    class PlayerInfo
    {
        public Transform subjectName, lifePointTrans, bombTrans, pauseTrans, deathDisplay, redCross;
        public Text powerLevel_UI, highScore_UI, score_UI, percent_UI, multiplier_UI, multX_UI, link_Name;
        public Image linkBarImage, linkBarOutside, reviveSoulInside;
        public SpriteRenderer bombPotraitSR;

        // Used for coroutine count-up animation.
        public bool isScoreCoroutine = false, isMultCoroutine = false;
        public int startCurrScore, currScore, toReachScore;
        public float startCurrMult, currMult = 1, toReachMult;

        public PlayerInfo()
        {
            currMult = 1;
            subjectName = lifePointTrans = bombTrans = pauseTrans = deathDisplay = redCross = null;
            powerLevel_UI = highScore_UI = score_UI = percent_UI = multiplier_UI = multX_UI = link_Name = null;
            linkBarImage = linkBarOutside = null;
        }
    }
    List<PlayerInfo> mPlayerUIList = new List<PlayerInfo>();

    Text mSecondText, mMilliSecondText, mDotText;
    float mDuration = 0, mSavedTimeScale, mDefaultNameInputTimer, mP1CharChangeDelay, mP2CharChangeDelay;

    // Current selection of buttons.
    int mCurrPlayerIndex = 0, mPauseSelectIndex = 0, mGameOverSelectIndex = 0;

	int mP1NameInputNum, mP2NameInputNum, mTotalScore, mRank, mNumOfPlayers;
	string mP1SavedName, mP2SavedName;
	IEnumerator mP1Co, mP2Co;
    bool mIsP1Fading, mIsP2Fading, mP1StopFade, mP2StopFade, mIsWait;

    // Controller input.
    float mVerticalP1, mVerticalP1Dpad, mHorizontalP1, mHorizontalP1Dpad, mHorizontalP2, mHorizontalP2Dpad, mVerticalP2, mVerticalP2Dpad;
    bool mIsP1VerticalAxisUsed = false, mIsP1HorizontalAxisUsed = false, mIsP1KeybInput = false, mIsP2VerticalAxisUsed, mIsP2HorizontalAxisUsed;

	float mScoreTimer, mEndScoreTimer, mShowStageTimer, mMultTimer;
    bool mIsCoroutine = false, mIsStartCount = false, mIsInputName = false, mIsRedCross = false, mIsDeactivatedLink = false;

	List<int> mP1NameIndexList = new List<int> ();
	List<int> mP2NameIndexList = new List<int> ();
	List<Text> mP1NameInputTextList = new List<Text> ();
	List<Text> mP2NameInputTextList = new List<Text> ();

    LevelController mLevelController;

	public enum State
	{
		NONE = 0,
		PAUSE,
		SHOW_SCORE,
        NAME_INPUT,
		GAME_OVER,
	};
	public State state = State.NONE;

    Color mSelectedButtonColor, mUnselectedButtonColor;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

	void Start () 
    {
        mLevelController = GetComponent<LevelController>();
        mIsP1KeybInput = JoystickManager.sSingleton.IsP1KeybInput;

        mNumOfPlayers = GameManager.sSingleton.TotalNumOfPlayer();

        for (int i = 0; i < 2; i++)
        { 
            Transform playerUI = null;
            mPlayerUIList.Add(new PlayerInfo()); 

//            if (GameManager.sSingleton.IsThisPlayerActive(i + 1))
//            {
                if (i == 0) playerUI = player1_UI;
                else if (i == 1) playerUI = player2_UI;
                InitializeUI(playerUI, i);
//            }
        }

        for (int i = 0; i < timerUI.childCount; i++)
        {
            Text textScript = timerUI.GetChild(i).GetComponent<Text>();

            if (i == 0) mSecondText = textScript;
            else if (i == 1) mMilliSecondText = textScript;
            else if (i == 2) mDotText = textScript;
        }
        timerUI.gameObject.SetActive(false);

        InitializeInputName ();
        mDefaultNameInputTimer = nameInputTimer;
        mP1CharChangeDelay = charChangeDelay;
        mP2CharChangeDelay = charChangeDelay;

        if (MainMenuManager.sSingleton != null)
        {
            string hiScore = GetScoreWithZero(MainMenuManager.sSingleton.GetFirstRankTotalScore().ToString());
            mPlayerUIList[0].highScore_UI.text = hiScore;
            if (mPlayerUIList[1].highScore_UI != null)
                mPlayerUIList[1].highScore_UI.text = hiScore;

            mSelectedButtonColor = MainMenuManager.sSingleton.selectedButtonColor;
            mUnselectedButtonColor = MainMenuManager.sSingleton.unselectedButtonColor;
        }
        else
        {
            mSelectedButtonColor = new Color(1, 1, 1, 0.78f);
            mUnselectedButtonColor = new Color(1, 1, 1, 0.09f);
        }

        if (GameManager.sSingleton.TotalNumOfPlayer() == 1) GreyOutPlayerUI(2);
	}

    void Update()
    {
        if (mDuration != 0)
        {
            mDuration -= Time.deltaTime;
            if (mDuration < 0) mDuration = 0;
            UpdateBossTimer(mDuration);
        }

        mVerticalP1 = Input.GetAxis("VerticalP1");
        mVerticalP1Dpad = Input.GetAxis("VerticalP1Dpad");
        mHorizontalP1 = Input.GetAxis("HorizontalP1");
        mHorizontalP1Dpad = Input.GetAxis("HorizontalP1Dpad");

        mVerticalP2 = Input.GetAxis("VerticalP2");
        mVerticalP2Dpad = Input.GetAxis("VerticalP2Dpad");
        mHorizontalP2 = Input.GetAxis("HorizontalP2");
        mHorizontalP2Dpad = Input.GetAxis("HorizontalP2Dpad");

        if (mVerticalP1 == 0 && mVerticalP1Dpad == 0) mIsP1VerticalAxisUsed = false;
        if (mHorizontalP1 == 0 && mHorizontalP1Dpad == 0) mIsP1HorizontalAxisUsed = false;

        if (mVerticalP2 == 0 && mVerticalP2Dpad == 0) mIsP2VerticalAxisUsed = false;
        if (mHorizontalP2 == 0 && mHorizontalP2Dpad == 0) mIsP2HorizontalAxisUsed = false;

		if (state == State.PAUSE)
        {
			Transform pauseTrans = mPlayerUIList[mCurrPlayerIndex].pauseTrans;
            
            if ( ((mCurrPlayerIndex == 0 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.T)) || mVerticalP1 > 0 || mVerticalP1Dpad < 0)) ||
                (mCurrPlayerIndex == 1 && (Input.GetKeyDown(KeyCode.UpArrow) || mVerticalP2 > 0 || mVerticalP2Dpad < 0))) && 
                mPauseSelectIndex > 0 && !mIsP1VerticalAxisUsed)
            {
                mIsP1VerticalAxisUsed = true;
                mPauseSelectIndex--;
                pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = mSelectedButtonColor;
                pauseTrans.GetChild(mPauseSelectIndex + 1).GetComponent<Image>().color = mUnselectedButtonColor;
            }
            else if ( ((mCurrPlayerIndex == 0 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.G)) || mVerticalP1 < 0 || mVerticalP1Dpad > 0)) ||
                (mCurrPlayerIndex == 1 && (Input.GetKeyDown(KeyCode.DownArrow) || mVerticalP2 < 0 || mVerticalP2Dpad > 0) )) && 
                mPauseSelectIndex < maxPauseButton - 1 && !mIsP1VerticalAxisUsed)
            {
                mIsP1VerticalAxisUsed = true;
                mPauseSelectIndex++;
                pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = mSelectedButtonColor;
                pauseTrans.GetChild(mPauseSelectIndex - 1).GetComponent<Image>().color = mUnselectedButtonColor;
            }

            if ( (mCurrPlayerIndex == 0 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.Z)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.acceptKey))) || 
				(mCurrPlayerIndex == 1 && Input.GetKeyDown(KeyCode.Period)))
            {
                if (mPauseSelectIndex == 0) DisablePauseScreen();
                else if (mPauseSelectIndex == 1) RestartLevel();
                else if (mPauseSelectIndex == 2) ReturnToTitleScreen();
            }
        }
		else if (state == State.SHOW_SCORE)
		{
            if (!mIsCoroutine)
            {
                // TODO : Add Game over bgm
                if (!mIsInputName)
                {
                    if (AudioManager.sSingleton != null) AudioManager.sSingleton.StopBGM();

                    mIsInputName = true;
                    StartCoroutine(CountUpRoutine(cumulativeScoreText, SetupShowRank));
                }
            }
		}
        else if (state == State.NAME_INPUT) HandleNameInput();
        else if (state == State.GAME_OVER) HandleGameOver();
    }

	public bool IsPause
	{ 
		get 
		{ 
			if (state == State.PAUSE) return true;
			return false;
		} 
	}

    public bool IsShowScoreRankNameInput
    {
        get
        {
            if (state == State.SHOW_SCORE || state == State.NAME_INPUT) return true;
            return false;
        }
    }

	public bool IsPauseGameOverMenu
	{ 
		get 
		{ 
            if (state == State.PAUSE || state == State.GAME_OVER) return true;
			return false;
		} 
	}

	public Vector3 GetLinkBarPos(int index) { return mPlayerUIList[index].linkBarImage.transform.position; }
    public Transform GetDeathTimerUI(int index) { return mPlayerUIList[index].deathDisplay; }
    public Image GetReviveUI(int index) { return mPlayerUIList[index].reviveSoulInside; }

    // ----------------------------------------------------------------------------------------------------
    // -------------------------------------- Screen Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

	public void ShowStageName()
	{
		if (mIsStartCount)	mShowStageTimer += Time.deltaTime;
		if (isShowStageName && mShowStageTimer > waitTimeAftAutoCollect) 
		{
			isShowStageName = false;
			stageNameTrans.gameObject.SetActive (true);
            StartCoroutine (FadeChildrenInOut(stageNameTrans, 1, stageShowSpeed, stageShowDuration, () => {}));
		}
	}

	public void ShowAutoCollectZone()
	{
		if (isShowAutoCollect) 
		{
			isShowAutoCollect = false;
			autoCollectTrans.gameObject.SetActive (true);
            StartCoroutine (FadeChildrenInOut (autoCollectTrans, autoCollectNumTimes, autoCollectFadeSpeed, autoCollectDelay, () => { mIsStartCount = true; }));
            BGM_Appear.SlideIn();
		}
	}

    public void ShowBonusScore(int bonusScore)
    {
        if(!mIsCoroutine) 
        {
            Text text = null;
            for (int i = 0; i < bonusScoreTrans.childCount; i++)
            {
                if (bonusScoreTrans.GetChild(i).name == TagManager.sSingleton.UI_BonusScoreName)
                    text = bonusScoreTrans.GetChild(i).GetComponent<Text>();
            }

            text.text = bonusScore.ToString();
            StartCoroutine (FadeChildrenInOut (bonusScoreTrans, 1, bonusScoreFadeSpeed, bonusScoreStayDuration, () => {}));
        }
    }

	public void EnableGameOverScreen()
	{
        StartCoroutine(WaitThenDo(GameManager.sSingleton.gameOverWaitDur, SetupShowScore));
	}

    public void EnablePauseScreen(int playerNum)
    {
		if (state == State.GAME_OVER || state == State.SHOW_SCORE || state == State.NAME_INPUT) return;

        if (playerNum == 1) isPlayer1Pause = true;
        else isPlayer2Pause = true;

		state = State.PAUSE;
        mSavedTimeScale = Time.timeScale;
        Time.timeScale = 0;
		mCurrPlayerIndex = playerNum - 1;

		Transform pauseTrans = mPlayerUIList[mCurrPlayerIndex].pauseTrans;
        pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = mSelectedButtonColor;
        pauseTrans.gameObject.SetActive(true);

        if (AudioManager.sSingleton != null) AudioManager.sSingleton.PauseBGM(true);
    }

    public void DisablePauseScreen()
    {
        isPlayer1Pause = false;
        isPlayer2Pause = false;

		Transform pauseTrans = mPlayerUIList[mCurrPlayerIndex].pauseTrans;
        pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = mUnselectedButtonColor;
        pauseTrans.gameObject.SetActive(false);

		mCurrPlayerIndex = 0;
        mPauseSelectIndex = 0;
        Time.timeScale = mSavedTimeScale;
		state = State.NONE;

        if (AudioManager.sSingleton != null) AudioManager.sSingleton.PauseBGM(false);
    }

    public void GreyOutPlayerUI(int playerID)
    {
        if (!mIsCoroutine)
        {
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[playerID - 1].powerLevel_UI.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[playerID - 1].highScore_UI.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[playerID - 1].score_UI.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[playerID - 1].multiplier_UI.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[playerID - 1].multX_UI.transform, 0.38f, 1));

            StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].linkBarImage.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].linkBarOutside.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].percent_UI.transform, 0.38f, 1));

            StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].linkBarImage.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].linkBarOutside.transform, 0.38f, 1));
            StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].percent_UI.transform, 0.38f, 1));

            StartCoroutine(ReduceChildrenColorBrightness(mPlayerUIList[playerID - 1].subjectName, 0.38f, 1));

            // Grey out the other player's link name.
            if (playerID == 1) StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].link_Name.transform, 0.38f, 1));
            else StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].link_Name.transform, 0.38f, 1));

//            ActivateRedCrossLinkBar();
            mIsDeactivatedLink = true;
        }
    }

    public void DeactivateBothLinkBar()
    {
        if (mIsDeactivatedLink) return;

        StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].linkBarImage.transform, 0.38f, 1));
        StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].linkBarOutside.transform, 0.38f, 1));
        StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].percent_UI.transform, 0.38f, 1));

        StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].linkBarImage.transform, 0.38f, 1));
        StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].linkBarOutside.transform, 0.38f, 1));
        StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].percent_UI.transform, 0.38f, 1));

        StartCoroutine(ReduceColorBrightness(mPlayerUIList[0].link_Name.transform, 0.38f, 1));
        StartCoroutine(ReduceColorBrightness(mPlayerUIList[1].link_Name.transform, 0.38f, 1));

        mPlayerUIList[0].percent_UI.text = "STOP";
        mPlayerUIList[1].percent_UI.text = "STOP";

        mIsDeactivatedLink = true;
    }

    // ----------------------------------------------------------------------------------------------------
    // ----------------------------------- Player UI Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    public void ActivateRedCrossLinkBar()
    {
        if (mIsRedCross) return;

        for (int i = 0; i < mPlayerUIList.Count; i++)
        {
            mPlayerUIList[i].redCross.gameObject.SetActive(true);
        }
        mIsRedCross = true;
    }

    public void UpdateLife(int playerNum, int currLife)
    {
        for (int i = 0; i < GameManager.sSingleton.plyMaxLife; i++)
        {
            Transform currTrans = mPlayerUIList[playerNum - 1].lifePointTrans.GetChild(i);

            if (currLife > 0) currTrans.gameObject.SetActive(true);
            else currTrans.gameObject.SetActive(false);

            currLife--;
        }
    }

    public void UpdatePower(int playerNum, float currPower, float maxPower)
    {
        string temp = currPower.ToString("F2") + " / " + maxPower.ToString("F2");
        mPlayerUIList[playerNum - 1].powerLevel_UI.text = temp;
    }

    public void UpdateScore(int playerNum, int score)
    {
        PlayerInfo currPlayer = mPlayerUIList[playerNum - 1];
        currPlayer.startCurrScore = currPlayer.currScore;
        currPlayer.toReachScore = score;

		mScoreTimer = 0;
        if(!currPlayer.isScoreCoroutine) StartCoroutine(AddScoreSequence(playerNum));
    }

    public void UpdateScoreMultiplier(int playerNum, float mult)
    {
        PlayerInfo currPlayer = mPlayerUIList[playerNum - 1];
        currPlayer.startCurrMult = currPlayer.currMult;
        currPlayer.toReachMult = mult;

        mMultTimer = 0;
        if(!currPlayer.isMultCoroutine) StartCoroutine(AnimateMultiplierSequence(playerNum));
    }

    public void UpdateBomb(int playerNum, int currBomb)
    {
        for (int i = 0; i < GameManager.sSingleton.plyMaxBomb; i++)
        {
            Transform currTrans = mPlayerUIList[playerNum - 1].bombTrans.GetChild(i);

            if (currBomb > 0) currTrans.gameObject.SetActive(true);
            else currTrans.gameObject.SetActive(false);

            currBomb--;
        }
    }

    public void UpdateLinkBar(int playerNum, float linkVal)
    {
        PlayerInfo currPlayerInfo = mPlayerUIList[playerNum - 1];
        currPlayerInfo.linkBarImage.fillAmount = linkVal;

        if (linkVal >= 1) currPlayerInfo.percent_UI.text = "MAX";
        else
        {
            Text percent_UI = currPlayerInfo.percent_UI;
            string valStr = (linkVal / 1 * 100).ToString("F1");

            percent_UI.text = valStr + "%";

            int valInt = (int)float.Parse(valStr);
            if (valInt == 100) percent_UI.text = "99.9%";
        }
    }

    public void MakeScoreYellow(int playerNum, bool isDarken)
    {
        mPlayerUIList[playerNum - 1].score_UI.color = Color.yellow;
        if (isDarken)
        {
            Color color = mPlayerUIList[playerNum - 1].score_UI.color;
            color.r -= 0.38f;
            color.g -= 0.38f;
            color.b -= 0.38f;
            mPlayerUIList[playerNum - 1].score_UI.color = color;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------- Boss Functions -----------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    public void ActivateBossTimer(float duration)
    {
        mSecondText.color = Color.white;
        mMilliSecondText.color = Color.white;
        mDotText.color = Color.white;

        timerUI.gameObject.SetActive(true);
        mDuration = duration;
        UpdateBossTimer(mDuration);
    }

    public void DeactivateBossTimer()
    {
        timerUI.gameObject.SetActive(false);
        mDuration = 0;
    }

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------- Private Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void InitializeUI(Transform playerUI, int index)
    {
        for (int i = 0; i < playerUI.childCount; i++)
        {
            Transform currChild = playerUI.GetChild(i);
            if (currChild.name == TagManager.sSingleton.UI_HighScoreName) mPlayerUIList[index].highScore_UI = currChild.GetComponent<Text>();
            else if (currChild.name == TagManager.sSingleton.UI_ScoreName) mPlayerUIList[index].score_UI = currChild.GetComponent<Text>();
            else if (currChild.name == TagManager.sSingleton.UI_LifePointName) mPlayerUIList[index].lifePointTrans = currChild;
            else if (currChild.name == TagManager.sSingleton.UI_BombName) mPlayerUIList[index].bombTrans = currChild;
            else if (currChild.name == TagManager.sSingleton.UI_PowerLevelName) mPlayerUIList[index].powerLevel_UI = currChild.GetComponent<Text>();
            else if (currChild.name == TagManager.sSingleton.UI_HighScoreMultName) mPlayerUIList[index].multiplier_UI = currChild.GetComponent<Text>();
            else if (currChild.name == TagManager.sSingleton.UI_HighScoreMultXName) mPlayerUIList[index].multX_UI = currChild.GetComponent<Text>();
            else if (currChild.name == TagManager.sSingleton.UI_PlyBombPotraitName) mPlayerUIList[index].bombPotraitSR = currChild.GetComponentInChildren<SpriteRenderer>();
            else if (currChild.name == TagManager.sSingleton.UI_LinkBarName) 
            {
                for (int j = 0; j < currChild.childCount; j++)
                {
                    Transform currGrandChild = currChild.GetChild(j);

                    if (currGrandChild.name == TagManager.sSingleton.UI_LinkBarInsideName)
                        mPlayerUIList[index].linkBarImage = currGrandChild.GetComponent<Image>();
                    else if (currGrandChild.name == TagManager.sSingleton.UI_LinkMaxName)
                        mPlayerUIList[index].percent_UI = currGrandChild.GetComponent<Text>();
                    else
                        mPlayerUIList[index].linkBarOutside = currGrandChild.GetComponent<Image>();
                }
            }
            else if (currChild.name == TagManager.sSingleton.UI_PauseMenuName) mPlayerUIList[index].pauseTrans = currChild;
            else if (currChild.name == TagManager.sSingleton.UI_DeathDisplayName) mPlayerUIList[index].deathDisplay = currChild;
            else if (currChild.name == TagManager.sSingleton.UI_RedCrossName) mPlayerUIList[index].redCross = currChild;
            else if (currChild.name == TagManager.sSingleton.UI_SubjectName) 
            {
                mPlayerUIList[index].subjectName = currChild;
                for (int j = 0; j < currChild.childCount; j++)
                {
                    Transform currGrandChild = currChild.GetChild(j);
                    if (currGrandChild.name == TagManager.sSingleton.UI_LinkName)
                    {
                        mPlayerUIList[index].link_Name = currGrandChild.GetComponent<Text>();
                        break;
                    }
                }
            }
            else if (currChild.name == TagManager.sSingleton.UI_ReviveSoulName) mPlayerUIList[index].reviveSoulInside = currChild.GetComponent<Image>();
        }

        for (int i = 0; i < GameManager.sSingleton.plyStartLife; i++)
        { mPlayerUIList[index].lifePointTrans.GetChild(i).gameObject.SetActive(true); }

        for (int i = 0; i < GameManager.sSingleton.plyStartBomb; i++)
        { mPlayerUIList[index].bombTrans.GetChild(i).gameObject.SetActive(true); }
    }

    void HandleNameInput()
    {
        if (scoreTimerText.enabled)
        {
            nameInputTimer -= Time.unscaledDeltaTime * nameInputCountDownSpd;
            scoreTimerText.text = ((int)nameInputTimer).ToString();

            if (nameInputTimer < 0) 
            {
                if (mP1Co != null) 
                {
                    mP1StopFade = true;
                    StopCoroutine(mP1Co);
                    ResetInputNameAlpha (1);
                    AddToSavedName(1);
                }
                if (mP2Co != null) 
                { 
                    mP2StopFade = true;
                    StopCoroutine(mP2Co);
                    ResetInputNameAlpha (2);
                    AddToSavedName(2);
                }
            }
        }

        if (!mIsP1Fading && !mP1StopFade) 
        {
            mP1Co = FadeInOut (1, mP1NameInputTextList [mP1NameInputNum], alphaChangeSpeed, alphaDelay);
            StartCoroutine (mP1Co);
        }
        if (mNumOfPlayers == 2 && !mIsP2Fading && !mP2StopFade) 
        {
            mP2Co = FadeInOut (2, mP2NameInputTextList [mP2NameInputNum], alphaChangeSpeed, alphaDelay);
            StartCoroutine(mP2Co);
        }

        if (!mP1StopFade)
        {
            if (mP1NameInputNum < totalCharNameInput) 
            {
                if (mP1CharChangeDelay > 0)
                {
                    mP1CharChangeDelay -= Time.unscaledDeltaTime;
                    if (mP1CharChangeDelay < 0) mP1CharChangeDelay = 0;
                }

                if (((mIsP1KeybInput && Input.GetKey(KeyCode.T)) || mVerticalP1 > 0 || mVerticalP1Dpad < 0) && mP1CharChangeDelay <= 0)
                {
                    mP1CharChangeDelay = charChangeDelay;

                    if (mP1NameIndexList [mP1NameInputNum] <= startUnicode) mP1NameIndexList [mP1NameInputNum] = endUnicode;
                    else mP1NameIndexList [mP1NameInputNum]--;

                    mP1NameInputTextList [mP1NameInputNum].text = ((char)mP1NameIndexList [mP1NameInputNum]).ToString();
                }
                else if (((mIsP1KeybInput && Input.GetKey(KeyCode.G)) || mVerticalP1 < 0 || mVerticalP1Dpad > 0) && mP1CharChangeDelay <= 0)
                {
                    mP1CharChangeDelay = charChangeDelay;

                    if (mP1NameIndexList [mP1NameInputNum] >= endUnicode) mP1NameIndexList [mP1NameInputNum] = startUnicode;
                    else mP1NameIndexList [mP1NameInputNum]++;

                    mP1NameInputTextList [mP1NameInputNum].text = ((char)mP1NameIndexList [mP1NameInputNum]).ToString();
                }
            }

            if (((mIsP1KeybInput && (Input.GetKeyDown (KeyCode.F) || Input.GetKeyDown (KeyCode.H))) || mHorizontalP1 != 0 || mHorizontalP1Dpad != 0) &&
                !mIsP1HorizontalAxisUsed) 
            {
                mIsP1HorizontalAxisUsed = true;
                ResetInputNameAlpha (1);

                if ((Input.GetKeyDown(KeyCode.F) || mHorizontalP1 < 0 || mHorizontalP1Dpad < 0) && mP1NameInputNum > 0) mP1NameInputNum--;
                else if ((Input.GetKeyDown(KeyCode.H) || mHorizontalP1 > 0 || mHorizontalP1Dpad > 0) && mP1NameInputNum < totalCharNameInput) mP1NameInputNum++;
            }

            if (((mIsP1KeybInput && Input.GetKeyDown (KeyCode.Z)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.acceptKey)) && mP1NameInputNum == 3)
            {
                ResetInputNameAlpha (1);
                mP1StopFade = true;
                AddToSavedName(1);
            }
        }

        if (!mP2StopFade)
        {
            if (mP2NameInputNum < totalCharNameInput) 
            {
                if (mP2CharChangeDelay > 0)
                {
                    mP2CharChangeDelay -= Time.unscaledDeltaTime;
                    if (mP2CharChangeDelay < 0) mP2CharChangeDelay = 0;
                }

                if ((Input.GetKey (KeyCode.UpArrow) || mVerticalP2 > 0 || mVerticalP2Dpad < 0)  && mP2CharChangeDelay <= 0) 
                {
                    mP2CharChangeDelay = charChangeDelay;

                    if (mP2NameIndexList [mP2NameInputNum] <= startUnicode) mP2NameIndexList [mP2NameInputNum] = endUnicode;
                    else mP2NameIndexList [mP2NameInputNum]--;

                    mP2NameInputTextList [mP2NameInputNum].text = ((char)mP2NameIndexList [mP2NameInputNum]).ToString();
                }
                else if ((Input.GetKey (KeyCode.DownArrow) || mVerticalP2 < 0 || mVerticalP2Dpad > 0) && mP2CharChangeDelay <= 0) 
                {
                    mP2CharChangeDelay = charChangeDelay;

                    if (mP2NameIndexList [mP2NameInputNum] >= endUnicode) mP2NameIndexList [mP2NameInputNum] = startUnicode;
                    else mP2NameIndexList [mP2NameInputNum]++;

                    mP2NameInputTextList [mP2NameInputNum].text = ((char)mP2NameIndexList [mP2NameInputNum]).ToString();
                }
            }

            if ((Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow) || mHorizontalP2 != 0 || mHorizontalP2Dpad != 0) &&
                !mIsP2HorizontalAxisUsed)
            {
                mIsP2HorizontalAxisUsed = true;
                ResetInputNameAlpha (2);

                if ((Input.GetKeyDown(KeyCode.LeftArrow) || mHorizontalP2 < 0 || mHorizontalP2Dpad < 0) && mP2NameInputNum > 0) mP2NameInputNum--;
                else if ((Input.GetKeyDown(KeyCode.RightArrow) || mHorizontalP2 > 0 || mHorizontalP2Dpad > 0) && mP2NameInputNum < totalCharNameInput) mP2NameInputNum++;   
            }

            if ((Input.GetKeyDown (KeyCode.Period) || Input.GetKeyDown(JoystickManager.sSingleton.p2_joystick.acceptKey)) && mP2NameInputNum == 3)
            {
                ResetInputNameAlpha (2);
                mP2StopFade = true;
                AddToSavedName(2);
            }
        }

        if (((mNumOfPlayers == 1 && mP1StopFade) ||
            (mP1StopFade && mP2StopFade)) && !mIsWait) 
        {
            SaveScore();
            scoreTimerText.enabled = false;
            StartCoroutine(WaitThenDo(GameManager.sSingleton.gameOverWaitDur, SetupGameOver));
        }
    }

    void AddToSavedName(int playerNum)
    {
        for (int i = 0; i < totalCharNameInput; i++) 
        {
            if (playerNum == 1) mP1SavedName += mP1NameInputTextList[i].text.ToString();
            else if (playerNum == 2) mP2SavedName += mP2NameInputTextList[i].text.ToString();
        }

        if (playerNum == 1) Debug.Log(mP1SavedName);
        else if (playerNum == 2) Debug.Log(mP2SavedName);
    }

    void SaveScore()
    {
        List<Text> p1NameTextList = MainMenuManager.sSingleton.p1NameTextList;
        List<Text> p2NameTextList = MainMenuManager.sSingleton.p2NameTextList;
        List<Text> totalScoreTextList = MainMenuManager.sSingleton.totalScoreTextList;

        for (int i = 0; i < totalScoreTextList.Count; i++)
        {
            Text currScoreText = totalScoreTextList[i];
            int loadedScore = PlayerPrefs.GetInt(currScoreText.name + i);

            if (mTotalScore > loadedScore)
            {
                mRank = i;

                // Push all entries down the highscore.
                for (int j = totalScoreTextList.Count - 2; j >= i; j--)
                {
                    // Push P1 name down.
                    Text currP1 = p1NameTextList[j];
                    string currP1Name = PlayerPrefs.GetString(currP1.name + j);
                    PlayerPrefs.SetString(p1NameTextList[j + 1].name + (j + 1), currP1Name);

                    // Push P2 name down.
                    Text currP2 = p2NameTextList[j];
                    string currP2Name = PlayerPrefs.GetString(currP2.name + j);
                    PlayerPrefs.SetString(p2NameTextList[j + 1].name + (j + 1), currP2Name);

                    // Push total score down.
                    Text currTotalScore = totalScoreTextList[j];
                    int currTotalScoreVal = PlayerPrefs.GetInt(currTotalScore.name + j);
                    PlayerPrefs.SetInt(totalScoreTextList[j + 1].name + (j + 1), currTotalScoreVal);
                }

                break;
            }
        }

        PlayerPrefs.SetString(p1NameTextList[mRank].name + mRank, mP1SavedName);
        PlayerPrefs.SetString(p2NameTextList[mRank].name + mRank, mP2SavedName);
        PlayerPrefs.SetInt(totalScoreTextList[mRank].name + mRank, mTotalScore);
    }

    void HandleGameOver()
    {
        if (((mIsP1KeybInput && Input.GetKeyDown(KeyCode.T)) || mVerticalP1 > 0 || mVerticalP1Dpad < 0) && 
            mGameOverSelectIndex > 0 && !mIsP1VerticalAxisUsed)
        {
            mIsP1VerticalAxisUsed = true;
            mGameOverSelectIndex--;
            gameOverButton_UI.GetChild(mGameOverSelectIndex).GetComponent<Image>().color = mSelectedButtonColor;
            gameOverButton_UI.GetChild(mGameOverSelectIndex + 1).GetComponent<Image>().color = mUnselectedButtonColor;
        }
        else if (((mIsP1KeybInput && Input.GetKeyDown(KeyCode.G)) || mVerticalP1 < 0 || mVerticalP1Dpad > 0) && 
            mGameOverSelectIndex < maxGameOverButton - 1 && !mIsP1VerticalAxisUsed)
        {
            mIsP1VerticalAxisUsed = true;
            mGameOverSelectIndex++;
            gameOverButton_UI.GetChild(mGameOverSelectIndex).GetComponent<Image>().color = mSelectedButtonColor;
            gameOverButton_UI.GetChild(mGameOverSelectIndex - 1).GetComponent<Image>().color = mUnselectedButtonColor;
        }

        if ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.Z)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.acceptKey))
        {
            if (mGameOverSelectIndex == 0) RestartLevel();
            else if (mGameOverSelectIndex == 1) ReturnToTitleScreen();
        }
    }

	void ResetInputNameAlpha(int playerNum)
	{
		if (playerNum == 1)
		{
			StopCoroutine (mP1Co);
			mIsP1Fading = false;

			Color color = mP1NameInputTextList [mP1NameInputNum].color;
			color.a = 1;
			mP1NameInputTextList [mP1NameInputNum].color = color;
		}
		else if (playerNum == 2)
		{
			StopCoroutine (mP2Co);
			mIsP2Fading = false;

			Color color = mP2NameInputTextList [mP2NameInputNum].color;
			color.a = 1;
			mP2NameInputTextList [mP2NameInputNum].color = color;
		}
	}

    void UpdateBossTimer(float duration)
    {
        float seconds = (int)duration;
        mSecondText.text = seconds.ToString();

        float milliSeconds = duration % 1;
        string milliSecStr = milliSeconds.ToString("F2");

        char c1 = milliSecStr[2];
        char c2 = milliSecStr[3];
        milliSecStr = c1.ToString() + c2.ToString();

        mMilliSecondText.text = milliSecStr;

        if (seconds < 10)
        {
            mSecondText.color = Color.red;
            mMilliSecondText.color = Color.red;
            mDotText.color = Color.red;
        }

        if(duration == 0) timerUI.gameObject.SetActive(false);
    }

    void RestartLevel()
    {
        Time.timeScale = 1;
        AudioManager.sSingleton.FadeOutBGM ();
        StartCoroutine(mLevelController.Fading("DreamCarnage"));
    }

    void ReturnToTitleScreen()
    {
        Time.timeScale = 1;
        AudioManager.sSingleton.FadeOutBGM ();
        StartCoroutine(mLevelController.Fading("MainMenu", () => { MainMenuManager.sSingleton.SetToDefaultScene(); } ));
    }

    string GetScoreWithZero(string score)
    {
        int addZero = mPlayerUIList[0].score_UI.text.Length - score.Length;

        for (int i = 0; i < addZero; i++)
        { score = "0" + score; }

        return score;
    }

    void SetupShowScore()
    {
        Time.timeScale = 0;
        state = State.SHOW_SCORE;

        gameOver_UI.gameObject.SetActive(true);
        showScore_UI.gameObject.SetActive(true);

        mTotalScore = mPlayerUIList[0].toReachScore + mPlayerUIList[1].toReachScore;
    }

	void SetupShowRank()
	{
        bool isRank = false;
        List<Text> totalScoreTextList = MainMenuManager.sSingleton.totalScoreTextList;

        for (int i = 0; i < MainMenuManager.sSingleton.maxNames; i++)
        {
            Text currScoreText = totalScoreTextList[i];
            int loadedScore = PlayerPrefs.GetInt(currScoreText.name + i);

            if (mTotalScore > loadedScore)
            {
                ShowRank(i);
                isRank = true;
                break;
            }
        }

        if (isRank) StartCoroutine(WaitThenDo(GameManager.sSingleton.gameOverWaitDur, SetupNameInput));
        else StartCoroutine(WaitThenDo(GameManager.sSingleton.gameOverWaitDur, SetupGameOver));
	}

	void ShowRank(int index)
	{
		mRank = index;

		int rank = index + 1;
		string backStr = "";

		if (rank == 1) backStr = "st";
		else if (rank == 2) backStr = "nd";
		else if (rank == 3) backStr = "rd";
		else backStr = "th";

		rankText.text = rank.ToString();
		backWordText.text = backStr;

		rankText.gameObject.SetActive(true);
		backWordText.gameObject.SetActive(true);
	}

    void InitializeInputName()
    {
        RectTransform rect = charInputTrans.GetComponent<RectTransform> ();
        for (int i = 0; i < GameManager.sSingleton.TotalNumOfPlayer(); i++) 
        {
            Transform charTrans = null;
            if (i == 0) charTrans = p1CharInputTrans;
            else charTrans = p2CharInputTrans;

            Vector3 latestPos = Vector3.zero;
            for (int j = 0; j < totalCharNameInput; j++) 
            {
                Transform trans = Instantiate (charInputTrans, charInputTrans.position, Quaternion.identity);
                trans.SetParent (charTrans);

                RectTransform currRect = trans.GetComponent<RectTransform> ();
                currRect.offsetMin = rect.offsetMin;
                currRect.offsetMax = rect.offsetMax;

                Vector3 pos = trans.localPosition;
                pos.x += j * xOffsetBetwChar;
                trans.localPosition = pos;
                trans.localScale = charInputTrans.localScale;

                latestPos = trans.localPosition;

                if (i == 0) 
                {
                    mP1NameIndexList.Add (startUnicode); // 65 char is A
                    mP1NameInputTextList.Add (trans.GetComponent<Text>());
                }
                else if (i == 1)
                {
                    mP2NameIndexList.Add (startUnicode);
                    mP2NameInputTextList.Add (trans.GetComponent<Text>());
                }
            }

            Transform endTrans = Instantiate (charEndTrans, charEndTrans.position, Quaternion.identity);
            endTrans.SetParent (charTrans);

            RectTransform endRect = endTrans.GetComponent<RectTransform> ();
            endRect.offsetMin = endRect.offsetMin;
            endRect.offsetMax = endRect.offsetMax;

            Vector3 endPos = endTrans.localPosition;
            endPos.x = latestPos.x + xOffsetBetwChar;
            endPos.y = charEndTrans.localPosition.y;
            endTrans.localPosition = endPos;
            endTrans.localScale = charEndTrans.localScale;

            if (i == 0) mP1NameInputTextList.Add (endTrans.GetComponent<Text>());
            else if (i == 1) mP2NameInputTextList.Add (endTrans.GetComponent<Text>());
        }
    }

    void SetupNameInput()
    {
        state = State.NAME_INPUT;
        scoreTimerText.text = mDefaultNameInputTimer.ToString();
        scoreTimerText.enabled = true;
        showNameInput_UI.gameObject.SetActive(true);
    }

    void SetupGameOver()
    {
        state = State.GAME_OVER;
        gameOverButton_UI.GetChild(mPauseSelectIndex).GetComponent<Image>().color = mSelectedButtonColor;
        gameOverButton_UI.gameObject.SetActive (true);
    }

    IEnumerator AddScoreSequence(int player)
    {
        PlayerInfo currPlayer = mPlayerUIList[player - 1];
        currPlayer.isScoreCoroutine = true;


		while (mScoreTimer < scoreCountTime)
		{
            while (state == State.PAUSE)
            {
                yield return null;
            }

			mScoreTimer += Time.deltaTime;
			if (mScoreTimer > scoreCountTime) mScoreTimer = scoreCountTime;

            float differences = (float)(currPlayer.toReachScore - currPlayer.startCurrScore);
            float currValForCurrTime = (mScoreTimer / scoreCountTime) * differences;
            currPlayer.currScore = (int)Mathf.Ceil((float)currPlayer.startCurrScore + currValForCurrTime);

            if (currPlayer.currScore > currPlayer.toReachScore) currPlayer.currScore = currPlayer.toReachScore;
            currPlayer.score_UI.text = GetScoreWithZero(currPlayer.currScore.ToString());
			yield return null;
		}
        currPlayer.isScoreCoroutine = false;
    }

    IEnumerator AnimateMultiplierSequence(int player)
    {
        PlayerInfo currPlayer = mPlayerUIList[player - 1];
        currPlayer.isMultCoroutine = true;

        Text multText = currPlayer.multiplier_UI;
        Text xText = currPlayer.multX_UI;

        float time = multCountUpTime;

        while (mMultTimer < time)
        {
            while (state == State.PAUSE)
            {
                yield return null;
            }

            if (currPlayer.currMult < currPlayer.toReachMult) time = multCountUpTime;
            else if (currPlayer.currMult > currPlayer.toReachMult) time = multCountDownTime;

            mMultTimer += Time.deltaTime;
            if (mMultTimer > time) mMultTimer = time;

            float differences = currPlayer.toReachMult - currPlayer.startCurrMult;
            float currValForCurrTime = (mMultTimer / time) * differences;
            currPlayer.currMult = currPlayer.startCurrMult + currValForCurrTime;

            if (currPlayer.toReachMult >= 1 && currPlayer.currMult > currPlayer.toReachMult) currPlayer.currMult = currPlayer.toReachMult;
            multText.text = currPlayer.currMult.ToString("F2");

            if (currPlayer.toReachMult < 1)
            {
                multText.color = Color.red;
                xText.color = Color.red;
            }
            else if (currPlayer.toReachMult >= 1)
            {
                multText.color = Color.white;
                xText.color = Color.white;
            }

            yield return null;
        }
        currPlayer.isMultCoroutine = false;
    }

    IEnumerator WaitThenDo(float waitDur, Action doLast)
    {
        mIsWait = true;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(waitDur));
        doLast();
        mIsWait = false;
    }

	IEnumerator FadeChildrenInOut(Transform trans, int numOfTimes, float speed, float delay, Action doLast)
	{
        mIsCoroutine = true;
        List<Text> textList = new List<Text>();
		List<SpriteRenderer> srList = new List<SpriteRenderer> ();

		for (int i = 0; i < trans.childCount; i++) 
		{
            if (trans.GetChild(i).GetComponent<Text> () != null) textList.Add(trans.GetChild(i).GetComponent<Text> ());
			else if (trans.GetChild(i).GetComponent<SpriteRenderer>() != null) srList.Add(trans.GetChild(i).GetComponent<SpriteRenderer>());
		}

		while (numOfTimes != 0)
		{
            while (textList[0].color.a < 1)
			{
                Color color = textList[0].color;
				color.a += Time.unscaledDeltaTime * speed;
				if (color.a > 1) color.a = 1;

                for (int i = 0; i < textList.Count; i++)
                {
                    textList[i].color = color;
                }
				for (int i = 0; i < srList.Count; i++)
				{
					srList[i].color = color;
				}
				yield return null; 
			}

			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(delay));

            while (textList[0].color.a > 0)
			{
                Color color = textList[0].color;
				color.a -= Time.unscaledDeltaTime * speed;
				if (color.a < 0) color.a = 0;

                for (int i = 0; i < textList.Count; i++)
                {
                    textList[i].color = color;
                }
				for (int i = 0; i < srList.Count; i++)
				{
					srList[i].color = color;
				}
				yield return null;
			}
			numOfTimes--;
		}
		doLast ();
        mIsCoroutine = false;
	}

	IEnumerator FadeInOut(int playerIndex, Text text, float speed, float delay)
	{
        mIsCoroutine = true;
		if (playerIndex == 1) mIsP1Fading = true;
		else mIsP2Fading = true;

		while (text.color.a > 0)
		{
			Color color = text.color;
            color.a -= Time.unscaledDeltaTime * speed;
			if (color.a < 0) color.a = 0;
			text.color = color;

			yield return null;
		}

		while (text.color.a < 1)
		{
			Color color = text.color;
            color.a += Time.unscaledDeltaTime * speed;
			if (color.a > 1) color.a = 1;
			text.color = color;

			yield return null;
		}
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(delay));

		if (playerIndex == 1) mIsP1Fading = false;
		else mIsP2Fading = false;

        mIsCoroutine = false;
	}

    IEnumerator ReduceColorBrightness(Transform trans, float minusValue, float speed)
    {
        mIsCoroutine = true;

        Text text = trans.GetComponent<Text>();
        Image image = trans.GetComponent<Image>();

        if (text != null && image != null) yield break;

        float currMinusVal = 0;
        while (currMinusVal < minusValue)
        {
            Color color = Color.white;
            if (text != null) color = text.color;
            else if (image != null) color = image.color;

            float val = 0;
            if (BombManager.sSingleton.isTimeStopBomb) val = Time.unscaledDeltaTime * speed;
            else val = Time.deltaTime * speed;

            currMinusVal += val;
            if (currMinusVal > minusValue)
            {
                val = currMinusVal - minusValue;
                currMinusVal = minusValue;
            }

            color.r -= val;
            color.g -= val;
            color.b -= val;

            if (text != null) text.color = color;
            else if (image != null) image.color = color;

            yield return null;
        }
        mIsCoroutine = false;
    }

    IEnumerator ReduceChildrenColorBrightness(Transform trans, float minusValue, float speed)
    {
        mIsCoroutine = true;
        List<Text> textList = new List<Text>();
        List<SpriteRenderer> srList = new List<SpriteRenderer> ();

        for (int i = 0; i < trans.childCount; i++) 
        {
            if (trans.GetChild(i).GetComponent<Text>() != null) textList.Add(trans.GetChild(i).GetComponent<Text>());
            else if (trans.GetChild(i).GetComponent<SpriteRenderer>() != null) srList.Add(trans.GetChild(i).GetComponent<SpriteRenderer>());
        }

        if (textList.Count == 0 && srList.Count == 0) yield break;

        float currMinusVal = 0;
        while (currMinusVal < minusValue)
        {
            Color color = Color.white;
            if (textList.Count != 0) color = textList[0].color;
            else if (srList.Count != 0) color = srList[0].color;

            float val = 0;
            if (BombManager.sSingleton.isTimeStopBomb) val = Time.unscaledDeltaTime * speed;
            else val = Time.deltaTime * speed;

            currMinusVal += val;
            if (currMinusVal > minusValue)
            {
                val = currMinusVal - minusValue;
                currMinusVal = minusValue;
            }

            color.r -= val;
            color.g -= val;
            color.b -= val;

            for (int i = 0; i < textList.Count; i++)
            {
                textList[i].color = color;
            }

            for (int i = 0; i < srList.Count; i++)
            {
                srList[i].color = color;
            }

            yield return null;
        }
        mIsCoroutine = false;
    }

	IEnumerator CountUpRoutine(Text text, Action doLast)
	{
		mIsCoroutine = true;
		while (mEndScoreTimer != endScoreCountTime) 
		{
			mEndScoreTimer += Time.unscaledDeltaTime;
			if (mEndScoreTimer > endScoreCountTime) mEndScoreTimer = endScoreCountTime;

			float val = Mathf.Ceil((mEndScoreTimer / endScoreCountTime) * mTotalScore);
			text.text = GetScoreWithZero(((int)val).ToString());
			yield return null;
		}
		doLast ();
		mIsCoroutine = false;
	}
}
