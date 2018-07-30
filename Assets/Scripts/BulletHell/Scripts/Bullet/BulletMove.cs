using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMove : MonoBehaviour 
{
    AttackPattern.Properties properties = new AttackPattern.Properties();
    List<AttackPattern.UpdateSpeed> newChangeList = new List<AttackPattern.UpdateSpeed>();

    BulletManager.GroupIndex groupType = BulletManager.GroupIndex.PLAYER_MAIN;

    Vector2 mStartPos;
    int mCurrChangeIndex = 0;
    float mAngle = 0, mChangeSpeedTimer = 0;
    bool mIsPiercing = false;

    IEnumerator currCo;
    Transform mToTarget;

	void Update () 
    {
        float deltaTime = 0;
        if (properties.ownerType == AttackPattern.OwnerType.PLAYER && BombManager.sSingleton.isTimeStopBomb) deltaTime = Time.unscaledDeltaTime;
        else deltaTime = Time.deltaTime;

        HandleSpeedChange();

        AttackPattern.Template currTemplate = properties.template;
        if (currTemplate == AttackPattern.Template.SINGLE_SHOT || currTemplate == AttackPattern.Template.ANGLE_SHOT || currTemplate == AttackPattern.Template.SHOOT_AROUND_IN_CIRCLE)
        {
            transform.Translate(properties.direction * properties.speed * deltaTime, Space.World);
        }
        else if (currTemplate == AttackPattern.Template.DOUBLE_SINE_WAVE)
        {
            mStartPos += properties.direction * properties.speed * deltaTime;
            transform.position = (Vector3)mStartPos + (Vector3)properties.curveAxis * Mathf.Sin (mAngle * properties.frequency) * (properties.magnitude + (mAngle * properties.magExpandMult));

            mAngle += deltaTime;
            if (mAngle >= (Mathf.PI * 2)) mAngle = 0;
        }
        else if (currTemplate == AttackPattern.Template.SHOCK_REPEL_AND_TRAP_LASER)
        {
            transform.position = Vector3.MoveTowards(transform.position, mToTarget.position, properties.speed * deltaTime);

            Vector3 dir = mToTarget.position - transform.position;
            float bulletAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(-90 + bulletAngle, Vector3.forward);
        }
	}

    public void SetBaseProperties(AttackPattern.Properties properties) 
    { 
        this.properties.ownerType = properties.ownerType; 
        this.properties.template = properties.template; 
        this.properties.isAlternateFire = properties.isAlternateFire; 
        this.properties.isMagnum = properties.isMagnum; 
        this.properties.damage = properties.damage; 
        this.properties.speed = properties.speed; 
        this.properties.frequency = properties.frequency; 
        this.properties.magnitude = properties.magnitude; 
        this.properties.magExpandMult = properties.magExpandMult;

        this.properties.giveStatRepelDur = properties.giveStatRepelDur;
        this.properties.slowValue = properties.slowValue;
        this.properties.repelValue = properties.repelValue;

        this.properties.trapDelayExpand = properties.trapDelayExpand;
        this.properties.trapExpandDur = properties.trapExpandDur;
        this.properties.trapExpandSpd = properties.trapExpandSpd;
        this.properties.trapRotateSpd = properties.trapRotateSpd;
        this.properties.trapMoveSpd = properties.trapMoveSpd;
        this.properties.trapDelayShrink = properties.trapDelayShrink;
		this.properties.trapShrinkSpd = properties.trapShrinkSpd;
		this.properties.trapFadeSpd = properties.trapFadeSpd;
    }

    public void SetProperties(AttackPattern.Template template, float damage, float speed)
    {
        this.properties.template = template;
        this.properties.damage = damage;
        this.properties.speed = speed; 
    }

    public void SetIsPiercing(bool isPierce) { mIsPiercing = isPierce; }
    public bool GetIsPiercing() { return mIsPiercing; }

    public void SetPlayer() { this.properties.ownerType = AttackPattern.OwnerType.PLAYER; }
    public void SetShockRepelTarget(Transform target) 
    { 
        mToTarget = target;
        this.properties.damage = 0;
    }

    public void SetDirection(Vector2 dir) 
    { 
        properties.direction = dir; 
        transform.rotation = Quaternion.identity;

        // Change the rotation of bullet to match direction.
        float bulletAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(-90 + bulletAngle, Vector3.forward);
    }

    public void SetNewBulletSpeed(List<AttackPattern.UpdateSpeed> newList) 
    {
        mCurrChangeIndex = 0;
        mChangeSpeedTimer = 0;
        newChangeList = newList; 
    }

    public void SetCurveAxis(Vector2 curveAxis) 
    {
        mStartPos = (Vector2) transform.position;
        mAngle = 0;
        properties.curveAxis = curveAxis; 
    }

    public BulletManager.GroupIndex BulletType 
    { 
        get { return groupType; } 
        set { groupType = value; }
    }

    public AttackPattern.Properties GetAttackProperties { get { return properties; } }
    public AttackPattern.Template GetTemplate { get { return properties.template; } }
    public bool GetIsMagnum { get { return properties.isMagnum; } }
    public float GetBulletDamage { get { return properties.damage; } }
    public float GetStatRepelDur { get { return properties.giveStatRepelDur; } }

    void HandleSpeedChange()
    {
        // Change the speed of enemy bullet.
        if (mCurrChangeIndex < newChangeList.Count)
        {
            mChangeSpeedTimer += Time.deltaTime;

            AttackPattern.UpdateSpeed currUpdate = newChangeList[mCurrChangeIndex];

            if (mChangeSpeedTimer > currUpdate.changeSpeedTime)
            {
                // Stop the current coroutine before moving to a new speed.
                if(currCo != null) StopCoroutine(currCo);

                if (currUpdate.toSpeedQuickness == 0) properties.speed = currUpdate.toSpeed;
                else
                {
                    currCo = ToNewSpeedRoutine(currUpdate.toSpeed, currUpdate.toSpeedQuickness);
                    StartCoroutine(currCo);
                }
                mChangeSpeedTimer = 0;
                mCurrChangeIndex++;
            }
        }
    }

    IEnumerator ToNewSpeedRoutine(float toSpeed, float quickness)
    {
        while(properties.speed != toSpeed)
        {
            float value = Time.deltaTime * quickness;
            if (properties.speed < toSpeed)
            {
                properties.speed += value;
                if (properties.speed > toSpeed) properties.speed = toSpeed;
            }
            else
            {
                properties.speed -= value;
                if (properties.speed < toSpeed) properties.speed = toSpeed;
            }

            yield return null;
        }
    }
}
