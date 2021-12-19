using CognitiveXR.CogStream;
using UnityEngine;

public class Test : MonoBehaviour
{
    private MediatorClient mediatorClient;

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
        

        mediatorClient = new MediatorClient("ws://192.168.1.104:8191");
        await mediatorClient.Open();
        await mediatorClient.SendMessage(message);
        //mediatorClient.Receive();
    }

    private void OnDestroy()
    {
        mediatorClient.Close();
    }

}
