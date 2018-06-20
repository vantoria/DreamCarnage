using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour 
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "CapsuleCollider" || other.tag == TagManager.sSingleton.player1Tag ||
            other.tag == TagManager.sSingleton.player2Tag || other.tag == TagManager.sSingleton.repelStat ||
            other.tag == TagManager.sSingleton.ENV_OBJ_DamagePlayerTag) return;
        other.gameObject.SetActive(false);
    }
}
