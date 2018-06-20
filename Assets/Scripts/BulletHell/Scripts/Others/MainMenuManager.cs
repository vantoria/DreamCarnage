using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour 
{
    public static MainMenuManager sSingleton { get { return _sSingleton; } }
    static MainMenuManager _sSingleton;

    public Transform startScreenUI, highScore_UI, mainMenu_UI, option_UI, credit_UI, characterSelect_UI;

    // StartScreen UI
    public Text startScreenText;
    public float alphaChangeSpeed = 1;

    // HighScore
    public int scoreMaxDigit = 9;
    public int maxNames = 10;
    public Transform p1NamesTrans, p2NamesTrans, totalScoreTrans;
    public Image highScoreBackImage;

    public List<Text> p1NameTextList = new List<Text>();
    public List<Text> p2NameTextList = new List<Text>();
    public List<Text> totalScoreTextList = new List<Text>();

    // Character select
    public Transform p1SelectionPanel, p2SelectionPanel, p1SelectedPanel, p2SelectedPanel;
    public Text timerText, p1Text, p2TopText, p2Text, nowLoadingText;
    public float selectionSpeed = 1, timerCountDown = 60, timerSpeed = 1, p2AlphaChangeSpeed = 1, p2Delay;
    public List<Transform> characterPotraitList;
    public List<VideoClip> characterVideoList;
    public VideoPlayer video1, video2;
    public Color selectedColor;

    // Option
    public AudioSource bgmSource, sfxSource;
    public List<AudioClip> sfxList;
    public Slider BGM_Slider, SFX_Slider;
    public Text BGM_Text, SFX_Text;
    public float sliderDelay;
    public Image BGM_Knob, SFX_Knob, optionBackImg;

    // Credits
    public Image creditBackImg;

    public enum State
    {
        NONE = 0,
        START_SCREEN,
        MAIN_MENU,
        HIGH_SCORE,
        OPTION,
        CREDIT,
        CHARACTER_SELECT
    };
    public State state = State.NONE;

    int mMainMenuIndex = 0, mCharacterIndex = 0, mOptionIndex = 0, mPlayerNum = 1, mMaxPlayer = 1;
    float mDefaultSliderDelay, mDefaultTimerCountDown;
    bool mIsCoroutine = false, mIsFading = false;
    Color mDefaultCharSelectColor;
    string mSelectCharStr = "Selecting character...", mSelectedCharStr = "Selected", mWaitStr = "Please wait...", mP2JoinStr = "Press Start to join";

    List<int> mSelectedIndexList = new List<int>();
    List<Image> mMainMenuImageList = new List<Image>();
    List<Renderer> mCharSelectRendList = new List<Renderer>();

    IEnumerator mFadeCo;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;

        DontDestroyOnLoad(this.gameObject);
    }

	void Start () 
    {
        for (int i = 0; i < mainMenu_UI.childCount; i++) 
        {
            Image childImage = mainMenu_UI.GetChild (i).GetComponent<Image> ();
            mMainMenuImageList.Add (childImage);
        }

        for (int i = 0; i < characterPotraitList.Count; i++)
        {
            Renderer rend = characterPotraitList[i].GetComponent<Renderer>();
            mCharSelectRendList.Add(rend);
        }

        mDefaultTimerCountDown = timerCountDown;
        mDefaultSliderDelay = sliderDelay;
        sliderDelay = 0;
        mDefaultCharSelectColor = mCharSelectRendList[0].material.color;

        state = State.START_SCREEN;
        characterSelect_UI.gameObject.SetActive(false);

//         PlayerPrefs.DeleteAll();
        // Set the Text component into the respective List.
        for (int i = 0; i < maxNames; i++)
        {
            Transform p1Child = p1NamesTrans.GetChild(i);
            p1NameTextList.Add(p1Child.GetComponent<Text>());

            Transform p2Child = p2NamesTrans.GetChild(i);
            p2NameTextList.Add(p2Child.GetComponent<Text>());

            Transform totalScoreChild = totalScoreTrans.GetChild(i);
            totalScoreTextList.Add(totalScoreChild.GetComponent<Text>());

//            PlayerPrefs.SetString(p1Child.name + i, "AFK" + i);
//            PlayerPrefs.SetString(p2Child.name + i, "QWE" + i);
//            PlayerPrefs.SetInt(totalScoreChild.name + i, (10 - i));
        }
	}

    public List<int> GetSelectedIndexList { get { return mSelectedIndexList; } }
	
	void Update () 
    {
        if (state == State.START_SCREEN) HandleStartScreenState();
        else if (state == State.MAIN_MENU) HandleMainMenuState();
        else if (state == State.HIGH_SCORE) HandleHighScoreState();
        else if (state == State.CHARACTER_SELECT) HandleCharacterSelect();
        else if (state == State.OPTION) HandleOptionState();
        else if (state == State.CREDIT) HandleCredits();

        if (state == State.NONE)
        {
//            Debug.Log("AAA");
        }
	}

    public void SetToDefaultScene()
    {
        mMainMenuIndex = mCharacterIndex = mOptionIndex = 0;
        mPlayerNum = mMaxPlayer = 1;

        for (int i = 0; i < characterPotraitList.Count; i++)
        {
            mCharSelectRendList[i].material.color = mDefaultCharSelectColor;
        }

        p1Text.text = mSelectCharStr;
        p2Text.text = mP2JoinStr;

        p1SelectionPanel.position = characterPotraitList[0].position;
        p2SelectionPanel.position = characterPotraitList[0].position;

//        p1SelectionPanel.gameObject.SetActive(false);
        p2SelectionPanel.gameObject.SetActive(false);
        p1SelectedPanel.gameObject.SetActive(false);
        p2SelectedPanel.gameObject.SetActive(false);
        characterSelect_UI.gameObject.SetActive(false);
        nowLoadingText.enabled = false;

        ShowStartScreen();
        gameObject.SetActive(true);
    }

    // ----------------------------------------------------------------------------------------------------
    // ----------------------------------- Show different screen UI ---------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void ShowStartScreen()
    {
        state = State.START_SCREEN;
        startScreenUI.gameObject.SetActive (true);
    }

    void ShowMainMenu()
    {
        state = State.MAIN_MENU;
        mMainMenuIndex = 0;
        mainMenu_UI.gameObject.SetActive (true);
        mMainMenuImageList [mMainMenuIndex].color = Color.red;
    }

    void StartGame()
    {
        state = State.CHARACTER_SELECT;
        characterSelect_UI.gameObject.SetActive(true);

        video1.GetComponent<SpriteRenderer>().enabled = true;
        video2.GetComponent<SpriteRenderer>().enabled = true;
        video1.clip = characterVideoList[2];
        video2.clip = characterVideoList[3];
        video1.clip = characterVideoList[0];
        video2.clip = characterVideoList[1];
    }

    void ShowHighScore()
    {
        state = State.HIGH_SCORE;

        for (int i = 0; i < maxNames; i++)
        {
            Transform p1Child = p1NamesTrans.GetChild(i);
            p1NameTextList[i].text = PlayerPrefs.GetString(p1Child.name + i);

            Transform p2Child = p2NamesTrans.GetChild(i);
            p2NameTextList[i].text = PlayerPrefs.GetString(p2Child.name + i);

            Transform totalScoreChild = totalScoreTrans.GetChild(i);
            string val = PlayerPrefs.GetInt(totalScoreChild.name + i).ToString();
            totalScoreTextList[i].text = GetScoreWithZero(val);
        }

        highScore_UI.gameObject.SetActive(true);
        highScoreBackImage.color = Color.red;
    }

    void ShowCredits()
    {
        state = State.CREDIT;
        credit_UI.gameObject.SetActive(true);
        creditBackImg.color = Color.red;
    }

    void ShowOption()
    {
        state = State.OPTION;
        mOptionIndex = 0;
        option_UI.gameObject.SetActive (true);
        BGM_Knob.color = Color.red;
    }

    // ----------------------------------------------------------------------------------------------------
    // --------------------------------- Handle different screen UI ---------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void HandleStartScreenState()
    {
        if (!mIsFading)
        {
            mFadeCo = FadeInOut(startScreenText, alphaChangeSpeed, 0);
            StartCoroutine(mFadeCo);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            StopCoroutine(mFadeCo);
            mIsFading = false;

            Color color = startScreenText.color;
            color.a = 1;
            startScreenText.color = color;

            startScreenUI.gameObject.SetActive (false);

            state = State.MAIN_MENU;
            ShowMainMenu();
        }
    }

    void HandleMainMenuState()
    {
        if (Input.GetKeyDown(KeyCode.T) && mMainMenuIndex > 0)
        {
            PlayClickSfx();
            mMainMenuIndex--;
            mMainMenuImageList [mMainMenuIndex].color = Color.red;
            mMainMenuImageList [mMainMenuIndex + 1].color = Color.white;
        }
        else if (Input.GetKeyDown(KeyCode.G) && mMainMenuIndex < mMainMenuImageList.Count - 1)
        {
            PlayClickSfx();
            mMainMenuIndex++;
            mMainMenuImageList [mMainMenuIndex].color = Color.red;
            mMainMenuImageList [mMainMenuIndex - 1].color = Color.white;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayClickSfx();
            mainMenu_UI.gameObject.SetActive (false);
            mMainMenuImageList [mMainMenuIndex].color = Color.white;

            if (mMainMenuIndex == 0) StartGame();
            else if (mMainMenuIndex == 1) ShowHighScore();
            else if (mMainMenuIndex == 2) ShowOption();
            else if (mMainMenuIndex == 3) ShowCredits();
            else if (mMainMenuIndex == 4) Application.Quit();
        }
    }

    void HandleHighScoreState()
    {
        if (Input.GetKeyDown (KeyCode.Escape) || Input.GetKeyDown (KeyCode.Z)) 
        {
            PlayClickSfx();
            highScore_UI.gameObject.SetActive (false);
            ShowMainMenu ();
        }
    }

    void HandleOptionState()
    {
        if (Input.GetKeyDown(KeyCode.T) && mOptionIndex > 0)
        {
            PlayClickSfx();
            mOptionIndex--;
            if (mOptionIndex == 0) 
            {
                BGM_Knob.color = Color.red;
                SFX_Knob.color = Color.white;
            }
            else if (mOptionIndex == 1) 
            {
                SFX_Knob.color = Color.red;
                optionBackImg.color = Color.white;
            }
        }
        else if (Input.GetKeyDown(KeyCode.G) && mOptionIndex < 3)
        {
            PlayClickSfx();
            mOptionIndex++;
            if (mOptionIndex == 1) 
            {
                BGM_Knob.color = Color.white;
                SFX_Knob.color = Color.red;
            }
            else if (mOptionIndex == 2) 
            {
                SFX_Knob.color = Color.white;
                optionBackImg.color = Color.red;
            }
        }

        if (sliderDelay > 0)
        {
            sliderDelay -= Time.deltaTime;
            if (sliderDelay < 0) sliderDelay = 0;
        }

        if(Input.GetKey(KeyCode.F) && sliderDelay <= 0)
        {
            if (mOptionIndex == 0 && BGM_Slider.value > 0) OptionSliderControl (bgmSource, ref BGM_Slider, ref BGM_Text, true);
            else if (mOptionIndex == 1 && SFX_Slider.value > 0) OptionSliderControl (sfxSource, ref SFX_Slider, ref SFX_Text, true);
        }
        else if(Input.GetKey(KeyCode.H) && sliderDelay <= 0)
        {
            if (mOptionIndex == 0 && BGM_Slider.value < 1) OptionSliderControl (bgmSource,ref BGM_Slider, ref BGM_Text, false);
            else if (mOptionIndex == 1 && SFX_Slider.value < 1) OptionSliderControl (sfxSource, ref SFX_Slider, ref SFX_Text, false);
        }

        if (Input.GetKeyDown (KeyCode.Escape) || (mOptionIndex == 2 && Input.GetKeyDown (KeyCode.Z))) 
        {
            PlayClickSfx();
            BGM_Knob.color = Color.white;
            SFX_Knob.color = Color.white;
            optionBackImg.color = Color.white;

            mOptionIndex = 0;
            option_UI.gameObject.SetActive (false);
            ShowMainMenu ();
        }
    }

    void OptionSliderControl(AudioSource audio, ref Slider slider, ref Text text, bool isMinus)
    {
        PlayClickSfx();
        sliderDelay = mDefaultSliderDelay;

        float value = 0.05f;
        if (isMinus) value = -value;

        slider.value += value;
        if (slider.value < 0) slider.value = 0;
        else if (slider.value > 1) slider.value = 1;

        audio.volume = slider.value;

        int intVal = Mathf.CeilToInt(slider.value * 100);
        text.text = intVal.ToString();
    }

    void HandleCharacterSelect()
    {
        // Countdown timer.
        if (mPlayerNum <= mMaxPlayer)
        {
            timerCountDown -= Time.deltaTime * timerSpeed;
            timerText.text = ((int)timerCountDown).ToString();
        }

        // Fading in and out for player 2 to press Start.
        if (!mIsFading && mMaxPlayer != 2)
        {
            mFadeCo = FadeInOut(p2Text, p2AlphaChangeSpeed, p2Delay);
            StartCoroutine(mFadeCo);
        }

        // Selection for player 1 and 2.
        if (mPlayerNum <= mMaxPlayer)
        {
            if (((Input.GetKeyDown(KeyCode.F) && mPlayerNum == 1) || (Input.GetKeyDown(KeyCode.LeftArrow) && mPlayerNum == 2)) &&
                mCharacterIndex > 0 && !mIsCoroutine)
            {
                mCharacterIndex--;
                LeftRightCharacterSlect();
            }
            else if (((Input.GetKeyDown(KeyCode.H) && mPlayerNum == 1) || (Input.GetKeyDown(KeyCode.RightArrow) && mPlayerNum == 2)) &&
                     mCharacterIndex < characterPotraitList.Count - 1 && !mIsCoroutine)
            {
                mCharacterIndex++;
                LeftRightCharacterSlect();
            }
            if (((Input.GetKeyDown(KeyCode.Z) && mPlayerNum == 1) || (Input.GetKeyDown(KeyCode.Period) && mPlayerNum == 2)) && !mIsCoroutine)
            {
                if (mPlayerNum == 1 || mCharacterIndex != mSelectedIndexList[0])
                {
                    PlayClickSfx();
                    mSelectedIndexList.Add(mCharacterIndex);
                    mCharSelectRendList[mCharacterIndex].material.color = selectedColor;

                    if (mPlayerNum == 1)
                    {
                        Vector3 pos = p1SelectedPanel.position;
                        pos.x = characterPotraitList[mCharacterIndex].position.x;
                        p1SelectedPanel.position = pos;

                        p1SelectedPanel.gameObject.SetActive(true);
                        if (mMaxPlayer == 2)
                        {
                            mCharacterIndex = 0;
                            PlayTheVideo();
                            p2SelectionPanel.gameObject.SetActive(true);
                        }

                        p1Text.text = mSelectedCharStr;
						if (mMaxPlayer == 2) 
						{
							timerCountDown = mDefaultTimerCountDown;
                            p2Text.text = mSelectCharStr;
						}
                    }
                    else
                    {
                        Vector3 pos = p2SelectedPanel.position;
                        pos.x = characterPotraitList[mCharacterIndex].position.x;
                        p2SelectedPanel.position = pos;
                        p2SelectedPanel.gameObject.SetActive(true);

                        p2Text.text = mSelectedCharStr;
                    }
                    mPlayerNum++;
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                mMaxPlayer++;
                mIsFading = false;
                StopCoroutine(mFadeCo);

                Color color = p2Text.color;
                color.a = 1;
                p2Text.color = color;
                p2Text.text = mWaitStr;
            }
        }
        else
        {
            state = State.NONE;
            mIsFading = false;
            if (mFadeCo != null) StopCoroutine(mFadeCo);

            nowLoadingText.gameObject.SetActive(true);
            SceneManager.LoadSceneAsync("DreamCarnage");
        }
    }

    void LeftRightCharacterSlect()
    {
        PlayClickSfx();

        Transform selectPanel = null;
        if (mPlayerNum == 1) selectPanel = p1SelectionPanel;
        else selectPanel = p2SelectionPanel;

        StartCoroutine(MoveToPos(selectPanel, characterPotraitList[mCharacterIndex]));
        PlayTheVideo();
    }

    void PlayTheVideo()
    {
        video1.clip = characterVideoList[mCharacterIndex * 2];
        video2.clip = characterVideoList[mCharacterIndex * 2 + 1];
        video1.Play();
        video2.Play();
    }

    void HandleCredits()
    {
        if (Input.GetKeyDown (KeyCode.Escape) || Input.GetKeyDown (KeyCode.Z)) 
        {
            PlayClickSfx();
            credit_UI.gameObject.SetActive (false);
            ShowMainMenu ();
        }
    }

    string GetScoreWithZero(string score)
    {
        int addZero = scoreMaxDigit - score.Length;

        for (int i = 0; i < addZero; i++)
        { score = "0" + score; }

        return score;
    }

    IEnumerator MoveToPos(Transform panel, Transform targetTrans)
    {
        mIsCoroutine = true;
        while (panel.position != targetTrans.position)
        {
            panel.position = Vector3.MoveTowards(panel.position, targetTrans.position, selectionSpeed * Time.deltaTime);
            yield return null;
        }
        mIsCoroutine = false;
    }

    IEnumerator FadeInOut(Text text, float speed, float delay)
    {
        mIsFading = true;
        while (text.color.a > 0)
        {
            Color color = text.color;
			color.a -= Time.deltaTime * speed;
            if (color.a < 0) color.a = 0;
            text.color = color;

            yield return null;
        }

        while (text.color.a < 1)
        {
            Color color = text.color;
            color.a += Time.deltaTime * speed;
            if (color.a > 1) color.a = 1;
            text.color = color;

            yield return null;
        }
        yield return new WaitForSeconds(delay);
        mIsFading = false;
    }

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------------ BGM / SFX -----------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void PlayClickSfx()
    {
        sfxSource.PlayOneShot(sfxList[0]);
    }
}
