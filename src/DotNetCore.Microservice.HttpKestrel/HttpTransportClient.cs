
using DotNetCore.Microservice.Owin;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.HttpKestrel
{
    public class HttpTransportClient : ITransportClient
    {
        private readonly HttpClient _client;
        private readonly ISerializer<string> _serializer;
        private readonly EndPoint _endPoint;

        public HttpTransportClient(IHttpClientFactory clientFactory, ISerializer<string> serializer, EndPoint endPoint)
        {
            _client = HttpClientFactoryExtensions.CreateClient(clientFactory);
            _client.BaseAddress = new Uri($"http://{endPoint}");
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<OwinResponse> SendAsync(OwinRequest request)
        {
            StringContent content = new StringContent(_serializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("/", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string resultData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return _serializer.Deserialize<OwinResponse>(resultData);
        }
    }
}
