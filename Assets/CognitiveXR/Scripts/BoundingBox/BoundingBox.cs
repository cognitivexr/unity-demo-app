using TMPro;
using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    private Vector3 pos;
    private Vector3 bbSize;
    public uint frameId = 0;

    public TextMeshPro label;

    public void SetPosition(Vector3 _pos)
    {
        pos = _pos;
    }

    public void SetDimensions(Vector3 _dim)
    {
        bbSize = _dim;
    }

    public void SetLabel(string text)
    {
        if (label)
        {
            label.text = text;
        }
    }
    
    void Update()
    {
        this.gameObject.transform.position = pos;
        
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider)
        {
            boxCollider.size = bbSize;
            boxCollider.center = new Vector3(-bbSize.x * 0.5f, bbSize.y * 0.5f, 0.0f);
        }

        if (label)
        {
            label.transform.position = transform.position + new Vector3(-bbSize.x * 0.5f, bbSize.y + 0.2f, 0);

            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.y = label.transform.position.y;
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - cameraPosition);
        }
    }
}
