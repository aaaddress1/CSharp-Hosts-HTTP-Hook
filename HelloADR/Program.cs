using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
namespace HelloADR
{
    class WebProxy
    {
        Socket clientSocket;
        Byte[] read = new byte[1024];
        Byte[] Buffer = null;

        public WebProxy(Socket socket)
        {
            this.clientSocket = socket;
        }

        public void run()
        {
            String clientmessage = " ";
            int bytes = readmessage(read, ref clientSocket, ref clientmessage);
            if (bytes == 0)
            {
                return;
            }
            int index1 = clientmessage.IndexOf(' ');
            int index2 = clientmessage.IndexOf(' ', index1 + 1);
            if ((index1 == -1) || (index2 == -1))
            {
                throw new IOException();
            }
            string HostStr = new Regex("Host:([^\r]+)\r").Match(clientmessage).Groups[1].Value;
            string RequestPath = new Regex("GET(.*?)HTTP").Match(clientmessage).Groups[1].Value;
            if (RequestPath.Contains("check/ipc.php?url=http://www2"))
            {
                Console.WriteLine("Connecting to Site: {0}", RequestPath);
                Console.WriteLine("Connection from {0}", clientSocket.RemoteEndPoint);
                string next_uri = "http://www2.kugou.com/fm2/index.html?v=7400&ver=7680&pass=vip";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(next_uri);
                String strRetPage = new StreamReader(request.GetResponse().GetResponseStream(),Encoding.UTF8 ).ReadToEnd();
                sendmessage(clientSocket, strRetPage);
            }
            clientSocket.Close();
        }
        private int readmessage(byte[] ByteArray, ref Socket s, ref String clientmessage)
        {
            int bytes = s.Receive(ByteArray, 1024, 0);
            string messagefromclient = Encoding.ASCII.GetString(ByteArray);
            clientmessage = (String)messagefromclient;
            return bytes;
        }
        private void sendmessage(Socket s, string message)
        {
            Buffer = new Byte[message.Length*2 + 1];
            int length = Encoding.UTF8.GetBytes(message, 0, message.Length, Buffer, 0);
            s.Send(Buffer, length, 0);
        }

        public static void SetHosts()
        {
            string fpath = @"C:\WINDOWS\system32\drivers\etc\hosts";
            File.SetAttributes(@fpath, FileAttributes.Normal);
            FileStream fs = new FileStream(@fpath, FileMode.OpenOrCreate);
            StreamReader rw = new StreamReader(fs);
            if (rw.ReadToEnd().Contains("127.0.0.1 www.kugou.com")) return;
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("\n127.0.0.1 www.kugou.com");
            sw.Close();
            fs.Close();
        }

        static void Main(string[] args)
        {
            try
            {
                WebProxy.SetHosts();
                const int port = 80;
                TcpListener tcplistener = new TcpListener(port);
                Console.WriteLine("Hello, I'm ADR. I'm Hooking");
                Console.WriteLine("Powered By aaaddress1@gmail.com");
                tcplistener.Start();
                while (true)
                {
                    Socket socket = tcplistener.AcceptSocket();
                    WebProxy webproxy = new WebProxy(socket);
                    Thread thread = new Thread(new ThreadStart(webproxy.run));
                    thread.Start();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Fail To Listen Port 80");
                throw;
            }
        }
    }
}