using System;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SNotiSSL.Config;
using SNotiSSL.Model;

namespace SNotiSSL
{
    public class SNotiSocket : IDisposable
    {
        private readonly SNotiClientConfig _config;
        private TcpClient tcpClient;
        private SslStream sslStream;

        public BlockingCollection<string> ReceiveMessageQueue;
        public BlockingCollection<string> SendMessageQueue;

        private Task SendMessageTask;
        private Task ReceiveMessageTask;

        public Subject<bool> ConnectStatus { get; set; }
        private bool IsConnected = false;

        public SNotiSocket(SNotiClientConfig config)
        {
            _config = config;
            ConnectStatus = new Subject<bool>();
            ConnectStatus.Subscribe((conn) => IsConnected = conn, (err) => System.Console.Error.WriteLine(err.Message));
        }

        public void Connect()
        {
            InitMessageQueue();

            try
            {
                tcpClient = new TcpClient(_config.Host, _config.Port);
                if (tcpClient.Connected)
                {
                    // Begin connect and authorize ssl stream.

                    sslStream = new SslStream(tcpClient.GetStream(),
                                              false,
                                             new RemoteCertificateValidationCallback(ValidateServerCertificate),
                                             null);
                    try
                    {
                        sslStream.AuthenticateAsClient(_config.Host);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                        {
                            Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        }
                        Console.WriteLine("Authentication failed - closing the connection.");
                        tcpClient.Close();
                        ConnectStatus.OnError(e);
                    }
                    ConnectStatus.OnNext(true);
                    BeginReceiveMessageTask();
                    BeginSendMessageTask();
                }
                else ConnectStatus.OnError(new System.Exception($"Cannot connect to server {_config.Host}:{_config.Port}"));
            }
            catch (Exception outer)
            {
                ConnectStatus.OnError(outer);
            }
        }

        public void SendHeartbeat()
        {
            SendMessageQueue.Add(SNotiCommandType.Ping.Order);
        }

        public void SendMessage(string message)
        {
            SendMessageQueue.Add(message + "\n");
        }
        private void BeginReceiveMessageTask()
        {
            ReceiveMessageTask = Task.Run(() =>
            {
                while (true)
                {
                    string message = ReadMessage();
                    if (message != null)
                    {
                        ReceiveMessageQueue.Add(message);
                    }
                }
            });
        }
        private void BeginSendMessageTask()
        {
            SendMessageTask = Task.Run(() =>
            {
                while (true)
                {
                    string message = null;
                    try
                    {
                        message = SendMessageQueue.Take();
                        System.Console.WriteLine("发送:  " + message);
                    }
                    catch (InvalidOperationException) { }
                    if (message != null)
                    {
                        var msgBytes = Encoding.UTF8.GetBytes(message);
                        sslStream.Write(msgBytes);
                        sslStream.Flush();
                    }
                }
            });
        }
        /*  ----  Static Methods ---- */

        public static bool ValidateServerCertificate(
                    object sender,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        private string ReadMessage()
        {
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("\n") != -1)
                {
                    break;
                }
            } while (bytes != 0);
            return messageData.ToString();
        }
        public void DisConnect()
        {
            if (IsConnected)
            {
                sslStream.Close();
                tcpClient.Close();
            }
            ReceiveMessageTask?.Dispose();
            SendMessageTask?.Dispose();
            ConnectStatus.OnNext(false);
        }
        private void InitMessageQueue()
        {
            SendMessageQueue?.Dispose();
            SendMessageQueue = new BlockingCollection<string>(_config.ControlQueueCapacity);
            ReceiveMessageQueue?.Dispose();
            ReceiveMessageQueue = new BlockingCollection<string>(_config.ReceiveQueueCapacity);
        }
        public void Dispose()
        {
            sslStream?.Dispose();
            tcpClient?.Dispose();
            ReceiveMessageTask?.Dispose();
            SendMessageTask?.Dispose();
            SendMessageQueue?.Dispose();
            ReceiveMessageQueue?.Dispose();
        }
    }
}