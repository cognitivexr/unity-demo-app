using System.Collections.Generic;
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
    }
    
    [SerializeField] private XRLineRenderer lineRenderer;
    [SerializeField] private TextMeshProUGUI emotionTextField;

    public EmotionInfo Info { get; private set;  }
    
    private void Awake()
    {
        Debug.Assert(lineRenderer != null);
        Debug.Assert(emotionTextField != null);

        lineRenderer.loop = true;
        
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    public void Init(EmotionInfo info)
    {
        Info = info;
        
        emotionTextField.text = string.IsNullOrEmpty(info.DominantEmotion) ? "None" : info.DominantEmotion;

        if (info.Bounds.Count == 4)
        {
            // calculate mid point
            Vector3 middle = Vector3.zero;
            foreach (Vector3 point in info.Bounds)
            {
                middle += point;
            }
            middle /= (float)info.Bounds.Count;

            emotionTextField.transform.position = middle;
            
            // bounds
            
            Vector3[] points = new Vector3[4];

            points[0] = info.Bounds[0];
            points[1] = info.Bounds[1];
            points[2] = info.Bounds[2];
            points[3] = info.Bounds[3];
            
            lineRenderer.SetPositions(points);
            

        }
    }
    
}
