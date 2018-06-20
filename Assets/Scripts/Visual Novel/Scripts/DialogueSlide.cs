using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DialogueSlide : MonoBehaviour 
{
    public static DialogueSlide sSingleton;

    [System.Serializable]
    public class MovableObject
    {
        public enum Movement
        {
            BTM_TO_TOP = 0,
            TOP_TO_BTM,
            LEFT_TO_RIGHT,
            RIGHT_TO_LEFT
        };

        public Transform objectTrans;
        public Movement objectMove;
        public float offset;
		public float startDelay;
        public float endDelay;

        public MovableObject()
        {
            this.objectTrans = null;
            this.objectMove = Movement.BTM_TO_TOP;
            this.offset = 0.0f;
            this.startDelay = 0.0f;
            this.endDelay = 0.0f;
        }

        public MovableObject(Transform objectTrans, Movement objectMove, float offset, float startDelay, float endDelay)
        {
            this.objectTrans = objectTrans;
            this.objectMove = objectMove;
            this.offset = offset;
			this.startDelay = startDelay;
			this.endDelay = endDelay;
        }
    }
    public List<MovableObject> objectList;

    public float moveTime = 0.5f; 
    public float ansBoxYMult = 1.5f;

    string mDialogueBoxTag, mLeftCharacterTag, mRightCharacterTag, mAnswerBoxTag, mAnswerBoxBgTag;

    RectTransform mAnswerBoxBgRect;
    Vector3 mDefaultAnsBoxBgScale;

	new Camera camera;
    bool mIsAppearing = false;
    IEnumerator coroutine;

    void Awake()
    {
        sSingleton = this;
        mDialogueBoxTag = TagManager.sSingleton.dialogueBoxTag;
        mLeftCharacterTag = TagManager.sSingleton.leftCharacterTag;
        mRightCharacterTag = TagManager.sSingleton.rightCharacterTag;
        mAnswerBoxTag = TagManager.sSingleton.answerBoxTag;
        mAnswerBoxBgTag = TagManager.sSingleton.answerBoxBgTag;
    }

    void Start () 
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        for(int i = 0; i < objectList.Count; i++)
        {
            GetRendererSetAlpha(objectList[i], 0.0f);
            GetCanvasRendererSetAlpha(objectList[i], 0.0f);
            InitialOffset(objectList[i]);

            // Find answerBoxBg and store both transform and scale.
            if (objectList[i].objectTrans.tag == mAnswerBoxTag)
            {
                for (int j = 0; j < objectList[i].objectTrans.childCount; j++)
                {
                    Transform currChild = objectList[i].objectTrans.GetChild(j);
                    if (currChild.tag == mAnswerBoxBgTag)
                    {
                        mAnswerBoxBgRect = currChild.GetComponent<RectTransform>();
                        mDefaultAnsBoxBgScale = mAnswerBoxBgRect.localScale;
                        break;
                    }
                }
            }
        }
	}

    //----------------------------------------------------------------------------------------------
    //-------------------------------------PUBLIC FUNCTIONS-----------------------------------------
    //----------------------------------------------------------------------------------------------

    public bool isAppearing
    {
        get { return this.mIsAppearing; }
    }

    // Slide sprite into screen vertically.
    public void SpriteSlideIn(Transform spriteTrans, Vector3 target, bool isLeft, float moveTime)
	{
        coroutine = SpriteSlideSequence(spriteTrans, target, isLeft, moveTime);
		StartCoroutine(coroutine);
	}

    // Slide sprite out of screen vertically.
    public void SpriteSlideOut(Transform spriteTrans, bool isLeft, float moveTime)
    {
		coroutine = SpriteSlideSequence(spriteTrans, isLeft, moveTime);
        StartCoroutine(coroutine);
    }

    // Put sprite out of the screen instantly.
	public void SpriteOut(Transform spriteTrans, bool isLeft)
	{
		PutSpriteOut (spriteTrans, isLeft);
	}

    // This will slide in dialogue box, (left OR right character).
	public void SlideIn(bool isDialogueBox, bool isLeft)
	{
		for (int i = 0; i < objectList.Count; i++)
		{
			string objTag = objectList [i].objectTrans.tag;
            if ((isDialogueBox && objTag == mDialogueBoxTag) || (isLeft && objTag == mLeftCharacterTag) || 
                (!isLeft && objTag == mRightCharacterTag)) 
			{
                AppearSequence(objectList[i], true, true);
			}
		}
	}

	// Move in/out left/right character with/without delay.
    public void MoveChar(bool isMoveIn, bool isLeft, bool isDelay, float moveTime)
	{
        if(moveTime == 0) moveTime = this.moveTime;

		for (int i = 0; i < objectList.Count; i++)
		{
			string objTag = objectList [i].objectTrans.tag;
            if ((isLeft && objTag == mLeftCharacterTag) || (!isLeft && objTag == mRightCharacterTag)) 
			{
                AppearSequence(objectList[i], isMoveIn, isDelay, moveTime, SetToDialogueShowEnded);
			}
		}
	}

    // Slide in dialogue box and both characters.
    public void AppearAll()
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            if(objectList[i].objectTrans.tag == mAnswerBoxTag) continue;
            AppearSequence(objectList[i], true, true);
        }
    }

    // Slide in answer box.
    public void AppearAnswerBox(int noOfAnswers)
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            if (objectList[i].objectTrans.tag == mAnswerBoxTag)
            { 
                mAnswerBoxBgRect.localScale = mDefaultAnsBoxBgScale;

                Vector3 localScale = mAnswerBoxBgRect.localScale;
                localScale.y = mDefaultAnsBoxBgScale.y + (localScale.y * (noOfAnswers - 1) * ansBoxYMult);
                mAnswerBoxBgRect.localScale = localScale;

                AppearSequence(objectList[i], true, true); 
                break;
            }
        }
    }

    public void CollapseAnswerBox()
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            if (objectList[i].objectTrans.tag == mAnswerBoxTag)
            { 
                AppearSequence(objectList[i], false, true, ResetAnswerBox); 
                break;
            }
        }
    }

    // Slide out dialogue box and both characters.
    public void DisappearAll(bool isResetVal)
    {
        float maxDelay = objectList[0].endDelay;
        for (int i = 0; i < objectList.Count; i++)
        {
            MovableObject currObj = objectList[i];
            if (currObj.endDelay > maxDelay) maxDelay = currObj.endDelay;
            if(IsRendererAlphaZero(currObj) || IsCanvasRendererAlphaZero(currObj)) continue;

            AppearSequence(objectList[i], false, true);
        }
        if(isResetVal) StartCoroutine(WaitThenResetEndDialogue(maxDelay + moveTime));
    }
        
    //----------------------------------------------------------------------------------------------
    //------------------------------------PRIVATE FUNCTIONS-----------------------------------------
    //----------------------------------------------------------------------------------------------

    void InitialOffset(MovableObject thisObj)
    {
        Vector3 pos = thisObj.objectTrans.position;
        float offset = thisObj.offset;

        // Set up their offset-ed position.
        pos = OffsetBasedOnMovement(thisObj.objectMove, pos, offset);
        thisObj.objectTrans.position = pos;
    }

    // Put sprite out the screen instantly.
	void PutSpriteOut(Transform spriteTrans, bool isLeft)
	{
		Vector3 currScreenPos = camera.WorldToScreenPoint(spriteTrans.position);
		float spriteWidth = spriteTrans.GetComponent<SpriteRenderer>().sprite.bounds.size.x * spriteTrans.lossyScale.x * 0.5f;

		currScreenPos.x = 0.0f;
		Vector3 minVect = camera.ScreenToWorldPoint(currScreenPos) - new Vector3(spriteWidth, 0, 0);
		currScreenPos.x = Screen.width;
		Vector3 maxVect = camera.ScreenToWorldPoint(currScreenPos) + new Vector3(spriteWidth, 0, 0);

		Vector3 currPos = spriteTrans.position;

        if (isLeft) currPos.x = minVect.x;
        else currPos.x = maxVect.x;

		spriteTrans.position = currPos;

        for (int i = 0; i < objectList.Count; i++)
        {
            string objTag = objectList [i].objectTrans.tag;
            if ((isLeft && objTag == mLeftCharacterTag) || (!isLeft && objTag == mRightCharacterTag)) 
            {
                GetRendererSetAlpha(objectList [i], 1);
                break;
            }
        }
	}

    // Sprite sliding into camera view.
    IEnumerator SpriteSlideSequence(Transform spriteTrans, Vector3 target, bool isLeft, float moveTime)
    {
        float spriteWidth = spriteTrans.GetComponent<SpriteRenderer>().sprite.bounds.size.x * spriteTrans.lossyScale.x * 0.5f;
        float offset = GetCharOffset(isLeft);

        if (isLeft)
        {
            target.x = target.x + offset;
            offset = target.x - spriteWidth;
        }
        else
        {
            target.x = target.x - offset;
            offset = target.x + spriteWidth;
        }
            
        Vector3 currPos = spriteTrans.position;
        Vector3 defaultPos = currPos;
        float totalTime = 0;

        while (currPos.x < target.x || currPos.x > target.x)
        {
            currPos = defaultPos;
            totalTime += Time.deltaTime;
            currPos.x -= totalTime / moveTime * offset;

            if ((isLeft && currPos.x > target.x) || !isLeft && currPos.x < target.x) currPos.x = target.x;
            spriteTrans.position = currPos;

            yield return null;
        }
        DialogueManager.sSingleton.SetDialogueShowInScreen();
    }

    // Sprite sliding out of camera view.
    IEnumerator SpriteSlideSequence(Transform spriteTrans, bool isLeft, float moveTime)
    {
        Vector3 currScreenPos = camera.WorldToScreenPoint(spriteTrans.position);
        float spriteWidth = spriteTrans.GetComponent<SpriteRenderer>().sprite.bounds.size.x * spriteTrans.lossyScale.x * 0.5f;

        currScreenPos.x = 0.0f;
        Vector3 minVect = camera.ScreenToWorldPoint(currScreenPos) - new Vector3(spriteWidth, 0, 0);
        currScreenPos.x = Screen.width;
        Vector3 maxVect = camera.ScreenToWorldPoint(currScreenPos) + new Vector3(spriteWidth, 0, 0);

        Vector3 currPos = spriteTrans.position;
        Vector3 defaultPos = currPos;
        float offset, totalTime = 0;

        if (isLeft) offset = minVect.x - currPos.x;
        else offset = maxVect.x - currPos.x;

        while (currPos.x > minVect.x && currPos.x < maxVect.x)
        {
            currPos = defaultPos;
            totalTime += Time.deltaTime;
            currPos.x += totalTime / moveTime * offset;

            if (currPos.x < minVect.x) currPos.x = minVect.x;
            else if (currPos.x > maxVect.x) currPos.x = maxVect.x;
            spriteTrans.position = currPos;

            yield return null;
        }
        DialogueManager.sSingleton.SetDialogueShowOutScreen();
    }

    void AppearSequence(MovableObject thisObj, bool isMoveIn, bool isDelay)
    {
        Action emptyAct = () => { };
        coroutine = IEAppearSequence(thisObj, isMoveIn, isDelay, moveTime, emptyAct);
        StartCoroutine(coroutine);
    }

    void AppearSequence(MovableObject thisObj, bool isMoveIn, bool isDelay, Action doLast)
    {
        coroutine = IEAppearSequence(thisObj, isMoveIn, isDelay, moveTime, doLast);
        StartCoroutine(coroutine);
    }

    void AppearSequence(MovableObject thisObj, bool isMoveIn, bool isDelay, float newMoveTime)
    {
        Action emptyAct = () => { };
        coroutine = IEAppearSequence(thisObj, isMoveIn, isDelay, newMoveTime, emptyAct);
        StartCoroutine(coroutine);
    }

    void AppearSequence(MovableObject thisObj, bool isMoveIn, bool isDelay, float newMoveTime, Action doLast)
    {
        coroutine = IEAppearSequence(thisObj, isMoveIn, isDelay, newMoveTime, doLast);
        StartCoroutine(coroutine);
    }   

    // Move object into or out of screen.
    IEnumerator IEAppearSequence (MovableObject thisObj, bool isMoveIn, bool isDelay, float newMoveTime, Action doLast)
    {
        mIsAppearing = true;
		List<Renderer> rendererList = new List<Renderer>();
		List<CanvasRenderer> canvasRendererList = new List<CanvasRenderer>();

        Vector3 pos = thisObj.objectTrans.position;
        Vector3 defaultPos = pos;
        float offset = thisObj.offset;
        float delay = 0;

        if (isMoveIn)
        {
            rendererList = GetRendererSetAlpha(thisObj, 0.0f);
            canvasRendererList = GetCanvasRendererSetAlpha(thisObj, 0.0f);
        }
        else
        {
            rendererList = GetRenderer(thisObj);
            canvasRendererList = GetCanvasRenderer(thisObj);
        }

		if (isDelay) 
		{
            if (isMoveIn) delay = thisObj.startDelay;
			else delay = thisObj.endDelay;
		}
        yield return new WaitForSeconds (delay); 

        float currOffsetVal = 0.0f, totalTime = 0.0f;
        while(currOffsetVal < offset)
        {
            totalTime += Time.deltaTime;
            currOffsetVal = totalTime / newMoveTime * offset;
            pos = defaultPos;
            if (currOffsetVal > offset) currOffsetVal = offset;

            float alphaVal = 0;
            if (isMoveIn)
            {
                pos = OffsetBasedOnMovement(thisObj.objectMove, pos, -currOffsetVal);
                alphaVal = currOffsetVal / offset;
            }
            else if (!isMoveIn)
            {
                pos = OffsetBasedOnMovement(thisObj.objectMove, pos, currOffsetVal);
                alphaVal = 1 - (currOffsetVal / offset);
            }

            thisObj.objectTrans.position = pos;

            // Fade in/out sequence. Change the alpha.
			for (int i = 0; i < rendererList.Count; i++)
			{
                Renderer rend = rendererList[i];
                SetAlpha(ref rend, alphaVal);
			}

			for (int i = 0; i < canvasRendererList.Count; i++)
			{ canvasRendererList[i].SetAlpha(alphaVal); }

            yield return null; 
        }

        if (isMoveIn)
        {
            for (int i = 0; i < rendererList.Count; i++)
            {
                Renderer rend = rendererList[i];
                SetAlpha(ref rend, 1);
            }

            for (int i = 0; i < canvasRendererList.Count; i++)
            { canvasRendererList[i].SetAlpha(1); }
        }
        else
        {
            for (int i = 0; i < rendererList.Count; i++)
            {
                Renderer rend = rendererList[i];
                SetAlpha(ref rend, 0);
            }

            for (int i = 0; i < canvasRendererList.Count; i++)
            { canvasRendererList[i].SetAlpha(0); }
        }

        doLast();
        mIsAppearing = false;
    }

    IEnumerator WaitThenResetEndDialogue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DialogueManager.sSingleton.ResetEndDialogue();
    }

    void ResetAnswerBox()
    {
        DialogueManager.sSingleton.ResetAnswerBox();
    }

    Vector3 OffsetBasedOnMovement(MovableObject.Movement movement, Vector3 pos, float offsetVal)
    {
        if (movement == MovableObject.Movement.BTM_TO_TOP) pos.y -= offsetVal;
        else if(movement == MovableObject.Movement.TOP_TO_BTM) pos.y += offsetVal;
        else if(movement == MovableObject.Movement.LEFT_TO_RIGHT) pos.x -= offsetVal;
        else if(movement == MovableObject.Movement.RIGHT_TO_LEFT) pos.x += offsetVal;
        return pos;
    }

    // Get character's offset.
    float GetCharOffset(bool isLeft)
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            string objTag = objectList [i].objectTrans.tag;
            if (isLeft && objTag == mLeftCharacterTag || !isLeft && objTag == mRightCharacterTag)
                return objectList[i].offset;
        }
        return 0;
    }

    void SetToDialogueShowEnded()
    {
        DialogueManager.sSingleton.SetToDialogueShowEnded();
    }

    //----------------------------------------------------------------------------------------------
    //---------------------------------ALPHA-RELATED FUNCTIONS--------------------------------------
    //----------------------------------------------------------------------------------------------

    List<Renderer> GetRenderer(MovableObject thisObj)
	{
		List<Renderer> rendererList = new List<Renderer>();
		Transform objTransform = thisObj.objectTrans;

		if (objTransform.GetComponent<Renderer>() != null) rendererList.Add(objTransform.GetComponent<Renderer>());
		else
		{
			for (int i = 0; i < objTransform.childCount; i++)
			{
				Transform child = objTransform.GetChild(i);
				if (child.GetComponent<Renderer>() != null) rendererList.Add(child.GetComponent<Renderer>());
			}
		}
		return rendererList;
	}

    List<Renderer> GetRendererSetAlpha(MovableObject thisObj, float alphaVal)
    {
		List<Renderer> rendererList = GetRenderer(thisObj);

        for (int i = 0; i < rendererList.Count; i++)
        {
            Renderer rend = rendererList[i];
            SetAlpha(ref rend, alphaVal);
        }

        return rendererList;
    }

    List<CanvasRenderer> GetCanvasRenderer(MovableObject thisObj)
    {
        List<CanvasRenderer> canvasRendererList = new List<CanvasRenderer>();
        Transform objTransform = thisObj.objectTrans;

        CanvasRenderer[] canvasRendArray = objTransform.GetComponentsInChildren<CanvasRenderer>();
        for (int i = 0; i < canvasRendArray.Length; i++)
        { canvasRendererList.Add(canvasRendArray[i]); }

        return canvasRendererList;
    }

    List<CanvasRenderer> GetCanvasRendererSetAlpha(MovableObject thisObj, float alphaVal)
    {
        List<CanvasRenderer> canvasRendererList = GetCanvasRenderer(thisObj);

        for (int i = 0; i < canvasRendererList.Count; i++)
        { canvasRendererList[i].SetAlpha(alphaVal);  }

        return canvasRendererList;
    }

    bool IsRendererAlphaZero(MovableObject thisObj)
	{
		List<Renderer> rendererList = GetRenderer(thisObj);

		for (int i = 0; i < rendererList.Count; i++)
		{
			if(rendererList[i].material.color.a <= 0.0f) return true;
		}
		return false;
	}

    bool IsCanvasRendererAlphaZero(MovableObject thisObj)
    {
        List<CanvasRenderer> canvasRendererList = GetCanvasRenderer(thisObj);

        for (int i = 0; i < canvasRendererList.Count; i++)
        {
            if(canvasRendererList[i].GetAlpha() <= 0.0f) return true;
        }
        return false;
    }

    void SetAlpha(ref Renderer renderer, float alphaVal)
    {
        Color newColor = renderer.material.color;
        newColor.a = alphaVal;
        renderer.material.SetColor("_Color", newColor); 
    }
}
