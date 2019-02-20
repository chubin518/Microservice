using DotNetCore.Microservice.Owin;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace DotNetCore.Microservice.NetMQ
{
    public class NetmqTransportClient : ITransportClient
    {
        private readonly string Identity;
        private readonly NetMQSocket _dealerSocket; //连接到服务端
        private readonly NetMQSocket _routerSocket;
        private readonly NetMQPoller _poller;
        private readonly ISerializer<string> _serializer;
        private readonly EndPoint _endPoint;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<OwinResponse>> _requests = new ConcurrentDictionary<string, TaskCompletionSource<OwinResponse>>();
        public NetmqTransportClient(ISerializer<string> serializer, EndPoint endPoint)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            Identity = Guid.NewGuid().ToString();
            _dealerSocket = new DealerSocket($"tcp://{endPoint}");
            _dealerSocket.Options.Identity = Encoding.UTF8.GetBytes(Identity);
            _dealerSocket.ReceiveReady += DealerSocket_ReceiveReady;
            _routerSocket = new RouterSocket($"inproc://{Identity}");
            _routerSocket.ReceiveReady += RouterSocket_ReceiveReady;
            _poller = new NetMQPoller { _dealerSocket, _routerSocket };
            _poller.RunAsync();
        }

        private void RouterSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage message = e.Socket.ReceiveMultipartMessage();
            _dealerSocket.SendMultipartMessage(message);
        }

        private void DealerSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage message = e.Socket.ReceiveMultipartMessage();
            string address = message.Pop().ConvertToString(Encoding.UTF8);
            string content = message.Pop().ConvertToString(Encoding.UTF8);
            OwinResponse response = _serializer.Deserialize<OwinResponse>(content);
            if (_requests.TryRemove(response.RequestId, out var taskCompletion))
            {
                taskCompletion.SetResult(response);
            }
        }

        public void Dispose()
        {
            _dealerSocket.Dispose();
            _routerSocket.Dispose();
            _poller.StopAsync();
        }

        public Task<OwinResponse> SendAsync(OwinRequest request)
        {
            var taskCompletionSource = new TaskCompletionSource<OwinResponse>();
            using (NetMQSocket requestSocket = new DealerSocket($"inproc://{Identity}"))
            {
                NetMQMessage sendMessage = new NetMQMessage();
                sendMessage.AppendEmptyFrame();
                sendMessage.Append(_serializer.Serialize(request), Encoding.UTF8);
                requestSocket.SendMultipartMessage(sendMessage);
                _requests.AddOrUpdate(request.Id, taskCompletionSource, (key, value) => taskCompletionSource);
                return taskCompletionSource.Task;
            }
        }
    }

    /// <summary>
    /// Request-Response模式： NetMQ.FiniteStateMachineException:“Req.XSend - cannot send another request”
    /// </summary>
    //public class NetmqTransportClient : ITransportClient
    //{
    //    private readonly NetMQSocket _clientSocket;
    //    private readonly ISerializer<string> _serializer;
    //    private readonly EndPoint _endPoint;
    //    public NetmqTransportClient(ISerializer<string> serializer, EndPoint endPoint)
    //    {
    //        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    //        _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
    //        _clientSocket = new RequestSocket($"tcp://{endPoint}");
    //        _clientSocket.Options.Linger = TimeSpan.Zero;
    //    }

    //    public void Dispose()
    //    {
    //        _clientSocket.Dispose();
    //        NetMQConfig.Cleanup(false);
    //    }

    //    public Task<OwinResponse> SendAsync(OwinRequest request)
    //    {
    //        return Task.Factory.StartNew(() =>
    //        {
    //            _clientSocket.SendFrame(_serializer.Serialize(request));
    //            string resultData = _clientSocket.ReceiveFrameString();
    //            return _serializer.Deserialize<OwinResponse>(resultData);
    //        });
    //    }
    //}
}
