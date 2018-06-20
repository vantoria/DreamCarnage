using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PotraitShowManager : MonoBehaviour 
{
    public static PotraitShowManager sSingleton { get { return _sSingleton; } }
    static PotraitShowManager _sSingleton;

    public Transform charPotraitTrans;
    public Transform spellCardTrans;

    [System.Serializable]
    public class Potrait
    {
        public enum Stay
        {
            NONE = 0,
            CONTINUE_MOVE
        }

        public enum Closing
        {
            NONE = 0,
            EXPAND,
            MOVE_AND_FLATTEN
        }

        public Sprite sprite;
        public Transform startPlacement;
        public Vector2 moveDirection;
        public float offset;
        public float maxAlpha;
        public float moveTime;
        public Stay stayMethod;
        public float stayMoveSpeed;
        public float stayDuration;
        public Closing closeMethod;
        public float closeMoveSpeed;
        public float closeSpeed;
        [HideInInspector] public bool isCoroutine;

        public Potrait()
        {
            this.sprite = null;
            this.startPlacement = null;
            this.moveDirection = Vector2.zero;
            this.offset = 0;
            this.maxAlpha = 1;
            this.moveTime = 0;
            this.stayMethod = Stay.NONE;
            this.stayMoveSpeed = 1;
            this.stayDuration = 0;
            this.closeMethod = Closing.NONE;
            this.closeMoveSpeed = 1;
            this.closeSpeed = 1;
            this.isCoroutine = false;
        }
    }
    public List<Potrait> potraitList = new List<Potrait>();

    SpriteRenderer mCharSR, mSpellCardSR;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Start()
    {
        mCharSR = charPotraitTrans.GetComponent<SpriteRenderer>();
        mSpellCardSR = spellCardTrans.GetComponent<SpriteRenderer>();
    }

    public void RunShowPotrait(Sprite charSprite, Sprite spellCardSprite)
    {
        potraitList[0].sprite = charSprite;
        potraitList[1].sprite = spellCardSprite;

        if (!potraitList[0].isCoroutine && !potraitList[1].isCoroutine)
        {
            ShowPotrait(mCharSR, potraitList[0]);
            ShowPotrait(mSpellCardSR, potraitList[1]);
        }
    }

    void ShowPotrait(SpriteRenderer main, Potrait potrait)
    {
        // Add sprite and set alpha to 0.
        main.sprite = potrait.sprite;
        main.enabled = true;
        FadeController.sSingleton.SetAlpha(main, 0);

        main.transform.position = potrait.startPlacement.position;
        StartCoroutine(IEAppearSequence(main, potrait));
    }

    // Move sprite into and out of screen.
    IEnumerator IEAppearSequence (SpriteRenderer sr, Potrait potrait)
    {
        potrait.isCoroutine = true;
        Vector3 defaultPos = sr.transform.position;

        float alphaVal = 0, currOffsetVal = 0, totalTime = 0;
        while(currOffsetVal < potrait.offset)
        {
            totalTime += Time.unscaledDeltaTime;
            currOffsetVal = totalTime / potrait.moveTime * potrait.offset;
            sr.transform.position = defaultPos;

            if (currOffsetVal > potrait.offset) currOffsetVal = potrait.offset;

            sr.transform.position = OffsetPosition(sr.transform.position, potrait.moveDirection, currOffsetVal);
            alphaVal = currOffsetVal / potrait.offset;

            if (alphaVal > potrait.maxAlpha) alphaVal = potrait.maxAlpha;
            FadeController.sSingleton.SetAlpha(sr, alphaVal);

            yield return null; 
        }

        if (potrait.stayMethod == Potrait.Stay.NONE)
            yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(potrait.stayDuration));
        else if (potrait.stayMethod == Potrait.Stay.CONTINUE_MOVE)
        {
            float timer = 0;
            while (timer < potrait.stayDuration)
            {
                timer += Time.unscaledDeltaTime;
                sr.transform.position = OffsetPosition(sr.transform.position, potrait.moveDirection, Time.unscaledDeltaTime * potrait.stayMoveSpeed);
                yield return null; 
            }
        }

        Transform currTrans = sr.transform;
        Vector3 defaultScale = currTrans.localScale;

        if (potrait.closeMethod == Potrait.Closing.EXPAND)
        {
            alphaVal = sr.color.a;
            while (sr.color.a > 0)
            {
                Vector3 temp = currTrans.localScale;
                temp.x += Time.unscaledDeltaTime * potrait.closeSpeed;
                temp.y += Time.unscaledDeltaTime * potrait.closeSpeed;
                currTrans.localScale = temp;

                alphaVal -= Time.unscaledDeltaTime;
                FadeController.sSingleton.SetAlpha(sr, alphaVal);

                yield return null; 
            }
        }
        else if (potrait.closeMethod == Potrait.Closing.MOVE_AND_FLATTEN)
        {
            float totalYScale = currTrans.localScale.y;
            while(currTrans.localScale.y > 0)
            {
                sr.transform.position = OffsetPosition(sr.transform.position, potrait.moveDirection, Time.unscaledDeltaTime * potrait.closeMoveSpeed);

                Vector3 temp = currTrans.localScale;
                temp.y -= Time.unscaledDeltaTime * potrait.closeSpeed;
                if (temp.y < 0) temp.y = 0;
                currTrans.localScale = temp;

                alphaVal = temp.y / totalYScale;
                FadeController.sSingleton.SetAlpha(sr, alphaVal);

                yield return null; 
            }
        }

        currTrans.localScale = defaultScale;
        sr.enabled = false;
        potrait.isCoroutine = false;
    }

    Vector3 OffsetPosition(Vector2 pos, Vector2 dir, float offsetVal)
    {
        if (dir.y > 0) pos.y -= offsetVal;
        else if(dir.y < 0) pos.y += offsetVal;
        else if(dir.x < 0) pos.x -= offsetVal;
        else if(dir.x > 0) pos.x += offsetVal;
        return pos;
    }
}
