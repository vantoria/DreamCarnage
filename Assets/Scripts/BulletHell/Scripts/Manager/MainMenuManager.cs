using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager sSingleton { get { return _sSingleton; } }
    static MainMenuManager _sSingleton;

    public LevelController levelController;

    public Transform startScreenUI, highScore_UI, mainMenu_UI, option_UI, credit_UI, characterSelect_UI;
    public float fadeSpeed = 5, moveSpeed = 3.5f;

    // StartScreen UI
    public Text startScreenText;
    public float startStayDuration = 1;
    public float startUIStayDuration = 0.5f;
    public float alphaChangeSpeed = 1;
    public float alphaChangeSpeedAfterWait = 2;
    public Color selectedButtonColor;
    public Color unselectedButtonColor;

    // HighScore
    public int scoreMaxDigit = 9;
    public int maxNames = 10;
    public Transform engHighScore, jpHighScore, allFadeInHS, p1NamesTrans, p2NamesTrans, totalScoreTrans;

    public List<Text> p1NameTextList = new List<Text>();
    public List<Text> p2NameTextList = new List<Text>();
    public List<Text> totalScoreTextList = new List<Text>();

    // Character select
    public Transform charLeftArrows, charRightArrows, nowLoadingTrans;
    public Text timerText, p1Text, p2TopText, p2Text;
    public Text char1Name, char2Name, char3Name;
    public float selectionSpeed = 1, selectOffsetVal = 1, charAlphaChangeSpeed = 3, charSelectDelay = 0.5f, charArrowExConSpd = 2, charArrowExConScale = 0.1f;
    public float timerCountDown = 60, timerSpeed = 1, p2AlphaChangeSpeed = 1, p2Delay;
    public List<Transform> characterPotraitList;
//    public List<VideoClip> characterVideoList;
    public VideoPlayer char1Video1, char1Video2;
    public VideoPlayer char2Video1, char2Video2;
    public VideoPlayer char3Video1, char3Video2;
    public Color selectedColor;
    public SpriteRenderer avatar1SR, avatar2SR;

    // Option
    public Slider BGM_Slider, SFX_Slider;
    public Text BGM_Text, SFX_Text, p1Control, p2Control;
    public int maxControlType = 2, maxOptionButton = 4;
    public float sliderDelay;
    public Color activeColor;
    public Transform engTrans, jpTrans;
    public Image BGMButton, BGM_Knob, SFXButton, SFX_Knob, languageButton, engButton, jpButton, controlLayoutImg, p1ControlImg, p2ControlImg;
    public List<Sprite> controlSpriteList = new List<Sprite>();
    public Transform p1Arrows, p2Arrows, editBgParent;
    public Transform engOption, engP1Control, engP2Control, engP1EditButton, engP2EditButton, engP1OkButton, engP2OkButton;
    public Transform jpOption, jpP1Control, jpP2Control, jpP1EditButton, jpP2EditButton, jpP1OkButton, jpP2OkButton;

    // Credit
    public Transform engCredit, jpCredit, allFadeInCredit;

    // TextRotation
    public Transform optionTitle;
    public Transform highScoreTitle;
    public Transform creditsTitle;

    public float timeForRotation;
    public int setInitialAxis = 90;
    public int angleToRotate = 90;

    int currentAngle = 0;
    float mTimer = 0;

    public enum State
    {
        NONE = 0,
        START_SCREEN,
        MAIN_MENU,
        HIGH_SCORE,
        OPTION,
        CREDIT,
        CHARACTER_SELECT,
    };
    public State state = State.NONE;

    ParticleSystem mNowLoadingPS;

    Color mDefaultCharSelectColor;
    float mDefaultSliderDelay, mDefaultTimerCountDown;

    int mMainMenuIndex, mPrevCharIndex, mCharacterIndex, mOptionIndex, mPlayerNum = 1, mMaxPlayer = 1;
    string mSelectCharStr = "Selecting character...", mSelectedCharStr = "Selected", mWaitStr = "Please wait...", mP2JoinStr = "Press Start to join";
    bool mIsCoroutine = false, mIsFading = false, mIsAllChildFading = false, mIsP2Selected = false, mIsWaitForP2Reset;

    // Controls for option controller change.
    float mVerticalP1, mVerticalP1Dpad, mHorizontalP1, mHorizontalP1Dpad, mHorizontalP2, mHorizontalP2Dpad;
    bool mIsP1VerticalAxisUsed = false, mIsP1HorizontalAxisUsed = false, mIsP2HorizontalAxisUsed = false, mIsP1KeybInput = false;
    bool mIsP1Edit, mIsP2Edit;

    List<int> mSelectedIndexList = new List<int>();
    List<Image> mMainMenuImageList = new List<Image>();
    List<Renderer> mCharSelectRendList = new List<Renderer>();

    Text mLanguageText, mControllerLayoutText;
    int mP1SelectedControl, mP2SelectedControl, mP1SavedControl, mP2SavedControl;
    Vector3 mFrontalVid1Pos, mFrontalVid2Pos, mBackVid1Pos, mBackVid2Pos, mDefaultNamePos;

    bool mIsPressedStart, mIsStartEnded, mIsP1ArrowFade, mIsP2ArrowFade;
    IEnumerator mFadeCo;
    List<IEnumerator> mCharFadeCoList = new List<IEnumerator>();


    enum Language
    {
        ENGLISH = 0,
        JAPANESE
    }
    Language language = Language.ENGLISH;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;

        DontDestroyOnLoad(this.gameObject);
    }

    void Start () 
    {
        mIsP1KeybInput = JoystickManager.sSingleton.IsP1KeybInput;

        for (int i = 0; i < mainMenu_UI.childCount; i++) 
        {
            Image childImage = mainMenu_UI.GetChild (i).GetComponent<Image> ();
            if (childImage != null) mMainMenuImageList.Add(childImage);
        }

        for (int i = 0; i < characterPotraitList.Count; i++)
        {
            Renderer rend = characterPotraitList[i].GetComponentInChildren<Renderer>();
            mCharSelectRendList.Add(rend);
        }

        mDefaultTimerCountDown = timerCountDown;
        mDefaultSliderDelay = sliderDelay;
        sliderDelay = 0;
        mDefaultCharSelectColor = mCharSelectRendList[0].material.color;
        mDefaultNamePos = char1Name.transform.position;

        if (state == State.NONE)
        {
            state = State.START_SCREEN;
            characterSelect_UI.gameObject.SetActive(false);
        }

        mNowLoadingPS = nowLoadingTrans.GetChild(1).GetComponent<ParticleSystem>();

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

        mFrontalVid1Pos = char1Video1.transform.position;
        mFrontalVid2Pos = char1Video2.transform.position;

        mBackVid1Pos = char2Video1.transform.position;
        mBackVid2Pos = char2Video2.transform.position;

        BGM_Slider.value = AudioManager.sSingleton.bgmSource.volume;
        SFX_Slider.value = AudioManager.sSingleton.sfxSource.volume;

        int intVal = Mathf.FloorToInt(BGM_Slider.value * 100);
        BGM_Text.text = intVal.ToString();

        intVal = Mathf.FloorToInt(SFX_Slider.value * 100);
        SFX_Text.text = intVal.ToString();

        mLanguageText = languageButton.GetComponentInChildren<Text>();
        mControllerLayoutText = controlLayoutImg.GetComponentInChildren<Text>();
    }

    public List<int> GetSelectedIndexList { get { return mSelectedIndexList; } }

    void Update () 
    {
        mVerticalP1 = Input.GetAxis("VerticalP1");
        mVerticalP1Dpad = Input.GetAxis("VerticalP1Dpad");
        mHorizontalP1 = Input.GetAxis("HorizontalP1");
        mHorizontalP1Dpad = Input.GetAxis("HorizontalP1Dpad");

        mHorizontalP2 = Input.GetAxis("HorizontalP2");
        mHorizontalP2Dpad = Input.GetAxis("HorizontalP2Dpad");

        if (mVerticalP1 == 0 && mVerticalP1Dpad == 0) mIsP1VerticalAxisUsed = false;
        if (mHorizontalP1 == 0 && mHorizontalP1Dpad == 0) mIsP1HorizontalAxisUsed = false;

        if (mHorizontalP2 == 0 && mHorizontalP1Dpad == 0) mIsP2HorizontalAxisUsed = false;

        if (state == State.START_SCREEN) HandleStartScreenState();
        else if (state == State.MAIN_MENU) HandleMainMenuState();
        else if (state == State.HIGH_SCORE) HandleHighScoreState();
        else if (state == State.CHARACTER_SELECT) HandleCharacterSelect();
        else if (state == State.OPTION) HandleOptionState();
        else if (state == State.CREDIT) HandleCredits();
    }

    public void SetToDefaultScene()
    {
//        AudioManager.sSingleton.StopBGM();
        AudioManager.sSingleton.ResetFadeIEnumerator();
        AudioManager.sSingleton.SetBGM_Volume(1);
        AudioManager.sSingleton.PlayMainMenuBGM();

        mSelectedIndexList.Clear ();
        mIsStartEnded = false;
        mIsP2Selected = false;
        mMainMenuIndex = mCharacterIndex = mOptionIndex = 0;
        mPlayerNum = mMaxPlayer = 1;

        SetDarkerColor(avatar1SR, 1);
        SetDarkerColor(avatar2SR, 1);

        mPrevCharIndex = 0;
        mCharacterIndex = 0;
        PlayTheVideo(mCharacterIndex);

        for (int i = 0; i < characterPotraitList.Count; i++)
        {
            SetSR_Alpha(characterPotraitList[i].GetComponentInChildren<SpriteRenderer>(), 0);
        }

        SetText_Alpha(char1Name, 0);
        SetText_Alpha(char2Name, 0);
        SetText_Alpha(char3Name, 0);

        for (int i = 0; i < characterPotraitList.Count; i++)
        {
            mCharSelectRendList[i].material.color = mDefaultCharSelectColor;
        }

        p1Text.text = mSelectCharStr;
        p2Text.text = mP2JoinStr;

        characterSelect_UI.gameObject.SetActive(false);
        avatar2SR.gameObject.SetActive(false);

        char1Video1.GetComponent<SpriteRenderer>().enabled = false;
        char1Video2.GetComponent<SpriteRenderer>().enabled = false;
        char2Video1.GetComponent<SpriteRenderer>().enabled = false;
        char2Video2.GetComponent<SpriteRenderer>().enabled = false;
        char3Video1.GetComponent<SpriteRenderer>().enabled = false;
        char3Video2.GetComponent<SpriteRenderer>().enabled = false;

        ShowStartScreen();
        gameObject.SetActive(true);
    }

    public int GetFirstRankTotalScore()
    {
        return PlayerPrefs.GetInt(totalScoreTrans.GetChild(0).name + 0);
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
        mMainMenuImageList [mMainMenuIndex].color = selectedButtonColor;

        engTrans.gameObject.SetActive(false);
        jpTrans.gameObject.SetActive(false);
    }

    void StartGame()
    {
        state = State.CHARACTER_SELECT;
        characterSelect_UI.gameObject.SetActive(true);

        StartCoroutine(FadeAllChildImageOutIn(characterPotraitList[0], charAlphaChangeSpeed, 0, false, true, () => {}, true ));

        char1Video1.GetComponent<SpriteRenderer>().enabled = true;
        char1Video2.GetComponent<SpriteRenderer>().enabled = true;
        char2Video1.GetComponent<SpriteRenderer>().enabled = true;
        char2Video2.GetComponent<SpriteRenderer>().enabled = true;
        char3Video1.GetComponent<SpriteRenderer>().enabled = true;
        char3Video2.GetComponent<SpriteRenderer>().enabled = true;

        char1Video1.Play();
        char1Video2.Play();

        char2Video1.Play();
        char2Video2.Play();
        char2Video1.Pause();
        char2Video2.Pause();

        char3Video1.Play();
        char3Video2.Play();
        char3Video1.Pause();
        char3Video2.Pause();

        StartCoroutine(FadeHorizontalSlideIn(char1Name.transform, fadeSpeed, moveSpeed, -0.5f));
    }

    void ShowHighScore()
    {
        state = State.HIGH_SCORE;
        InitializeLanguage(state);

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
        SetInitialZAxis(highScoreTitle);
        StartCoroutine(RotateText(highScoreTitle));
        SetChildrendsTextAlphaZero_FadeSlideIn(allFadeInHS);
    }

    void ShowCredits()
    {
        state = State.CREDIT;
        InitializeLanguage(state);
        credit_UI.gameObject.SetActive(true);

        SetInitialZAxis(creditsTitle);
        StartCoroutine(RotateText(creditsTitle));
        SetChildrendsTextAlphaZero_FadeSlideIn(allFadeInCredit);
    }

    void ShowOption()
    {
        state = State.OPTION;
        InitializeLanguage(state);
        mOptionIndex = 0;
        option_UI.gameObject.SetActive (true);
        BGMButton.color = selectedButtonColor;

        mP1SavedControl = mP1SelectedControl;
        mP2SavedControl = mP2SelectedControl;

        if (language == Language.ENGLISH) engOption.gameObject.SetActive(true);
        else if (language == Language.JAPANESE) jpOption.gameObject.SetActive(true);

        SetInitialZAxis(optionTitle);
        StartCoroutine(RotateText(optionTitle));
    }

    void SetChildrendsTextAlphaZero_FadeSlideIn(Transform trans)
    {
        Text[] textArray = trans.GetComponentsInChildren<Text>();
        for (int i = 0; i < textArray.Length; i++)
        {
            SetText_Alpha(textArray[i], 0);
        }

        for (int i = 0; i < textArray.Length; i++)
        {
            StartCoroutine(FadeHorizontalSlideIn(textArray[i].transform, fadeSpeed, moveSpeed, -0.5f));
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // --------------------------------- Handle different screen UI ---------------------------------------
    // ----------------------------------------------------------------------------------------------------

    void HandleStartScreenState()
    {
        if (mIsStartEnded) return;

        if (!mIsFading && !mIsPressedStart)
        {
            mFadeCo = FadeTextInOut(startScreenText, alphaChangeSpeed, 0, () => {});
            StartCoroutine(mFadeCo);
        }
        else if (!mIsFading && mIsPressedStart)
        {
            mFadeCo = WaitThenFadeOut(startScreenText, alphaChangeSpeedAfterWait, startUIStayDuration, () => { mIsPressedStart = false; });
            StartCoroutine(mFadeCo);
            mIsStartEnded = true;
        }

        if ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.Return)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.startKey))
        {
            if (!mIsCoroutine)
            {
                PlayStartSfx();
                mIsPressedStart = true;
                mIsFading = false;
                StopCoroutine(mFadeCo);

                Color color = startScreenText.color;
                color.a = 1;
                startScreenText.color = color;

                StartCoroutine(ShowFromStartToMainMenu(startStayDuration));
            }
        }
    }

    void HandleMainMenuState()
    {
        if (((mIsP1KeybInput && Input.GetKeyDown(KeyCode.T)) || mVerticalP1 > 0 || mVerticalP1Dpad < 0) && 
            mMainMenuIndex > 0 && !mIsP1VerticalAxisUsed)
        {
            mIsP1VerticalAxisUsed = true;
            PlayMoveSfx();
            mMainMenuIndex--;
            mMainMenuImageList [mMainMenuIndex].color = selectedButtonColor;
            mMainMenuImageList [mMainMenuIndex + 1].color = unselectedButtonColor;
        }
        else if (((mIsP1KeybInput && Input.GetKeyDown(KeyCode.G)) || mVerticalP1 < 0 || mVerticalP1Dpad > 0) && 
            mMainMenuIndex < mMainMenuImageList.Count - 1 && !mIsP1VerticalAxisUsed)
        {
            mIsP1VerticalAxisUsed = true;
            PlayMoveSfx();
            mMainMenuIndex++;
            mMainMenuImageList [mMainMenuIndex].color = selectedButtonColor;
            mMainMenuImageList [mMainMenuIndex - 1].color = unselectedButtonColor;
        }

        if ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.Z)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.acceptKey))
        {
            PlayAcceptSfx();
            mainMenu_UI.gameObject.SetActive (false);
            mMainMenuImageList [mMainMenuIndex].color = unselectedButtonColor;

            if (mMainMenuIndex == 0) StartGame();
            else if (mMainMenuIndex == 1) ShowHighScore();
            else if (mMainMenuIndex == 2) ShowOption();
            else if (mMainMenuIndex == 3) ShowCredits();
            else if (mMainMenuIndex == 4) Application.Quit();
        }
    }

    void HandleHighScoreState()
    {
        if ((mIsP1KeybInput && (Input.GetKeyDown (KeyCode.Escape))) ||
            Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.backKey)) 
        {
            PlayBackSfx();
            highScore_UI.gameObject.SetActive (false);
            engHighScore.gameObject.SetActive (false);
            jpHighScore.gameObject.SetActive (false);
            ShowMainMenu ();
        }

    }

    void HandleOptionState()
    {
        if (!mIsP1Edit && !mIsP2Edit) 
        {
            if (((mIsP1KeybInput && Input.GetKeyDown(KeyCode.T)) || mVerticalP1 > 0 || mVerticalP1Dpad < 0) && 
                mOptionIndex > 0 && !mIsP1VerticalAxisUsed && !mIsP1HorizontalAxisUsed)
            {
                mIsP1VerticalAxisUsed = true;
                PlayMoveSfx();
                mOptionIndex--;
                if (mOptionIndex == 0) 
                {
                    BGMButton.color = selectedButtonColor;
                    SFXButton.color = unselectedButtonColor;
                }
                else if (mOptionIndex == 1) 
                {
                    SFXButton.color = selectedButtonColor;
                    languageButton.color = unselectedButtonColor;
                }
                else if (mOptionIndex == 2) 
                {
                    languageButton.color = selectedButtonColor;
                    controlLayoutImg.color = unselectedButtonColor;
                }
            }
            else if (((mIsP1KeybInput && Input.GetKeyDown(KeyCode.G)) || mVerticalP1 < 0 || mVerticalP1Dpad > 0) && 
                mOptionIndex < maxOptionButton - 1 && !mIsP1VerticalAxisUsed && !mIsP1HorizontalAxisUsed)
            {
                mIsP1VerticalAxisUsed = true;
                PlayMoveSfx();
                mOptionIndex++;
                if (mOptionIndex == 1)
                {
                    BGMButton.color = unselectedButtonColor;
                    SFXButton.color = selectedButtonColor;
                }
                else if (mOptionIndex == 2)
                {
                    SFXButton.color = unselectedButtonColor;
                    languageButton.color = selectedButtonColor;
                }
                else if (mOptionIndex == 3)
                {
                    languageButton.color = unselectedButtonColor;
                    controlLayoutImg.color = selectedButtonColor;
                }
            }

            if (sliderDelay > 0)
            {
                sliderDelay -= Time.deltaTime;
                if (sliderDelay < 0) sliderDelay = 0;
            }

            if(((mIsP1KeybInput && Input.GetKey(KeyCode.F)) || mHorizontalP1 < 0 || mHorizontalP1Dpad < 0) && sliderDelay <= 0)
            {
                if (mOptionIndex == 0 && BGM_Slider.value > 0) OptionSliderControl (AudioManager.sSingleton.bgmSource, ref BGM_Slider, ref BGM_Text, true);
                else if (mOptionIndex == 1 && SFX_Slider.value > 0) OptionSliderControl (AudioManager.sSingleton.sfxSource, ref SFX_Slider, ref SFX_Text, true);
            }
            else if(((mIsP1KeybInput && Input.GetKey(KeyCode.H)) || mHorizontalP1 > 0 || mHorizontalP1Dpad > 0) && sliderDelay <= 0)
            {
                if (mOptionIndex == 0 && BGM_Slider.value < 1) OptionSliderControl (AudioManager.sSingleton.bgmSource, ref BGM_Slider, ref BGM_Text, false);
                else if (mOptionIndex == 1 && SFX_Slider.value < 1) OptionSliderControl (AudioManager.sSingleton.sfxSource, ref SFX_Slider, ref SFX_Text, false);
            }

            // Buttons at the bottom of screen.
            if ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.Escape)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.backKey))
            {
                PlayBackSfx();

                if (mP1SavedControl != mP1SelectedControl) JoystickManager.sSingleton.SaveControlLayout(1, mP1SelectedControl);
                if (mP2SavedControl != mP2SelectedControl) JoystickManager.sSingleton.SaveControlLayout(2, mP2SelectedControl);

                ResetEditControl();
                engOption.gameObject.SetActive(false);
                jpOption.gameObject.SetActive(false);
                option_UI.gameObject.SetActive(false);

                SFXButton.color = unselectedButtonColor;
                languageButton.color = unselectedButtonColor;
                controlLayoutImg.color = unselectedButtonColor;

                mOptionIndex = 0;
                ShowMainMenu();
            }
        }

        // Handle controller type change.
        if (mOptionIndex == 2)
        {
            ResetEditControl();
            if (language != Language.ENGLISH && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.F)) || mHorizontalP1 < 0 || mHorizontalP1Dpad < 0))
            {
                language = Language.ENGLISH;
                engButton.color = activeColor;
                jpButton.color = BGMButton.color;

                engTrans.gameObject.SetActive(true);
                engOption.gameObject.SetActive(true);
                jpTrans.gameObject.SetActive(false);

                engP1Control.GetChild (mP1SelectedControl).gameObject.SetActive (true);
                engP2Control.GetChild (mP2SelectedControl).gameObject.SetActive (true);

                mLanguageText.text = "Language";
                mControllerLayoutText.text = "Controller Layout";

                AudioManager.sSingleton.PlayMainMenuMoveSfx();
            }
            else if (language != Language.JAPANESE && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.H)) || mHorizontalP1 > 0 || mHorizontalP1Dpad > 0))
            {
                language = Language.JAPANESE;
                jpButton.color = activeColor;
                engButton.color = BGMButton.color;

                jpOption.gameObject.SetActive(true);

                jpTrans.gameObject.SetActive(true);
                engTrans.gameObject.SetActive(false);

                jpP1Control.GetChild (mP1SelectedControl).gameObject.SetActive (true);
                jpP2Control.GetChild (mP2SelectedControl).gameObject.SetActive (true);

                mLanguageText.text = "言語";
                mControllerLayoutText.text = "ジョイスティック";

                AudioManager.sSingleton.PlayMainMenuMoveSfx();
            }
        }
        else if (mOptionIndex == 3)
        {
            if (!editBgParent.gameObject.activeSelf)
            {
                editBgParent.gameObject.SetActive(true);
                if (language == Language.ENGLISH)
                {
                    engP1EditButton.gameObject.SetActive(true);
                    engP2EditButton.gameObject.SetActive(true);
                }
                else if (language == Language.JAPANESE)
                {
                    jpP1EditButton.gameObject.SetActive(true);
                    jpP2EditButton.gameObject.SetActive(true);
                }
            }

            // Player 1.
            if (!mIsP1Edit && ((mIsP1KeybInput && Input.GetKeyDown (KeyCode.Z)) || Input.GetKeyDown (JoystickManager.sSingleton.p1_joystick.acceptKey)))
            {
                mIsP1Edit = true;
                p1Arrows.gameObject.SetActive (true);

                if (language == Language.ENGLISH)
                {
                    engP1EditButton.gameObject.SetActive(false);
                    engP1OkButton.gameObject.SetActive(true);
                }
                else if (language == Language.JAPANESE)
                {
                    jpP1EditButton.gameObject.SetActive(false);
                    jpP1OkButton.gameObject.SetActive(true);
                }
            }
            else if (mIsP1Edit && !mIsP1HorizontalAxisUsed) 
            {
                if (!mIsP1ArrowFade)
                {
                    mIsP1ArrowFade = true;
                    if (!mIsAllChildFading) FadeAllChildImageOutIn(p1Arrows, alphaChangeSpeed, 0, true, true, () => { mIsP1ArrowFade = false; });
                }

                if ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.F)) || mHorizontalP1 < 0 || mHorizontalP1Dpad < 0)
                {
                    mIsP1HorizontalAxisUsed = true;
                    if (mP1SelectedControl - 1 >= 0)
                    {
                        mP1SelectedControl--;
                        ChangeControlName(mP1SelectedControl, p1Control);
                    }
                }
                else if (((mIsP1KeybInput && Input.GetKey(KeyCode.H)) || mHorizontalP1 > 0 || mHorizontalP1Dpad > 0))
                {
                    mIsP1HorizontalAxisUsed = true;
                    if (mP1SelectedControl + 1 < maxControlType)
                    {
                        mP1SelectedControl++;
                        ChangeControlName(mP1SelectedControl, p1Control);
                    }
                }
                p1ControlImg.sprite = controlSpriteList[mP1SelectedControl];

                if ( ((mIsP1KeybInput && Input.GetKeyDown (KeyCode.Z)) || Input.GetKeyDown (JoystickManager.sSingleton.p1_joystick.acceptKey)) ||
                    ((mIsP1KeybInput && Input.GetKeyDown (KeyCode.X)) || Input.GetKeyDown (JoystickManager.sSingleton.p1_joystick.backKey)) )
                {
                    mIsP1Edit = false;
                    p1Arrows.gameObject.SetActive (false);

                    if (language == Language.ENGLISH) engP1EditButton.gameObject.SetActive(true);
                    else if (language == Language.JAPANESE) jpP1EditButton.gameObject.SetActive(true);

                    engP1OkButton.gameObject.SetActive(false);
                    jpP1OkButton.gameObject.SetActive(false);

                    if ((mIsP1KeybInput && Input.GetKeyDown (KeyCode.X)) || Input.GetKeyDown (JoystickManager.sSingleton.p1_joystick.backKey))
                    {
                        mP1SelectedControl = mP1SavedControl;
                        ChangeControlName(mP1SelectedControl, p1Control);
                    }
                }

                // Change the UI for English / Japanese.
                if (language == Language.ENGLISH) 
                {
                    SetAllChildToActive (engP1Control, false);
                    engP1Control.GetChild (mP1SelectedControl).gameObject.SetActive (true);
                }
                else if (language == Language.JAPANESE)
                {
                    SetAllChildToActive(jpP1Control, false);
                    jpP1Control.GetChild(mP1SelectedControl).gameObject.SetActive(true);
                }
            }

            // Player 2.
            if (!mIsP2Edit && (Input.GetKeyDown (KeyCode.Period) || Input.GetKeyDown (JoystickManager.sSingleton.p2_joystick.acceptKey)))
            {
                mIsP2Edit = true;
                p2Arrows.gameObject.SetActive (true);

                if (language == Language.ENGLISH)
                {
                    engP2EditButton.gameObject.SetActive(false);
                    engP2OkButton.gameObject.SetActive(true);
                }
                else if (language == Language.JAPANESE)
                {
                    jpP2EditButton.gameObject.SetActive(false);
                    jpP2OkButton.gameObject.SetActive(true);
                }
            }
            else if (mIsP2Edit && !mIsP2HorizontalAxisUsed)
            {
                if (!mIsP2ArrowFade)
                {
                    mIsP2ArrowFade = true;
                    if (!mIsAllChildFading) FadeAllChildImageOutIn(p1Arrows, alphaChangeSpeed, 0, true, true, () => { mIsP2ArrowFade = false; });
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || mHorizontalP2 < 0 || mHorizontalP2Dpad < 0)
                {
                    mIsP2HorizontalAxisUsed = true;
                    if (mP2SelectedControl - 1 >= 0)
                    {
                        mP2SelectedControl--;
                        ChangeControlName(mP2SelectedControl, p2Control);
                    }
                }
                else if (Input.GetKey(KeyCode.RightArrow) || mHorizontalP2 > 0 || mHorizontalP2Dpad > 0)
                {
                    mIsP2HorizontalAxisUsed = true;
                    if (mP2SelectedControl + 1 < maxControlType)
                    {
                        mP2SelectedControl++;
                        ChangeControlName(mP2SelectedControl, p2Control);
                    }
                }
                p2ControlImg.sprite = controlSpriteList[mP2SelectedControl];

                if ( (Input.GetKeyDown (KeyCode.Period) || Input.GetKeyDown (JoystickManager.sSingleton.p2_joystick.acceptKey)) ||
                    (Input.GetKeyDown (KeyCode.Slash) || Input.GetKeyDown (JoystickManager.sSingleton.p2_joystick.backKey)) )
                {
                    mIsP2Edit = false;
                    p2Arrows.gameObject.SetActive (false);

                    if (language == Language.ENGLISH) engP2EditButton.gameObject.SetActive(true);
                    else if (language == Language.JAPANESE) jpP2EditButton.gameObject.SetActive(true);

                    engP2OkButton.gameObject.SetActive(false);
                    jpP2OkButton.gameObject.SetActive(false);

                    if (Input.GetKeyDown(KeyCode.Slash) || Input.GetKeyDown(JoystickManager.sSingleton.p2_joystick.backKey))
                    {
                        mP2SelectedControl = mP2SavedControl;
                        ChangeControlName(mP2SelectedControl, p2Control);
                    }
                }

                // Change the UI for English / Japanese.
                if (language == Language.ENGLISH) 
                {
                    SetAllChildToActive(engP2Control, false);
                    engP2Control.GetChild(mP2SelectedControl).gameObject.SetActive(true);
                }
                else if (language == Language.JAPANESE)
                {
                    SetAllChildToActive(jpP2Control, false);
                    jpP2Control.GetChild(mP2SelectedControl).gameObject.SetActive(true);
                }
            }
        }
    }

    void InitializeLanguage(State state)
    {
        if (language == Language.ENGLISH)
        {
            engTrans.gameObject.SetActive(true);
            jpTrans.gameObject.SetActive(false);

            if (state == State.HIGH_SCORE) engHighScore.gameObject.SetActive(true);
            else if (state == State.OPTION) engOption.gameObject.SetActive(true);
            else if (state == State.CREDIT) engCredit.gameObject.SetActive(true);
        }
        else if (language == Language.JAPANESE)
        {
            engTrans.gameObject.SetActive(false);
            jpTrans.gameObject.SetActive(true);

            if (state == State.HIGH_SCORE) jpHighScore.gameObject.SetActive(true);
            else if (state == State.OPTION) jpOption.gameObject.SetActive(true);
            else if (state == State.CREDIT) jpCredit.gameObject.SetActive(true);
        }
    }

    void SetAllChildToActive(Transform trans, bool isActive)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            trans.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    void ChangeControlName(int index, Text text)
    {
        if (index == 0) text.text = "Control A";
        else if (index == 1) text.text = "Control B";
    }

    void ResetEditControl()
    {
        p1Arrows.gameObject.SetActive (false);
        p2Arrows.gameObject.SetActive (false);

        editBgParent.gameObject.SetActive(false);

        if (language == Language.ENGLISH)
        {
            engP1EditButton.gameObject.SetActive(false);
            engP2EditButton.gameObject.SetActive(false);
        }
        else if (language == Language.JAPANESE)
        {
            jpP1EditButton.gameObject.SetActive (false);
            jpP2EditButton.gameObject.SetActive (false);
        }
    }

    void OptionSliderControl(AudioSource audio, ref Slider slider, ref Text text, bool isMinus)
    {
        mIsP1HorizontalAxisUsed = true;
        PlayMoveSfx();
        sliderDelay = mDefaultSliderDelay;

        float value = 0.05f;
        if (isMinus) value = -value;

        slider.value += value;
        if (slider.value < 0) slider.value = 0;
        else if (slider.value > 1) slider.value = 1;

        audio.volume = slider.value;

        int intVal = 0;
        if (isMinus) intVal = (int)Mathf.Round(slider.value * 100);
        else intVal = (int)Mathf.Round(slider.value * 100);

        text.text = intVal.ToString();
    }

    void HandleCharacterSelect()
    {
        // Countdown timer.
        if (mPlayerNum <= mMaxPlayer)
        {
            timerCountDown -= Time.deltaTime * timerSpeed;
            if (timerCountDown <= 0) timerCountDown = 0;
            timerText.text = ((int)timerCountDown).ToString();
        }

        // Fading in and out for player 2 to press Start.
        if (!mIsFading && mMaxPlayer != 2)
        {
            mFadeCo = FadeTextInOut(p2Text, p2AlphaChangeSpeed, p2Delay, () => {});
            StartCoroutine(mFadeCo);
        }

        if (mIsWaitForP2Reset) return;

        // Selection for player 1 and 2.
        if (mPlayerNum <= mMaxPlayer)
        {
            if (sliderDelay > 0)
            {
                sliderDelay -= Time.deltaTime;
                if (sliderDelay < 0) sliderDelay = 0;
            }

            if (((mPlayerNum == 1 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.F)) || mHorizontalP1 < 0 || mHorizontalP1Dpad < 0)) || 
                (mPlayerNum == 2 && (Input.GetKeyDown(KeyCode.LeftArrow) || mHorizontalP2 < 0 || mHorizontalP2Dpad < 0))) &&
                sliderDelay <= 0 && !mIsCoroutine)
            {
                LeftRightCharacterSlect("Left");
            }
            else if (((mPlayerNum == 1 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.H)) || mHorizontalP1 > 0 || mHorizontalP1Dpad > 0)) || 
                (mPlayerNum == 2 && (Input.GetKeyDown(KeyCode.RightArrow) || mHorizontalP2 > 0 || mHorizontalP2Dpad > 0))) &&
                sliderDelay <= 0 && !mIsCoroutine)
            {
                LeftRightCharacterSlect("Right");
            }
            if ( (((mPlayerNum == 1 && ((mIsP1KeybInput && Input.GetKeyDown(KeyCode.Z)) || Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.acceptKey))) || 
                (mPlayerNum == 2 && (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(JoystickManager.sSingleton.p2_joystick.acceptKey))) ) || 
                timerCountDown <= 0) && !mIsCoroutine)
            {
                if (mPlayerNum == 1 || mCharacterIndex != mSelectedIndexList[0])
                {
                    PlayAcceptSfx();
                    mSelectedIndexList.Add(mCharacterIndex);
                    mCharSelectRendList[mCharacterIndex].material.color = selectedColor;

                    SetDarkerColor(avatar1SR, 0.588f);

                    if (mPlayerNum == 1)
                    {
                        if (mMaxPlayer == 2)
                        {
                            mIsWaitForP2Reset = true;
                            StartCoroutine(WaitFor(1f, ResetSelectionForP2));

                            // The text change after selecting.
                            p1Text.text = mSelectedCharStr;
                            timerCountDown = mDefaultTimerCountDown;
                        }
                    }
                    else
                    {
                        SetDarkerColor(avatar2SR, 0.588f);
                        p2Text.text = mSelectedCharStr;
                    }
                    mPlayerNum++;
                }

                if (timerCountDown <= 0 && mPlayerNum == 2 && mMaxPlayer == 2 && mCharacterIndex == mSelectedIndexList[0])
                {
                    if (mCharacterIndex - 1 <= -1) mCharacterIndex += 1;
                    else if (mCharacterIndex + 1 >= characterPotraitList.Count) mCharacterIndex -= 1;
                    else mCharacterIndex += 1;
                }
            }

            if (!mIsP2Selected && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(JoystickManager.sSingleton.p2_joystick.startKey)))
            {
                mMaxPlayer++;
                mIsP2Selected = true;
                mIsFading = false;
                StopCoroutine(mFadeCo);

                Color color = p2Text.color;
                color.a = 1;
                p2Text.color = color;
                p2Text.text = mWaitStr;

                avatar2SR.gameObject.SetActive(true);
                AudioManager.sSingleton.PlayMainMenuAccept2Sfx();
            }
        }
        else
        {
            state = State.NONE;
            mIsFading = false;
            if (mFadeCo != null) StopCoroutine(mFadeCo);

            nowLoadingTrans.gameObject.SetActive(true);
            mNowLoadingPS.Play();

            AudioManager.sSingleton.FadeOutBGM();
            StartCoroutine(levelController.Fading("DreamCarnage"));
        }
    }

    void LeftRightCharacterSlect(string dirStr)
    {
        sliderDelay = charSelectDelay;
        mPrevCharIndex = mCharacterIndex;

        if (mIsAllChildFading)
        {
            StopAllCharCoroutine();

            SpriteRenderer sr = characterPotraitList[mCharacterIndex].GetComponentInChildren<SpriteRenderer>();
            SetSR_Alpha(sr, 0);
            ResetAllCharPosition();
        }

        IEnumerator ie = null;
        if (dirStr == "Left")
        {
            mCharacterIndex--;
            if (mCharacterIndex < 0) mCharacterIndex = characterPotraitList.Count - 1;

            // Arrow expand and contract.
            StartCoroutine(ExpandContract(charLeftArrows, charArrowExConSpd, charArrowExConScale));

            // Movement of character image to the left.
            ie = OffSetXFromPosition(characterPotraitList[mPrevCharIndex], -1.0f, selectionSpeed, selectOffsetVal);

        }
        else if (dirStr == "Right")
        {
            mCharacterIndex++;
            if (mCharacterIndex == characterPotraitList.Count) mCharacterIndex = 0;

            // Arrow expand and contract.
            StartCoroutine(ExpandContract(charRightArrows, charArrowExConSpd, charArrowExConScale));
           
            // Movement of character image to the right.
            ie = OffSetXFromPosition(characterPotraitList[mPrevCharIndex], 1.0f, selectionSpeed, selectOffsetVal);
        }

        StartCoroutine(ie);
        mCharFadeCoList.Add(ie);

        IEnumerator ie1 = FadeAllChildImageOutIn(characterPotraitList[mPrevCharIndex], charAlphaChangeSpeed, 0, true, false, () => {}, true );
        IEnumerator ie2 = FadeAllChildImageOutIn(characterPotraitList[mCharacterIndex], charAlphaChangeSpeed, 0, false, true, () => {}, false );

        StartCoroutine(ie1);
        StartCoroutine(ie2);
        mCharFadeCoList.Add(ie1);
        mCharFadeCoList.Add(ie2);

        PlayMoveSfx();
        PlayTheVideo(mCharacterIndex);

        // Fade character's name in.
        if (mCharacterIndex == 0)
        {
            char2Name.transform.position = mDefaultNamePos;
            char3Name.transform.position = mDefaultNamePos;
            SetText_Alpha(char2Name, 0);
            SetText_Alpha(char3Name, 0);

            StartCoroutine(FadeHorizontalSlideIn(char1Name.transform, fadeSpeed, moveSpeed, -0.5f));
        }
        else if (mCharacterIndex == 1)
        {
            char1Name.transform.position = mDefaultNamePos;
            char3Name.transform.position = mDefaultNamePos;
            SetText_Alpha(char1Name, 0);
            SetText_Alpha(char3Name, 0);

            StartCoroutine(FadeHorizontalSlideIn(char2Name.transform, fadeSpeed, moveSpeed, -0.5f));
        }
        else if (mCharacterIndex == 2)
        {
            char1Name.transform.position = mDefaultNamePos;
            char2Name.transform.position = mDefaultNamePos;
            SetText_Alpha(char1Name, 0);
            SetText_Alpha(char2Name, 0);

            StartCoroutine(FadeHorizontalSlideIn(char3Name.transform, fadeSpeed, moveSpeed, -0.5f));
        }
    }

    void ResetSelectionForP2()
    {
        // Put back to default pos.
        char1Name.transform.position = mDefaultNamePos;
        char2Name.transform.position = mDefaultNamePos;
        char3Name.transform.position = mDefaultNamePos;

        // Set text alpha to 0.
        SetText_Alpha(char1Name, 0);
        SetText_Alpha(char2Name, 0);
        SetText_Alpha(char3Name, 0);

        if (mCharacterIndex != 0)
        {
            // Fade out selected character.
            StartCoroutine(FadeAllChildImageOutIn(characterPotraitList[mCharacterIndex], charAlphaChangeSpeed, 0, true, false, () => {}, true ));
        }

        // Reset back the value to default for 2nd player selection.
        mCharacterIndex = 0;
        PlayTheVideo(mCharacterIndex);

        // Fade in default character.
        StartCoroutine(FadeAllChildImageOutIn(characterPotraitList[mCharacterIndex], charAlphaChangeSpeed, 0, false, true, () => {}, true ));
        StartCoroutine(FadeHorizontalSlideIn(char1Name.transform, fadeSpeed, moveSpeed, -0.5f));

        SetDarkerColor(avatar2SR, 1);
        p2Text.text = mSelectCharStr;
        sliderDelay += charSelectDelay * 0.5f;
        mIsWaitForP2Reset = false;
    }

    void PlayTheVideo(int charIndex)
    {
        if (charIndex == 0)
        {
            char1Video1.Play();
            char1Video2.Play();
            char1Video1.transform.position = mFrontalVid1Pos;
            char1Video2.transform.position = mFrontalVid2Pos;

            char2Video1.Pause();
            char2Video2.Pause();
            char2Video1.transform.position = mBackVid1Pos;
            char2Video2.transform.position = mBackVid2Pos;

            char3Video1.Pause();
            char3Video2.Pause();
            char3Video1.transform.position = mBackVid1Pos;
            char3Video2.transform.position = mBackVid2Pos;
        }
        else if (charIndex == 1)
        {
            char1Video1.Pause();
            char1Video2.Pause();
            char1Video1.transform.position = mBackVid1Pos;
            char1Video2.transform.position = mBackVid2Pos;

            char2Video1.Play();
            char2Video2.Play();
            char2Video1.transform.position = mFrontalVid1Pos;
            char2Video2.transform.position = mFrontalVid2Pos;

            char3Video1.Pause();
            char3Video2.Pause();
            char3Video1.transform.position = mBackVid1Pos;
            char3Video2.transform.position = mBackVid2Pos;
        }
        else if (charIndex == 2)
        {
            char1Video1.Pause();
            char3Video2.Pause();
            char1Video2.transform.position = mBackVid1Pos;
            char1Video2.transform.position = mBackVid2Pos;

            char2Video1.Pause();
            char2Video2.Pause();
            char2Video1.transform.position = mBackVid1Pos;
            char2Video2.transform.position = mBackVid2Pos;

            char3Video1.Play();
            char3Video2.Play();
            char3Video1.transform.position = mFrontalVid1Pos;
            char3Video2.transform.position = mFrontalVid2Pos;
        }
    }

    void HandleCredits()
    {
        if ((mIsP1KeybInput && (Input.GetKeyDown (KeyCode.Escape))) || 
            Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.backKey)) 
        {
            PlayBackSfx();
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

    void SetSR_Alpha(SpriteRenderer sr, float value)
    {
        Color color = sr.color;
        color.a = value;
        sr.color = color;
    }

    void SetText_Alpha(Text text, float value)
    {
        Color color = text.color;
        color.a = value;
        text.color = color;
    }

    void SetDarkerColor(SpriteRenderer sr, float value)
    {
        Color color = sr.color;
        color.r = value;
        color.g = value;
        color.b = value;
        sr.color = color;
    }

    IEnumerator ExpandContract(Transform trans, float speed, float expandVal)
    {
        Vector3 toBeScale = trans.localScale;
        Vector3 defaultScale = toBeScale;
        toBeScale.x += expandVal;
        toBeScale.y += expandVal;

        while (trans.localScale != toBeScale)
        {
            Vector3 localScale = trans.localScale;
            float val = Time.deltaTime * speed;
            localScale.x += val;
            localScale.y += val;

            if (localScale.x > toBeScale.x)
            {
                localScale.x = toBeScale.x;
                localScale.y = toBeScale.y;
            }

            trans.localScale = localScale;
            yield return null;
        }

        while (trans.localScale != defaultScale)
        {
            Vector3 localScale = trans.localScale;
            float val = Time.deltaTime * speed;
            localScale.x -= val;
            localScale.y -= val;

            if (localScale.x < defaultScale.x)
            {
                localScale.x = defaultScale.x;
                localScale.y = defaultScale.y;
            }

            trans.localScale = localScale;
            yield return null;
        }
    }

    IEnumerator WaitFor (float duration, Action doLast)
    {
        yield return new WaitForSeconds(duration);
        doLast();
    }

    IEnumerator OffSetXFromPosition(Transform trans, float dir, float speed, float offsetVal)
    {
        Vector3 defaultPos = trans.position;
        float val = 0, totalVal = 0;

        while (totalVal != offsetVal)
        {
            Vector3 pos = trans.position;
            val = dir * Time.deltaTime * speed;
            totalVal += Mathf.Abs(val);
            pos.x += val;

            // If it's over the offset value.
            if (totalVal > offsetVal)
            {
                float valToMinus = totalVal - offsetVal;
                pos.x -= valToMinus;
                totalVal = offsetVal;
            }
            trans.position = pos;

            yield return null;
        }
        trans.position = defaultPos;
    }

    IEnumerator ShowFromStartToMainMenu(float duration)
    {
        mIsCoroutine = true;
        yield return new WaitForSeconds(duration);
        mIsCoroutine = false;

        startScreenUI.gameObject.SetActive (false);

        state = State.MAIN_MENU;
        ShowMainMenu();
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

    IEnumerator FadeIn(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    IEnumerator FadeOut(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    IEnumerator WaitThenFadeOut(Text text, float speed, float waitDur, Action doLast)
    {
        mIsFading = true;
        yield return new WaitForSeconds(waitDur);

        while (text.color.a > 0)
        {
            Color color = text.color;
            color.a -= Time.deltaTime * speed;
            if (color.a < 0) color.a = 0;
            text.color = color;

            yield return null;
        }
        doLast();
        mIsFading = false;
    }

    IEnumerator FadeTextInOut(Text text, float speed, float delay, Action doLast)
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
        doLast();
        mIsFading = false;
    }

    void FadeAllChildImageOutIn(Transform trans, float speed, float delay, bool isFadeOut, bool isFadeIn, Action doLast)
    {
        StartCoroutine(FadeAllChildImageOutIn(trans, speed, delay, isFadeOut, isFadeIn, doLast, false));
    }

    IEnumerator FadeAllChildImageOutIn(Transform trans, float speed, float delay, bool isFadeOut, bool isFadeIn, Action doLast, bool endBool)
    {
        mIsAllChildFading = true;

        List<Image> childImageList = new List<Image>();
        List<SpriteRenderer> childSRList = new List<SpriteRenderer>();

        for (int i = 0; i < trans.childCount; i++)
        {
            Transform currChild = trans.GetChild(i);
            if (currChild.GetComponent<Image>() != null) childImageList.Add(currChild.GetComponent<Image>());
            else if (currChild.GetComponent<SpriteRenderer>() != null) childSRList.Add(currChild.GetComponent<SpriteRenderer>());
        }

        if (isFadeOut)
        {
            while (childImageList.Count != 0 && childImageList[0].color.a > 0)
            {
                Color color = childImageList[0].color;
                color.a -= Time.deltaTime * speed;
                if (color.a < 0) color.a = 0;

                for (int i = 0; i < childImageList.Count; i++)
                {
                    childImageList[i].color = color;
                }
                yield return null;
            }

            while (childSRList.Count != 0 && childSRList[0].color.a > 0)
            {
                Color color = childSRList[0].color;
                color.a -= Time.deltaTime * speed;
                if (color.a < 0) color.a = 0;

                for (int i = 0; i < childSRList.Count; i++)
                {
                    childSRList[i].color = color;
                }
                yield return null;
            }
        }

        if (isFadeIn)
        {
            while (childImageList.Count != 0 && childImageList[0].color.a < 1)
            {
                Color color = childImageList[0].color;
                color.a += Time.deltaTime * speed;
                if (color.a > 1) color.a = 1;

                for (int i = 0; i < childImageList.Count; i++)
                {
                    childImageList[i].color = color;
                }

                yield return null;
            }

            while (childSRList.Count != 0 && childSRList[0].color.a < 1)
            {
                Color color = childSRList[0].color;
                color.a += Time.deltaTime * speed;
                if (color.a > 1) color.a = 1;

                for (int i = 0; i < childSRList.Count; i++)
                {
                    childSRList[i].color = color;
                }

                yield return null;
            }
        }

        yield return new WaitForSeconds(delay);
        doLast();
        mIsAllChildFading = endBool;
    }

    IEnumerator FadeHorizontalSlideIn(Transform trans, float fadeSpeed, float moveSpeed, float xOffset)
    {
        Vector3 pos = trans.position;
        Vector3 defaultPos = pos;
        pos.x += xOffset;
        trans.position = pos;

        Text text = trans.GetComponent<Text>();

        while (trans.position.x != defaultPos.x || text.color.a != 1)
        {
            Vector3 currPos = trans.position;
            float val = Time.deltaTime * moveSpeed;

            if (xOffset > 0) val = -val;
            currPos.x += val;

            if ((xOffset > 0 && currPos.x < defaultPos.x) || (xOffset < 0 && currPos.x > defaultPos.x)) currPos.x = defaultPos.x;
            trans.position = currPos;

            Color newColor = trans.GetComponent<Text>().color;
            newColor.a += Time.deltaTime * fadeSpeed;
            if (newColor.a > 1) newColor.a = 1;
            text.color = newColor;

            yield return null;
        }
    }

    IEnumerator RotateText(Transform text)
    {
        while (mTimer != timeForRotation)
        {
            mTimer += Time.deltaTime;
            if (mTimer > timeForRotation) mTimer = timeForRotation;

            float val = (mTimer / timeForRotation) * angleToRotate;
            currentAngle = (int)val;
            text.rotation = Quaternion.Euler(0, 0, setInitialAxis -currentAngle);

            yield return null;
        }
        mTimer = 0;
        currentAngle = 0;
    }

    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("DreamCarnage");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (asyncLoad.progress <= 0.89f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    void SetInitialZAxis(Transform text)
    {
        text.rotation = Quaternion.Euler(0, 0, setInitialAxis);
    }

    void StopAllCharCoroutine()
    {
        for (int i = 0; i < mCharFadeCoList.Count; i++)
        {
            StopCoroutine(mCharFadeCoList[i]);
        }
    }

    void ResetAllCharPosition()
    {
        for (int i = 0; i < characterPotraitList.Count; i++)
        {
            characterPotraitList[i].localPosition = Vector3.zero;
        }
    }

    void OnDisable()
    {
        levelController.gameObject.SetActive(false);
        if (nowLoadingTrans != null) nowLoadingTrans.gameObject.SetActive(false);
        if (mNowLoadingPS != null) mNowLoadingPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void PlayMoveSfx() { AudioManager.sSingleton.PlayMainMenuMoveSfx(); }
    void PlayAcceptSfx() { AudioManager.sSingleton.PlayMainMenuAcceptSfx(); }
    void PlayBackSfx() { AudioManager.sSingleton.PlayMainMenuBackSfx(); }
    void PlayStartSfx() { AudioManager.sSingleton.PlayMainStartGameSfx(); }
}
