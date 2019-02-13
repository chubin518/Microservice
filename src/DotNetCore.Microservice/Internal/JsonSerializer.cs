using Newtonsoft.Json;
using System;

namespace DotNetCore.Microservice.Internal
{
    public class JsonSerializer : ISerializer<string>
    {
        public TObject Deserialize<TObject>(string content)
        {
            return JsonConvert.DeserializeObject<TObject>(content);
        }

        public string Serialize(object instance)
        {
            return JsonConvert.SerializeObject(instance);
        }
    }
}
