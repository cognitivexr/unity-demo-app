using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NewTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            int width = 500;
            int height = 500;
            
            StreamSpec streamSpec = new StreamSpec
            {
                engineAddress = $"127.0.0.1:54321",
                attributes = new Attributes()
                    .Set("format.width", width.ToString())
                    .Set("format.height", height.ToString())
                    .Set("format.colorMode", "4")
                    .Set("format.orientation", "3")
            };

            JpgSendChannel sendChannel = new JpgSendChannel(width, height);
            DebugReceiveChannel receiveChannel = new DebugReceiveChannel();

            EngineClient engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel, 42);
            engineClient.Open();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            int width = 500;
            int height = 500;
            
            StreamSpec streamSpec = new StreamSpec
            {
                engineAddress = $"127.0.0.1:54321",
                attributes = new Attributes()
                    .Set("format.width", width.ToString())
                    .Set("format.height", height.ToString())
                    .Set("format.colorMode", "4")
                    .Set("format.orientation", "3")
            };

            JpgSendChannel sendChannel = new JpgSendChannel(width, height);
            DebugReceiveChannel receiveChannel = new DebugReceiveChannel();

            EngineClient engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel, 42);
            engineClient.Open();
            yield return null;
        }
    }
}
