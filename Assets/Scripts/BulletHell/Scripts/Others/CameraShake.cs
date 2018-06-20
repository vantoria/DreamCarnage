///Daniel Moore (Firedan1176) - Firedan1176.webs.com/
///26 Dec 2015
///
///Shakes camera parent object

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraShake : MonoBehaviour 
{
    public static CameraShake sSingleton { get { return _sSingleton; } }
    static CameraShake _sSingleton;

    public float shakeAmount;//The amount to shake this frame.
    public float shakeDuration;//The duration this frame.

    //Readonly values...
    float shakePercentage;//A percentage (0-1) representing the amount of shake to be applied when setting rotation.
    float startAmount;//The initial shake amount (to determine percentage), set when ShakeCamera is called.
    float startDuration;//The initial shake duration, set when ShakeCamera is called.

    float savedStartAmount;
    float savedStartDuration;

    bool isRunning = false; //Is the coroutine running right now?

    public bool smooth;//Smooth rotation?
    public float smoothAmount = 5f;//Amount to smooth

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Start () 
    {
        savedStartAmount = shakeAmount;
        savedStartDuration = shakeDuration;
    }

    public void ShakeCamera() 
    {
        if (isRunning) return;

        startAmount = savedStartAmount;//Set default (start) values
        startDuration = savedStartDuration;//Set default (start) values
        shakeDuration = savedStartDuration;
        shakePercentage = 0;

        StartCoroutine (Shake());
    }

    public bool IsRunning { get { return isRunning; } } 

    IEnumerator Shake() 
    {
        isRunning = true;
        while (shakeDuration > 0.01f) {
            Vector3 rotationAmount = UnityEngine.Random.insideUnitSphere * shakeAmount;//A Vector3 to add to the Local Rotation
            rotationAmount.z = 0;//Don't change the Z; it looks funny.

            shakePercentage = shakeDuration / startDuration;//Used to set the amount of shake (% * startAmount).

            shakeAmount = startAmount * shakePercentage;//Set the amount of shake (% * startAmount).
            shakeDuration = Mathf.Lerp(shakeDuration, 0, Time.unscaledDeltaTime);//Lerp the time, so it is less and tapers off towards the end.

            if(smooth)
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotationAmount), Time.deltaTime * smoothAmount);
            else
                transform.localRotation = Quaternion.Euler (rotationAmount);//Set the local rotation the be the rotation amount.

            yield return null;
        }
        transform.localRotation = Quaternion.identity;//Set the local rotation to 0 when done, just to get rid of any fudging stuff.
        isRunning = false;
    }
}
