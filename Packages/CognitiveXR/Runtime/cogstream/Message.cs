using System.Collections.Generic;
using CognitiveXR.SimpleJSON;

namespace CognitiveXR.CogStream
{
    public struct Message
    {
        public int type;
        public Content content;
    }

    public struct Engine
    {
        public string name;
        public Attributes attributes;
    }

    public struct Content
    {
        public string code;
        public string engine;
        public string engineAddress;
        public List<Engine> engines;
        public Attributes attributes;
    }
    
    public static class MediatorClientMessage
    {
        public static Message Parse(string json)
        {
            JSONNode rootNode = JSON.Parse(json);

            Message message = new Message();
            JSONNode typeNode = rootNode["type"];
            if (typeNode == 3)
            {
                message.type = typeNode;
                var contentNode = rootNode["content"];
                if (contentNode != null)
                {
                    message.content = new Content();
                    JSONNode enginesJson = contentNode["engines"];
                    if (enginesJson != null)
                    {
                        message.content.engines = new List<Engine>();
                        foreach (var jsonEngine in enginesJson)
                        {
                            Engine engine = new Engine();
                            engine.name = jsonEngine.Value["name"];

                            JSONNode attributesNode = jsonEngine.Value["attributes"];
                            if (attributesNode != null)
                            {
                                engine.attributes = new Attributes();
                                foreach (var attribute in attributesNode)
                                {
                                    engine.attributes.Add(attribute.Key, attribute.Value.AsStringList);
                                }
                            }
                            
                            message.content.engines.Add(engine);
                        }
                    }
                }
            }

            if (typeNode == 5)
            {
                message.type = typeNode;
                var contentNode = rootNode["content"];
                if (contentNode != null)
                {
                    message.content = new Content();
                    JSONNode engineAddressNode = contentNode["engineAddress"];
                    if (engineAddressNode != null)
                    {
                        message.content.engineAddress = engineAddressNode;
                    }

                    JSONNode attributeNode = contentNode["attributes"];
                    if (attributeNode != null)
                    {
                        message.content.attributes = new Attributes();
                        foreach (var attribute in attributeNode)
                        {
                            message.content.attributes.Add(attribute.Key, attribute.Value.AsStringList);
                        }
                    }

                }
            }
            
            return message;
        }

        public static Message GetServicesMessage()
        {
            Message message = new Message
            {
                type = 2,
                content = new Content()
                {
                    code = "analyze"
                }
            };
            return message;
        }
        
        public static Message GetSelectEngineMessage(Engine engine)
        {
            Message message = new Message()
            {
                type = 4, //  type 4 for selecting an engien
                content = new Content()
                {
                    engine = engine.name,
                    attributes = new Attributes()
                },
            };
            return message;
        }
    }
}