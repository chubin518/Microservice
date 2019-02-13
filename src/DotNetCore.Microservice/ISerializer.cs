using System;

namespace DotNetCore.Microservice
{
    public interface ISerializer<T>
    {
        T Serialize(object instance);

        TObject Deserialize<TObject>(T content);
    }
}
