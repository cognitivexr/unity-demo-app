using cpop_client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CXRBoundingBox : MonoBehaviour
{
    private Vector3 pos;
    private Vector3 bbSize;
    //private float height;
    //private float width;
    //private float depth;


    public void SetPosition(Vector3 _pos)
    {
        pos = _pos;
    }

    public void SetDimensions(Vector3 _dim)
    {
        bbSize = _dim;
        //height = _dim.x;
        //width = _dim.y;
        //depth = _dim.z;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = pos; // needs smoothing
        this.gameObject.GetComponent<BoxCollider>().size = bbSize;
    }

    private void ExtractCpopData(CpopData updateData)
    {
        pos = new Vector3(updateData.Position.X, updateData.Position.Y, updateData.Position.Z);
        bbSize = new Vector3(updateData.Shape[0].X, updateData.Shape[0].Y, updateData.Shape[0].Z);
    }

    void OnEnable()
    {
        EventManager.PublishBBUPdate += ExtractCpopData;
    }  

    void OnDisable()
    {
        EventManager.PublishBBUPdate -= ExtractCpopData;
    }

    private void OnDestroy()
    {
        EventManager.PublishBBUPdate -= ExtractCpopData;
    }

}
