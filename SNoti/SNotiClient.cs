using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SNotiSSL.Config;
using SNotiSSL.Model;
using SNotiSSL.Model.Request;
using SNotiSSL.Model.Response;

namespace SNotiSSL
{
    public class SNotiClient
    {
        private readonly SNotiSocket sockets;
        private readonly SNotiClientConfig _config;
        public bool IsSSLConnected { get; private set; } = false;
        public bool IsSNotiLogined { get; private set; } = false;
        private Subject<bool> IsLogined = new Subject<bool>();
        private IDisposable HeartbeatGenerator = null;
        private Subject<DateTime> HeartbeatTimer = new Subject<DateTime>();
        private IDisposable HeartbeatTimeoutWatcher = null;
        public List<Action<JObject>> MessageHandler = new List<Action<JObject>>();
        private Task ReceiveMessageTask = null;
        public SNotiClient(IOptions<SNotiClientConfig> config)
        {
            _config = config.Value;
            sockets = new SNotiSocket(_config);
            
            // 订阅状态
            sockets.ConnectStatus.Subscribe(OnSSLConnectChanged,
                                            (Err) => Console.Error.WriteLine(Err.Message));
            IsLogined.Subscribe(login => IsSNotiLogined = login);

            RegisterHeartbeatSender();
        }

        public bool CanUseSNotiClient() => IsSSLConnected && IsSNotiLogined;

        public void AddMessageHandler(Action<JObject> handler) => MessageHandler.Add(handler);
        public void RemoveMessageHandler(Action<JObject> handler) => MessageHandler.Remove(handler);
        public void Connect()
        {
            if (IsSSLConnected)
                DisConnect();
            sockets.Connect();
        }
        public void DisConnect()
        {
            ReceiveMessageTask?.Dispose();
            sockets.DisConnect();
        }
        private void OnSSLConnectChanged(bool status)
        {
            IsSSLConnected = status;
            if (status)
            {
                System.Console.WriteLine("成功连接. 准备登陆.");
                Login();
            }
            else
            {
                System.Console.WriteLine("连接已断开.");
                IsLogined.OnNext(false);
            }
        }
        private void Login()
        {
            if (IsSSLConnected)
            {
                var login = new Login()
                {
                    cmd = SNotiCommandType.Login_Req,
                    PrefetchCount = _config.PrefetchCount,
                    data = new[] {
                        new LoginAuthorizationData()
                        {
                            ProductKey = _config.ProductKey,
                            AuthId = _config.AuthId,
                            AuthSecret = _config.AuthSecret,
                            SubKey = _config.SubKey
                        }
                    }
                };
                SendMessage(JsonConvert.SerializeObject(login));
                ReceiveMessage();
            }
            else throw new Exception("SSL没有成功连接, 不允许登陆.");
        }
        public void SendMessage(string Message)
        {
            sockets.SendMessage(Message);
        }
        private void ReceiveMessage()
        {
            ReceiveMessageTask = Task.Run(() =>
            {
                while (true)
                {
                    string message = sockets.ReceiveMessageQueue.Take();
                    System.Console.WriteLine($"收到信息: {message}");
                    var jObj = JObject.Parse(message);
                    if (jObj.ContainsKey("delivery_id") && jObj.ContainsKey("msg_id"))
                    {
                        AckMessage(jObj);
                    }
                    HandleMessage(jObj);
                }
            });
        }

        private void AckMessage(JObject message)
        {
            string MessageId = message["msg_id"].Value<string>();
            int DeliveryId = message["delivery_id"].Value<int>();
            var ack = new MessageAck()
            {
                DeliveryId = DeliveryId,
                MsgId = MessageId
            };
            sockets.SendMessage(JsonConvert.SerializeObject(ack));
        }
        private void HandleMessage(JObject message)
        {
            var command = SNotiCommandType.Parse(message["cmd"].Value<string>());
            if (command.Equals(SNotiCommandType.Login_Res))
            {
                Observable.Return(message)
                           .Select(x => x["data"].Value<JObject>())
                           .Select(x => x["result"].Value<bool>())
                           .Subscribe(x => IsLogined.OnNext(x));
            }
            else if (command.Equals(SNotiCommandType.Pong))
            {
                HeartbeatTimer.OnNext(DateTime.Now);
            }
            else MessageHandler.ForEach(handle => 
            {
                handle(message);
            });
        }

        private void RegisterHeartbeatSender()
        {
            HeartbeatTimeoutWatcher = Observable.Timeout(HeartbeatTimer, TimeSpan.FromSeconds(_config.HeartbeatIntervalSeconds * 1.5))
                                    .Subscribe((_) => { }, HeartbeatTimeoutHandler);
            IsLogined.Subscribe((login) =>
            {
                if (login)
                {
                    System.Console.WriteLine("登陆成功. 注册心跳发送定时器.");
                    HeartbeatGenerator = Observable.Interval(TimeSpan.FromSeconds(_config.HeartbeatIntervalSeconds))
                                                   .Subscribe(_ => {sockets.SendHeartbeat();});
                }
                else
                {
                    System.Console.WriteLine("取消心跳发送定时器。");
                    HeartbeatTimeoutWatcher?.Dispose();
                    HeartbeatGenerator?.Dispose();
                }
            });
        }

        private void HeartbeatTimeoutHandler(Exception timeout)
        {
            if (timeout is TimeoutException)
            {
                System.Console.WriteLine($"服务器未能及时回应心跳包, 可能连接已经终止。开始重新连接.");
                DisConnect();
                Connect();
            }
        }
    }
}