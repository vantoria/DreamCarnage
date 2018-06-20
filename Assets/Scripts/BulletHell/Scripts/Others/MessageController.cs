using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour 
{
    public Transform messagePrefab;
    public float moveSpeed = 1;
    public bool isLeftMoveOut = true;

    class Message
    {
        public Transform trans;
        public bool isCoroutine;

        public Message()
        {
            trans = null;
            isCoroutine = false;
        }

        public Message(Transform trans, bool isCoroutine)
        {
            this.trans = trans;
            this.isCoroutine = isCoroutine;
        }
    }
    List<Message> mMessageList = new List<Message>();

	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsAllMoveComplete())
        {
            // Push down other messages first.
            for (int i = 0; i < mMessageList.Count; i++)
            {
                if(!mMessageList[i].isCoroutine) StartCoroutine(SlideDown(mMessageList[i]));
            }

            Transform trans = Instantiate(messagePrefab, transform, false);
            Message msg = new Message(trans, false);
            mMessageList.Add(msg);

            StartCoroutine(SlideDown(msg));
        }
	}

    public void RemoveMessageList(Transform trans)
    {
        mMessageList.RemoveAt(0);
    }

    IEnumerator SlideDown(Message msg)
    {
        msg.isCoroutine = true;
        Vector3 finalPos = msg.trans.position;
        finalPos.y -= 1f;

        while(msg.trans != null && msg.trans.position.y != finalPos.y)
        {
            Vector3 pos = msg.trans.position;
            pos.y -= Time.deltaTime * moveSpeed;

            if (pos.y < finalPos.y) pos.y = finalPos.y;
            msg.trans.position = pos;

            yield return null;
        }
        msg.isCoroutine = false;
    }

    bool IsAllMoveComplete()
    {
        for (int i = 0; i < mMessageList.Count; i++)
        {
            if (mMessageList[i].isCoroutine) return false;
        }
        return true;
    }
}
