using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPrefabData : ScriptableObject 
{
    public List<Transform> plyMainBulletTransList = new List<Transform>();
    public List<Transform> plySecondaryBulletTransList = new List<Transform>();
    public List<Transform> plyLaserTransNoCacheList = new List<Transform>();
    public List<Transform> enemyBulletTransList = new List<Transform>();
}
