using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Haumea.Network
{
    class Server
    {
        Thread _serverListener;

        public Server()
        {
            //Should be moved to another method, this class is for handling all server relations
            _serverListener = new Thread(() => serverListener(6667)); //dont forget to fix the port number
            _serverListener.Start();
        }
        private void serverListener(int portNumber)
        {
            //Set up UDP basics
            UdpClient listen = new UdpClient(portNumber);
            IPEndPoint target = new IPEndPoint(IPAddress.Any, portNumber);
            byte[] read_bytes;
            bool active = true;
            while (active)
            {
                try
                {
                    //Blocking read call
                    read_bytes = listen.Receive(ref target);
                    //Sender is target.ToString(), decode to string for example: Encoding.ASCII.GetString(read_bytes, 0, read_bytes.Length)
                    //Handle checksum and such, perhaps delegate to another thread per user
                }
                catch (Exception)
                {
                    //If delegated, exceptions generated here should not kill the loop
                    active = false;
                }
            }
            listen.Close();
        }

        //Idea is to have one of these spinning for every client connected, reading from synchronized game data container
        private void serverSender(String ip, int portNumber, String SYNCHRONIZED_CONTAINER) //TODO: create handler for outgoing buffer
        {
            bool bufferEmpty = false; //change to buffer handler
            //Set up UDP basics, unreliable Datagrams
            Socket sending = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(ip), portNumber);
            while (!bufferEmpty)
            {
                //some fetch string, bytes, whatever
                byte[] write_bytes = Encoding.ASCII.GetBytes(SYNCHRONIZED_CONTAINER); 
                try
                {
                    sending.SendTo(write_bytes, target);
                }
                catch (Exception)
                {
                    //Perhaps resend the message
                }
            }
        }
    }
}
