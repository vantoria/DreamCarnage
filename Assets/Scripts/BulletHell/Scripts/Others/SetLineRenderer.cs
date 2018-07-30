using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLineRenderer : MonoBehaviour 
{
    LineRenderer lineRenderer;

	void Start () 
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingLayerName = "TopGroundUI";
        lineRenderer.sortingOrder = 8;
	}
}
