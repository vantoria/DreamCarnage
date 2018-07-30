using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnemyMovement : MonoBehaviour 
{
    [System.Serializable]
    public class MoveInfo
    {
        [ReadOnly] public Vector3 target;
        [ReadOnly] public Vector3 moveDirection;
        [ReadOnly] public Vector3 velocity;
        [ReadOnly] public int currWayPoint;
    }

    [System.Serializable]
    public class Movement
    {
        [System.Serializable]
        public enum Type
        {
            WAY_POINT = 0,
            SINE_WAVE,
            IN_AND_OUT
        }

        [System.Serializable]
        public class WayPoint
        {
            public Transform targetTrans;
            public float speed;
            public float startDelay;

            public WayPoint()
            {
                this.targetTrans = null;
                this.speed = 1;
                this.startDelay = 0;
            }

            public WayPoint(Transform targetTrans, float speed, float startDelay)
            {
                this.targetTrans = targetTrans;
                this.speed = speed;
                this.startDelay = startDelay;
            }
        }

        [System.Serializable]
        public class SineWave
        {
            public Vector2 direction;
            public float speed;
            public float frequency;
            public float magnitude;
            public float magExpandMult;
            public bool isStartLeft;

            [HideInInspector] public Vector2 curveAxis;
            [HideInInspector] public Vector2 startPos;
            [HideInInspector] public float currAngle = 0;

            public SineWave()
            {
                direction = curveAxis = Vector2.zero;
                speed = frequency = magnitude = magExpandMult = 0;
                isStartLeft = true;
            }

            public SineWave(Vector2 direction, Vector2 curveAxis, bool isStartLeft, float speed, float frequency, float magnitude, float magExpandMult)
            {
                this.direction = direction;
                this.curveAxis = curveAxis;
                this.speed = speed;
                this.frequency = frequency;
                this.magnitude = magnitude;
                this.magExpandMult = magExpandMult;
            }
        }

        [System.Serializable]
        public class InAndOut
        {
            public Vector2 direction;
            public float time;
            public float speed;
            public float waitBeforeGoingOut;

            public InAndOut()
            {
                direction = new Vector2(0, -1);
                time = 1;
                speed = 1;
                waitBeforeGoingOut = 1;
            }

            public InAndOut(Vector2 direction, float time, float speed, float waitBeforeGoingOut)
            {
                this.time = time;
                this.direction = direction;
                this.speed = speed;
                this.waitBeforeGoingOut = waitBeforeGoingOut;
            }
        }

        public Type type = Type.WAY_POINT;
        public bool isRepeat, isConstantSpeed;
        public float constantSpeed;

        public Transform wayPointParent;
        public List<WayPoint> wayPointList = new List<WayPoint>();
        public SineWave sinewave = new SineWave();
        public InAndOut inOut = new InAndOut();

        [HideInInspector] public bool isCoroutine;

        public Movement()
        {
            wayPointParent = null;
            isConstantSpeed = false;
            constantSpeed = 1;
            wayPointList = new List<WayPoint>();
            sinewave = new SineWave();
            inOut = new InAndOut();
            isRepeat = false;
            isCoroutine = false;
        }
    }

    // Enemy movement.
    public Transform spawnPosTrans;
//    public float moveSpeed = 1;
    public MoveInfo moveInfo;
    public bool isDrawGizmo = true;
    public List<Movement> movementList = new List<Movement>();

    SpriteRenderer sr;
    Vector3 mPrevPos = Vector3.zero;
    float mTimer;
    bool mIsCoroutine = false, mIsWaitedOver = false;
    Rigidbody2D rgBody;

	void Start () 
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        rgBody = GetComponent<Rigidbody2D>();

        if (movementList.Count != 0)
        {
            Movement currMovement = movementList[0];
            currMovement.sinewave.curveAxis = GetCurveAxis(currMovement.sinewave.direction, currMovement.sinewave.isStartLeft);
            mPrevPos = transform.position;
        }
	}
	
	void Update () 
    {
        if (Application.isPlaying && (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause)) return;

        if (!Application.isPlaying)
        {
            for (int i = 0; i < movementList.Count; i++)
            {
                if (movementList[i].wayPointParent != null)
                {
                    for (int j = 0; j < movementList[i].wayPointParent.childCount; j++)
                    {
                        if (movementList[i].wayPointList.Count - 1 < j) movementList[i].wayPointList.Add(new Movement.WayPoint());
                        movementList[i].wayPointList[j].targetTrans = movementList[i].wayPointParent.GetChild(j);

                        if (movementList[i].isConstantSpeed) movementList[i].wayPointList[j].speed = movementList[i].constantSpeed;
                    }
                }
            }
        }
        else if (movementList.Count > 0)
        {
            if (UIManager.sSingleton.IsPauseGameOverMenu || BombManager.sSingleton.IsPause) return;

            Movement movement = movementList[0];
            if (movement.type == Movement.Type.SINE_WAVE)
            {
                Movement.SineWave sinewave = movement.sinewave;
                sinewave.startPos += sinewave.direction * sinewave.speed * Time.deltaTime;
                Vector3 val = (Vector3)sinewave.curveAxis * Mathf.Sin (sinewave.currAngle * sinewave.frequency) * (sinewave.magnitude + (sinewave.currAngle * sinewave.magExpandMult));
                transform.position = (Vector3)sinewave.startPos + val;

                sinewave.currAngle += Time.deltaTime;
                if (sinewave.currAngle >= (Mathf.PI * 2)) sinewave.currAngle = 0;

                if (transform.position.x < mPrevPos.x) sr.flipX = true;
                else sr.flipX = false;

                mPrevPos = transform.position;
            }
            else if (movement.type == Movement.Type.IN_AND_OUT)
            {
                Movement.InAndOut inOut = movement.inOut;   
                if (mTimer < inOut.time)
                {
                    Vector3 currPos = transform.position;
                    currPos.x += inOut.direction.x * inOut.speed * Time.timeScale;
                    currPos.y += inOut.direction.y * inOut.speed * Time.timeScale;
                    transform.position = currPos;

                    mTimer += Time.deltaTime;
					if (mTimer >= inOut.time && !mIsCoroutine) StartCoroutine(WaitFor(inOut.waitBeforeGoingOut));
                }
                else if (mIsWaitedOver)
                {
                    Vector3 currPos = transform.position;
                    currPos.x -= inOut.direction.x * inOut.speed;
                    currPos.y -= inOut.direction.y * inOut.speed;
                    transform.position = currPos;
                }
            }
        }
	}

    IEnumerator WaitFor(float sec)
    {
        mIsCoroutine = true;
        yield return new WaitForSeconds(sec);
        mIsWaitedOver = true;
        mIsCoroutine = false;
    }

    public void SetMovement(Transform spawnPos, Movement movement)
    {
        if (movement.type == Movement.Type.WAY_POINT)
        {
            spawnPosTrans = spawnPos;
            if (movementList.Count == 0)
            {
                movementList.Add(new Movement());
                Movement currMovement = movementList[0];

                currMovement.type = Movement.Type.WAY_POINT;
                currMovement.wayPointParent = movement.wayPointParent;
                currMovement.constantSpeed = movement.constantSpeed;
                currMovement.isRepeat = movement.isRepeat;
                currMovement.isConstantSpeed = movement.isConstantSpeed;

                for (int i = 0; i < movement.wayPointParent.childCount; i++)
                {
                    if (currMovement.wayPointList.Count - 1 < i) currMovement.wayPointList.Add(new Movement.WayPoint());

                    Movement.WayPoint currWaypoint = movementList[0].wayPointList[i];
                    currWaypoint.targetTrans = movement.wayPointParent.GetChild(i);
                    currWaypoint.speed = movement.wayPointList[i].speed;
                    currWaypoint.startDelay = movement.wayPointList[i].startDelay;

                    if (currMovement.isConstantSpeed) currWaypoint.speed = movement.constantSpeed;
                }
            }
        }
        else if (movement.type == Movement.Type.SINE_WAVE)
        {
            if (movementList.Count == 0)
            {
                movementList.Add(new Movement());
                Movement currMovement = movementList[0];

                currMovement.type = Movement.Type.SINE_WAVE;
                currMovement.sinewave.direction = movement.sinewave.direction;
                currMovement.sinewave.speed = movement.sinewave.speed;
                currMovement.sinewave.frequency = movement.sinewave.frequency;
                currMovement.sinewave.magnitude = movement.sinewave.magnitude;
                currMovement.sinewave.magExpandMult = movement.sinewave.magExpandMult;
                currMovement.sinewave.isStartLeft = movement.sinewave.isStartLeft;

                currMovement.sinewave.startPos = (Vector2)spawnPos.position;
                currMovement.sinewave.curveAxis = GetCurveAxis(currMovement.sinewave.direction, currMovement.sinewave.isStartLeft);
                mPrevPos = transform.position;
            }
        }
        else if (movement.type == Movement.Type.IN_AND_OUT)
        {
            movementList.Add(new Movement());
            Movement currMovement = movementList[0];

            currMovement.type = Movement.Type.IN_AND_OUT;
            currMovement.inOut.direction = movement.inOut.direction;
            currMovement.inOut.speed = movement.inOut.speed;
            currMovement.inOut.time = movement.inOut.time;
            currMovement.inOut.waitBeforeGoingOut = movement.inOut.waitBeforeGoingOut;
        }
    }

    public IEnumerator MoveToWayPoint(int currActionNum)
    {
        int savedActionNum = currActionNum;
        movementList[savedActionNum].isCoroutine = true;
        moveInfo.currWayPoint = 0;

        while(savedActionNum == currActionNum)
        {
            Movement currMoveThisAct = movementList[savedActionNum];

            int currWayIndex = moveInfo.currWayPoint;
            if (currWayIndex < currMoveThisAct.wayPointList.Count)
            {
                Movement.WayPoint currWayPoint = currMoveThisAct.wayPointList[currWayIndex];

                if (currWayPoint.startDelay != 0)
                {
                    rgBody.velocity = Vector3.zero;
                    yield return new WaitForSeconds(currWayPoint.startDelay);
                    currWayPoint.startDelay = 0;
                }
                moveInfo.target = currWayPoint.targetTrans.position;
                moveInfo.moveDirection = moveInfo.target - transform.position;
                moveInfo.velocity = rgBody.velocity;

                if (moveInfo.moveDirection.magnitude < 0.5f) moveInfo.currWayPoint++;
                else moveInfo.velocity = moveInfo.moveDirection.normalized * currWayPoint.speed;
            }
            else
            {
                if (currMoveThisAct.isRepeat) moveInfo.currWayPoint = 0;
                else moveInfo.velocity = Vector3.zero;
            }
            rgBody.velocity = moveInfo.velocity;

            yield return null;
        }
        rgBody.velocity = Vector3.zero;
        movementList[savedActionNum].isCoroutine = false;
    }

    public void StopCurrMovement(IEnumerator co) 
    { 
        if (co == null) return;
        StopCoroutine(co);
        rgBody.velocity = Vector3.zero;
    }

    Vector2 GetCurveAxis(Vector2 dir, bool isStartLeft)
    {
        Vector2 curveAxis = Vector2.zero;
        float xVal = 0, yVal = 0;

        if (dir.y < 0) xVal = -Mathf.Abs(dir.y);
        else xVal = Mathf.Abs(dir.y);

        if (dir.x < 0) yVal = Mathf.Abs(dir.x);
        else yVal = -Mathf.Abs(dir.x);

        if (isStartLeft) curveAxis = new Vector2(xVal, yVal);
        else curveAxis = new Vector2(-xVal, -yVal);

        return curveAxis;
    }

    void OnDrawGizmos()
    {
        if (!isDrawGizmo) return;
        if (movementList.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < movementList.Count; i++)
            {
                for (int j = 0; j < movementList[i].wayPointList.Count; j++)
                {
                    Gizmos.DrawSphere(movementList[i].wayPointList[j].targetTrans.position, 0.2f);
                }
                Gizmos.color = Color.red;

                for (int j = 0; j < movementList[i].wayPointList.Count - 1; j++)
                {
                    Gizmos.DrawLine(movementList[i].wayPointList[j].targetTrans.position, movementList[i].wayPointList[j + 1].targetTrans.position);
                }
            }
        }
    }
}
