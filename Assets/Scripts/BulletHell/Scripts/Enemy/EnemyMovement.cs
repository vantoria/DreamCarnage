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
            SIN_WAVE
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

        public Type type = Type.WAY_POINT;
        public bool isRepeat, isConstantSpeed;
        public float constantSpeed;

        public Transform wayPointParent;
        public List<WayPoint> wayPointList = new List<WayPoint>();

        [HideInInspector] public bool isCoroutine;

        public Movement()
        {
            wayPointParent = null;
            isConstantSpeed = false;
            constantSpeed = 1;
            wayPointList = new List<WayPoint>();
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

    Rigidbody2D rgBody;

	void Start () 
    {
        rgBody = GetComponent<Rigidbody2D>();
	}
	
	void Update () 
    {
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
	}

    public void SetMovement(Transform spawnPos, Movement movement)
    {
        spawnPosTrans = spawnPos;
        if (movementList.Count == 0)
        {
            movementList.Add(new Movement());
            movementList[0].wayPointParent = movement.wayPointParent;
            movementList[0].constantSpeed = movement.constantSpeed;
            movementList[0].isRepeat = movement.isRepeat;
            movementList[0].isConstantSpeed = movement.isConstantSpeed;

            for (int i = 0; i < movement.wayPointParent.childCount; i++)
            {
                if (movementList[0].wayPointList.Count - 1 < i) movementList[0].wayPointList.Add(new Movement.WayPoint());

                Movement.WayPoint currWaypoint = movementList[0].wayPointList[i];
                currWaypoint.targetTrans = movement.wayPointParent.GetChild(i);
                currWaypoint.speed = movement.wayPointList[i].speed;
                currWaypoint.startDelay = movement.wayPointList[i].startDelay;

                if (movementList[0].isConstantSpeed) currWaypoint.speed = movement.constantSpeed;
            }
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
