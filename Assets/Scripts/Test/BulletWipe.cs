using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWipe : MonoBehaviour 
{
    public float circleRadiusSpeed = 5.0f;

//    List<int> mHitID = new List<int>();

    ParticleSystem mParticleSys;
    CircleCollider2D mCircleCollider;

    float mDefaultRadius;
    bool mIsPlay = false;

    BombController mBombController;

	void Awake () 
    {
        mParticleSys = GetComponent<ParticleSystem>();
        mCircleCollider = GetComponent<CircleCollider2D>();

        mDefaultRadius = mCircleCollider.radius;
	}
	
	void Update () 
    {
        if (mIsPlay)
        {
            if (mParticleSys.isStopped)
            {
                mIsPlay = false;
                mCircleCollider.radius = mDefaultRadius;
                mBombController.DeactivateBulletWipe();
            }
            else mCircleCollider.radius += Time.deltaTime * circleRadiusSpeed;
        }
	}

    public void SetOwnerBombCtrl(BombController bombCtrl) { mBombController = bombCtrl; }

    public void Activate()
    {
        if (!mIsPlay)
        {
            mIsPlay = true;
			mParticleSys.transform.position = mBombController.transform.position;
            mParticleSys.Play();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == TagManager.sSingleton.enemyBulletTag)
            other.gameObject.SetActive(false);
//        else if (other.tag == TagManager.sSingleton.enemyTag)
//        {
//            for (int i = 0; i < mHitID.Count; i++)
//            {
//                if (other.gameObject.GetInstanceID() == mHitID[i]) return;
//            }
//            mHitID.Add(other.gameObject.GetInstanceID());
//        }
    }
}
