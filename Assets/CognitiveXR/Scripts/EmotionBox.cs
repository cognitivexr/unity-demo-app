using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.XRTools.Rendering;

public class EmotionBox : MonoBehaviour
{

    public class EmotionInfo
    {
        public string DominantEmotion;
        public List<Vector3> Bounds;
        public uint frameId;
        public Pose cameraPose;
    }
    
    [SerializeField] private XRLineRenderer lineRenderer;
    [SerializeField] private TextMeshProUGUI emotionTextField;

    public Transform c1, c2, c3, c4;
    
    public EmotionInfo Info { get; private set;  }
    
    private void Awake()
    {
        Debug.Assert(lineRenderer != null);
        Debug.Assert(emotionTextField != null);

        lineRenderer.loop = true;
    }

    public void Init(EmotionInfo info)
    {
        Info = info;
        
        emotionTextField.text = string.IsNullOrEmpty(info.DominantEmotion) ? "None" : info.DominantEmotion;

        if (info.Bounds.Count != 4) return;
        
        // calculate mid point
        Vector3 middle = Vector3.zero;
        foreach (Vector3 point in info.Bounds)
        {
            middle += point;
        }
        middle /= (float)info.Bounds.Count;

        emotionTextField.transform.parent.localPosition = middle;
        emotionTextField.transform.parent.LookAt(Camera.main.transform);
        
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
