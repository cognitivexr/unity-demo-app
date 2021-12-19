using System.Linq;

namespace CognitiveXR.CogStream
{
    public static class SpecExtension
    {
        public static string ToJson(this StreamSpec streamSpec)
        {
            return $"{{" +
                   $"\"engineAddress\": \"{streamSpec.engineAddress}\"" +
                   ", " + ToJson(streamSpec.attributes) +
                   $"}}";
        }

        public static string ToJson(this Message message)
        {
            return $"{{" +
                   $"\"type\": {message.type}," +
                   $"{ToJson(message.content)}" +
                   $"}}";
        }

        public static string ToJson(this Attributes attributes)
        {
            if (attributes == null)
            {
                return "\"attributes\": {}";
            }
            
            string result = attributes.Aggregate(
                "\"attributes\": {",
                (current, streamSpecAttribute)
                    => current + $"\"{streamSpecAttribute.Key}\": [\"{streamSpecAttribute.Value[0]}\"],");

            result = result.TrimEnd(',');

            result += "}";

            return result;
        }

        public static string ToJson(this Content content)
        {
            string result = "\"content\":{";

            if (!string.IsNullOrEmpty(content.code))
            {
                result = result + $"\"code\": \"{content.code}\",";
            }

            if (!string.IsNullOrEmpty(content.engine))
            {
                result = result + $"\"engine\": \"{content.engine}\","; 
            }

            result = result + ToJson(content.attributes);
            result = result + "}";
            return result;
        }
    }
}