using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Timers;

namespace ConcurrencyTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static Hashtable htClient = new Hashtable();
        public static System.Timers.Timer cmdTimer = new System.Timers.Timer();

        public byte[] cmd;// = new byte[] { 0xA5, 0xA5, 0xFF, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0x5A, 0x5A };

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            int ConnectNum = Convert.ToInt32(ConnectNumBox.Text);
            string localIP = LocalIPBox.Text;
            int localPort = Convert.ToInt32(LocalStartPortBox.Text);

            string serverIP = SeverIPBox.Text;
            int serverPort = Convert.ToInt32(ServerPortBox.Text);

            IPEndPoint remote = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

            cmdTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendCmdOnTime);

            int n = Convert.ToInt32(LengthBox.Text);
            cmd = new byte[n];
            cmd[0] = 0xA5;
            cmd[1] = 0xA5;
            cmd[2] = 0xFF;
            cmd[n-2] = 0xA5;
            cmd[n-1] = 0xA5;
            try
            {
                for (int j = 3; j < n-2; j++)
                {
                    cmd[j] = 0xFF;
                }


                for (i = 0; i < ConnectNum; i++)
                {
                    Socket clientSocket = new Socket(AddressFamily.InterNetwork,
                                                     SocketType.Stream,
                                                     ProtocolType.Tcp);
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort + i);
                    clientSocket.Bind(ipEndPoint);

                    htClient.Add(i, clientSocket);
                }

                foreach (DictionaryEntry de in htClient)
                {
                    Socket socket = (Socket)de.Value;
                    socket.BeginConnect(remote, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int time = Convert.ToInt32(TimeBox.Text);
            cmdTimer.Interval = time; //执行间隔时间,单位为毫秒;

            if (cmdTimer.Enabled == false)
            {
                cmdTimer.Start();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (cmdTimer.Enabled == true)
            {
                cmdTimer.Stop();
            }
        }

        //定时发送命令
        public void SendCmdOnTime(object source, ElapsedEventArgs e)
        {
            try
            {
                foreach (DictionaryEntry de in htClient)
                {
                    Socket socket = (Socket)de.Value;
                    int id = (int)de.Key;
                    byte[] bytesid = new byte[2];
                    bytesid = intToBytes(id);
                    cmd[5] = bytesid[0];
                    cmd[6] = bytesid[1];
                    socket.BeginSend(cmd, 0, cmd.Length, SocketFlags.None, new AsyncCallback(OnSend), socket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        //将int数值转换为占byte数组
        public static byte[] intToBytes(int value)
        {
            byte[] src = new byte[2];

            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }

        //发送数据
        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                string error = DateTime.Now.ToString() + "出错信息：" + "---" + ex.Message + "\n";
                System.Diagnostics.Debug.WriteLine(error);
            }
        }

        
    }
}
