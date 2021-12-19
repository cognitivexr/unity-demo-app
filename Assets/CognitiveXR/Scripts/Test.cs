using CognitiveXR.CogStream;
using UnityEngine;

public class Test : MonoBehaviour
{
    private MediatorClient mediatorClient;
    // Start is called before the first frame update
    
    async void Start()
    {
        Message message = new Message
        {
            type = 2,
            content = new Content()
            {
                code = "analyze"
            }
        };
        
        Debug.Log(message.ToJson());

        mediatorClient = new MediatorClient("ws://192.168.1.104:8080");
        await mediatorClient.Open();
        await mediatorClient.SendMessage(message);
    }

    private void OnDestroy()
    {
        mediatorClient.Close();
    }

}
