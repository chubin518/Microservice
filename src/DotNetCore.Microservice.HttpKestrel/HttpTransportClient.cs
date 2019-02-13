
using DotNetCore.Microservice.Owin;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.HttpKestrel
{
    public class HttpTransportClient : ITransportClient
    {
        private HttpClient _client;
        private ISerializer<string> _serializer;

        public HttpTransportClient(IHttpClientFactory clientFactory, ISerializer<string> serializer)
        {
            _client = HttpClientFactoryExtensions.CreateClient(clientFactory);
            _serializer = serializer;
        }

        public async Task<OwinResponse> SendAsync(EndPoint end, OwinRequest request)
        {
            StringContent content = new StringContent(_serializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync($"http://{end}", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string resContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return _serializer.Deserialize<OwinResponse>(resContent);
        }
    }
}
