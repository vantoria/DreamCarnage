using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WithinRange : MonoBehaviour 
{
	public List<Transform> hitList = new List<Transform>();

    Transform mPlayerTrans;

	void Start () 
    {
        mPlayerTrans = transform.parent;
	}

    public Transform GetClosestEnemy()
    {
        if (hitList.Count == 0) return null;

        Transform closestTrans = hitList[0];
        float minSqrLength = (mPlayerTrans.position - hitList[0].position).sqrMagnitude;

        for (int i = 1; i < hitList.Count; i++)
        {
            float magnitude = (mPlayerTrans.position - hitList[i].position).sqrMagnitude;
            if (magnitude < minSqrLength)
            {
                closestTrans = hitList[i];
                minSqrLength = magnitude;
            }
        }

//        Vector2 dir = closestTrans.position - mPlayerTrans.position;
        return closestTrans;
    }

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == TagManager.sSingleton.enemyTag) 
        {
			hitList.Add (other.transform);
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		hitList.Remove (other.transform);
	}
}
