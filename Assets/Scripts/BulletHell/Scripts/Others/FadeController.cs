using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour 
{
    public static FadeController sSingleton { get { return _sSingleton; } }
    static FadeController _sSingleton;
	
    float timer;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    public void SetAlpha(SpriteRenderer sr, float val)
    {
        Color color = sr.color;
        color.a = val;
        sr.color = color;
    }
}
