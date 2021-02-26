using cpop_client;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public delegate void PublishBBUpdateEvent(CpopData updateData);
    public static event PublishBBUpdateEvent PublishBBUPdate;

    public void FireEvent_BBUpdate(CpopData _updateData)
    {
        if (PublishBBUPdate != null)
            PublishBBUPdate(_updateData);
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

}
