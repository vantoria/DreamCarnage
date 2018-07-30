using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour 
{
    public static TutorialManager sSingleton { get { return _sSingleton; } }
    static TutorialManager _sSingleton;

    public List<Transform> tutorials = new List<Transform>();
    public float fadeTime;

    int curCount = 0;
    float mHorizontalP1, mHorizontalP1Dpad;
    bool isLeft, isRight, mIsFadeIn, mIsFadeOut, mIsEnableTutorial;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Update() 
    {
        if (!mIsEnableTutorial) return;
            
        mHorizontalP1 = Input.GetAxis("HorizontalP1");
        mHorizontalP1Dpad = Input.GetAxis("HorizontalP1Dpad");

        if (mHorizontalP1 < 0 || mHorizontalP1Dpad < 0)
        {
            isLeft = true;
            isRight = false;
        }
        else if (mHorizontalP1 > 0  || mHorizontalP1Dpad > 0)
        {
            isLeft = false;
            isRight = true;
        }
        else
        {
            isRight = false;
            isLeft = false;
        }

        if (tutorials != null && !mIsFadeIn && !mIsFadeOut)
        {
            if (isRight == true && curCount < tutorials.Count - 1)
            {
                // Right page.
                Image prevImage = tutorials[curCount].gameObject.GetComponent<Image>();
                Image nextImage = tutorials[curCount + 1].gameObject.GetComponent<Image>();

                StartCoroutine(FadeOut(fadeTime, prevImage));
                StartCoroutine(FadeIn(fadeTime, nextImage));
                curCount += 1;
            }
            else if (isLeft == true && curCount > 0)
            {
                // Left page.
                Image prevImage = tutorials[curCount].gameObject.GetComponent<Image>();
                Image nextImage = tutorials[curCount - 1].gameObject.GetComponent<Image>();

                StartCoroutine(FadeOut(fadeTime, prevImage));
                StartCoroutine(FadeIn(fadeTime, nextImage));
                curCount -= 1;
            }
        }
    }

    public void SetEnableTutorial()
    { 
        mIsEnableTutorial = true; 
        tutorials[curCount].gameObject.SetActive(true);
    }

    IEnumerator FadeIn(float t, Image item)
    {
        mIsFadeIn = true;
        item.color = new Color(item.color.r, item.color.g, item.color.b, 0);

        while(item.color.a < 1)
        {
            // item transparent color = 1;
            item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a + (Time.deltaTime / t));
            yield return null;
        }

        if (item.color.a >1) item.color = new Color(item.color.r, item.color.g, item.color.b, 1);
        mIsFadeIn = false;
    }

    IEnumerator FadeOut(float t, Image item)
    {
        mIsFadeOut = true;
        item.color = new Color(item.color.r, item.color.g, item.color.b, 1);

        while (item.color.a > 0)
        {
            // item transparent = 0
            item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a - (Time.deltaTime / t));
            yield return null;
        }

        if (item.color.a < 0) item.color = new Color(item.color.r, item.color.g, item.color.g, 0);
        mIsFadeOut = false;
    } 
}
