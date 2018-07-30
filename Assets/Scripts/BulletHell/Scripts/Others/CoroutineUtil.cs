using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class CoroutineUtil
{
    public static bool isCoroutine = false;

    public static IEnumerator WaitForRealSeconds(float time)
    {
        float timerPause = 0;
        float start = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup < start + time + timerPause)
        {
			while (UIManager.sSingleton.IsPauseGameOverMenu)
            {
                timerPause += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return null;
        }
    }

    public static IEnumerator WaitFor(float duration, Action doLast)
    {
        isCoroutine = true;
        yield return new WaitForSeconds(duration);
        doLast();
        isCoroutine = false;
    }
}
