using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropController : MonoBehaviour 
{
    public enum Item
    {
        POWERUP1 = 0,
        POWERUP2,
        LIFE,
        SCORE1,
        SCORE2,
    }
    public Item ItemDrop = Item.LIFE;

    public int dropAmount;
    public float speedToFlyOut = 5;
    public float speedSlowDown = 8;
//    public float radius = 1;

    public void ItemDropFunc()
    {
        float angleTemp = 360f / dropAmount;
        float angle = 0f;

        for (var i = 0; i < dropAmount; i++)
        {
//            float dropXPos = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180) * radius;
//            float dropYPos = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180) * radius;
//            Vector3 DropVector = new Vector3(dropXPos, dropYPos, 0);

            Transform trans = null;
            if (ItemDrop == Item.POWERUP1) trans = EnvObjManager.sSingleton.GetSmallPowerUp();
            else if (ItemDrop == Item.POWERUP2) trans = EnvObjManager.sSingleton.GetBigPowerUp();
            else if (ItemDrop == Item.LIFE) trans = EnvObjManager.sSingleton.GetLifePickUp();
            else if (ItemDrop == Item.SCORE1) trans = EnvObjManager.sSingleton.GetScorePickUp();
            else if (ItemDrop == Item.SCORE2) trans = EnvObjManager.sSingleton.GetBigScorePickUp();

            if (trans != null)
            {
                trans.position = transform.position;

                Vector2 target = new Vector3(transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180), transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180));
                Vector3 dir = (target - (Vector2)transform.position).normalized;
//
                trans.GetComponent<EnvironmentalObject>().SetToFlyOut(dir, speedToFlyOut, speedSlowDown);
                trans.gameObject.SetActive(true);
//                trans.gameObject.transform.Translate(DropVector);
            }
            angle += angleTemp;
            // future to do co-routine to push gameObject out
        }
    }
}

