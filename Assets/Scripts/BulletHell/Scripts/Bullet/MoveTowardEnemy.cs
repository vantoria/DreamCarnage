using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardEnemy : MonoBehaviour 
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.enemyTag)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            GetComponentInParent<BulletMove>().SetDirection(dir);
        }
    }
}
