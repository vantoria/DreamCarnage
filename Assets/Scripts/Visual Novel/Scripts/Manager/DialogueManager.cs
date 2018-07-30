using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour 
{
    public static DialogueManager sSingleton { get { return _sSingleton; } }
    static DialogueManager _sSingleton;

    public int currDialogueData = 0;
    public List<DialogueData> dialogueDataList = new List<DialogueData>();

    public SpriteRenderer leftCharacter, rightCharacter, endDialogueIcon;
    public Transform dialogueBox;
    public Transform ansSelBoxDefault;

    [Tooltip("Typewriter speed delay.")]
    public float textDelay = 0.0f;
    [Tooltip("Delay before player can press the next button.")]
	public float endTextDelay = 0.1f;
    [Tooltip("Speed of rotating icon.")]
    public float endIconSpeed = 1.0f;
    [Tooltip("The spacing multiplier between answer choices.")]
    public float ansChoiceYMult = 1.5f;

    public Transform answerPrefab;

	public class CharacterSprite
	{
        public Type type;
		public SpriteRenderer characterSpriteRend;
		public SpriteState spriteState;

		public enum Type
		{
			NONE = 0,
			LEFT_CHARACTER,
			RIGHT_CHARACTER,
		};

		public enum SpriteState
		{
			NONE = 0,
			IN_SCREEN,
			OUT_SCREEN,
			INVISIBLE_IN_SCREEN,
		};

		public CharacterSprite()
		{
            this.type = Type.NONE;
			this.characterSpriteRend = null;
			this.spriteState = SpriteState.NONE;
		}

        public CharacterSprite(Type type, SpriteRenderer characterSprite, SpriteState spriteState)
		{
			this.type = type;
			this.characterSpriteRend = characterSprite;
			this.spriteState = spriteState;
		}
	}
	public List<CharacterSprite> characterSpriteList = new List<CharacterSprite>();

    enum CurrentState
    {
        NO_DIALOGUE = 0,
        DIALOGUE_SHOW,
        DIALOGUE_RE_OPEN,
        DIALOGUE_PAUSE,
        DIALOGUE_ENDED,
        RESPONSE_SHOW
    };
    CurrentState mCurrentState = CurrentState.NO_DIALOGUE;

	enum ActiveCharacterSpriteState
	{
		NONE = 0,
		LEFT,
		RIGHT
	}
	ActiveCharacterSpriteState mActiveCharacterSpriteState = ActiveCharacterSpriteState.NONE;

    Text mCharacterText, mDialogueText;

    int mCurrDialogueIndex = 0, mCurrSentenceIndex = 0, mCurrResponseIndex = 0;
    bool mIsShowingSentence = false, mIsSwapActive = false, mIsCoroutine = false, mIsFirstStart = true;
    bool mIsP1KeybInput = true, mIsUpdatedPosScale = false;

    int mAnswerClicked = -1;
    List<Transform> mAnswerTransList = new List<Transform>();

    CharacterData.Info.Character mSavedActiveCharacter; // Only used when there's a NONE character.
    Vector3 defaultLeftSprPos, defaultRightSprPos;

    IEnumerator coroutine;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

	void Start () 
	{
        mIsP1KeybInput = JoystickManager.sSingleton.IsP1KeybInput;

        Initialization();
        CharInitialization();
        CharSpriteState();
	}
	
	void Update () 
	{
        endDialogueIcon.transform.Rotate (Vector3.up * endIconSpeed, Space.World);
//        HandleDialogue();
	}

    // Value initialization.
    void Initialization()
    {
        if (dialogueBox != null)
        {
            for (int i = 0; i < dialogueBox.childCount; i++)
            {
                if (dialogueBox.GetChild(i).name == "CharacterName")
                    mCharacterText = dialogueBox.GetChild(i).GetComponent<Text>();
                else if (dialogueBox.GetChild(i).name == "Dialogue")
                    mDialogueText = dialogueBox.GetChild(i).GetComponent<Text>();
            }
        }

        defaultLeftSprPos = leftCharacter.transform.position;
        defaultRightSprPos = rightCharacter.transform.position;
    }

    // Character initialization.
    void CharInitialization()
    {
        CharacterSprite.Type charType1 = CharacterSprite.Type.LEFT_CHARACTER;
        CharacterSprite.Type charType2 = CharacterSprite.Type.RIGHT_CHARACTER;
        CharacterSprite.SpriteState sprState = CharacterSprite.SpriteState.OUT_SCREEN;

        characterSpriteList.Add(new CharacterSprite(charType1, leftCharacter, sprState));
        characterSpriteList.Add(new CharacterSprite(charType2, rightCharacter, sprState));
    }

    // Set left and right character's sprite and state.
    void CharSpriteState()
    {
        if (currDialogueData > dialogueDataList.Count - 1) return;

        DialogueData.Dialogue firstDiag = dialogueDataList[currDialogueData].dialogueList [0];
        Sprite firstSprite = firstDiag.sentenceList [0].sprite;

        DialogueData.Dialogue secondDiag = null;
        Sprite secondSprite = null;
        for (int i = 1; i < dialogueDataList[currDialogueData].dialogueList.Count; i++) 
        {
            DialogueData.Dialogue currDiag = dialogueDataList[currDialogueData].dialogueList [i];
            if (currDiag.character.name != CharacterData.Info.Character.NONE &&
                firstDiag.character.name != currDiag.character.name)  { secondDiag = currDiag; break; }
        }
        if (secondDiag != null) secondSprite = secondDiag.sentenceList [0].sprite;

        if (firstDiag.isLeft) 
        {
            mActiveCharacterSpriteState = ActiveCharacterSpriteState.LEFT;
            characterSpriteList[0].characterSpriteRend.sprite = firstSprite;
            if(secondSprite != null) characterSpriteList[1].characterSpriteRend.sprite = secondSprite; 

            if (!dialogueDataList[currDialogueData].isBothAppear) characterSpriteList[1].spriteState = CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN;
        }
        else 
        {
            mActiveCharacterSpriteState = ActiveCharacterSpriteState.RIGHT;
            characterSpriteList[1].characterSpriteRend.sprite = firstSprite;
            if (secondSprite != null) characterSpriteList[0].characterSpriteRend.sprite = secondSprite; 

            if (!dialogueDataList[currDialogueData].isBothAppear) characterSpriteList[0].spriteState = CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN;
        }
        SetActiveCharacterSpriteShade();
    }

    // Handle dialogue based on current state it is in.
    public void HandleDialogue()
    {
        if (mCurrentState == CurrentState.DIALOGUE_PAUSE) return;

		if (((mIsP1KeybInput && (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown (0) || Input.GetKey(KeyCode.Space))) || 
            Input.GetKeyDown(JoystickManager.sSingleton.p1_joystick.acceptKey) || Input.GetKey(JoystickManager.sSingleton.p1_joystick.skipConvoKey) || 
            mIsFirstStart) && !DialogueSlide.sSingleton.isAppearing) 
        {
            mIsFirstStart = false;
            if (mCurrentState == CurrentState.NO_DIALOGUE)
            {
                if (currDialogueData > dialogueDataList.Count - 1) return;
                else if (mCurrDialogueIndex < dialogueDataList[currDialogueData].dialogueList.Count) HandleNoDialogue();
            }

            if (mCurrentState == CurrentState.DIALOGUE_SHOW) HandleDialogueShow();
            else if (mCurrentState == CurrentState.RESPONSE_SHOW) HandleAnswerResponse();
            else if (mCurrentState == CurrentState.DIALOGUE_RE_OPEN) StartCoroutine(WaitTimeBasedOnCurrentDialogue());
            else if (mCurrentState == CurrentState.DIALOGUE_ENDED) HandleDialogueEnded();
        }
    }

    // Handle when no dialogue is displayed on screen.
    void HandleNoDialogue()
    {
        if (dialogueDataList[currDialogueData].isBothAppear) DialogueSlide.sSingleton.AppearAll();
        else
        {
            bool isLeft = dialogueDataList[currDialogueData].dialogueList [0].isLeft;

            UpdateCharPositionAndScale(isLeft);
            DialogueSlide.sSingleton.SlideIn (true, isLeft);
            characterSpriteList[0].spriteState = CharacterSprite.SpriteState.IN_SCREEN;
        }
        mCurrentState = CurrentState.DIALOGUE_SHOW;
    }

    // Handle the moving in of character potraits and dialogue sentences.
    void HandleDialogueShow()
    {
        DialogueData.Dialogue currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];

		// Update current character state.
		if (currDialogue.isLeft) mActiveCharacterSpriteState = ActiveCharacterSpriteState.LEFT;
		else mActiveCharacterSpriteState = ActiveCharacterSpriteState.RIGHT;

        if (mCurrDialogueIndex != 0 && !mIsUpdatedPosScale) UpdateCharPositionAndScale(currDialogue.isLeft);
		mIsSwapActive = false;

        // ------------------------------HANDLE MOVE-IN TIME------------------------------
        if (currDialogue.time.moveInTime > 0 &&
            ((mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT && characterSpriteList[0].spriteState != CharacterSprite.SpriteState.IN_SCREEN) ||
            (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT && characterSpriteList[1].spriteState != CharacterSprite.SpriteState.IN_SCREEN)))
        { 
            if (currDialogue.appearMeth == DialogueData.Dialogue.AppearMethod.FADE)
            {
                // TODO : defaultLeft and defaultRight to follow characterData postList.
//                if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) leftCharacter.transform.position = defaultLeftSprPos;
//                else if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) rightCharacter.transform.position = defaultRightSprPos;
                AppearCharOnScreen(currDialogue);
            }
            else if (currDialogue.appearMeth == DialogueData.Dialogue.AppearMethod.SLIDE)
            {
                PutSpriteOutScreen(currDialogue);
                UpdateCharacterSprite(currDialogue.sentenceList[0]);
                HandleMoveInTime(currDialogue); 
            }
        }
        else
        {
            // Fade in character if it's invisible in screen. (Alpha 0 to 1).
            if((mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT && characterSpriteList[0].spriteState == CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN) || 
                (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT && characterSpriteList[1].spriteState == CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN))
            {
                AppearCharOnScreen(currDialogue);
                if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) characterSpriteList[0].spriteState = CharacterSprite.SpriteState.IN_SCREEN;
                else characterSpriteList[1].spriteState = CharacterSprite.SpriteState.IN_SCREEN;
            }
            else HandleDialogueText(currDialogue);
        }
    }
        
    // Handle the showing of how to show the sentences to player.
	void HandleDialogueText(DialogueData.Dialogue currDialogue)
    {
        int currSentenceCount = currDialogue.sentenceList.Count;
        if (mCurrSentenceIndex < currSentenceCount)
        {
            DialogueData.Dialogue.Sentence currSentence = currDialogue.sentenceList[mCurrSentenceIndex];
            if (mIsShowingSentence)
            {
                // Skip the type-writer style writing and show full sentence.
                StopCoroutine(coroutine);
                mDialogueText.text = currSentence.text;
                SetToNextSentence();
            }
            else if (!mIsShowingSentence)
            {
                if(mActiveCharacterSpriteState == ActiveCharacterSpriteState.NONE)
                {
                    if (currDialogue.isLeft) mActiveCharacterSpriteState = ActiveCharacterSpriteState.LEFT;
                    else mActiveCharacterSpriteState = ActiveCharacterSpriteState.RIGHT;
                }
                string charName = currDialogue.character.name.ToString();
                UpdateCharacterSprite(currSentence);
                ShowNextDialogue(currSentence, charName);
            }
        }
    }

    void HandleAnswerResponse()
    {
        List<DialogueData.Dialogue.Sentence> responseList = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex].answerChoiceList[mAnswerClicked - 1].responseList;

        int responseCount = responseList.Count;
        if(responseCount == 0) SetToNextResponse(responseCount);
        else
        {
            DialogueData.Dialogue.Sentence currResponse = responseList[mCurrResponseIndex];

            if (mIsShowingSentence)
            {
                // Skip the type-writer style writing and show full sentence.
                StopCoroutine(coroutine);
                mDialogueText.text = currResponse.text;
                SetToNextResponse(responseCount);
            }
            else if (!mIsShowingSentence)
            {
                // TODO update character sprite.
                UpdateCharacterSprite(currResponse);
                coroutine = TypewriterStyle(currResponse.text, () => SetToNextResponse(responseCount));
                StartCoroutine(coroutine);
                mIsShowingSentence = true;
            }
        }
    }

    // Set state to pause to prevent any input from player until everything is out from screen.
    void HandleDialogueEnded()
    {
        DialogueSlide.sSingleton.DisappearAll(true);
        endDialogueIcon.gameObject.SetActive(false);
        mCurrentState = CurrentState.DIALOGUE_PAUSE;

        mIsFirstStart = true;
        GameManager.sSingleton.SetToTutorialState();
    }
        
    // Change character sprite for current sentence.
    void UpdateCharacterSprite(DialogueData.Dialogue.Sentence currSentence)
    {
        if (currSentence.isCharVisible)
        {
            if (!leftCharacter.enabled || !rightCharacter.enabled)
            {
                leftCharacter.enabled = true;
                rightCharacter.enabled = true;
            }

            // Update current character's sprite.
            Sprite sprite = currSentence.sprite;
            if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) leftCharacter.sprite = sprite;
            else if(mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) rightCharacter.sprite = sprite;
        }
        else if (!currSentence.isCharVisible)
        {
            if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) leftCharacter.enabled = false;
            else if(mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) rightCharacter.enabled = false;
        }
    }

    void ShowNextDialogue(DialogueData.Dialogue.Sentence currSentence, string charName)
    {
        if (charName == "NONE") DarkenBothCharacterSpriteShade();
        else if (mIsSwapActive) SwapActiveCharacterSpriteShade();
        else SetActiveCharacterSpriteShade();
        mIsSwapActive = false;

        // Change character's name.
        if (charName == "NONE") mCharacterText.text = "";
        else mCharacterText.text = UppercaseStartAlphabet(charName, '_');

        // Display character's text.
        coroutine = TypewriterStyle(currSentence.text, SetToNextSentence);
        StartCoroutine(coroutine);
        mIsShowingSentence = true;
    }

    void SetToNextDialogue()
    {
        DialogueData.Dialogue currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];
        int dialogueCount = dialogueDataList[currDialogueData].dialogueList.Count;

        mIsShowingSentence = false;

        if (mCurrDialogueIndex + 1 < dialogueCount)
        { 
            CharacterData.Info.Character currChar = currDialogue.character.name;
            CharacterData.Info.Character nextChar = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex + 1].character.name;
            bool isNextCharNone = IsNextCharacterNone(currDialogue);

            if (currChar != CharacterData.Info.Character.NONE && isNextCharNone) 
                mSavedActiveCharacter = currDialogue.character.name;
            else if (currChar != CharacterData.Info.Character.NONE && nextChar != mSavedActiveCharacter)
                mIsSwapActive = !isNextCharNone && !IsNextCharacterTheSame(currDialogue);

            if (currDialogue.time.reOpenTime != 0) mCurrentState = CurrentState.DIALOGUE_RE_OPEN;
            else
            {
                // ------------------------------HANDLE MOVE-OUT TIME------------------------------
                if (currDialogue.time.moveOutTime > 0) HandleMoveOutTime(currDialogue);
                else mCurrentState = CurrentState.DIALOGUE_SHOW;
                // --------------------------------END MOVE-OUT TIME-------------------------------

                mCurrDialogueIndex++;
                mIsUpdatedPosScale = false;

                // Set back the active character's state.
                if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.NONE) 
                {
                    DialogueData.Dialogue nextDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];
                    if (nextDialogue.isLeft) mActiveCharacterSpriteState = ActiveCharacterSpriteState.LEFT;
                    else mActiveCharacterSpriteState = ActiveCharacterSpriteState.RIGHT;
                }
            }
        }
        else if (mCurrDialogueIndex + 1 >= dialogueCount) 
        {
            mCurrentState = CurrentState.DIALOGUE_ENDED;
            currDialogueData++;
//            mIsFirstStart = true;
//            GameManager.sSingleton.state = GameManager.State.NONE;
//            Debug.Log("Ended");
        }
    }

    // Skip to the next dialogue box.
    void SetToNextSentence()
    {
        DialogueData.Dialogue currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];
        int textCount = currDialogue.sentenceList.Count;
        if (mCurrSentenceIndex + 1 < textCount)
        {
            mCurrSentenceIndex++;
            mIsShowingSentence = false;
            mCurrentState = CurrentState.DIALOGUE_SHOW;
            endDialogueIcon.gameObject.SetActive(true);
        }
        else
        {
            mCurrSentenceIndex = 0;
            int answerCount = currDialogue.answerChoiceList.Count;
            StartCoroutine(CoroutineInOrder(DelayTime(currDialogue.time.delayTime), WaitForAnswerChoices(currDialogue.answerChoiceList)));
        }
    }

    void SetToNextResponse(int responseCount)
    {
        endDialogueIcon.gameObject.SetActive(true);
        if (mCurrResponseIndex + 1 < responseCount)
        {
            mCurrResponseIndex++;
            mIsShowingSentence = false;
        }
        else
        {
            mCurrResponseIndex = 0;

            int dialogueCount = dialogueDataList[currDialogueData].dialogueList.Count;
            if (mCurrDialogueIndex + 1 >= dialogueCount && responseCount == 0) HandleDialogueEnded();

            SetToNextDialogue();
            if (responseCount == 0) HandleDialogueShow();
        }
    }

    // TypewriteStyle 1-by-1 alphabet reveal.
    IEnumerator TypewriterStyle(string words, Action doLast) 
    {
        endDialogueIcon.gameObject.SetActive(false);

        // Get to know which words(index) to skip to next line.
        List<int> nextLineList = new List<int>();
        mDialogueText.text = words;
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < mDialogueText.cachedTextGenerator.lines.Count; i++) 
        {
            int startIndex = mDialogueText.cachedTextGenerator.lines[i].startCharIdx;
            nextLineList.Add (startIndex);
        }
        mDialogueText.text = "";

        int lineIndex = -1, nextIndex = -1;
        if (nextLineList.Count > 1)
        {
            lineIndex = 1;
            nextIndex = nextLineList [lineIndex];
        }

        for (int i = 0; i < words.Length; i++) 
        {
            if (i == nextIndex)
            {
                mDialogueText.text += "\n";
                if (lineIndex + 1 < nextLineList.Count) nextIndex = nextLineList[++lineIndex];
            }
            mDialogueText.text += words[i];

            if(textDelay != 0.0f) yield return new WaitForSeconds(textDelay);
            else yield return null;
        }

        yield return new WaitForSeconds(endTextDelay); // Prevent player from accidentally pressing too fast and skipping the next sentence.
//        Debug.Log("This dialogue box ended.");
        doLast();
    }

    IEnumerator WaitTimeBasedOnCurrentDialogue()
    {
        mCurrentState = CurrentState.DIALOGUE_PAUSE;

        DialogueData.Dialogue currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];
        DialogueSlide.sSingleton.DisappearAll(false);
        yield return new WaitForSeconds (currDialogue.time.reOpenTime); 

        mCurrDialogueIndex++;
        mIsUpdatedPosScale = false;
        currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];

        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.NONE)
        {
            if (currDialogue.isLeft) mActiveCharacterSpriteState = ActiveCharacterSpriteState.LEFT;
            else mActiveCharacterSpriteState = ActiveCharacterSpriteState.RIGHT;
        }

        DialogueData.Dialogue.Sentence currSentence = currDialogue.sentenceList[0];
        string charName = currDialogue.character.name.ToString();

        UpdateCharacterSprite(currSentence);
        ShowNextDialogue(currSentence, charName);

        if (dialogueDataList[currDialogueData].isBothAppear) 
        {
            DialogueSlide.sSingleton.AppearAll ();
            mCurrentState = CurrentState.DIALOGUE_SHOW;
        }
    }

    // Check for delay.
    IEnumerator DelayTime(float time)
    {
        mIsCoroutine = true;
        mCurrentState = CurrentState.DIALOGUE_PAUSE;
        yield return new WaitForSeconds (time); 
        endDialogueIcon.gameObject.SetActive(true);
        mIsCoroutine = false;
//        Debug.Log("DELAY ENDED");
    }

    IEnumerator WaitForAnswerChoices(List<DialogueData.Dialogue.AnswerChoice> answerList)
    {
        int noOfAnswers = answerList.Count;

        if (noOfAnswers > 0)
        {
            DialogueSlide.sSingleton.AppearAnswerBox(noOfAnswers);
            for (int i = 0; i < noOfAnswers; i++)
            {
                Transform trans = Instantiate(answerPrefab, ansSelBoxDefault.position, Quaternion.identity);
                trans.SetParent(ansSelBoxDefault.parent);
                trans.name = answerPrefab.name + (i + 1).ToString();
                trans.GetComponentInChildren<Text>().text = answerList[i].answer;

                // Move answer selection to the top in an orderly fashion.
                Vector3 temp = trans.position;
                Vector3 defaultPos = ansSelBoxDefault.position;

                temp.y = defaultPos.y - (temp.y * ((noOfAnswers - i - 1) * ansChoiceYMult));
                trans.position = temp;
                mAnswerTransList.Add(trans);
            }

            mAnswerClicked = -1;
            while(mAnswerClicked == -1)
            { yield return null; }

            // Disable the selection for the answers to avoid player pressing the button again.
            for (int i = 0; i < mAnswerTransList.Count; i++)
            {
                mAnswerTransList[i].GetComponent<Image>().enabled = false;
                mAnswerTransList[i].GetComponent<Button>().enabled = false;
            }

            mIsShowingSentence = false;
            DialogueSlide.sSingleton.CollapseAnswerBox();
            mCurrentState = CurrentState.RESPONSE_SHOW;
            HandleAnswerResponse();
        }
        else SetToNextDialogue();
    }

    void UpdateCharPositionAndScale(bool isLeft)
    {
        mIsUpdatedPosScale = true;

        DialogueData.Dialogue currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];
        if (currDialogue.character.charPosList.Count != 0)
        {
            if (isLeft)
            {
                if (characterSpriteList[0].spriteState == CharacterSprite.SpriteState.IN_SCREEN) return;

                leftCharacter.transform.localPosition = currDialogue.character.charPosList[0].position;
                leftCharacter.transform.localScale = currDialogue.character.charPosList[0].localScale;
            }
            else
            {
                if (characterSpriteList[1].spriteState == CharacterSprite.SpriteState.IN_SCREEN) return;

                rightCharacter.transform.localPosition = currDialogue.character.charPosList[1].position;
                rightCharacter.transform.localScale = currDialogue.character.charPosList[1].localScale;
            }
        }
    }

    // Move the other character into screen if it is not in screen or is invisible.
    void AppearCharOnScreen(DialogueData.Dialogue currDialogue)
    {
        bool isLeft = currDialogue.isLeft;
        DialogueSlide.sSingleton.MoveChar (true, isLeft, false, currDialogue.time.moveInTime);

        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) characterSpriteList [0].spriteState = CharacterSprite.SpriteState.IN_SCREEN;
        else if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) characterSpriteList [1].spriteState = CharacterSprite.SpriteState.IN_SCREEN;

        HandleDialogueText(currDialogue);
    }

    // Put sprite out of the screen if it is still within the screen.
    void PutSpriteOutScreen(DialogueData.Dialogue currDialogue)
    {
        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT && characterSpriteList[0].spriteState == CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN)
        {
            DialogueSlide.sSingleton.SpriteOut(leftCharacter.transform, currDialogue.isLeft);
            characterSpriteList[0].spriteState = CharacterSprite.SpriteState.OUT_SCREEN;
        }
        else if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT && characterSpriteList[1].spriteState == CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN)
        {
            DialogueSlide.sSingleton.SpriteOut(rightCharacter.transform, currDialogue.isLeft);
            characterSpriteList[1].spriteState = CharacterSprite.SpriteState.OUT_SCREEN;
        }
    }

    // Set character's shade.
    void SetActiveCharacterSpriteShade()
    {
        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT)  // Left character active.
        {
            leftCharacter.color = Color.white;
            rightCharacter.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        }
        else // Right character active.
        {
            leftCharacter.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            rightCharacter.color = Color.white;
        }
    }

    // This happens when both of the characters on screen are not talking.
    void DarkenBothCharacterSpriteShade()
    {
        mActiveCharacterSpriteState = ActiveCharacterSpriteState.NONE;
        leftCharacter.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        rightCharacter.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    }

    // Swap character's shade.
    void SwapActiveCharacterSpriteShade()
    {
		if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT)  // Change right character to active.
		{
			mActiveCharacterSpriteState = ActiveCharacterSpriteState.RIGHT;
			leftCharacter.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			rightCharacter.color = Color.white;
		}
		else // Change left character to active.
		{
			mActiveCharacterSpriteState = ActiveCharacterSpriteState.LEFT;
			leftCharacter.color = Color.white;
			rightCharacter.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		}
    }

    void HandleMoveInTime(DialogueData.Dialogue currDialogue)
    {
        mCurrentState = CurrentState.DIALOGUE_PAUSE;
        endDialogueIcon.gameObject.SetActive(false);

        Transform trans = null;
        Vector3 defaultTarget = new Vector3();

        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) 
        { 
            trans = leftCharacter.transform; 
            defaultTarget = defaultLeftSprPos; 
        }
        else if(mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) 
        { 
            trans = rightCharacter.transform; 
            defaultTarget = defaultRightSprPos; 
        }

        DialogueSlide.sSingleton.SpriteSlideIn (trans, defaultTarget, currDialogue.isLeft, currDialogue.time.moveInTime);
    }

    void HandleMoveOutTime(DialogueData.Dialogue currDialogue)
    {
		if (currDialogue.disappearMeth == DialogueData.Dialogue.AppearMethod.FADE) 
		{
			bool isLeft = currDialogue.isLeft;
            DialogueSlide.sSingleton.MoveChar (false, isLeft, false, currDialogue.time.moveOutTime);

			if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) characterSpriteList[0].spriteState = CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN;
			else if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) characterSpriteList[1].spriteState = CharacterSprite.SpriteState.INVISIBLE_IN_SCREEN;
			mCurrentState = CurrentState.DIALOGUE_SHOW;
		}
		else if (currDialogue.disappearMeth == DialogueData.Dialogue.AppearMethod.SLIDE) 
		{
			Transform trans = null;
			if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) trans = leftCharacter.transform;
			else if(mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT)  trans = rightCharacter.transform;

            endDialogueIcon.gameObject.SetActive(false);
            DialogueSlide.sSingleton.SpriteSlideOut (trans, currDialogue.isLeft, currDialogue.time.moveOutTime);	
		}
    }

    // Check to see whether the next character is None.
    bool IsNextCharacterNone(DialogueData.Dialogue currDialogue)
    {
        CharacterData.Info.Character nextChar;

        int nextIndex = mCurrDialogueIndex + 1;
        if (nextIndex >= dialogueDataList[currDialogueData].dialogueList.Count) nextIndex = 0;
        nextChar = dialogueDataList[currDialogueData].dialogueList[nextIndex].character.name;

        if (nextChar.ToString() == "NONE") return true;
        return false;
    }

    // Check to see whether the next character is the same.
    bool IsNextCharacterTheSame(DialogueData.Dialogue currDialogue)
    {
        CharacterData.Info.Character currChar, nextChar;
        currChar = currDialogue.character.name;

        int nextIndex = mCurrDialogueIndex + 1;
        if (nextIndex >= dialogueDataList[currDialogueData].dialogueList.Count) nextIndex = 0;
        nextChar = dialogueDataList[currDialogueData].dialogueList[nextIndex].character.name;

        if(currChar == nextChar) return true;
        return false;
    }

    // Set uppercase for the start of 1st and 2nd name.
    string UppercaseStartAlphabet(string str, char space)
    {
        str = str.ToLower();
        int index = str.IndexOf('_');

        if (index < 0)  return char.ToUpper(str[0]) + str.Substring(1);;

        string firstName = str.Substring(0, index);
        firstName = char.ToUpper(firstName[0]) + firstName.Substring(1);

        string secondName = str.Substring(index + 1);
        secondName = char.ToUpper(secondName[0]) + secondName.Substring(1);
        return firstName + " " + secondName;
    }

    void Reset()
    {
        mIsFirstStart = true;
        mIsSwapActive = false;
        mCurrResponseIndex = 0;
        mCurrSentenceIndex = 0;
        mCurrDialogueIndex = 0;
        CharSpriteState();
        mCurrentState = CurrentState.NO_DIALOGUE;
    }

    //----------------------------------------------------------------------------------------------
    //--------------------------------HELPER IENUMERATOR FUNCTIONS----------------------------------
    //----------------------------------------------------------------------------------------------

    IEnumerator CoroutineInOrder(IEnumerator ie1, IEnumerator ie2)
    {
        StartCoroutine(ie1);

        while (mIsCoroutine)
        { yield return null; }

        StartCoroutine(ie2);
    }

    //----------------------------------------------------------------------------------------------
    //-------------------------------------PUBLIC FUNCTIONS-----------------------------------------
    //-----------------------Only called when DialogueSlide animation is done-----------------------
    //----------------------------------------------------------------------------------------------

    public void SetDialogueShowInScreen()
    {
        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) characterSpriteList[0].spriteState = CharacterSprite.SpriteState.IN_SCREEN;
        else if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) characterSpriteList[1].spriteState = CharacterSprite.SpriteState.IN_SCREEN;

        mCurrentState = CurrentState.DIALOGUE_SHOW;

        DialogueData.Dialogue currDialogue = dialogueDataList[currDialogueData].dialogueList[mCurrDialogueIndex];
        HandleDialogueText(currDialogue);
    }

    public void SetDialogueShowOutScreen()
    {
        if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.LEFT) characterSpriteList[0].spriteState = CharacterSprite.SpriteState.OUT_SCREEN;
        else if (mActiveCharacterSpriteState == ActiveCharacterSpriteState.RIGHT) characterSpriteList[1].spriteState = CharacterSprite.SpriteState.OUT_SCREEN;

        SetToDialogueShowEnded();
    }

    public void SetToDialogueShowEnded()
    {
//        mCurrentState = CurrentState.DIALOGUE_SHOW;
        endDialogueIcon.gameObject.SetActive(true);
    }

    public void ResetAnswerBox()
    {
        for (int i = 0; i < mAnswerTransList.Count; i++)
        {
            Destroy(mAnswerTransList[i].gameObject);
        }
        mAnswerTransList.Clear();
    }

    public void ResetEndDialogue()
    {
        Reset();
    }

    //----------------------------------------------------------------------------------------------
    //----------------------------------Button on clicked function----------------------------------
    //----------------------------------------------------------------------------------------------

    public void AnswerClicked(int val)
    {
        mAnswerClicked = val;
    }
}
