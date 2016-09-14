using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Haumea.Network
{
    using Debug = System.Diagnostics.Debug;
    class Server
    {
        private int _portNumber;
        private UdpClient _server;
        private Thread _serverListener, _serverMessageProcessor, _serverSender;
        private ServerMessageBuffer _buffer;
        private bool _cancellationToken;
        private readonly object _opReadMutex, _opProcessMutex, _opSendMutex;
        private readonly object _throttleMutex;
        private HashSet<IPAddress> _throttleAddresses;
        private IDictionary<IPEndPoint, ClientConnection> _clients;

        public Server(int portNumber)
        {
            _portNumber = portNumber;
            _serverListener = new Thread(() => serverReceiver());
            _serverMessageProcessor = new Thread(() => serverMessageProcessor());
            _serverSender = new Thread(() => serverSender());
            _buffer = new ServerMessageBuffer();
            _cancellationToken = false;
            _opReadMutex = new object();
            _opProcessMutex = new object();
            _opSendMutex = new object();
            _throttleMutex = new object();
            _throttleAddresses = new HashSet<IPAddress>();
            _clients = new Dictionary<IPEndPoint, ClientConnection>();
        }
        private void cancelOperations()
        {
            lock (_opSendMutex) lock (_opProcessMutex) lock (_opReadMutex)
                    {
                        _cancellationToken = true;
                    }
        }
        private bool readCancellation() { lock (_opReadMutex) { return _cancellationToken; } }
        private bool processCancellation() { lock (_opProcessMutex) { return _cancellationToken; } }
        private bool sendingCancellation() { lock (_opSendMutex) { return _cancellationToken; } }

        public void start()
        {
            if ((_serverListener != null) && (_serverListener.IsAlive))
            {
                throw new InvalidOperationException("Server is already running.");
            }
            try
            {
                Debug.WriteLine("Initializing server...");
                _server = new UdpClient(_portNumber);
                _serverListener.Start();
                _serverMessageProcessor.Start();
                _serverSender.Start();
                Debug.WriteLine("Server initialized.");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void stop()
        {
            if ((_serverListener == null) || (!_serverListener.IsAlive))
            {
                throw new InvalidOperationException("Server is not running.");
            }
            try
            {
                Debug.WriteLine("Attempting to close server...");
                cancelOperations();
                _server.Close();
                _buffer.cancelReaders();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void serverReceiver()
        {
            Debug.WriteLine("Server Receiver started");
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            byte[] read_bytes;
            uint count = 0, countvalid = 0;
            while (!readCancellation())
            {
                try
                {
                    // Blocking read call
                    read_bytes = _server.Receive(ref client);
                    count++;
                    if ((read_bytes != null) && (read_bytes.Length > 0))
                    {
                        lock (_throttleMutex)
                        {
                            if (_throttleAddresses.Contains(client.Address)) { continue; }
                        }
                        countvalid++;
                        _buffer.putReceived(client, read_bytes);
                    }

                }
                catch (SocketException)
                {
                    continue;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("ServerListener exception: {0}", exception.Message);
                }
            }
            Debug.WriteLine("Server thread terminated. Read UDP {0} times, {1} nonthrottled ({2}%)", count, countvalid, ((double)countvalid / count * 100).ToString("F"));
        }

        private void serverMessageProcessor()
        {
            Debug.WriteLine("Server Message Processor started");
            uint count = 0, countvalid = 0;
            Dictionary<IPAddress, int> formatOffenders = new Dictionary<IPAddress, int>();
            while (!processCancellation())
            {
                // Blocking read call
                var messageList = _buffer.getReceived();
                if (messageList != null)
                {
                    //Debug.WriteLine("Processing {0} messages!", messageList.Count);
                    foreach (var entry in messageList)
                    {
                        count++;
                        IPEndPoint source = entry.Key;
                        byte[] message = entry.Value;
                        Packet packet = Packet.Deserialize(message);
                        if (packet == null)
                        {
                            if (!formatOffenders.ContainsKey(source.Address)) { formatOffenders.Add(source.Address, 0); }
                            formatOffenders[source.Address]++;
                            if (formatOffenders[source.Address] > 10)
                            {
                                lock (_throttleMutex)
                                {
                                    _throttleAddresses.Add(source.Address);
                                }
                            }
                            continue;
                        }
                        if ((formatOffenders.ContainsKey(source.Address)) && (formatOffenders[source.Address] > 10))
                        {
                            continue;
                        }
                        if (_clients.ContainsKey(source))
                        {
                            //add logic for client handling later
                        }
                        //if (!clients.Contains(message.Key)) { clients.Add(message.Key); }

                        countvalid++;
                        foreach (Payload p in packet.Payload)
                        {
                            Type type = p.GetType();
                            if (type.Equals(typeof(Player)))
                            {
                                Payload[] test = new Payload[1];
                                test[0] = new Player("ACK", 57);
                                _buffer.putOutgoing(source, new Packet(1, test).Serialize());
                            }
                           
                        }
                    }
                }
            }
            // Debug.WriteLine("Size:{0}", clients.Count);
            // foreach (var a in clients)
            //{
            //Debug.WriteLine("Client: {0}", a.Address);
            //}
            Debug.WriteLine("Server Message Processor terminated. Processed {0} messages, {1} valid ({2}%)", count, countvalid, ((double)countvalid / count * 100).ToString("F"));
        }

        private void serverSender()
        {
            Debug.WriteLine("Server Sender started");
            uint count = 0, countvalid = 0;
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            while (!sendingCancellation())
            {
                // Blocking read call
                var messageList = _buffer.getOutgoing();
                if (messageList != null)
                {
                    foreach (var entry in messageList)
                    {
                        count++;
                        IPEndPoint target = entry.Key;
                        byte[] message = entry.Value;
                        try
                        {
                            sender.SendTo(message, target);
                            //Debug.WriteLine("Server sent message to {0}:{1}", target.Address, target.Port);
                            countvalid++;
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Exception while trying to send, " + e.Message);
                        }
                    }
                }
            }
            Debug.WriteLine("Server Sender terminated. Processed {0} messages, {1} were sent ({2}%)", count, countvalid, ((double)countvalid / count * 100).ToString("F"));
        }


        private class ClientConnection
        {
            private uint _sequence;
            private uint _acknowledged;

            public ClientConnection() { }

        }

        private class ServerMessageBuffer
        {
            private List<KeyValuePair<IPEndPoint, byte[]>> _incomingBuffer;
            private List<KeyValuePair<IPEndPoint, byte[]>> _outgoingBuffer;
            private readonly object _incomingBufferMutex;
            private readonly object _outgoingBufferMutex;
            private bool _cancellationToken;
            public ServerMessageBuffer()
            {
                _incomingBuffer = new List<KeyValuePair<IPEndPoint, byte[]>>();
                _outgoingBuffer = new List<KeyValuePair<IPEndPoint, byte[]>>();
                _incomingBufferMutex = new object();
                _outgoingBufferMutex = new object();
                _cancellationToken = false;
            }
            public void cancelReaders()
            {
                try
                {
                    lock (_incomingBufferMutex)
                    {
                        lock (_outgoingBufferMutex)
                        {
                            _cancellationToken = true;
                            Monitor.PulseAll(_incomingBufferMutex);
                            Monitor.PulseAll(_outgoingBufferMutex);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            public void putReceived(IPEndPoint source, byte[] message)
            {
                try
                {
                    lock (_incomingBufferMutex)
                    {
                        _incomingBuffer.Add(new KeyValuePair<IPEndPoint, byte[]>(source, message));
                        Monitor.Pulse(_incomingBufferMutex);

                    }
                }
                //Empty catch, threads not expected to ever throw anyway
                catch (Exception) { return; }
            }
            public List<KeyValuePair<IPEndPoint, byte[]>> getReceived()
            {
                List<KeyValuePair<IPEndPoint, byte[]>> bufferClone = null;
                lock (_incomingBufferMutex)
                {
                    while ((_incomingBuffer.Count == 0) && (!_cancellationToken))
                    {
                        try
                        {
                            Monitor.Wait(_incomingBufferMutex);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    if (_cancellationToken) { return null; }
                    bufferClone = new List<KeyValuePair<IPEndPoint, byte[]>>(_incomingBuffer);
                    _incomingBuffer.Clear();
                }
                return bufferClone;
            }
            public void putOutgoing(IPEndPoint source, byte[] message)
            {
                try
                {
                    lock (_outgoingBufferMutex)
                    {
                        _outgoingBuffer.Add(new KeyValuePair<IPEndPoint, byte[]>(source, message));
                        Monitor.Pulse(_outgoingBufferMutex);

                    }
                }
                //Empty catch, threads not expected to ever throw anyway
                catch (Exception) { return; }
            }
            public List<KeyValuePair<IPEndPoint, byte[]>> getOutgoing()
            {
                List<KeyValuePair<IPEndPoint, byte[]>> bufferClone = null;
                lock (_outgoingBufferMutex)
                {
                    while ((_outgoingBuffer.Count == 0) && (!_cancellationToken))
                    {
                        try
                        {
                            Monitor.Wait(_outgoingBufferMutex);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    if (_cancellationToken) { return null; }
                    bufferClone = new List<KeyValuePair<IPEndPoint, byte[]>>(_outgoingBuffer);
                    _outgoingBuffer.Clear();
                }
                return bufferClone;
            }
        }
    }
}
