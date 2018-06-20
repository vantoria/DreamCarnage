using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour
{
    public int degreesPerSegment = 1;
    public float radius = 1;
    public float width = 0.1f;

    LineRenderer l_Renderer;

    void Start()
    {
        l_Renderer = GetComponent<LineRenderer>();
        l_Renderer.positionCount = 360 / degreesPerSegment + 1;
        l_Renderer.startWidth = width;
        l_Renderer.endWidth = width;
    }

    void Update()
    {
        CreatePoints();
    } 

    void CreatePoints()
    {
        float curAngle = 0;
        float x, y, z = 0;

//        Vector3 pos = transform.position;
        for (int i = 0; i < 360 / degreesPerSegment + 1; i++)
        {
            x = Mathf.Sin(curAngle * Mathf.Deg2Rad);
            y = Mathf.Cos(curAngle * Mathf.Deg2Rad);

            l_Renderer.SetPosition(i, new Vector3(x, y, z) * radius);
            curAngle += degreesPerSegment;
        }
    }
}
