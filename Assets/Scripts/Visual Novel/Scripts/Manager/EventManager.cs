using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class EventManager : MonoBehaviour 
{
    public void OnAnswerButtonClick()
    {
        GameObject go = EventSystem.current.currentSelectedGameObject;
        if (go != null)
        {
            char lastChar = go.name[go.name.Length - 1];
            int val = (int) Char.GetNumericValue(lastChar);

            if(val != -1) DialogueManager.sSingleton.AnswerClicked(val);
        }
    }
}
