using NetMQ;
using NetMQ.Sockets;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class NetMQRequestor : IDisposable
    {
        private readonly DealerSocket clientSocket;
        private readonly string connectionString;

        public NetMQRequestor(string connString)
        {
            clientSocket = new DealerSocket();
            connectionString = connString;
            clientSocket.Connect(connString);
        }

        public string SendMessage(string message)
        {
            clientSocket.SendFrame(message, false);
            string res= clientSocket.ReceiveFrameString();
            Console.WriteLine(res);
            return res;
        }

        public void Dispose()
        {
            clientSocket?.Disconnect(connectionString);
            clientSocket?.Dispose();
        }
    }

    public class IpcCollectorDevice
    {
        private NetMQPoller _poller;
        private RouterSocket _frontendSocket;
        private DealerSocket _backendSocket;
        private readonly string _backEndAddress;
        private readonly string _frontEndAddress;
        private readonly ManualResetEvent _semaphoreStart = new ManualResetEvent(false);
        private readonly Thread _collectorThread;

        public IpcCollectorDevice(string frontEndAddress, string backEndAddress)
        {
            _backEndAddress =  backEndAddress; //connect address
            _frontEndAddress = frontEndAddress; //bind address

            _collectorThread = new Thread(DoWork) { Name = "IPC Collector Device Thread" };
        }

        public void Start()
        {
            _collectorThread.Start();
            _semaphoreStart.WaitOne();
        }

        public void Stop()
        {
            _poller.Stop();
        }

        private void DoWork()
        {
            try
            {
                _poller = new NetMQPoller();

                using (_frontendSocket = new RouterSocket(_frontEndAddress))
                using (_backendSocket = new DealerSocket(_backEndAddress))
                {
                    _backendSocket.ReceiveReady += OnBackEndReady;
                    _frontendSocket.ReceiveReady += OnFrontEndReady;

                    _poller.Add(_backendSocket);
                    _poller.Add(_frontendSocket);

                    _semaphoreStart.Set();

                    _poller.Run();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void OnBackEndReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage message = _backendSocket.ReceiveMultipartMessage();
            _frontendSocket.SendMultipartMessage(message);
        }

        private void OnFrontEndReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage message = _frontendSocket.ReceiveMultipartMessage();
            _backendSocket.SendMultipartMessage(message);
        }

    }


    class Program
    {
        public static void Main(string[] args)
        {
            string tcp = "tcp://127.0.0.1:20000";
            string inproc = "inproc://response_dealer";

            ResponseSocket res = new ResponseSocket(inproc);

            IpcCollectorDevice ipcCollectorDevice = new IpcCollectorDevice(tcp, inproc);
            ipcCollectorDevice.Start();

            NetMQRequestor requestor = new NetMQRequestor(tcp);
            var message = Guid.NewGuid().ToString();

            requestor.SendMessage(message);
            // NOTES
            // 1. Use ThreadLocal&lt;DealerSocket&gt; where each thread has
            //    its own client DealerSocket to talk to server
            // 2. Each thread can send using it own socket
            // 3. Each thread socket is added to poller

            const int delay = 10; // millis

            var clientSocketPerThread = new ThreadLocal<DealerSocket>();
            int d = 0;
            using (var server = new RouterSocket("@tcp://127.0.0.1:5556"))
            using (var poller = new NetMQPoller())
            {
                // Start some threads, each with its own DealerSocket
                // to talk to the server socket. Creates lots of sockets,
                // but no nasty race conditions no shared state, each
                // thread has its own socket, happy days.
                for (int i = 0; i < 3; i++)
                {
                    Task.Factory.StartNew(state =>
                    {
                        DealerSocket client = null;

                        if (!clientSocketPerThread.IsValueCreated)
                        {
                            client = new DealerSocket();
                            client.Options.Identity =
                                Encoding.Unicode.GetBytes(state.ToString());
                            client.Connect("tcp://127.0.0.1:5556");
                            client.ReceiveReady += Client_ReceiveReady;
                            clientSocketPerThread.Value = client;
                            poller.Add(client);
                        }
                        else
                        {
                            client = clientSocketPerThread.Value;
                        }

                        while (true)
                        {
                            var messageToServer = new NetMQMessage();
                            messageToServer.AppendEmptyFrame();
                            messageToServer.Append(Interlocked.Increment(ref d).ToString());
                            //Console.WriteLine("======================================");
                            //Console.WriteLine(" OUTGOING MESSAGE TO SERVER ");
                            //Console.WriteLine("======================================");
                            //PrintFrames("Client Sending", messageToServer);
                            //messageToServer.ToString();
                            client.SendMultipartMessage(messageToServer);
                            Thread.Sleep(delay);
                        }

                    }, string.Format("client {0}", i), TaskCreationOptions.LongRunning);
                }

                // start the poller
                poller.RunAsync();

                // server loop
                while (true)
                {
                    string date = DateTime.Now.ToString();
                    var clientMessage = server.ReceiveMultipartMessage();
                    //Console.WriteLine("======================================");
                    //Console.WriteLine(" INCOMING CLIENT MESSAGE FROM CLIENT ");
                    //Console.WriteLine("======================================");
                    //PrintFrames("Server receiving", clientMessage);
                    Thread.Sleep(new Random().Next(0, 1000));
                    if (clientMessage.FrameCount == 3)
                    {
                        var clientAddress = clientMessage[0];
                        var clientOriginalMessage = clientMessage[2].ConvertToString();
                        string response = string.Format("{0} back from server {1}", clientOriginalMessage, date);
                        var messageToClient = new NetMQMessage();
                        messageToClient.Append(clientAddress);
                        messageToClient.AppendEmptyFrame();
                        messageToClient.Append(response);
                        server.SendMultipartMessage(messageToClient);
                    }
                }
            }
        }

        static void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }

        static void Client_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            bool hasmore = false;
            e.Socket.ReceiveFrameString(out hasmore);
            if (hasmore)
            {
                string result = e.Socket.ReceiveFrameString(out hasmore);
                Console.WriteLine("REPLY {0}", result);
            }
        }
    }
}
