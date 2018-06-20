using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvObjPrefabData : ScriptableObject 
{
    public List<Transform> pickableObjTransList = new List<Transform>();
    public List<Transform> hazardTransList = new List<Transform>();
}
