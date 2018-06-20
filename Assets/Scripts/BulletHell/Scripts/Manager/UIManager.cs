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

    public float countUpScoreSpd = 10, nameInputTimer = 60, nameInputCountDownSpd = 1;
	public int maxPauseButton = 3, maxGameOverButton = 3;

    [HideInInspector] public bool isPlayer1Pause = false, isPlayer2Pause = false;

    // Score show and highscore input.
	public Transform charInputTrans, charEndTrans;
	public Transform p1CharInputTrans, p2CharInputTrans;
    public Text cumulativeScoreText, rankText, backWordText, scoreTimerText;
	public int startUnicode = 65, endUnicode = 90;
	public float totalCharNameInput = 3, xOffsetBetwChar = 1, alphaChangeSpeed = 1, alphaDelay = 0, charChangeDelay = 0.1f;

    class PlayerInfo
    {
        public Transform lifePointTrans, bombTrans, pauseTrans, deathDisplay;
        public Text powerLevel_UI, highScore_UI, score_UI, percent_UI;
        public Image linkBarImage;

        // Used for coroutine count-up animation.
        public bool isCoroutine = false;
        public int currScore, toReachScore;

        public PlayerInfo()
        {
            lifePointTrans = bombTrans = pauseTrans = deathDisplay = null;
            powerLevel_UI = highScore_UI = score_UI = percent_UI = null;
            linkBarImage = null;
        }

        public PlayerInfo(Transform lifePointTrans, Transform bombTrans, Transform pauseTrans, Text powerLevel, Text highScore, Text score, 
            Image linkBarImage, Text percent_UI)
        {
            this.lifePointTrans = lifePointTrans;
            this.bombTrans = bombTrans;
            this.pauseTrans = pauseTrans;
            this.powerLevel_UI = powerLevel;
            this.highScore_UI = highScore;
            this.score_UI = score;
            this.linkBarImage = linkBarImage;
            this.percent_UI = percent_UI;
        }
    }
    List<PlayerInfo> mPlayerUIList = new List<PlayerInfo>();

    Text mSecondText, mMilliSecondText;
    float mDuration = 0, mSavedTimeScale, mDefaultNameInputTimer, mP1CharChangeDelay, mP2CharChangeDelay;

    int mCurrPlayerNum = 0, mPauseSelectIndex = 0, mGameOverSelectIndex = 0;

	int mP1NameInputNum, mP2NameInputNum, mTotalScore, mRank;
	string mP1SavedName, mP2SavedName;
	IEnumerator mP1Co, mP2Co;
    bool mIsP1Fading = false, mIsP2Fading = false, mP1StopFade = false, mP2StopFade = false, mIsWait = false;

	List<int> mP1NameIndexList = new List<int> ();
	List<int> mP2NameIndexList = new List<int> ();
	List<Text> mP1NameInputTextList = new List<Text> ();
	List<Text> mP2NameInputTextList = new List<Text> ();

	public enum State
	{
		NONE = 0,
		PAUSE,
		SHOW_SCORE,
        NAME_INPUT,
		GAME_OVER,
	};
	public State state = State.NONE;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

	void Start () 
    {
        for (int i = 0; i < 2; i++)
        { 
            Transform playerUI = null;
            mPlayerUIList.Add(new PlayerInfo()); 

            if (GameManager.sSingleton.IsThisPlayerActive(i + 1))
            {
                if (i == 0) playerUI = player1_UI;
                else if (i == 1) playerUI = player2_UI;
                InitializeUI(playerUI, i);
            }
        }

        for (int i = 0; i < timerUI.childCount; i++)
        {
            Text textScript = timerUI.GetChild(i).GetComponent<Text>();

            if (i == 0) mSecondText = textScript;
            else if(i == 1)mMilliSecondText = textScript;
        }
        timerUI.gameObject.SetActive(false);

		SetupInputName ();
        mDefaultNameInputTimer = nameInputTimer;
        mP1CharChangeDelay = charChangeDelay;
        mP2CharChangeDelay = charChangeDelay;
	}

    void Update()
    {
        if (mDuration != 0)
        {
            mDuration -= Time.deltaTime;
            if (mDuration < 0) mDuration = 0;
            UpdateBossTimer(mDuration);
        }

		if (state == State.PAUSE)
        {
            Transform pauseTrans = mPlayerUIList[mCurrPlayerNum].pauseTrans;
            if (Input.GetKeyDown(KeyCode.UpArrow) && mPauseSelectIndex > 0)
            {
                mPauseSelectIndex--;
                pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = Color.red;
                pauseTrans.GetChild(mPauseSelectIndex + 1).GetComponent<Image>().color = Color.white;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && mPauseSelectIndex < maxPauseButton - 1)
            {
                mPauseSelectIndex++;
                pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = Color.red;
                pauseTrans.GetChild(mPauseSelectIndex - 1).GetComponent<Image>().color = Color.white;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (mPauseSelectIndex == 0) DisablePauseScreen();
                else if (mPauseSelectIndex == 1) RestartLevel();
                else if (mPauseSelectIndex == 2) ReturnToTitleScreen();
            }
        }
		else if (state == State.SHOW_SCORE)
		{
            // TODO: Counting of score. UI.
		}
        else if (state == State.NAME_INPUT) HandleNameInput();
        else if (state == State.GAME_OVER) HandleGameOver();
    }

	public bool IsPauseGameOverMenu
	{ 
		get 
		{ 
			if (state == State.PAUSE || state == State.GAME_OVER) return true;
			else return false;
		} 
	}

    public Transform GetDeathTimerUI(int index) { return mPlayerUIList[index].deathDisplay; }

    // ----------------------------------------------------------------------------------------------------
    // -------------------------------------- Screen Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

	public void EnableGameOverScreen()
	{
        StartCoroutine(WaitThenDo(GameManager.sSingleton.gameOverWaitDur, SetupShowScore));
	}

    public void EnablePauseScreen(int playerNum)
    {
        if (playerNum == 1) isPlayer1Pause = true;
        else isPlayer2Pause = true;

		state = State.PAUSE;
        mSavedTimeScale = Time.timeScale;
        Time.timeScale = 0;
        mCurrPlayerNum = playerNum - 1;

        Transform pauseTrans = mPlayerUIList[mCurrPlayerNum].pauseTrans;
        pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = Color.red;
        pauseTrans.gameObject.SetActive(true);
    }

    public void DisablePauseScreen()
    {
        isPlayer1Pause = false;
        isPlayer2Pause = false;

        Transform pauseTrans = mPlayerUIList[mCurrPlayerNum].pauseTrans;
        pauseTrans.GetChild(mPauseSelectIndex).GetComponent<Image>().color = Color.white;
        pauseTrans.gameObject.SetActive(false);

        mCurrPlayerNum = 0;
        mPauseSelectIndex = 0;
        Time.timeScale = mSavedTimeScale;
		state = State.NONE;
    }

    // ----------------------------------------------------------------------------------------------------
    // ----------------------------------- Player UI Functions --------------------------------------------
    // ----------------------------------------------------------------------------------------------------

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
        currPlayer.toReachScore = score;

        if(!currPlayer.isCoroutine) StartCoroutine(AddScoreSequence(playerNum));
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

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------- Boss Functions -----------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    public void ActivateBossTimer(float duration)
    {
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
            else if (currChild.name == TagManager.sSingleton.UI_LinkBarName) 
            {
                for (int j = 0; j < currChild.childCount; j++)
                {
                    Transform currGrandChild = currChild.GetChild(j);

                    if (currGrandChild.name == TagManager.sSingleton.UI_LinkBarInsideName)
                        mPlayerUIList[index].linkBarImage = currGrandChild.GetComponent<Image>();
                    else if (currGrandChild.name == TagManager.sSingleton.UI_LinkMaxName)
                        mPlayerUIList[index].percent_UI = currGrandChild.GetComponent<Text>();
                }
            }
            else if (currChild.name == TagManager.sSingleton.UI_PauseMenuName) mPlayerUIList[index].pauseTrans = currChild;
            else if (currChild.name == TagManager.sSingleton.UI_DeathDisplayName) mPlayerUIList[index].deathDisplay = currChild;
        }

        for (int i = 0; i < GameManager.sSingleton.plyStartLife; i++)
        { mPlayerUIList[index].lifePointTrans.GetChild(i).gameObject.SetActive(true); }

        for (int i = 0; i < GameManager.sSingleton.plyStartBomb; i++)
        { mPlayerUIList[index].bombTrans.GetChild(i).gameObject.SetActive(true); }
    }

    void HandleShowScore()
    {
        // TODO: Count up animation for score.


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
        if (!mIsP2Fading && !mP2StopFade) 
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

                if (Input.GetKey(KeyCode.T) && mP1CharChangeDelay <= 0)
                {
                    mP1CharChangeDelay = charChangeDelay;

                    if (mP1NameIndexList [mP1NameInputNum] <= startUnicode) mP1NameIndexList [mP1NameInputNum] = endUnicode;
                    else mP1NameIndexList [mP1NameInputNum]--;

                    mP1NameInputTextList [mP1NameInputNum].text = ((char)mP1NameIndexList [mP1NameInputNum]).ToString();
                }
                else if (Input.GetKey(KeyCode.G) && mP1CharChangeDelay <= 0)
                {
                    mP1CharChangeDelay = charChangeDelay;

                    if (mP1NameIndexList [mP1NameInputNum] >= endUnicode) mP1NameIndexList [mP1NameInputNum] = startUnicode;
                    else mP1NameIndexList [mP1NameInputNum]++;

                    mP1NameInputTextList [mP1NameInputNum].text = ((char)mP1NameIndexList [mP1NameInputNum]).ToString();
                }
            }

            if (Input.GetKeyDown (KeyCode.F) || Input.GetKeyDown (KeyCode.H)) 
            {
                ResetInputNameAlpha (1);

                if (Input.GetKeyDown(KeyCode.F) && mP1NameInputNum > 0) mP1NameInputNum--;
                else if (Input.GetKeyDown(KeyCode.H) && mP1NameInputNum < totalCharNameInput) mP1NameInputNum++;
            }

            if (Input.GetKeyDown (KeyCode.Z) && mP1NameInputNum == 3)
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

                if (Input.GetKey (KeyCode.UpArrow) && mP2CharChangeDelay <= 0) 
                {
                    mP2CharChangeDelay = charChangeDelay;

                    if (mP2NameIndexList [mP2NameInputNum] <= startUnicode) mP2NameIndexList [mP2NameInputNum] = endUnicode;
                    else mP2NameIndexList [mP2NameInputNum]--;

                    mP2NameInputTextList [mP2NameInputNum].text = ((char)mP2NameIndexList [mP2NameInputNum]).ToString();
                }
                else if (Input.GetKey (KeyCode.DownArrow) && mP2CharChangeDelay <= 0) 
                {
                    mP2CharChangeDelay = charChangeDelay;

                    if (mP2NameIndexList [mP2NameInputNum] >= endUnicode) mP2NameIndexList [mP2NameInputNum] = startUnicode;
                    else mP2NameIndexList [mP2NameInputNum]++;

                    mP2NameInputTextList [mP2NameInputNum].text = ((char)mP2NameIndexList [mP2NameInputNum]).ToString();
                }
            }

            if (Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow)) 
            {
                ResetInputNameAlpha (2);

                if (Input.GetKeyDown(KeyCode.LeftArrow) && mP2NameInputNum > 0) mP2NameInputNum--;
                else if (Input.GetKeyDown(KeyCode.RightArrow) && mP2NameInputNum < totalCharNameInput) mP2NameInputNum++;   
            }

            if (Input.GetKeyDown (KeyCode.Comma) && mP2NameInputNum == 3)
            {
                ResetInputNameAlpha (2);
                mP2StopFade = true;
                AddToSavedName(2);
            }
        }

        if (mP1StopFade && mP2StopFade && !mIsWait) 
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
        if (Input.GetKeyDown(KeyCode.T) && mGameOverSelectIndex > 0)
        {
            mGameOverSelectIndex--;
            gameOverButton_UI.GetChild(mGameOverSelectIndex).GetComponent<Image>().color = Color.red;
            gameOverButton_UI.GetChild(mGameOverSelectIndex + 1).GetComponent<Image>().color = Color.white;
        }
        else if (Input.GetKeyDown(KeyCode.G) && mGameOverSelectIndex < maxGameOverButton - 1)
        {
            mGameOverSelectIndex++;
            gameOverButton_UI.GetChild(mGameOverSelectIndex).GetComponent<Image>().color = Color.red;
            gameOverButton_UI.GetChild(mGameOverSelectIndex - 1).GetComponent<Image>().color = Color.white;
        }

        if (Input.GetKeyDown(KeyCode.Z))
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

        if(duration == 0) timerUI.gameObject.SetActive(false);
    }

    void RestartLevel()
    {
        Time.timeScale = 1;
		SceneManager.LoadScene("DreamCarnage");
    }

    void ReturnToTitleScreen()
    {
        Time.timeScale = 1;
        MainMenuManager.sSingleton.SetToDefaultScene();
        SceneManager.LoadScene("MainMenu");
    }

    string GetScoreWithZero(string score)
    {
        int addZero = mPlayerUIList[0].score_UI.text.Length - score.Length;

        for (int i = 0; i < addZero; i++)
        { score = "0" + score; }

        return score;
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

    void SetupShowScore()
    {
        Time.timeScale = 0;
        state = State.SHOW_SCORE;

        gameOver_UI.gameObject.SetActive(true);
        showScore_UI.gameObject.SetActive(true);

        mTotalScore = mPlayerUIList[0].toReachScore + mPlayerUIList[1].toReachScore;
        cumulativeScoreText.text = GetScoreWithZero(mTotalScore.ToString());

        bool isRank = false;
        List<Text> totalScoreTextList = MainMenuManager.sSingleton.totalScoreTextList;

        for (int i = 0; i < MainMenuManager.sSingleton.maxNames; i++)
        {
            //            string name = "Score" + (i + 1);
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

    void SetupInputName()
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
        gameOverButton_UI.GetChild(mPauseSelectIndex).GetComponent<Image>().color = Color.red;
        gameOverButton_UI.gameObject.SetActive (true);
    }

    IEnumerator AddScoreSequence(int player)
    {
        PlayerInfo currPlayer = mPlayerUIList[player - 1];
        currPlayer.isCoroutine = true;

        while (currPlayer.currScore < currPlayer.toReachScore)
        {
			if (state != State.PAUSE)
            {
                currPlayer.currScore += (int)(1 * countUpScoreSpd);
                if (currPlayer.currScore > currPlayer.toReachScore) currPlayer.currScore = currPlayer.toReachScore;

                currPlayer.score_UI.text = GetScoreWithZero(currPlayer.currScore.ToString());
                yield return new WaitForEndOfFrame();
            }
            else yield return null;
        }
        currPlayer.isCoroutine = false;
    }

    IEnumerator WaitThenDo(float waitDur, Action doLast)
    {
        mIsWait = true;
        yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(waitDur));
        doLast();
        mIsWait = false;
    }

	IEnumerator FadeInOut(int playerIndex, Text text, float speed, float delay)
	{
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
	}
}
