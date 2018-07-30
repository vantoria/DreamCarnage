using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInMoveOut : MonoBehaviour 
{
    public float horizontalMoveValue;
    public float horizontalSpeed;
    public float stayDuration;
    public float verticalMoveValue;
    public float verticalSpeed;

    public enum State
    {
        NONE = 0,
        SLIDE_IN,
        SLIDE_OUT
    }
    public State state = State.NONE;

    Vector3 mDefaultPos;
    bool mIsCoroutine = false;

	void Start () 
    {
        mDefaultPos = transform.position;
	}
	
	void Update () 
    {
        if (state == State.SLIDE_IN)
        {
            if (!mIsCoroutine)
                StartCoroutine(SlideInSequence());
        }
	}

    public void SlideIn()
    {
        state = State.SLIDE_IN;
    }

    IEnumerator SlideInSequence()
    {
        mIsCoroutine = true;

        float endHorizontalVal = transform.position.x + horizontalMoveValue;
        while (transform.position.x != endHorizontalVal)
        {
            float currentMoveVal = Time.deltaTime * horizontalSpeed;

            if (horizontalMoveValue < 0) currentMoveVal = -currentMoveVal;

            // If it's right moving to left.
            if (endHorizontalVal < transform.position.x)
            {
                if (transform.position.x + currentMoveVal < endHorizontalVal)
                    currentMoveVal = endHorizontalVal - transform.position.x;
            }
            // If it's left moving to right.
            else if (endHorizontalVal > transform.position.x)
            {
                if (transform.position.x + currentMoveVal > endHorizontalVal)
                    currentMoveVal = -(transform.position.x - endHorizontalVal);
            }

            Vector3 pos = transform.position;
            pos.x += currentMoveVal;
            transform.position = pos;

            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);
        state = State.SLIDE_OUT;

        float endVerticalVal = transform.position.y + verticalMoveValue;
        while (transform.position.y != endVerticalVal)
        {
            float currentMoveVal = Time.deltaTime * verticalSpeed;

            if (verticalMoveValue < 0) currentMoveVal = -currentMoveVal;

            // If it's up moving to down.
            if (endVerticalVal < transform.position.y)
            {
                if (transform.position.y + currentMoveVal < endVerticalVal)
                    currentMoveVal = endVerticalVal - transform.position.y;
            }
            // If it's down moving to yp.
            else if (endVerticalVal > transform.position.y)
            {
                if (transform.position.y + currentMoveVal > endVerticalVal)
                    currentMoveVal = -(transform.position.y - endVerticalVal);
            }

            Vector3 pos = transform.position;
            pos.y += currentMoveVal;
            transform.position = pos;

            yield return null;
        }

        state = State.NONE;
        mIsCoroutine = false;
    }
}
