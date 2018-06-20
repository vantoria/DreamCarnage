using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueData : ScriptableObject 
{
    [System.Serializable]
    public class Dialogue
    {
        [System.Serializable]
        public class Sentence
        {
            public string text;
            public Sprite sprite;
            public bool isCharVisible;
            public AudioClip audioClip;

            public Sentence()
            {
                this.text = "";
                this.sprite = new Sprite();
                this.isCharVisible = true;
                this.audioClip = null;
            }

            public Sentence(string text, Sprite sprite, bool isCharVisible)
            {
                this.text = text;
                this.sprite = sprite;
                this.isCharVisible = isCharVisible;
                this.audioClip = null;
            }

            public Sentence(string text, Sprite sprite, bool isCharVisible, AudioClip audioClip)
            {
                this.text = text;
                this.sprite = sprite;
                this.isCharVisible = isCharVisible;
                this.audioClip = audioClip;
            }
        }

		[System.Serializable]
		public class Time
		{
			public float moveInTime;    // Only used when this character moves into the scene. 
			public float delayTime;     // Period where player can't press anything. Dialogue showed.
			public float reOpenTime;    // Both character and dialogue close. Re-open after this value.
			public float moveOutTime;   // Only used when this character moves out of the scene. 

            public Time()
            {
                this.moveInTime = 0;
                this.delayTime = 0;
                this.reOpenTime = 0;
                this.moveOutTime = 0;
            }
		}

        [System.Serializable]
        public class AnswerChoice
        {
            public string answer;
            public List<Sentence> responseList;
            Object objEffect;

            public AnswerChoice()
            {
                answer = "Answer";
                responseList = new List<Sentence>();
                objEffect = null;
            }

            public AnswerChoice(string answer, List<Sentence> responseList, Object objEffect)
            {
                this.answer = answer;
                this.responseList = responseList;
                this.objEffect = objEffect;
            }
        };

		public enum AppearMethod
		{
			FADE = 0,
			SLIDE,
		};

        public CharacterData.Info character;
        public List<Sentence> sentenceList;
		public Time time;
        public bool isLeft;
		public AppearMethod appearMeth, disappearMeth;
        public List<AnswerChoice> answerChoiceList;

        public Dialogue()
        {
            this.character = new CharacterData.Info();
			this.appearMeth = AppearMethod.FADE;
            this.sentenceList = new List<Sentence>(1);
            this.time = new Time();
            this.isLeft = true;
            this.answerChoiceList = new List<AnswerChoice>();
            this.disappearMeth = AppearMethod.FADE;
        }

        public Dialogue(CharacterData.Info character, AppearMethod appearMeth, List<Sentence> sentenceList, 
            bool isLeft, bool isOutScreen, List<AnswerChoice> answerChoiceList, AppearMethod disappearMeth)
        {
            this.character = character;
			this.appearMeth = appearMeth;
            this.sentenceList = sentenceList;
            this.time = new Time();
            this.isLeft = isLeft;
            this.answerChoiceList = answerChoiceList;
			this.disappearMeth = disappearMeth;
        }
    }

    public class AllDialogueInfo
    {
        public int totalDialogue;
        public int totalSentence;
        public int selectionPhase;
        public int totalAnswer;
        public int totalResponse;
        public int totalCharacter;

        public void Clear()
        {
            totalDialogue = totalSentence = selectionPhase = totalAnswer = totalResponse = totalCharacter = 0;
        }
    }
    AllDialogueInfo allDialogueInfo = new AllDialogueInfo();

    public CharacterData characterData;
    public List<Dialogue> dialogueList = new List<Dialogue>();

    public CharacterData.Info.Character defaultCharacter = CharacterData.Info.Character.UNITY_CHAN;
	public bool isBothAppear; 		// Do both character sprites appear at the start of the dialogue.

    public AllDialogueInfo GetDialogueInfo()
    {
        allDialogueInfo.Clear();

        List<string> charStrList = new List<string>();
        allDialogueInfo.totalDialogue = dialogueList.Count;
        for (int i = 0; i < dialogueList.Count; i++)
        {
            DialogueData.Dialogue currDialogue = dialogueList[i];
            string charStr = currDialogue.character.name.ToString();
            bool isInside = false;

            if (charStr != "NONE")
            {
                for (int j = 0; j < charStrList.Count; j++)
                {
                    if (charStrList[j] == charStr) { isInside = true; break; }
                }
                if (!isInside) charStrList.Add(charStr);
            }

            allDialogueInfo.totalSentence += currDialogue.sentenceList.Count;

            int answerCount = dialogueList[i].answerChoiceList.Count;
            if (answerCount != 0)
            {
                allDialogueInfo.selectionPhase += 1;
                allDialogueInfo.totalAnswer += answerCount;
                for (int j = 0; j < answerCount; j++)
                {
                    allDialogueInfo.totalResponse += dialogueList[i].answerChoiceList[j].responseList.Count;
                }
            }
        }
        allDialogueInfo.totalCharacter = charStrList.Count;
        return allDialogueInfo;
    }

    public void addTextBox(int dialogueIndex, CharacterData.Info.Character name)
    {
        Dialogue currDialogue = dialogueList[dialogueIndex];
        currDialogue.sentenceList.Add(new Dialogue.Sentence("New text", characterData.GetCharacterSprites(name)[0], true));
    }

    public void deleteTextBox(int dialogueIndex)
    {
        Dialogue currDialogue = dialogueList[dialogueIndex];

        if (currDialogue.sentenceList.Count <= 1) return;
        int lastIndex = currDialogue.sentenceList.Count - 1;
        currDialogue.sentenceList.RemoveAt(lastIndex);
    }

    // Set default sprite when character is changed.
    public void UpdateSpriteList(int dialogueIndex, CharacterData.Info.Character name)
    {
        Dialogue currDialogue = dialogueList[dialogueIndex];
        for (int i = 0; i < currDialogue.sentenceList.Count; i++)
        {
            currDialogue.sentenceList[i].sprite = characterData.GetCharacterSprites(name)[0];
        }
    }

    public void addDialogue()
    {
        dialogueList.Add(new Dialogue());
        int index = dialogueList.Count - 1;
        addTextBox(index, defaultCharacter);
    }

    public void addDialogue(int index)
    {
        dialogueList.Insert(index, new Dialogue());
        addTextBox(index, defaultCharacter);
    }

    public void addAnswer(int index)
    {
        Dialogue.AnswerChoice answerChoice = new Dialogue.AnswerChoice();
        dialogueList[index].answerChoiceList.Add(answerChoice);
    }

    public void addResponse(int dialogueIndex, int answerIndex)
    {
        Dialogue.Sentence response = new Dialogue.Sentence();
        CharacterData.Info.Character name = dialogueList[dialogueIndex].character.name;
        response.text = "Response";
        response.sprite = characterData.GetCharacterSprites(name)[0];
        dialogueList[dialogueIndex].answerChoiceList[answerIndex].responseList.Add(response);
    }

    public void deleteDialogue()
    {
        if (dialogueList.Count == 0) return;
        dialogueList.RemoveAt(dialogueList.Count - 1);
    }

    public void deleteDialogue(int index)
    {
        dialogueList.RemoveAt(index);
    }

    public void deleteAnswer(int index)
    {
        int answerCount = dialogueList[index].answerChoiceList.Count;

        if (answerCount == 0) return;
        dialogueList[index].answerChoiceList.RemoveAt(answerCount - 1);
    }

    public void deleteResponse(int dialogueIndex, int answerIndex)
    {
        Dialogue.AnswerChoice currAns = dialogueList[dialogueIndex].answerChoiceList[answerIndex];
        int count = currAns.responseList.Count;

        if (count == 0) return;
        currAns.responseList.RemoveAt(count - 1);
    }

    public void ResetDialogue()
    {
        dialogueList.Clear();
    }
}
