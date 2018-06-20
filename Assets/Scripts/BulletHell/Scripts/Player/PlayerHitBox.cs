using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour 
{
    PlayerController mPlayerController;
    RepelStatus mRepelStatus;

    void Start()
    {
        mPlayerController = GetComponentInParent<PlayerController>();
        mRepelStatus = transform.parent.GetComponentInChildren<RepelStatus>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.enemyBulletTag)
        {
            BulletMove bulletMove = other.GetComponent<BulletMove>();
            if (bulletMove.GetBulletDamage != 0) mPlayerController.GetDamaged();

            if (bulletMove.GetTemplate == AttackPattern.Template.SHOCK_REPEL_AND_TRAP_LASER && bulletMove.GetStatRepelDur > 0) 
                mRepelStatus.SetStatRepel(bulletMove.GetAttackProperties);

            if (!mPlayerController.IsInvinsible) other.gameObject.SetActive(false);
        }
        else if (other.tag == TagManager.sSingleton.ENV_OBJ_PowerUp1Tag || other.tag == TagManager.sSingleton.ENV_OBJ_PowerUp2Tag)
        {
            float val = other.GetComponent<EnvironmentalObject>().value;
            mPlayerController.GetPowerUp(val); 
            other.gameObject.SetActive(false);
        }
		else if (other.tag == TagManager.sSingleton.ENV_OBJ_LifePickUpTag)
        {
            mPlayerController.PlusLife();
            other.gameObject.SetActive(false);
        }
		else if (other.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp1Tag || other.tag == TagManager.sSingleton.ENV_OBJ_ScorePickUp2Tag)
        {
			int val = (int)other.GetComponent<EnvironmentalObject>().value;
			mPlayerController.UpdateScore (val);
            other.gameObject.SetActive(false);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.ENV_OBJ_RockTag || other.tag == TagManager.sSingleton.ENV_OBJ_DamagePlayerTag) 
            mPlayerController.GetDamaged();
    }
}
