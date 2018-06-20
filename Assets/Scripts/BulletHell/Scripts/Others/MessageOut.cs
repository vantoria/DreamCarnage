using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageOut : MonoBehaviour 
{
    public float waitTime = 3.0f;
    public float moveSpeed = 1;
    public float x_Offset = 5;

    float mWaitTimer;
    bool mIsCoroutine = false;
    MessageController mMessageCtrl;

    void Start()
    {
        mMessageCtrl = transform.GetComponentInParent<MessageController>();
    }

	void Update () 
    {
        mWaitTimer += Time.deltaTime;
        if (mWaitTimer > waitTime && !mIsCoroutine)
        {
            StartCoroutine(SlideOut());
        }
	}

    IEnumerator SlideOut()
    {
        mIsCoroutine = true;

        if (mMessageCtrl.isLeftMoveOut) x_Offset = -Mathf.Abs(x_Offset);
        else x_Offset = Mathf.Abs(x_Offset);

        Vector3 finalPos = transform.position;
        finalPos.x += x_Offset;

        while(transform.position.x != finalPos.x)
        {
            float val = 0;
            if (mMessageCtrl.isLeftMoveOut) val = -Mathf.Abs(Time.deltaTime * moveSpeed);
            else val = Mathf.Abs(Time.deltaTime * moveSpeed);

            Vector3 pos = transform.position;
            pos.x += val;

            if ((mMessageCtrl.isLeftMoveOut && pos.x < finalPos.x) || (!mMessageCtrl.isLeftMoveOut && pos.x > finalPos.x)) 
                pos.x = finalPos.x;
            
            transform.position = pos;
            yield return null;
        }

        mMessageCtrl.RemoveMessageList(transform);
        mIsCoroutine = false;
        Destroy(gameObject);
    }
}
