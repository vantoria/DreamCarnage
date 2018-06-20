using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCirlce : MonoBehaviour 
{
    public float fadeInTime = 1;
    public float maxSpriteAlpha = 0.5f;

    public float rotateSpeed = 1;
    public float addedToScale = 1;
    public float expandSpeed = 1;
    public float shrinkSpeed = 1;

    float mTimer;
    bool mIsExpand = true;
    Vector2 mDefaultScale, mMaxScale;

    SpriteRenderer sr;

    void Start()
    {
        mDefaultScale = (Vector2)transform.localScale;
        mMaxScale.x += addedToScale;
        mMaxScale.y += addedToScale;

        sr = GetComponent<SpriteRenderer>();
        FadeController.sSingleton.SetAlpha(sr, 0);
        StartCoroutine(FadeIn(fadeInTime));
    }

	void Update () 
    {
		if (BombManager.sSingleton.isTimeStopBomb) return;

        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        Vector2 scale = (Vector2)transform.localScale;
        if (mIsExpand)
        {
            float temp = expandSpeed * Time.deltaTime;
            scale.x += temp;
            scale.y += temp;

            if (scale.x > mMaxScale.x || scale.y > mMaxScale.y)
            {
                scale.x = mMaxScale.x;
                scale.y = mMaxScale.y;
                mIsExpand = false;
            }
        }
        else
        {
            float temp = shrinkSpeed * Time.deltaTime;
            scale.x -= temp;
            scale.y -= temp;

            if (scale.x < mDefaultScale.x || scale.y < mDefaultScale.y)
            {
                scale.x = mDefaultScale.x;
                scale.y = mDefaultScale.y;
                mIsExpand = true;
            }
        }

        transform.localScale = (Vector3)scale;
	}

    IEnumerator FadeIn(float time)
    {
        while (mTimer < time)
        {
            mTimer += Time.deltaTime;
            float alpha = mTimer / time * maxSpriteAlpha;

            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
            yield return null;
        }
        mTimer = 0;
    }
}
