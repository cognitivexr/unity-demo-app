using System;
using System.Collections.Concurrent;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using UnityEngine;

namespace CognitiveXR.Cpop
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
                var payload = e.ApplicationMessage.Payload;
             
                String jsonText = System.Text.Encoding.UTF8.GetString(payload);
             
                var cpopData = JsonUtility.FromJson<CpopData>(jsonText);

                Queue.Enqueue(cpopData);
                
            }
            catch (Exception exp)
            {
                Debug.LogError(exp);
            }
        }
    }
}