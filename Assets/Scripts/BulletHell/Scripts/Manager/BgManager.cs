using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgManager : MonoBehaviour 
{
    public List<Transform> bgList = new List<Transform>();
    public float scrollSpeed = 1;

    float height, halfHeight, bottomOfScreen;

	void Start () 
    {
        float distance = (transform.position - Camera.main.transform.position).z;
        bottomOfScreen = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, distance)).y;

        Transform firstBg = bgList[0];
        height = firstBg.GetComponent<Renderer>().bounds.size.y;
        halfHeight = height / 2;

        Vector3 pos = firstBg.position;
        pos.y += height;

        Transform trans = Instantiate(firstBg, pos, Quaternion.identity);
        bgList.Add(trans);
	}
	
	void Update () 
    {
        int index = -1;
        for (int i = 0; i < bgList.Count; i++)
        {
            Transform bg = bgList[i];
            Vector3 pos = bg.transform.position;
            pos.y -= Time.deltaTime * scrollSpeed;
            bg.transform.position = pos;

            float currBgTop = bg.transform.position.y + halfHeight;
            if (currBgTop < bottomOfScreen) index = i;
        }

        if (index != -1)
        {
            int nextIndex = -1;
            if (index - 1 < 0) nextIndex = bgList.Count - 1;
            else nextIndex = index - 1;

            float nextBgTop = bgList[nextIndex].transform.position.y + halfHeight;

            Transform bg = bgList[index];
            Vector3 pos = bg.transform.position;
            pos.y = nextBgTop + halfHeight;
            bg.transform.position = pos;

            index = -1;
        }
	}
}
