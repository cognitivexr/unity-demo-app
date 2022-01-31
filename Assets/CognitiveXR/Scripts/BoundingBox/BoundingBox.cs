using System.Collections.Generic;
using TMPro;
using Unity.XRTools.Rendering;
using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    public class BoundingBoxInfo
    {
        public string text;
        public List<Vector3> Bounds;
        public uint frameId;
        public Pose cameraPose;
    }
    
    [SerializeField] private XRLineRenderer lineRenderer;
    [SerializeField]  public TextMeshPro label;

    public Transform c1, c2, c3, c4;
    
    public BoundingBoxInfo Info { get; private set;  }
    
    
    private void Awake()
    {
        Debug.Assert(lineRenderer != null);
        Debug.Assert(label != null);

        lineRenderer.loop = true;
    }
    public void Set(BoundingBoxInfo info)
    {
        Info = info;
        
        label.text = string.IsNullOrEmpty(info.text) ? "None" : info.text;

        if (info.Bounds.Count != 4) return;
        
        // calculate mid point for the lable
        Vector3 middle = Vector3.zero;
        foreach (Vector3 point in info.Bounds)
        {
            middle += point;
        }
        middle /= (float)info.Bounds.Count;
        
        float lowerBorder = System.Math.Min(System.Math.Min(info.Bounds[0].y, info.Bounds[1].y),
            System.Math.Min(info.Bounds[2].y, info.Bounds[3].y)) - 0.05f;
        
        label.transform.parent.localPosition = new Vector3(middle.x, lowerBorder, middle.z);
        label.transform.parent.LookAt(Camera.main.transform);
        
        // bounds
            
        c1.localPosition = info.Bounds[0];
        c2.localPosition = info.Bounds[1];
        c3.localPosition = info.Bounds[2];
        c4.localPosition = info.Bounds[3];
        
        Vector3[] points = new Vector3[4];

        points[0] = info.Bounds[0];
        points[1] = info.Bounds[1];
        points[2] = info.Bounds[2];
        points[3] = info.Bounds[3];
            
        lineRenderer.SetPositions(points);
    }
}
