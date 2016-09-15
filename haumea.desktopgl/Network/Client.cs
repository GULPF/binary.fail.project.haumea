using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Haumea.Network
{
    using Debug = System.Diagnostics.Debug;
    class Client
    {
        private Thread _clientSender, _clientReceiver;
        private Socket _serverSocket;
        private IPEndPoint _serverEndpoint;
        private UdpClient _client;
        private string _serverIp;
        private int _serverPort;
        private bool _cancellationToken;
        public Client(string ip, int portNumber)
        {
            _serverIp = ip;
            _serverPort = portNumber;
            _cancellationToken = false;
            _clientReceiver = new Thread(() => clientReceiver());
            _clientSender = new Thread(() => clientSender());
        }
        public void start()
        {
            try
            {
                _serverEndpoint = new IPEndPoint(IPAddress.Parse(_serverIp), _serverPort);
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _serverSocket.Connect(_serverEndpoint);
                _client = new UdpClient((IPEndPoint)_serverSocket.LocalEndPoint);
                _clientSender.Start();
                _clientReceiver.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void stop()
        {
            _cancellationToken = true;
            _client.Close();
        }
        private void clientSender()
        {
            int count = 0;
            while (!_cancellationToken)
            {
                //this is just for testing, in the future, use buffers, same as server
                Payload[] test = new Payload[1];
                test[0] = new Handshake();
                byte[] write_bytes = new Packet(1, test).Serialize();
                try
                {
                    _serverSocket.SendTo(write_bytes, _serverEndpoint);
                    count++;
                }
                catch (Exception e)
                {
                    //Perhaps resend the message
                }
                Thread.Sleep(1000);
            }
            Debug.WriteLine("Client Sender terminated. Sent {0} messages", count);
        }
        private void clientReceiver()
        {

            byte[] read_bytes;
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            int count = 0, countvalid = 0;
            while (!_cancellationToken)
            {
                try
                {
                    // Blocking read call
                    read_bytes = _client.Receive(ref sender);
                    count++;
                    if ((read_bytes != null) && (read_bytes.Length > 0))
                    {
                        Packet packet = Packet.Deserialize(read_bytes);
                        if (packet != null)
                        {
                            countvalid++;
                            foreach (Payload p in packet.Payload)
                            {
                                Type type = p.GetType();
                                if (type.Equals(typeof(Ack)))
                                {
                                    Debug.WriteLine("Server returned an Ack object");
                                }

                            }
                        }
                        else { Debug.WriteLine("Unknown crap from the server"); }
                    }

                }
                catch (SocketException)
                {
                    continue;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Client Listener exception: {0}", exception.Message);
                }
            }
            Debug.WriteLine("Client Receiver terminated. Received {0} messages, {1} valid ({2}%)", count, countvalid, ((double)countvalid / count * 100).ToString("F"));

        }
    }
}
