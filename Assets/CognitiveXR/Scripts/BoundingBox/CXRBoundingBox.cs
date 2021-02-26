using cpop_client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// http://www.mindcontrol.org/~hplus/interpolation.html
public class Interpolator
{
    private readonly float calcAhead = 0.5f;        //  How far forward to extrapolate
    private readonly float displayBehind = 0.5f;    //  How far back to interpolate
    private readonly float slopTime = 1.0f;         //  How long without updates is OK
    
    private Vector3 lastPosition;
    private Vector3 lastVelocity;
    private float lastTime;
    private Vector3 lastReturnPosition;
    
    public Interpolator(Vector3 initPosition, Vector3 initVelocity, float initTime)
    {
        lastPosition = initPosition;
        lastVelocity = initVelocity;
        lastTime = initTime;
    }

    public void Update( Vector3 newPosition, Vector3 newVelocity, float newTime)
    {
        if(newTime < lastTime) return; // discard updates from the past

        if (displayBehind < 0.0001f)
        {
            lastPosition = newPosition;
            lastVelocity = newVelocity;
        }
        else
        {
            lastPosition = lastReturnPosition;
            lastVelocity = (newPosition + newVelocity * calcAhead - lastPosition) * (1.0f/displayBehind);
        }

        lastTime = newTime;
    }

    public Vector3? Get(float time)
    {
        if (time < (lastTime + slopTime))
        {
            lastReturnPosition = lastPosition + lastVelocity * (time - lastTime);
            return lastReturnPosition;
        }
        
        return null;
    }
}

public class CXRBoundingBox : MonoBehaviour
{
    private Vector3 pos;
    private Vector3 bbSize;
    //private float height;
    //private float width;
    //private float depth;
    private float time;

    private Interpolator interpolator;


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
        if(interpolator is null) return;
        
        this.gameObject.transform.position = interpolator.Get(Time.time) ?? gameObject.transform.position;
        this.gameObject.GetComponent<BoxCollider>().size = bbSize;
    }

    private void ExtractCpopData(CpopData updateData)
    {
        Vector3 oldPos = pos;
        float oldTime = time;
        pos = new Vector3(updateData.Position.X, updateData.Position.Y, updateData.Position.Z);
        bbSize = new Vector3(updateData.Shape[0].X, updateData.Shape[0].Y, updateData.Shape[0].Z);

        if (interpolator is null)
        {
            interpolator = new Interpolator(pos, Vector3.zero, Time.time);
        }
        else
        {
            Vector3 vel = (oldPos - pos) / (Time.time - oldTime);
            interpolator.Update(pos, vel, Time.time);

        }
        time = Time.time;

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
