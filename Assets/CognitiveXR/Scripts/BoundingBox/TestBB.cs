using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cpop_client;

public class TestBB : MonoBehaviour
{
    public Vector3 testSize = new Vector3(1, 2, 1);
    private Coordinates shape = new Coordinates();
    private Vector3 pos = new Vector3(0, 0, 0);
    public float moveX;
    public float moveY;
    public float moveZ;
    public float updateTime;
    private float timeLeft;

    private CpopData data = new CpopData();

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<CXRBoundingBox>().SetDimensions(testSize);
        timeLeft = updateTime;

        data.Position.X = pos.x;
        data.Position.Y = pos.y;
        data.Position.Z = pos.z;

        data.Shape = new List<Coordinates>();

        shape.X = testSize.x;
        shape.Y = testSize.y;
        shape.Z = testSize.z;

        data.Shape.Add(shape);

        StartCoroutine(nameof(SimulateUpdate));

        //data = new CpopData(Time.time, "person", new Coordinates(), new List<Coordinates>());
    }

    /*
    // Update is called once per frame
    void Update()
    {
        //this.gameObject.GetComponent<CXRBoundingBox>().SetPosition(pos);
        pos.x += moveX;
        pos.y += moveY;
        pos.z += moveZ;

        if (timeLeft < 0)
        {
            data.Position.X = pos.x;
            data.Position.Y = pos.y;
            data.Position.Z = pos.z;

            shape.X = testSize.x;
            shape.Y = testSize.y;
            shape.Z = testSize.z;


            data.Shape[0] = new Coordinates {X = testSize.x, Y = testSize.y, Z = testSize.z};

            EventManager.Instance.FireEvent_BBUpdate(data);
            timeLeft = updateTime;
        }
        else
        {
            timeLeft -= Time.deltaTime;
        }
    }
    */

    IEnumerator SimulateUpdate()
    {
        Vector3 dir = Vector3.forward;
        Vector3 truePosition = Vector3.zero;
        while (true)
        {
            float dt = GetRandomNumber(0.25f, 0.75f);
            yield return new WaitForSeconds(dt);
                
            dir = Quaternion.Euler(0.0f, GetRandomNumber(-5.0f,10.0f), 0.0f) * dir;
            truePosition += dir * dt * GetRandomNumber(0.5f, 1.0f);
            
            data.Position.X = truePosition.x;
            data.Position.Y = truePosition.y;
            data.Position.Z = truePosition.z;
            
            EventManager.Instance.FireEvent_BBUpdate(data);
        }
    }
    
    private static System.Random random = new System.Random();
    private float GetRandomNumber(float minimum, float maximum)
    { 
        return (float)random.NextDouble() * (maximum - minimum) + minimum;
    }
}
