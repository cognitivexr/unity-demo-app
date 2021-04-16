using System;
using System.Collections.Concurrent;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using UnityEngine;

namespace cpop_client
{
    public class CpopServerOptions
    {
        public string Server { get; set; }
        public int? Port { get; set; }
    }

    public class CpopSubscriber
    {
        public ConcurrentQueue<CpopData> Queue { get; }
        private CpopServerOptions _options;
        private CancellationTokenSource _cancellationTokenSource;
        private IMqttClient _client;

        public CpopSubscriber(ConcurrentQueue<CpopData> queue, CpopServerOptions options)
        {
            Queue = queue;
            _options = options;
            _cancellationTokenSource = new CancellationTokenSource();
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
        }

        public CpopSubscriber(CpopServerOptions options) : this(new ConcurrentQueue<CpopData>(), options)
        {
        }

        public CpopSubscriber() : this(new CpopServerOptions {Server = "localhost"})
        {
        }

        public void Unsubscribe()
        {
            _cancellationTokenSource.Cancel();
        }

        public async void Subscribe()
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId("CS-Client")
                .WithTcpServer(_options.Server, _options.Port)
                .WithCleanSession()
                .Build();
            _client.UseApplicationMessageReceivedHandler(DefaultCpopMessageHandlerJson);
            await _client.ConnectAsync(options, _cancellationTokenSource.Token);
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("cpop").Build());
        }

        protected void DefaultCpopMessageHandlerJson(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                //Debug.LogError("CPOP HANDLER CALLED!");

                var payload = e.ApplicationMessage.Payload;
                //Debug.LogError("PAYLOAD INIT DONE");

                String jsonText = System.Text.Encoding.UTF8.GetString(payload);
                //Debug.LogError("JSONTEXT CREATED");

                var cpopData = JsonUtility.FromJson<CpopData>(jsonText);

                //Debug.LogError("ENQUEUE CPOPDATA...");
                Queue.Enqueue(cpopData);

                //Debug.LogError("CPOPDATA ENQUEUED!");

            }
            catch (Exception exp)
            {
                Debug.LogError(exp);

            }
        }
    }
}