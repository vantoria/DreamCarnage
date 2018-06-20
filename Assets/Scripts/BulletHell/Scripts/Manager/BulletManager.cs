using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour 
{
    public static BulletManager sSingleton 
    { 
        get 
        { 
            if (_sSingleton == null)
                _sSingleton = (BulletManager)FindObjectOfType(typeof(BulletManager));
            return _sSingleton; 
        } 
    }
    static BulletManager _sSingleton;

    public enum GroupIndex
    {
        PLAYER_MAIN = 0,
        PLAYER_SECONDARY,
        ENEMY
    }

    public BulletPrefabData bulletPrefabData;
    List<int> plyCurrMainBulIndexList = new List<int>();
    List<int> plyCurrSecondBulIndexList = new List<int>();
    List<int> enemyCurrBulIndexList = new List<int>();

    List<List<Transform>> mPlyMainBulletList = new List<List<Transform>>();
    List<List<Transform>> mPlySecondaryBulletList = new List<List<Transform>>();
    List<List<Transform>> mEnemyBulletList = new List<List<Transform>>();

    bool mIsDisableSpawnBullet = false;
    float mDisableSpawnBulletTimer = 0, mDisableSpawnBulletTime = 0;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }

    void Start()
    {
        for (int i = 0; i < mPlyMainBulletList.Count; i++)
        {
            plyCurrMainBulIndexList.Add(0);
        }
        for (int i = 0; i < mPlySecondaryBulletList.Count; i++)
        {
            plyCurrSecondBulIndexList.Add(0);
        }
        for (int i = 0; i < mEnemyBulletList.Count; i++)
        {
            enemyCurrBulIndexList.Add(0);
        }
    }

    void Update()
    {
        if (mIsDisableSpawnBullet)
        {
            mDisableSpawnBulletTimer += Time.deltaTime;

            if (mDisableSpawnBulletTimer >= mDisableSpawnBulletTime)
            {
                mDisableSpawnBulletTimer = 0;
                mIsDisableSpawnBullet = false;
            }
        }
    }

    public void SetBulletTag(GroupIndex groupIndex, int index, string tag)
    {
        if (groupIndex == GroupIndex.PLAYER_MAIN)
        {
            for (int i = 0; i < mPlyMainBulletList[index].Count; i++)
            {
                mPlyMainBulletList[index][i].tag = tag;
            }
        }
        else if (groupIndex == GroupIndex.PLAYER_SECONDARY)
        {
            for (int i = 0; i < mPlySecondaryBulletList[index].Count; i++)
            {
                mPlySecondaryBulletList[index][i].tag = tag;
            }
        }
        else if (groupIndex == GroupIndex.ENEMY)
        {
            for (int i = 0; i < mEnemyBulletList[index].Count; i++)
            {
                mEnemyBulletList[index][i].tag = tag;
            }
        }
    }

    public bool IsDisableSpawnBullet { get { return mIsDisableSpawnBullet; } }

    public void InstantiateAndCacheBullet(Transform currBullet, int total, int groupIndex)
    {
        // Group name.
        GameObject go = new GameObject();
        go.name = currBullet.name;   

        List<Transform> sameBulletList = new List<Transform>();
        for (int i = 0; i < total; i++)
        {
            Transform trans = Instantiate(currBullet, Vector3.zero, Quaternion.identity);
            trans.name = currBullet.name;
            trans.SetParent(go.transform);
            trans.gameObject.SetActive(false);

            sameBulletList.Add(trans);

            // Set sort order for enemy bullets.
            SpriteRenderer transSr = trans.GetComponent<SpriteRenderer>();
            if (transSr != null && transSr.sortingLayerName == TagManager.sSingleton.sortLayerTopG) 
                transSr.sortingOrder = i;
        }

        if (groupIndex == 0) mPlyMainBulletList.Add(sameBulletList);
        else if(groupIndex == 1) mPlySecondaryBulletList.Add(sameBulletList);
        else mEnemyBulletList.Add(sameBulletList);
    }

    public Transform GetBulletTrans(GroupIndex groupIndex, int index)
    {
        // Update index to the next one.
        int bulletIndex = 0, total = 0;
        if (groupIndex == GroupIndex.PLAYER_MAIN)
        {
            bulletIndex = plyCurrMainBulIndexList[index];
            total = mPlyMainBulletList[index].Count - 1;

            if (bulletIndex + 1 > total) plyCurrMainBulIndexList[index] = 0;
            else plyCurrMainBulIndexList[index]++;

            return mPlyMainBulletList[index][bulletIndex];
        }
        else if (groupIndex == GroupIndex.PLAYER_SECONDARY)
        {
            bulletIndex = plyCurrSecondBulIndexList[index];
            total = mPlySecondaryBulletList[index].Count - 1;

            if (bulletIndex + 1 > total) plyCurrSecondBulIndexList[index] = 0;
            else plyCurrSecondBulIndexList[index]++;

            return mPlySecondaryBulletList[index][bulletIndex];
        }
        else if (groupIndex == GroupIndex.ENEMY)
        {
            bulletIndex = enemyCurrBulIndexList[index];
            total = mEnemyBulletList[index].Count - 1;

            if (bulletIndex + 1 > total) enemyCurrBulIndexList[index] = 0;
            else enemyCurrBulIndexList[index]++;

            return mEnemyBulletList[index][bulletIndex];
        }
        return null;
    }

    public void DisableEnemyBullets(bool isDisableSpawnBullet)
    {
        for (int i = 0; i < mEnemyBulletList.Count; i++)
        {
            List<Transform> sameBulletList = mEnemyBulletList[i];
            for (int j = 0; j < sameBulletList.Count; j++)
            {
                Transform currBullet = sameBulletList[j];
                SpriteRenderer sr = currBullet.GetComponent<SpriteRenderer>();

                currBullet.GetComponent<Collider2D>().enabled = false;
                StartCoroutine(IEAlphaOutSequence(sr));
            }
        }

        mIsDisableSpawnBullet = isDisableSpawnBullet;
        mDisableSpawnBulletTime = GameManager.sSingleton.enemyDisBulletTime;
    }

    public void TransformEnemyBulsIntoScorePU()
    {
        for (int i = 0; i < mEnemyBulletList.Count; i++)
        {
            List<Transform> sameBulletList = mEnemyBulletList[i];
            for (int j = 0; j < sameBulletList.Count; j++)
            {
                Transform currBullet = sameBulletList[j];
                if (currBullet.gameObject.activeSelf)
                {
                    Vector3 pos = currBullet.position;
                    EnvObjManager.sSingleton.TransformBulletIntoScorePU(pos);
                }
            }
        }
    }

	public void TransformEnemyBulIntoPlayerBul(int playerID, Transform other)
	{
        Transform trans = GetBulletTrans (GroupIndex.PLAYER_MAIN, playerID - 1);
		trans.position = other.position;

        BulletMove bulletMove = trans.GetComponent<BulletMove>();
        int dmg = BombManager.sSingleton.bombShieldReturnDmg;
        float spd = BombManager.sSingleton.bombShieldReturnSpd;
        bulletMove.SetProperties(AttackPattern.Template.SINGLE_SHOT, dmg, spd);
        bulletMove.SetPlayer(true);

        Vector2 dir = EnemyManager.sSingleton.GetClosestEnemyDir(trans);
        bulletMove.SetDirection(dir);
        trans.gameObject.SetActive (true);
	}

    IEnumerator IEAlphaOutSequence (SpriteRenderer sr)
    {
        Color color = Color.white;
        while(sr.color.a > 0)
        {
            float deltaTime = 0;
            if (BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
            else deltaTime = Time.deltaTime;

            color = sr.color;
            color.a -= deltaTime * GameManager.sSingleton.bulletDisappearSpeed;
            sr.color = color;

            yield return null;
        }

        // Reset the values back to default.
        color = sr.color;
        color.a = 1;
        sr.color = color;

        sr.gameObject.GetComponent<Collider2D>().enabled = true;
        sr.gameObject.SetActive(false);
    }
}
