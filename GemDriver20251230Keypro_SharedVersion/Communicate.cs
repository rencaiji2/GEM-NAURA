using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace CNCVision
{
    class Communicate
    {
       

        public Socket mySocket;
        public IPEndPoint myIPEndPoint;
        public byte[] tempByte = new byte[20000000];
        public string receiveMess;


        public enum NetBehavior
        {
            Server=1,Client=2

        }

        public bool conResult = false;

        public NetBehavior myNetBehavior;   //定义作为服务器运行还是客户端运行

        public delegate void ConnectResultEventHandler(object sender,ResultEventArgs e);

        public delegate void MessageReceivedEventHandler(object sender, MessageEventArgs e);

        public event MessageReceivedEventHandler MessageReceived;//接受到数据事件

        public event ConnectResultEventHandler ConnectedResult;//连接成功事件

         

        public Communicate(NetBehavior netBehavior,string ip,int port)
        {
            this.myIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myNetBehavior = netBehavior;

            System.Threading.Thread iniThread = new System.Threading.Thread(Connect);
            iniThread.IsBackground = true;
            iniThread.Start();
            receiveMess = "";

            System.Threading.Thread receiveThread = new System.Threading.Thread(ReceiveMessage);
            receiveThread.IsBackground = true;
            receiveThread.Start();
            receiveMess = "";
        }



        public class ResultEventArgs : EventArgs
        {
            public readonly bool conResult;

            public ResultEventArgs(bool conResult)
            {
                this.conResult = conResult;
            }
        }

        public class MessageEventArgs : EventArgs
        {
            public readonly string Message;

            public MessageEventArgs(string Message)
            {
                this.Message = Message;
            }
        }

        public string IpAddress//定义属性--IP地址
        {
            set
            {
                this.myIPEndPoint.Address = IPAddress.Parse(value);
            }
            get
            {
                return this.myIPEndPoint.Address.ToString();
            }

        }

        public int Port//定义属性--端口号
        {
            set
            {
                this.myIPEndPoint.Port = value;
            }
            get
            {
                return myIPEndPoint.Port;
            }

        }



        public void Connect()//尝试连接服务器或者客户端
        {
            while (true)
            {
                if (myNetBehavior == NetBehavior.Client)
                {
                    
                        try
                        {
                            System.Threading.Thread.Sleep(300);
                            mySocket.Connect(myIPEndPoint);
                            conResult = true;
                            ResultEventArgs e = new ResultEventArgs(conResult);
                            ResultToDo(e);
                            break;
                        }
                        catch
                        {
                            conResult = false;

                        }
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {
                       
                        this.mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        myIPEndPoint.Address = IPAddress.Parse(this.IpAddress);
                        mySocket.Bind(myIPEndPoint);
                        mySocket.Listen(1);
                        mySocket = mySocket.Accept();
                        conResult = true;
                        ResultEventArgs e = new ResultEventArgs(conResult);
                        ResultToDo(e);
                    }
                    catch
                    {
                    }
                    return;
                }
               
            }
            
        }

        protected virtual void ResultToDo(ResultEventArgs e)
        {
            if (ConnectedResult != null)
            {
                ConnectedResult(this, e);
            }

        }

        private void ReceiveMessage()//接收数据
        {
            while (true)
            {
                if (mySocket.Connected)
                {
                    try
                    {
                        int numReceive = mySocket.Receive(tempByte, SocketFlags.None);
                        if (numReceive > 0)
                        {
                            receiveMess = Encoding.ASCII.GetString(tempByte,0,numReceive);
                            MessageEventArgs e = new MessageEventArgs(receiveMess);
                            if (MessageReceived != null)
                            {
                                MessageReceived(this, e);
                            }
                        }
                        else
                        {
                            byte[] temp = Encoding.Default.GetBytes(" ");
                            mySocket.Send(temp);
                        }
                    }
                    catch
                    {
                        conResult = false;
                        ResultEventArgs e = new ResultEventArgs(conResult);
                        ResultToDo(e);
                        if (this.mySocket.Connected)
                        {
                            this.mySocket.Shutdown(SocketShutdown.Both);
                        }
                        if (this.mySocket != null)
                        {
                           this.mySocket.Close();
                            this.mySocket = null;
                            
                        }
                        GC.Collect();
                        this.mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        System.Threading.Thread connectThread = new System.Threading.Thread(Connect);
                        connectThread.IsBackground = true;
                        connectThread.Priority = System.Threading.ThreadPriority.Normal;
                        connectThread.Start();
                    }

                }

            }

        }
        public void SendMessage(string s)//发送数据
        {
            if (mySocket.Connected)
            {
                byte[] buffer = Encoding.Default.GetBytes(s);
                try
                {
                    mySocket.Send(buffer,buffer.Length ,SocketFlags.None);
                }
                catch
                { 
                    System.Windows.Forms.MessageBox.Show("网络可能没有成功连接，发送数据时出现错误!");
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("网络未连接!");
            }
        }


    }

   

}
