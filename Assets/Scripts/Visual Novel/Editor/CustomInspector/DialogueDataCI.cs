using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataCI : Editor
{
    class CharacterTotal
    {
        public string name;
        public int count;

        public CharacterTotal(string name, int count)
        {
            this.name = name;
            this.count = count;
        }
    }
    List<CharacterTotal> mCharTotalList = new List<CharacterTotal>();

    DialogueData mSelf;
    DialogueData.AllDialogueInfo dialogueInfo;

    // Values are initialized in Initialize function.
    List<bool> mIsControlList = new List<bool>();
    List<bool> mIsMessedUpList = new List<bool>();

	List<bool> mIsAppearList = new List<bool>();
    List<bool> mIsAddDelayList = new List<bool>();
    List<bool> mIsReOpenList = new List<bool>();
    List<bool> mIsDisappearList = new List<bool>();

    bool mIsShowInfoPanel = false, mIsShowMarker = false;

    Color mIDColor, mSentenceBgColor, mAnswerBgColor, mResponseBgColor, mMessedUpColor;
    GUIStyle mDifferentColorBgStyle = new GUIStyle();

	bool mIsAudio;
    // -----------------------------------------------

    void OnEnable () 
    {
        mSelf = (DialogueData)target;
        if (DialogueSlide.sSingleton == null) DialogueSlide.sSingleton = GameObject.FindObjectOfType<DialogueSlide>();

		Initialize ();
        InitializeUI();
    }

    public override void OnInspectorGUI()
    {
        EditorStyles.textField.wordWrap = true;

        mSelf.characterData = (CharacterData) EditorGUILayout.ObjectField("CharacterData : ", mSelf.characterData, typeof(CharacterData), true);
//        EditorGUILayout.ObjectField("CharacterData : ",mSelf.characterData, typeof(UnityEngine.Object), true);

        EditorGUILayout.Space();

        mIsShowInfoPanel = EditorGUILayout.Foldout(mIsShowInfoPanel, "Info panel", true);
        if (mIsShowInfoPanel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("GroupBox");

            string allCharacters = "";
            for (int i = 0; i < mCharTotalList.Count; i++)
            {
                allCharacters += mCharTotalList[i].name + ", ";
            }
            if(allCharacters != "") allCharacters = allCharacters.Remove(allCharacters.Length - 2, 2);

            GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Info Panel", infoStyle);

            GUILayout.Label("Total dialogue   : " + dialogueInfo.totalDialogue + "\t\tTotal characters : " + dialogueInfo.totalCharacter);
            GUILayout.Label("Total sentences : " + dialogueInfo.totalSentence + "\t\tSelection phase  : " + dialogueInfo.selectionPhase);
            GUILayout.Label("Total answers   : " + dialogueInfo.totalAnswer + "\t\tTotal response   : " + dialogueInfo.totalResponse);
            GUILayout.Label("\nList of characters :\n" + allCharacters);

            GUILayout.EndVertical ();
            GUILayout.EndHorizontal();
        }

        mIsShowMarker = EditorGUILayout.Foldout(mIsShowMarker, "Marker Info", true);
        if (mIsShowMarker)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("GroupBox");

            GUILayout.Label("+/-     : Add/Delete sentence\t+D/-D : Add/Delete dialogue");
            GUILayout.Label("+A/-A : Add/Delete answer\tApp/Dis : Appear/Disappear");
            GUILayout.Label("Dly     : Delay\t\tRe : Close and Re-open");
//            GUILayout.Label("+R : Add response\t-R : Delete response");

            GUILayout.EndVertical ();
            GUILayout.EndHorizontal();
        }

        if (mSelf.characterData == null) return;

        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField("DialogueList");
        GUILayout.EndVertical ();

        for (int i = 0; i < mSelf.dialogueList.Count; i++)
        {
            // ------------------------------NEW HORIZONTAL------------------------------
            if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
            else EditorGUILayout.BeginHorizontal ();

            int id = i + 1;
            if (id == 1 || id % 10 == 0)
            {
                GUIStyle s = new GUIStyle(EditorStyles.label);
                s.normal.textColor = mIDColor;
                s.fontStyle = FontStyle.Bold;
                GUILayout.Label("ID " + id, s, GUILayout.Width(50));
            }
            else EditorGUILayout.LabelField("ID " + id, GUILayout.Width(50));
            DialogueData.Dialogue currDialogue = mSelf.dialogueList[i];

            EditorGUI.BeginChangeCheck();

            if (mSelf.characterData != null)
            {
                GUI.color = mSelf.characterData.GetCharacterColor(currDialogue.character.name.ToString());

            }
            CharacterData.Info.Character prevSelectedName = currDialogue.character.name;
            currDialogue.character.name = (CharacterData.Info.Character) EditorGUILayout.EnumPopup (currDialogue.character.name, GUILayout.Width(100));
            GUI.color = Color.white;

            CharacterData.Info.Character currCharName = currDialogue.character.name;
            if (EditorGUI.EndChangeCheck())
            {
                if (currCharName == CharacterData.Info.Character.NONE)
                {
                    mIsAppearList[i] = false;
                    mIsDisappearList[i] = false;
                    UpdateTotalWhenCharIsDeleted(prevSelectedName.ToString());
                }
                else if(currCharName != CharacterData.Info.Character.NONE)
                {
                    UpdateTotalWhenSwappingName(currCharName.ToString(), prevSelectedName.ToString());
                    SetCharacterDirectionFromOthers(currDialogue);
                    mSelf.UpdateSpriteList(i, currCharName);
                }
                SetMessedUpMessage();
            }

            // Persistent messedUp message.
            if (currCharName != CharacterData.Info.Character.NONE)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUIUtility.labelWidth = 30.0f;
                currDialogue.isLeft = EditorGUILayout.Toggle("Left", currDialogue.isLeft);

                if (EditorGUI.EndChangeCheck())
                {
                    SetSameCharacterDirection(currDialogue);
                    SetMessedUpMessage();
                }

                if (mIsMessedUpList[i])
                {
                    GUIStyle s = new GUIStyle(EditorStyles.label);
                    s.normal.textColor = mMessedUpColor;
                    GUILayout.Label("Messed-up", s);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent("V", "IsVisible"), GUILayout.Width(15));

            if (GUILayout.Button("Edit", GUILayout.Width(50))) SetIsControl(i);

            EditorGUILayout.EndHorizontal ();
            // ------------------------------END HORIZONTAL------------------------------

            int textCount = currDialogue.sentenceList.Count;
            for (int j = 0; j < textCount; j++)
            {
                // ------------------------------NEW HORIZONTAL------------------------------
                if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
                else EditorGUILayout.BeginHorizontal ();

                DialogueData.Dialogue.Sentence currSentence = currDialogue.sentenceList[j];

                GUI.color = mSentenceBgColor;
                currSentence.text = EditorGUILayout.TextArea(currSentence.text);
                GUI.color = Color.white;

                if(mIsAudio) currSentence.audioClip = (AudioClip) EditorGUILayout.ObjectField(currSentence.audioClip, typeof(AudioClip), false, GUILayout.Width(50));

                if(currDialogue.character.name == CharacterData.Info.Character.NONE) GUI.enabled = false;
                currSentence.isCharVisible = EditorGUILayout.Toggle(currSentence.isCharVisible, GUILayout.Width(15));
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    List<Sprite> charSpriteList = new List<Sprite>();
                    if (mSelf.characterData.GetCharacterSprites(currCharName) != null) charSpriteList = mSelf.characterData.GetCharacterSprites(currCharName);
                   
                    int currActiveTexture = mSelf.characterData.GetSpriteIndex(currCharName, currSentence.sprite);

                    if (currActiveTexture != -1) TexturePickerEditor.ListToEditorWindow(currSentence, currActiveTexture, charSpriteList);
                    else Debug.Log("Couldn't find texture");
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal ();
                // ------------------------------END HORIZONTAL------------------------------
            }

            int answerCount = currDialogue.answerChoiceList.Count;
            for (int j = 0; j < answerCount; j++)
            {
                if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
                else EditorGUILayout.BeginHorizontal ();

                EditorGUILayout.LabelField("A" + (j + 1).ToString(), GUILayout.Width(20));
                GUI.color = mAnswerBgColor;
                DialogueData.Dialogue.AnswerChoice currAnswer = currDialogue.answerChoiceList[j];
                currAnswer.answer = EditorGUILayout.TextArea(currAnswer.answer);
                GUI.color = Color.white;

                int responseCount = currAnswer.responseList.Count;

                if (GUILayout.Button("+", GUILayout.Width(20))) AddResponse(i, j);

                if(responseCount <= 0) GUI.enabled = false;
                if (GUILayout.Button("-", GUILayout.Width(20))) { DeleteResponse(i, j); return; }
                GUI.enabled = true;
//                mSelf.characterData = (CharacterData) EditorGUILayout.ObjectField("CharacterData : ",mSelf.characterData, typeof(CharacterData), true);

                EditorGUILayout.EndHorizontal ();

                if (responseCount > 0)
                {
                    for (int k = 0; k < responseCount; k++)
                    {
                        EditorGUILayout.BeginHorizontal ();
                        GUI.color = mResponseBgColor;
                        DialogueData.Dialogue.Sentence currResponse = currAnswer.responseList[k];
                        currResponse.text = EditorGUILayout.TextArea(currResponse.text);
                        GUI.color = Color.white;

                        currResponse.isCharVisible = EditorGUILayout.Toggle(currResponse.isCharVisible, GUILayout.Width(15));

                        if (GUILayout.Button("Select", GUILayout.Width(50)))
                        {
                            List<Sprite> charSpriteList = new List<Sprite>();
                            if (mSelf.characterData.GetCharacterSprites(currCharName) != null) charSpriteList = mSelf.characterData.GetCharacterSprites(currCharName);

                            int currActiveTexture = mSelf.characterData.GetSpriteIndex(currCharName, currResponse.sprite);

                            if (currActiveTexture != -1) TexturePickerEditor.ListToEditorWindow(currResponse, currActiveTexture, charSpriteList);
                            else Debug.Log("Couldn't find texture");
                        }
                        EditorGUILayout.EndHorizontal ();
                    }
                }
            }

            // ------------------------------NEW HORIZONTAL------------------------------
            // --------------------------------SHOW BOXES--------------------------------
            if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
            else EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace();

            if (mIsAppearList.Count != 0 && mIsAppearList[i] || currDialogue.time.moveInTime > 0)
            {
                if (currDialogue.character.name != CharacterData.Info.Character.NONE)
                {
                    EditorGUILayout.LabelField("Appear", GUILayout.Width(50));
                    currDialogue.appearMeth = (DialogueData.Dialogue.AppearMethod) EditorGUILayout.EnumPopup (currDialogue.appearMeth, GUILayout.Width(60));

                    EditorGUIUtility.labelWidth = 35.0f;
                    currDialogue.time.moveInTime = EditorGUILayout.FloatField("Time", currDialogue.time.moveInTime, GUILayout.Width(70));
                    EditorGUIUtility.labelWidth = 0;
                }
            }

            if (mIsDisappearList.Count != 0 && mIsDisappearList[i] || currDialogue.time.moveOutTime > 0)
            {
                if (currDialogue.character.name != CharacterData.Info.Character.NONE)
                {
                    EditorGUILayout.LabelField("Disappear", GUILayout.Width(65));
                    currDialogue.disappearMeth = (DialogueData.Dialogue.AppearMethod) EditorGUILayout.EnumPopup (currDialogue.disappearMeth, GUILayout.Width(60));

                    EditorGUIUtility.labelWidth = 35.0f;
                    currDialogue.time.moveOutTime = EditorGUILayout.FloatField("Time", currDialogue.time.moveOutTime, GUILayout.Width(70));
                    EditorGUIUtility.labelWidth = 0;
                }
            }
            GUILayout.EndHorizontal();
            // ------------------------------END HORIZONTAL------------------------------

			// ------------------------------NEW HORIZONTAL------------------------------
			// --------------------------------SHOW BOXES--------------------------------
            if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
            else EditorGUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();

			if (mIsAddDelayList.Count != 0 && mIsAddDelayList[i] || currDialogue.time.delayTime != 0)
            {
                EditorGUIUtility.labelWidth = 70.0f;
				currDialogue.time.delayTime = EditorGUILayout.FloatField("Next Delay", currDialogue.time.delayTime, GUILayout.Width(105));
                EditorGUIUtility.labelWidth = 0;
            }
			if (mIsReOpenList.Count != 0 && mIsReOpenList[i] || currDialogue.time.reOpenTime != 0)
            {
                EditorGUIUtility.labelWidth = 70.0f;
                currDialogue.time.reOpenTime = EditorGUILayout.FloatField("Re-open", currDialogue.time.reOpenTime, GUILayout.Width(105));
                EditorGUIUtility.labelWidth = 0;
            }
            if (currDialogue.character.name == CharacterData.Info.Character.NONE)
            {
                currDialogue.time.moveInTime = 0;
                currDialogue.time.moveOutTime = 0;
            }
			GUILayout.EndHorizontal();
			// ------------------------------END HORIZONTAL------------------------------

            if (mIsControlList[i])
            {
                // ------------------------------NEW HORIZONTAL------------------------------
                DisplaySeparator();
				 
                if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
                else EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("");
                
				EditorGUILayout.LabelField("Dialogue", GUILayout.Width(60.0f));
                GUILayout.FlexibleSpace();	

                if (GUILayout.Button("+", GUILayout.Width(20))) mSelf.addTextBox(i, currCharName);

                if(currDialogue.sentenceList.Count == 1) GUI.enabled = false;
				if (GUILayout.Button("-", GUILayout.Width(20))) mSelf.deleteTextBox(i);
                GUI.enabled = true;

				if (GUILayout.Button("+D", GUILayout.Width(30))) AddDialogue(i);
				else if (GUILayout.Button("-D", GUILayout.Width(30))) { DeleteDialogue(i); return; }

                if (GUILayout.Button("+A", GUILayout.Width(30))) AddAnswer(i);
                else if (GUILayout.Button("-A", GUILayout.Width(30))) { DeleteAnswer(i); return; }

//                if (GUILayout.Button("+R", GUILayout.Width(30))) AddResponse(i);
//                else if (GUILayout.Button("-R", GUILayout.Width(30))) { DeleteResponse(i); return; }

				GUILayout.EndHorizontal();
				// ------------------------------END HORIZONTAL------------------------------

				// ------------------------------NEW HORIZONTAL------------------------------
                if(mIsControlList[i]) EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
                else EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("");

				EditorGUILayout.LabelField("Others", GUILayout.Width(60.0f));
				GUILayout.FlexibleSpace();	

				// Appear button.
                if(currDialogue.character.name == CharacterData.Info.Character.NONE) GUI.enabled = false;
				if (GUILayout.Button("App", GUILayout.Width(42))) 
				{
                    if (!mIsAppearList[i])
                    {
                        mIsAppearList[i] = true;
                        if (currDialogue.time.moveInTime == 0 && DialogueSlide.sSingleton.moveTime != 0) currDialogue.time.moveInTime = DialogueSlide.sSingleton.moveTime;
                    }
					else
					{
						currDialogue.time.moveInTime = 0.0f;
						mIsAppearList[i] = false;
					}
                    SetMessedUpMessage();
				}
				GUI.enabled = true;

				// Disappear button.
                if(currDialogue.character.name == CharacterData.Info.Character.NONE) GUI.enabled = false;
                if (GUILayout.Button("Dis", GUILayout.Width(42))) 
                {
                    if (!mIsDisappearList[i])
                    {
                        mIsDisappearList[i] = true;
                        if (currDialogue.time.moveOutTime == 0 && DialogueSlide.sSingleton.moveTime != 0) currDialogue.time.moveOutTime = DialogueSlide.sSingleton.moveTime;
                    }
					else
					{
						currDialogue.time.moveOutTime = 0.0f;
                    	mIsDisappearList[i] = false;
					}
                    SetMessedUpMessage();
                }
                GUI.enabled = true;

                // Delay button.
                if (GUILayout.Button("Dly", GUILayout.Width(42))) 
                {
					if(!mIsAddDelayList[i]) mIsAddDelayList[i] = true;
					else
					{
						currDialogue.time.delayTime = 0.0f;
						mIsAddDelayList[i] = false;
					}
                }

                // Re-open button.
                if (GUILayout.Button("Re", GUILayout.Width(42))) 
                {
					if(!mIsReOpenList[i]) mIsReOpenList[i] = true;
					else
					{
						currDialogue.time.reOpenTime = 0.0f;
                   		mIsReOpenList[i] = false;
					}
                }
                GUILayout.EndHorizontal ();
                // ------------------------------END HORIZONTAL------------------------------
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
        }

        // ------------------------------NEW HORIZONTAL------------------------------
        EditorGUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
        EditorGUIUtility.labelWidth = 70.0f;
		mIsAudio = EditorGUILayout.Toggle("Has audio", mIsAudio, GUILayout.Width(90.0f));
		EditorGUIUtility.labelWidth = 80.0f;
		mSelf.isBothAppear = EditorGUILayout.Toggle("Both appear", mSelf.isBothAppear);
        EditorGUIUtility.labelWidth = 0.0f;
		EditorGUILayout.EndHorizontal ();
		// ------------------------------END HORIZONTAL------------------------------

		// ------------------------------NEW HORIZONTAL------------------------------
		EditorGUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
        if (GUILayout.Button("+Dialogue", GUILayout.Width(80))) AddDialogue();
        else if (GUILayout.Button("-Dialogue", GUILayout.Width(80))) DeleteDialogue();
        else if (GUILayout.Button("Reset", GUILayout.Width(60))) ResetInspector();
		EditorGUILayout.EndHorizontal ();
		// ------------------------------END HORIZONTAL------------------------------

        if (GUI.changed)
        {
            dialogueInfo = mSelf.GetDialogueInfo();
            EditorUtility.SetDirty(target); 
        }
    }

    //----------------------------------------------------------------------------------------------
    //-------------------------------------PRIVATE FUNCTIONS----------------------------------------
    //----------------------------------------------------------------------------------------------

    void Initialize()
    {
        if (mSelf == null) return;

        dialogueInfo = mSelf.GetDialogueInfo();
        ClearLists();
        for (int i = 0; i < mSelf.dialogueList.Count; i++)
        { 
            DialogueData.Dialogue currDialogue = mSelf.dialogueList[i];

            mIsControlList.Add(false);
            mIsMessedUpList.Add(false);

            if(currDialogue.time.moveInTime != 0) mIsAppearList.Add(true);  
            else mIsAppearList.Add(false); 

            if(currDialogue.time.delayTime != 0) mIsAddDelayList.Add(true);  
            else mIsAddDelayList.Add(false); 

            if(currDialogue.time.reOpenTime != 0) mIsReOpenList.Add(true);  
            else mIsReOpenList.Add(false); 

            if(currDialogue.time.moveOutTime != 0) mIsDisappearList.Add(true);  
            else mIsDisappearList.Add(false);  
        }
        SetMessedUpMessage();
        SetCharactersList();
    }

    // Initialize the value for UIs.
    void InitializeUI()
    {
        mIDColor = new Color(0.7f, 0.15f, 0.25f);
        mSentenceBgColor = new Color(0.95f, 1, 0.95f);
        mAnswerBgColor = new Color(1, 0.95f, 0.95f);
        mResponseBgColor = new Color(0.95f, 0.95f, 1);
        mMessedUpColor = new Color(0.8f, 0, 0);

        Texture2D texture = null;
        if(EditorGUIUtility.isProSkin) texture = MakeTex(600, 2, new Color(1.0f, 1.0f, 1.0f, 0.1f));
        else texture = MakeTex(600, 2, new Color(0.0f, 0.0f, 0.0f, 0.1f));
        mDifferentColorBgStyle.normal.background = texture;
    }

    void SetIsControl(int activeID)
    {
        bool isActive = mIsControlList[activeID];
        for (int i = 0; i < mIsControlList.Count; i++)
        {
            mIsControlList[i] = false;
        }
        mIsControlList[activeID] = !isActive;
    }

    string GetFirstNameNotNone(List<DialogueData.Dialogue> dialogueList, int startIndex, out int outIndex)
    {
        for (int i = startIndex; i < dialogueList.Count; i++)
        {
            string charNameStr = dialogueList[i].character.name.ToString();
            if (charNameStr != "NONE")
            {
                outIndex = i;
                return charNameStr;
            }
        }
        outIndex = -1;
        return "";
    }

    void SetMessedUpMessage()
    {
        if (mSelf.dialogueList.Count == 0) return;

        List<List<DialogueData.Dialogue>> totalList = new List<List<DialogueData.Dialogue>>();
        List<DialogueData.Dialogue> tempList = new List<DialogueData.Dialogue>();

        DialogueData.Dialogue currDialogue = mSelf.dialogueList[0];
        tempList.Add(currDialogue);

        // Separate same bool(isLeft) in an orderly fashion.
        for (int i = 1; i < mSelf.dialogueList.Count; i++)
        {
            DialogueData.Dialogue nextDialogue = mSelf.dialogueList[i];

            if (currDialogue.isLeft == nextDialogue.isLeft) tempList.Add(nextDialogue);
            else
            {
                totalList.Add(tempList);
                tempList = new List<DialogueData.Dialogue>();
                tempList.Add(nextDialogue);
            }

            currDialogue = nextDialogue;
        }
        totalList.Add(tempList);

        int index = 0;
        for (int i = 0; i < totalList.Count; i++)
        {
            int jIndex = 0;
            string tempStr1 = GetFirstNameNotNone(totalList[i], 0, out jIndex);
            bool isDifferentName = false;

            if (tempStr1 != "")
            {
                // Check to see whether the names are the different in the nestedList.
                for (int j = jIndex + 1; j < totalList[i].Count; j++)
                {
                    string tempStr2 = totalList[i][j].character.name.ToString();

                    if (tempStr2 == "NONE") continue;
                    if (tempStr1 != tempStr2) 
                    { 
                        isDifferentName = true; 
                        break; 
                    }
                }
            }

            // Set mIsMessUpList to true or false.
            for (int j = 0; j < totalList[i].Count; j++)
            {
                string tempStr = totalList[i][j].character.name.ToString();

                if (tempStr != "NONE" && isDifferentName) mIsMessedUpList[j + index] = true;
                else mIsMessedUpList[j + index] = false;
            }
            index = index + totalList[i].Count;
        }

        string firstStrName = "", secondStrName = "";
        int firstNameIndex = -1, secondNameIndex = -1;
        List<int> savedHasTimeIndexList = new List<int>();

        // Check to see whether there's moveOut and moveIn time. If true, messedUp is not shown.
        for (int i = 0; i < mIsMessedUpList.Count; i++)
        {
            if (mIsMessedUpList[i])
            {
                if (firstStrName == "") firstStrName = GetFirstNameNotNone(mSelf.dialogueList, i, out firstNameIndex);
                secondStrName = GetFirstNameNotNone(mSelf.dialogueList, firstNameIndex + 1, out secondNameIndex);

                if (secondNameIndex == -1) break;

                if(firstStrName != secondStrName)
                {
                    if (mSelf.dialogueList[firstNameIndex].time.moveOutTime != 0 && mSelf.dialogueList[secondNameIndex].time.moveInTime != 0)
                    {
                        int startIndex = GetStartIndexRecursive(firstStrName, firstNameIndex);
                        for (int j = startIndex; j < firstNameIndex + 1; j++)
                        {
                            mIsMessedUpList[j] = false; 
                        }
                        savedHasTimeIndexList.Add(secondNameIndex);
                    }
                    firstStrName = secondStrName;
                    firstNameIndex = secondNameIndex;
                }
            }
        }

        // Set messedUp for the final character that has moveInTime. 
        // It just follows the value of character before itself(which is not NONE).
        if (savedHasTimeIndexList.Count > 0)
        {
            for (int i = 0; i < savedHasTimeIndexList.Count; i++)
            {
                string finalStrName = mSelf.dialogueList[savedHasTimeIndexList[i]].character.name.ToString();
                int finalIndex = GetEndIndexRecursive(finalStrName, savedHasTimeIndexList[i]);

                int prevLastIndex = GetPreviousLastIndexNotNone(finalStrName, savedHasTimeIndexList[i]);
                for (int j = savedHasTimeIndexList[i]; j < finalIndex + 1; j++)
                { 
                    mIsMessedUpList[j] = mIsMessedUpList[prevLastIndex]; 
                }
            }
        }
    }

    void SetSameCharacterDirection(DialogueData.Dialogue getDialogue)
    {
        CharacterData.Info.Character name = getDialogue.character.name;
        bool isLeft = getDialogue.isLeft;

        for (int i = 0; i < mSelf.dialogueList.Count; i++)
        {
            DialogueData.Dialogue currDialogue = mSelf.dialogueList[i];
			if (currDialogue == getDialogue) continue;
            else if (currDialogue.character.name == name) currDialogue.isLeft = isLeft;
        }
    }

    void SetCharacterDirectionFromOthers(DialogueData.Dialogue setDialogue)
    {
        CharacterData.Info.Character name = setDialogue.character.name;

        for (int i = 0; i < mSelf.dialogueList.Count; i++)
        {
            DialogueData.Dialogue currDialogue = mSelf.dialogueList[i];
			if (currDialogue == setDialogue) continue;
            else if (currDialogue.character.name == name) { setDialogue.isLeft = currDialogue.isLeft; break; }
        }
    }

    void SetDirectionToNewDialogue()
    {
        bool isLeft = true;
        for (int i = 0; i < mSelf.dialogueList.Count; i++)
        {
            DialogueData.Dialogue currDialogue = mSelf.dialogueList[i];
            if (currDialogue.character.name == mSelf.defaultCharacter) { isLeft = currDialogue.isLeft; break; }
        }

        mSelf.dialogueList[mSelf.dialogueList.Count - 1].isLeft = isLeft;
    }

    //----------------------------------------------------------------------------------------------
    //--------------------------------------BUTTON FUNCTIONS----------------------------------------
    //----------------------------------------------------------------------------------------------

    void AddDialogue()
    {
        mSelf.addDialogue();
        mIsControlList.Add(false);
        mIsMessedUpList.Add(false);
		mIsAppearList.Add(false);
        mIsAddDelayList.Add(false);
        mIsReOpenList.Add(false);
        mIsDisappearList.Add(false);

        UpdateTotalWhenCharIsAdded(mSelf.defaultCharacter.ToString());
        SetDirectionToNewDialogue();
        SetMessedUpMessage();
    }

    void AddDialogue(int index)
    {
        mSelf.addDialogue(index);
        mIsControlList.Insert(index, false);
        mIsMessedUpList.Insert(index, false);
        mIsAppearList.Insert(index, false);
        mIsAddDelayList.Insert(index, false);
        mIsReOpenList.Insert(index, false);
        mIsDisappearList.Insert(index, false);

        UpdateTotalWhenCharIsAdded(mSelf.defaultCharacter.ToString());
        SetDirectionToNewDialogue();
        SetMessedUpMessage();
    }

    void DeleteDialogue()
    {
        CharacterData.Info.Character lastName = mSelf.dialogueList[mSelf.dialogueList.Count - 1].character.name;
        if (lastName != CharacterData.Info.Character.NONE) UpdateTotalWhenCharIsDeleted(lastName.ToString());

        mSelf.deleteDialogue();
        mIsControlList.RemoveAt(mIsControlList.Count - 1);
        mIsMessedUpList.RemoveAt(mIsMessedUpList.Count - 1);
		mIsAppearList.RemoveAt(mIsAppearList.Count - 1);
        mIsAddDelayList.RemoveAt(mIsAddDelayList.Count - 1);
        mIsReOpenList.RemoveAt(mIsReOpenList.Count - 1);
        mIsDisappearList.RemoveAt(mIsDisappearList.Count - 1);
        SetMessedUpMessage();
    }

    void DeleteDialogue(int index)
    {
        CharacterData.Info.Character lastName = mSelf.dialogueList[index].character.name;
        if (lastName != CharacterData.Info.Character.NONE) UpdateTotalWhenCharIsDeleted(lastName.ToString());

        mSelf.deleteDialogue(index);
        mIsControlList.RemoveAt(index);
        mIsMessedUpList.RemoveAt(index);
		mIsAppearList.RemoveAt(index);
        mIsAddDelayList.RemoveAt(index);
        mIsReOpenList.RemoveAt(index);
        mIsDisappearList.RemoveAt(index);
        SetMessedUpMessage();
    }

    void AddAnswer(int index)
    {
        mSelf.addAnswer(index);
    }

    void DeleteAnswer(int index)
    {
        mSelf.deleteAnswer(index);
    }

    void AddResponse(int dialogueIndex, int answerIndex)
    {
        mSelf.addResponse(dialogueIndex, answerIndex);
    }

    void DeleteResponse(int dialogueIndex, int answerIndex)
    {
        mSelf.deleteResponse(dialogueIndex, answerIndex);
    }

    void ClearLists()
    {
        mCharTotalList.Clear();
        mIsControlList.Clear();
        mIsMessedUpList.Clear();
        mIsAppearList.Clear();
        mIsAddDelayList.Clear();
        mIsReOpenList.Clear();
        mIsDisappearList.Clear();
    }

    void ResetInspector()
    {
        ClearLists();
        mSelf.ResetDialogue();
    }

    //----------------------------------------------------------------------------------------------
    //------------------------------------INFO PANEL FUNCTIONS--------------------------------------
    //----------------------------------------------------------------------------------------------

    void SetCharactersList()
    {
        for (int i = 0; i < mSelf.dialogueList.Count; i++)
        {
            DialogueData.Dialogue currDialogue = mSelf.dialogueList[i];
            string charStr = currDialogue.character.name.ToString();
            bool isInside = false;

            if (charStr == "NONE") continue;

            // Check to see whether charStr is already inside mCharTotalList. If true, +1 count.
            for (int j = 0; j < mCharTotalList.Count; j++)
            {
                if (mCharTotalList[j].name == charStr)
                {
                    isInside = true;
                    mCharTotalList[j].count++;
                    break;
                }
            }
            if (!isInside) mCharTotalList.Add(new CharacterTotal(charStr, 1));
        }
    }

    // When a character is added, plus the value.
    void UpdateTotalWhenCharIsAdded(string addedName)
    {
        bool isInside = false;
        for (int j = 0; j < mCharTotalList.Count; j++)
        {
            if (mCharTotalList[j].name == addedName)
            {
                isInside = true;
                mCharTotalList[j].count++;
                break;
            }
        }
        if(!isInside) mCharTotalList.Add(new CharacterTotal(addedName, 1));
    }

    // When an existing character is deleted, minus count from deleted name.
    void UpdateTotalWhenCharIsDeleted(string deletedName)
    {
        for (int j = 0; j < mCharTotalList.Count; j++)
        {
            if (mCharTotalList[j].name == deletedName.ToString())
            {
                mCharTotalList[j].count--;
                if (mCharTotalList[j].count == 0) mCharTotalList.RemoveAt(j);
                break;
            }
        }
    }

    // When swapping 2 different names, perform both plus and minus action on the count.
    void UpdateTotalWhenSwappingName(string currCharName, string prevSelectedName)
    {
        bool isInside = false;
        for (int j = 0; j < mCharTotalList.Count; j++)
        {
            if (mCharTotalList[j].name == currCharName)
            {
                isInside = true;
                mCharTotalList[j].count++;
            }
            else if (mCharTotalList[j].name == prevSelectedName)
            {
                mCharTotalList[j].count--;
                if (mCharTotalList[j].count == 0) mCharTotalList.RemoveAt(j);
            }
        }
        if(!isInside) mCharTotalList.Add(new CharacterTotal(currCharName, 1));
    }

    //----------------------------------------------------------------------------------------------
    //-------------------------------------GET INDEX FUNCTIONS--------------------------------------
    //----------------------------------------------------------------------------------------------

    // Get the start index of clicked name.
    int GetStartIndexRecursive(string currCharStr, int currIndex)
    {
        if (currIndex == 0) return currIndex;

        string prevCharName = mSelf.dialogueList[currIndex - 1].character.name.ToString();

        if (currCharStr != prevCharName) return currIndex;
        return GetStartIndexRecursive(currCharStr, currIndex - 1);
    }

    // Get the end index of clicked name.
    int GetEndIndexRecursive(string currCharStr, int currIndex)
    {
        if (currIndex == mSelf.dialogueList.Count - 1) return currIndex;

        string nextCharName = mSelf.dialogueList[currIndex + 1].character.name.ToString();

        if (currCharStr != nextCharName) return currIndex;
        return GetEndIndexRecursive(currCharStr, currIndex + 1);
    }

    // Get the previous last index where the name is not NONE.
    int GetPreviousLastIndexNotNone(string currCharStr, int currIndex)
    {
        if (currIndex == 0) return -1;

        CharacterData.Info.Character prevCharName = mSelf.dialogueList[currIndex - 1].character.name;

        if (prevCharName != CharacterData.Info.Character.NONE &&
            currCharStr != prevCharName.ToString()) return currIndex - 1;
        return GetPreviousLastIndexNotNone(currCharStr, currIndex - 1);
    }

    //----------------------------------------------------------------------------------------------
    //---------------------------------------UI FUNCTIONS-------------------------------------------
    //----------------------------------------------------------------------------------------------

    public void DisplaySeparator()
    {
        EditorGUILayout.BeginHorizontal (mDifferentColorBgStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label("__________________________________________________________");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width*height];

        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    //----------------------------------------------------------------------------------------------
    //-------------------------------------UNUSED FUNCTIONS-----------------------------------------
    //----------------------------------------------------------------------------------------------

    // Get the count of same character's name from currIndex till the end of list or until the next name changes.
    int SameNameRecursive(string currCharStr, int currIndex, ref int count)
    {
        count++;
        if (currIndex == mSelf.dialogueList.Count - 1) return count;

        string nextCharName = mSelf.dialogueList[currIndex + 1].character.name.ToString();

        if(currCharStr != nextCharName) return count;
        return SameNameRecursive(currCharStr, currIndex + 1, ref count);
    }

    // Get the next starting index for a different name.
    int GetNextStartIndexRecursive(string currCharStr, int currIndex)
    {
        if (currIndex == mSelf.dialogueList.Count - 1) return -1;

        string nextCharName = mSelf.dialogueList[currIndex + 1].character.name.ToString();

        if (currCharStr != nextCharName) return currIndex + 1;
        return GetNextStartIndexRecursive(currCharStr, currIndex + 1);
    }
}