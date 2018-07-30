using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoFadeInOut : MonoBehaviour 
{
	public float moveSpeed = 1;
	public float fadeSpeed = 1;
	public float stayPeriod = 0.5f;

	Text currText;
	bool mIsCoroutine = false;

	void Start () 
	{
		currText = GetComponent<Text> ();
	}
	
	void Update () 
	{
		if (!mIsCoroutine) StartCoroutine(FadeInMoveUpThenOut ());
	}

	IEnumerator FadeInMoveUpThenOut()
	{
		mIsCoroutine = true;
		while (currText.color.a != 1)
		{
			Vector3 pos = currText.transform.position;
			pos.y += Time.deltaTime * moveSpeed;
			currText.transform.position = pos;

			Color color = currText.color;
			color.a += Time.deltaTime * fadeSpeed;
			if (color.a > 1) color.a = 1;

			currText.color = color;
			yield return null;
		}

		yield return new WaitForSeconds (stayPeriod);

		while (currText.color.a != 0)
		{
			Color color = currText.color;
			color.a -= Time.deltaTime * fadeSpeed;
			if (color.a < 0) color.a = 0;

			currText.color = color;

			yield return null;
		}
		mIsCoroutine = false;
		gameObject.SetActive (false);
	}
}
