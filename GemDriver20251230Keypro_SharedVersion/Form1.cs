using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace GemDriver
{
    public partial class Form1 : Form
    {

        public static Form1 Form;
        public Form1()
        {
            InitializeComponent();
            Form = this;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        CNCVision.Communicate Communicate;
        CNCVision.Communicate CommunicateSV;

        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        System.Threading.Thread GUI_RefreshThread;

        System.Threading.Thread S1F3_RefreshThread;

        public ArrayList EQ_S6F11 = new ArrayList();

        public ArrayList message = new ArrayList();

        public int aaa = 0;


        //得到當前線程的handler
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //SetThreadAffinityMask 指定hThread 运行在 核心 dwThreadAffinityMask
        static extern IntPtr GetCurrentThread();
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);
        private void Form1_Load(object sender, EventArgs e)
        {
            Global.WorkDir = Application.StartupPath.Substring(0, Application.StartupPath.LastIndexOf("\\") + 1);

            Global.Log_Path = Global.WorkDir + "Log/";
            Global.DisplayInfo("程序开始加载！。");
            //自動隱藏
            this.BeginInvoke(new Action(() => {
                this.Hide();
            }));
            InitialDialog();

            Equipment_Connect();
            Host_Connect();
            TCP_Connect();
            //TCP_ConnectSV();
            UpDataGEM_ToUI();

            //dataGrid_EQ_SVID_TableCH1.DoubleBuffered(true);


            Global.GUI_Refresh_Flag = true;
            GUI_RefreshThread = new System.Threading.Thread(RunGUI_RefreshThread);
            GUI_RefreshThread.Start();

            Global.S1F3_Refresh_Flag = true;
            S1F3_RefreshThread = new System.Threading.Thread(RunS1F3_RefreshThread);
            S1F3_RefreshThread.Start();

            //ReportID();   //北方华创报告ID初始化
            //CEID();

            //for (int i = 0; i <Global.SECS_RGA.Length; i++)
            //{
            //    Global.SECS_RGA[i] = "N";
            //    Global.SECS_RGA_EQ[i] = "N";
            //}
            //Global.Time = "yyyy-MM-dd'T'HH:mm:ssXXX";
            //string timer = DateTime.Now.ToString(Global.Time);
            //Global.SECS_RGA[25] = "NULL";
            //Global.SECS_RGA[31] = "NULL";
            //Global.SECS_RGA[37] = "NULL";
            //Global.SECS_RGA[43] = "NULL";
            //Global.SECS_RGA[47] = "NULL";
            //Global.SECS_RGA[48] = "NULL";
            //Global.SECS_RGA[51] = "NULL";
            //Global.SECS_RGA[57] = "NULL";
            //Global.SECS_RGA[61] = "NULL";
            //Global.SECS_RGA[66] = "NULL";
            //Global.SECS_RGA[100] = "0";
            //Global.SECS_RGA[101] = "0";

            timer1.Interval = 200;
            timer1.Enabled = true;
            timer3.Interval = 200;

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon1.Visible = true;
                this.Hide();
            }
            else
            {
                this.notifyIcon1.Visible = false;
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Global.GUI_Refresh_Flag = false;
            Global.S1F3_Refresh_Flag = false;
            timer1.Enabled = false;
            timer2.Enabled = false;
            Host_DisConnect();
            Equipment_DisConnect();
            Communicate.ConnectedResult -= _tcpcomm_ConnectedResult;
            Communicate.MessageReceived -= _tcpcomm_MessageReceived;
            CommunicateSV.ConnectedResult -= _tcpcomm_ConnectedResultSV;
            CommunicateSV.MessageReceived -= _tcpcomm_MessageReceivedSV;
            Environment.Exit(0);
        }
        private void InitialDialog()
        {
            Global.WorkDir = Application.StartupPath.Substring(0, Application.StartupPath.LastIndexOf("\\") + 1);

            Global.SystemParameterIni.SetFilePath(Global.WorkDir + "Parameter" + "/" + "ParameterIni.Ini");

            Global.EQ_Name = Global.SystemParameterIni.GetString("Cofig", "EQ_Name", "0");
            Global.EQ_SIF3_Time = Global.SystemParameterIni.GetString("Cofig", "EQ_SIF3_Time", "200");

            CNCVision.CtrDisplay.ShowText(textBox3, Global.EQ_SIF3_Time);
            if (Global.EQ_Name == "0")
            {
                com_EQ_Name.SelectedIndex = 0;
                Global.EQ_Name_Status = false;//XKL
                Global.EQ_Name = "新凯来";
                Global.SystemParameterIni.WriteString("Cofig", "EQ_Name", "新凯来");
            }
            else if (Global.EQ_Name == "新凯来")
            {
                com_EQ_Name.SelectedIndex = 0;
                Global.EQ_Name_Status = false;
            }
            else
            {
                com_EQ_Name.SelectedIndex = 1;
                Global.EQ_Name_Status = true;
            }

            read_ALID_Table();
            read_SVID_Table();
            read_CEID_Table();

            //read_EQ_SVID_Table();
            SECS.EQ.Read_CEID(); //read_EQ_CEID_Table();  //读取机台CEID文件
            read_EQ_SVID();
            //read_RGATCP_Table();   //读取tcp通讯格式
            int LengthSECS_RGA = Global.SECS_RGA.Length;
            for (int i = 0; i < LengthSECS_RGA; i++)
                Global.SECS_RGA[i] = "";
            Global.SECS_RGA[0] = "RND";

            //SECS.Host.loadReport();
        }
        #region "初始化更新資料"
        private void read_ALID_Table()
        {
            SECS.Host.Read_Alarn();
            Global.Read_Alarn();

        }
        private void read_SVID_Table()
        {
            Global.Read_SVID("Parameter/RGA_SVID_CH1.csv", "CH1");
            Global.Read_SVID("Parameter/RGA_SVID_CH2.csv", "CH2");
            Global.Read_SVID("Parameter/RGA_SVID_CH3.csv", "CH3");
            Global.Read_SVID("Parameter/RGA_SVID_CH4.csv", "CH4");
            Global.Read_SVID("Parameter/RGA_SVID_CH5.csv", "CH5");
            Global.Read_SVID("Parameter/RGA_SVID_CH6.csv", "CH6");
            Global.Read_SVID("Parameter/RGA_SVID_CHC.csv", "CHC");
            Global.Read_SVID("Parameter/RGA_SVID_CHD.csv", "CHD");
            Global.Read_SVID("Parameter/RGA_SVID_CHE.csv", "CHE");
            Global.Read_SVID("Parameter/RGA_SVID_CHF.csv", "CHF");
            Global.Read_SVID("Parameter/RGA_SVID_TM1.csv", "TM1");
            Global.Read_SVID("Parameter/RGA_SVID_TM2.csv", "TM2");
        }

        private void read_CEID_Table()
        {
            Global.Read_CEID("Parameter/RGA_CEID.csv");
        }

        //private void read_RGATCP_Table()
        //{
        //    Global.Read_RGA_TCP("Parameter/RGA_TCP.csv");
        //}

        //private void read_EQ_SVID_Table()
        //{
        //SECS.EQ.Read_SVID();
        //SECS.EQ.Read_RGA_TCP();
        //}

        private void read_EQ_SVID()
        {

            string path = Global.WorkDir + "SVID\\SVID.CSV";
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            if (File.Exists(path))
            {
                string meg = "";
                //读取报告的SVID
                string Line;
                var Send_SV = new List<string> { };
                Encoding encoder = Encoding.UTF8;
                using (StreamReader reader = new StreamReader(path, encoder))
                {
                    while ((Line = reader.ReadLine()) != null)
                    {
                        if (Line != "")
                            Send_SV.Add(Line);
                    }
                }
                //Global.SECS_RGA_SV = new string[Send_SV.Count];
                Global.SECS_RGA_SV = Send_SV;
            }
        }
        //private void read_EQ_CEID_Table()
        //{
        //    SECS.EQ.Read_CEID();
        //}
        #endregion
        #region "SECS GEM連線"
        private void Host_Connect()
        {
            SECS.Host.ChangeKSecsMessageEvent += HostchangeKSecsMessageEvent;
            SECS.Host.TransactionLog += HostTransactionLog;
            SECS.Host.Host_Connect();
        }
        public void Equipment_Connect()
        {
            SECS.EQ.ChangeKSecsMessageEvent += EQchangeKSecsMessageEvent;
            SECS.EQ.EQ_TransactionLog += EQTransactionLog;
            SECS.EQ.Host_Connect();
        }

        public void TCP_Connect()
        {
            try
            {
                switch ("Server")
                {
                    case "Server":
                        {
                            Communicate = new CNCVision.Communicate(CNCVision.Communicate.NetBehavior.Server, "127.0.0.1", 6000);
                            break;
                        }
                    case "Client":
                        {
                            //  Communicate = new CNCVision.Communicate(CNCVision.Communicate.NetBehavior.Client, "192.168.0.01",8001);
                            break;
                        }
                }

                Communicate.ConnectedResult += _tcpcomm_ConnectedResult;
                Communicate.MessageReceived += _tcpcomm_MessageReceived;
                Global.DisplayInfo("SECS TCP Link。");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString() + "网络连接失败！"); 
                Global.DisplayInfo("SECS TCP Link Fail!。");
            }
        }

        public void TCP_ConnectSV()
        {
            try
            {
                switch ("Server")
                {
                    case "Server":
                        {
                            CommunicateSV = new CNCVision.Communicate(CNCVision.Communicate.NetBehavior.Server, "127.0.0.1", 6001);
                            break;
                        }
                    case "Client":
                        {
                            //  Communicate = new CNCVision.Communicate(CNCVision.Communicate.NetBehavior.Client, "192.168.0.01",8001);
                            break;
                        }
                }

                CommunicateSV.ConnectedResult += _tcpcomm_ConnectedResultSV;
                CommunicateSV.MessageReceived += _tcpcomm_MessageReceivedSV;
                Global.DisplayInfo("SECS TCP Link SV。");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString() + "网络连接失败！"); 
                Global.DisplayInfo("SECS TCP Link Fail!。");
            }
        }
        #endregion
        #region "SECS GEM離線"
        private void Host_DisConnect()
        {
            SECS.Host.ChangeKSecsMessageEvent -= HostchangeKSecsMessageEvent;
            SECS.Host.TransactionLog -= HostTransactionLog;
            SECS.Host.Host_Close();
        }
        public void Equipment_DisConnect()
        {
            SECS.EQ.ChangeKSecsMessageEvent -= EQchangeKSecsMessageEvent;
            SECS.EQ.EQ_TransactionLog -= EQTransactionLog;
            SECS.EQ.Host_Close();
        }
        #endregion
        #region "更新SECS GEM資料到UI"
        private void UpDataGEM_ToUI()
        {
            txt_HOST_T1.Text = SECS.Host.Host_Control.T1.ToString();
            txt_HOST_T2.Text = SECS.Host.Host_Control.T2.ToString();
            txt_HOST_T3.Text = SECS.Host.Host_Control.T3.ToString();
            txt_HOST_T4.Text = SECS.Host.Host_Control.T4.ToString();
            txt_HOST_T5.Text = SECS.Host.Host_Control.T5.ToString();
            txt_HOST_T6.Text = SECS.Host.Host_Control.T6.ToString();
            txt_HOST_T7.Text = SECS.Host.Host_Control.T7.ToString();
            txt_HOST_T8.Text = SECS.Host.Host_Control.T8.ToString();
            txt_HOST_DeviceID.Text = SECS.Host.Host_Control.DeviceId.ToString();
            if (SECS.Host.Host_Control.LinkType == KSecsWrapperDotNet.KSecsLinkType.slSECS)
            { Combo_HOST_LinkType.SelectedIndex = 0; }
            else
            { Combo_HOST_LinkType.SelectedIndex = 1; }
            txt_HOST_IP.Text = SECS.Host.Host_Control.IP;
            txt_HOST_Port.Text = SECS.Host.Host_Control.Port.ToString();
            if (SECS.Host.Host_Control.Entity == KSecsWrapperDotNet.KHsmsEntity.etActive)
            { Combo_HOST_Entity.SelectedIndex = 0; }
            else
            { Combo_HOST_Entity.SelectedIndex = 1; }
            Combo_HOST_Line.SelectedItem = SECS.Host.Host_Control.Line;
            Combo_HOST_BaudRate.SelectedItem = SECS.Host.Host_Control.BaudRate.ToString();
            txt_HOST_Retry.Text = SECS.Host.Host_Control.Retry.ToString();

            if (SECS.Host.Host_Control.LogToFile == true)
            {
                chHS_LogEnbled.Checked = true;
                ch_HSQueueEnable.Checked = true;
            }
            else
            {
                chHS_LogEnbled.Checked = false;
                ch_HSQueueEnable.Checked = false;
            }
            txt_HSlog_path.Text = SECS.Host.Host_Control.TxLogName;
            num_HSMaxQueue.Value = SECS.Host.Host_Control.MaxQueueMessages;

            txt_EQ_T1.Text = SECS.EQ.Equipment_Control.T1.ToString();
            txt_EQ_T2.Text = SECS.EQ.Equipment_Control.T2.ToString();
            txt_EQ_T3.Text = SECS.EQ.Equipment_Control.T3.ToString();
            txt_EQ_T4.Text = SECS.EQ.Equipment_Control.T4.ToString();
            txt_EQ_T5.Text = SECS.EQ.Equipment_Control.T5.ToString();
            txt_EQ_T6.Text = SECS.EQ.Equipment_Control.T6.ToString();
            txt_EQ_T7.Text = SECS.EQ.Equipment_Control.T7.ToString();
            txt_EQ_T8.Text = SECS.EQ.Equipment_Control.T8.ToString();
            chEQ_LogEnbled.Checked = SECS.EQ.Equipment_Control.LogToFile;
            txt_EQlog_path.Text = SECS.EQ.Equipment_Control.TxLogName;
            txt_EQ_DeviceID.Text = SECS.EQ.Equipment_Control.DeviceId.ToString();
            if (SECS.EQ.Equipment_Control.LinkType == KSecsWrapperDotNet.KSecsLinkType.slSECS)
            { Combo_EQ_LinkType.SelectedIndex = 0; }
            else
            { Combo_EQ_LinkType.SelectedIndex = 1; }
            txt_EQ_IP.Text = SECS.EQ.Equipment_Control.IP;
            txt_EQ_Port.Text = SECS.EQ.Equipment_Control.Port.ToString();
            if (SECS.EQ.Equipment_Control.Entity == KSecsWrapperDotNet.KHsmsEntity.etActive)
            { Combo_EQ_Entity.SelectedIndex = 0; }
            else
            { Combo_EQ_Entity.SelectedIndex = 1; }
            Combo_EQ_Line.SelectedItem = SECS.EQ.Equipment_Control.Line;
            Combo_EQ_BaudRate.SelectedItem = SECS.EQ.Equipment_Control.BaudRate.ToString();
            txt_EQ_Retry.Text = SECS.EQ.Equipment_Control.Retry.ToString();

            if (SECS.EQ.Equipment_Control.LogToFile == true)
            {
                chEQ_LogEnbled.Checked = true;
                ch_EQQueueEnable.Checked = true;
            }
            else
            {
                chEQ_LogEnbled.Checked = false;
                ch_EQQueueEnable.Checked = false;
            }
            txt_EQlog_path.Text = SECS.EQ.Equipment_Control.TxLogName;
            num_EQMaxQueue.Value = SECS.EQ.Equipment_Control.MaxQueueMessages;
        }
        #endregion

        #region "控制线程"

        //獲取cpu的id號
        static ulong SetCpuID(int id)
        {
            ulong cpuid = 0;
            if (id < 0 || id >= System.Environment.ProcessorCount)
            {
                id = 0;
            }
            cpuid |= 1UL << id;

            return cpuid;
        }
        //volatile bool ThreadLocker;
        //private object ObjRefreshThread = new object();//RefreshThread线程互锁
        private void RunGUI_RefreshThread()
        {
            while (Global.GUI_Refresh_Flag == true)
            {
                try
                {
                    if (Global.Queue_Tcp.Count > 0)
                    {
                        if (Global.TCP_IP_Status == true)
                        {
                            string tcp = "";
                            lock (Global.ObjRefreshThread)
                            {
                                tcp = Global.Queue_Tcp.Dequeue();
                            }
                            Communicate.SendMessage(tcp);
                            Global.DisplayInfo("TCP Send:" + tcp);
                        }
                    }
                }
                catch (Exception ex)
                { }
                System.Threading.Thread.Sleep(1);
            }
        }

        private void RunS1F3_RefreshThread()
        {
            while (Global.S1F3_Refresh_Flag == true)
            {
                try
                {
                    if (SECS.EQ.Equipment_Control.IsConnected == true)
                    {
                        if (Global.SECS_RGA_SV.Count > 0)
                            SECS.EQ.S1F3_SendCommand(Global.SECS_RGA_SV.ToArray());
                    }
                }
                catch (Exception ex)
                { }
                System.Threading.Thread.Sleep(Convert.ToInt32(Global.EQ_SIF3_Time));
            }
        }
        #endregion


        

        
        //public int SIF1 = 0;
        //public int SIF3 = 0;

        //public int SIF1_Error_Count = 0;
        //public int SIF3_Count = 0;
        private bool HostS1F1 = false;
        private int countHostS1F1 = 0;
        private bool HostS1F13 = false;
        private int countHostS1F13 = 0;
        private bool IsConnectedHost()
        {
            Global.SECS_RGA[2] = Global.SECS_RGA[24] = "0";
            if (SECS.Host.Host_Control.IsConnected == false)
            {
                HostS1F1 = HostS1F13 = false;
                countHostS1F1 = countHostS1F13 = 0;
                return false;
            }

            if (HostS1F1 == false)
            {
                SECS.Host.S1F1_SendCommand();
                HostS1F1 = true;
                return false;
            }
            if ((++countHostS1F1 > 3) && (SECS.Host.okHostS1F1 == false))
            {
                countHostS1F1 = 0;
                HostS1F1 = false;
                return false;
            }
            Global.SECS_RGA[2] = "1";
            if (HostS1F13 == false)
            {
                SECS.Host.S1F13_SendCommand();
                HostS1F13 = true;
                return false;
            }
            if ((++countHostS1F13 > 3) && (SECS.Host.okHostS1F13 == false))
            {
                countHostS1F13 = 0;
                HostS1F13 = false;
                return false;
            }
            Global.SECS_RGA[24] = "1";
            return true;
        }

        private bool EQ_S1F1 = false;
        private int countEQ_S1F1 = 0;
        private bool EQ_S1F13 = false;
        private int countEQ_S1F13 = 0;
        private bool IsConnectedEQ()
        {
            Global.SECS_RGA[1] = Global.SECS_RGA[23] = "0";
            if (SECS.EQ.Equipment_Control.IsConnected == false)
            {
                EQ_S1F13 = EQ_S1F1 = false;
                countEQ_S1F13 = countEQ_S1F1 = 0;
                return false;
            }

            if (EQ_S1F1 == false)
            {
                SECS.EQ.S1F1_SendCommand();
                EQ_S1F1 = true;
                return false;
            }
            if ((++countEQ_S1F1 > 3) && (SECS.EQ.okEQ_S1F1 == false))
            {
                countEQ_S1F1 = 0;
                EQ_S1F1 = false;
                return false;
            }
            //Global.SECS_RGA[15] = "1";
            Global.SECS_RGA[1] = "1";

            if (EQ_S1F13 == false)
            {
                SECS.EQ.S1F13_SendCommand();
                EQ_S1F13 = true;
                return false;
            }
            if ((++countEQ_S1F13 > 3) && (SECS.EQ.okEQ_S1F13 == false))
            {
                countEQ_S1F13 = 0;
                EQ_S1F13 = false;
                return false;
            }
            Global.SECS_RGA[23] = "1";
            return true;
        }

        public bool eventReport = false;
        public int count = 0;
        private bool ifClearReport = false;
        private bool ifDefineReport = false;
        private bool ifLinkReport = false;
        private bool ifEnableReport = false;
        private int countEQ_S2F33 = 0;
        private int indexEQ_S2F33 = 0;
        private int countEQ_S2F35 = 0;
        private int indexEQ_S2F35 = 0;
        private int countEQ_S2F37 = 0;
        private int indexEQ_S2F37 = 0;
        private bool isReportFinish()
        {
            if (eventReport)
            {
                countEQ_S2F33 = indexEQ_S2F33 = countEQ_S2F35 = indexEQ_S2F35 =  countEQ_S2F37 = indexEQ_S2F37 = 0;
                return true;
            }
                
            //ifClearReport = true;
            if (!ifClearReport)
            {
                Global.SECS_RGA[19] = "1";
                //if (Global.EQ_Name_Status == false)
                //{//XKL不清除？
                //    Global.SECS_RGA[19] = "0";
                //    ifClearReport = true;
                //    for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                //    {
                //        SECS.EQ.gRPTID_EQ[i].isDefind = false;
                //    }
                //    return true;
                //}
                if (!SECS.EQ.okEQ_S2F33)
                {
                    if (countEQ_S2F33 > 3) { 
                        countEQ_S2F33 = 0;
                        SECS.EQ.S2F33_SendCommandNEW(SECS.EQ.gRPTID_EQ[indexEQ_S2F33], false);
                    }
                    ++countEQ_S2F33;
                    return false;
                }
                   
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    if (SECS.EQ.gRPTID_EQ[i].isDefind == false)
                    {
                        indexEQ_S2F33 = i;
                        SECS.EQ.S2F33_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], false);
                        SECS.EQ.gRPTID_EQ[i].isDefind = true;
                        return false;
                    }
                }
                Global.SECS_RGA[19] = "0";
                ifClearReport = true;
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    SECS.EQ.gRPTID_EQ[i].isDefind = false;
                }
            }

            if (!ifDefineReport)
            {
                Global.SECS_RGA[19] = "1";
                //if (!SECS.EQ.okEQ_S2F33)
                //    return false;
                if (!SECS.EQ.okEQ_S2F33)
                {
                    if (countEQ_S2F33 > 3)
                    {
                        countEQ_S2F33 = 0;
                        SECS.EQ.S2F33_SendCommandNEW(SECS.EQ.gRPTID_EQ[indexEQ_S2F33], true);
                    }
                    ++countEQ_S2F33;
                    return false;
                }
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    if (SECS.EQ.gRPTID_EQ[i].isDefind == false)
                    {
                        indexEQ_S2F33 = i;
                        SECS.EQ.S2F33_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], true);
                        SECS.EQ.gRPTID_EQ[i].isDefind = true;
                        return false;
                    }
                }
                Global.SECS_RGA[19] = "0";
                ifDefineReport = true;
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    SECS.EQ.gRPTID_EQ[i].isDefind = false;
                }
            }

            if (!ifLinkReport)
            {
                Global.SECS_RGA[20] = "1";
                //if (!SECS.EQ.okEQ_S2F35)
                //        return false;
                if (!SECS.EQ.okEQ_S2F35)
                {
                    if (countEQ_S2F35 > 3)
                    {
                        countEQ_S2F35 = 0;
                        SECS.EQ.S2F35_SendCommandNEW(SECS.EQ.gRPTID_EQ[indexEQ_S2F35], true);
                    }
                    ++countEQ_S2F35;
                    return false;
                }
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    if (SECS.EQ.gRPTID_EQ[i].isDefind == false)
                    {
                        indexEQ_S2F35 = i;
                        SECS.EQ.S2F35_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], true);
                        SECS.EQ.gRPTID_EQ[i].isDefind = true;
                        return false;
                    }
                }
                Global.SECS_RGA[20] = "0";
                ifLinkReport = true;
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    SECS.EQ.gRPTID_EQ[i].isDefind = false;
                }
            }

            if (!ifEnableReport)
            {
                Global.SECS_RGA[21] = "1";
                if (!SECS.EQ.okEQ_S2F37)
                {
                    if (countEQ_S2F37 > 3)
                    {
                        countEQ_S2F37 = 0;
                        SECS.EQ.S2F37_SendCommandNEW(SECS.EQ.gRPTID_EQ[indexEQ_S2F37], true);
                    }
                    ++countEQ_S2F37;
                    return false;
                }
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    //if (!SECS.EQ.okEQ_S2F37)
                    //    return false;
                    if (SECS.EQ.gRPTID_EQ[i].isDefind == false)
                    {
                        indexEQ_S2F37 = i;
                        SECS.EQ.S2F37_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], true);
                        SECS.EQ.gRPTID_EQ[i].isDefind = true;
                        return false;
                    }
                }
                Global.SECS_RGA[21] = "0";
                ifEnableReport = true;
                for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
                {
                    SECS.EQ.gRPTID_EQ[i].isDefind = false;
                }
            }
            eventReport = true;
            return true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Global.TCP_IP_Status == true /*&& Global.TCP_IPSV_Status == true*/)
            {
                CNCVision.CtrDisplay.ShowText(statusStrip1, "Online", Color.Lime, 5);
            }
            else
            {
                CNCVision.CtrDisplay.ShowText(statusStrip1, "Offline", Color.Red, 5);
            }

            if (IsConnectedHost())
            {
                //Global.DisplayInfo("SECS FDC Link 。");
                CNCVision.CtrDisplay.ShowText(statusStrip1, "Online", Color.Lime, 1);
            }
            else
            {
               // Global.DisplayInfo("SECS FDC Link Fail!。");
                CNCVision.CtrDisplay.ShowText(statusStrip1, "Offline", Color.Red, 1);
            }

            if (!IsConnectedEQ())
            {
                //Global.DisplayInfo("SECS EQ Link Fail!。");
                CNCVision.CtrDisplay.ShowText(statusStrip1, "Offline", Color.Red, 3);
                return;
            }
            //Global.DisplayInfo("SECS EQ Link 。");
            CNCVision.CtrDisplay.ShowText(statusStrip1, "Online", Color.Lime, 3);

            if (!isReportFinish())
            {
                //Global.DisplayInfo("Define report。");
                CNCVision.CtrDisplay.ShowText(statusStrip1, "Init", Color.Red, 7);
                return;
            }
            CNCVision.CtrDisplay.ShowText(statusStrip1, "Init", Color.Lime, 7);


                //if (!Global.EQ_Name_Status)
                //    break;
                //if (!SECS.EQ.Equipment_Control.IsConnected)
                //    break;
                //if (SECS.EQ.gS1F1)
                //{
                //    Global.SECS_RGA[15] = "1";
                //    SECS.EQ.gS1F1 = false;
                //    SECS.EQ.S1F1_SendCommand();
                //}
                //else
                //{
                //    if (++SIF1_Error_Count > 3)
                //    {
                //        SIF1_Error_Count = 0;
                //        Global.SECS_RGA[15] = "0";
                //        break;
                //    }
                //    SECS.EQ.S1F1_SendCommand();
                //}

                Form1.Form.send_TCPmeg_HC();
                if (Global.RecipeName_Flag)
                {
                    Global.RecipeName_Flag = false;
                    var messageList = new List<string> { };
                    messageList.Add("RNR");
                    //string Meg = "RNR,";
                    for (int i = 0; i < SECS.EQ.gS7F20_Message.Count; i++)
                        messageList.Add(SECS.EQ.gS7F20_Message[i].ToString());
                lock (Global.ObjRefreshThread)
                {
                    Global.Queue_Tcp.Enqueue(String.Join(",", messageList) + "\r\n");
                }
                //Global.Queue_Tcp.Enqueue(String.Join(",", messageList) + "\r\n");
                }

                if (Global.Recipe)
                {
                    Global.Recipe = false;
                    SECS.EQ.S7F19_SendCommand();
                }
            int test = 0;
            if (test == 5)
            {
                SECS.EQ.S5F1_SendAlarm(0x80, Convert.ToInt32("10128"), "RGA_TEST");
                //SECS.EQ.S2F41_SendCommand("TRIGGERALARM", "StationId", "1");//STOP TRIGGERALARM
            }
            if (test==1)//REC_START
            {
                if (Global.TCP_IP_Status == true)
                {
                    var messageList = new List<String>(Global.SECS_RGA);
                    /*5000001:EventName,5000002:Clock,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,5114027:ChamberType,5120001:ChamberName*/
                    string message = String.Join(",", messageList) + "," + "2330016"/*CEID*/ + ","
                        + "2330016,5000001:EventName,5000002:Clock,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,5114027:ChamberType,5120001:ChamberName"/*RPTID,SV0,SV1...*/ + "\r\n";
                    lock (Global.ObjRefreshThread)
                    {
                        Global.Queue_Tcp.Enqueue(message);
                    }
                }
            }
            if (test==2)//REC_END
            {
                if (Global.TCP_IP_Status == true)
                {
                    var messageList = new List<String>(Global.SECS_RGA);
                    /*5000001:EventName,5000002:Clock,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,5114027:ChamberType,5114029:WaferCount,5114030:RecipeProcessTime,5114033:RecipeProcessTimeInSeconds,5120001:ChamberName*/
                    string message = String.Join(",", messageList) + "," + "2330017"/*CEID*/ + ","
                        + "2330017,5000001:EventName,5000002:Clock,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,5114027:ChamberType,5114029:WaferCount,5114030:RecipeProcessTime,5114033:RecipeProcessTimeInSeconds,5120001:ChamberName"/*RPTID,SV0,SV1...*/ + "\r\n";
                    lock (Global.ObjRefreshThread)
                    {
                        Global.Queue_Tcp.Enqueue(message);
                    }
                }
            }
            if (test==3)//REC_STEP_3
            {
                if (Global.TCP_IP_Status == true)
                {
                    var messageList = new List<String>(Global.SECS_RGA);
                    /*5000001:EventName,5000002:Clock,5110003:PPID,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,5114017:RecipeStepIndex,5114018:RecipeStepName,5114027:ChamberType,5120001:ChamberName*/
                    string message = String.Join(",", messageList) + "," + "2330018"/*CEID*/ + ","
                        + "2330018,5000001:EventName,5000002:Clock,5110003:PPID,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,3,5114018:RecipeStepName,5114027:ChamberType,5120001:ChamberName"/*RPTID,SV0,SV1...*/ + "\r\n";
                    lock (Global.ObjRefreshThread)
                    {
                        Global.Queue_Tcp.Enqueue(message);
                    }
                }
            }
            if (test==4)//REC_STEP_4
            {
                if (Global.TCP_IP_Status == true)
                {
                    var messageList = new List<String>(Global.SECS_RGA);
                    /*5000001:EventName,5000002:Clock,5110003:PPID,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,5114017:RecipeStepIndex,5114018:RecipeStepName,5114027:ChamberType,5120001:ChamberName*/
                    string message = String.Join(",", messageList) + "," + "2330018"/*CEID*/ + ","
                        + "2330018,5000001:EventName,5000002:Clock,5110003:PPID,5110004:WaferId,5110006:RecID,5112001:CarrierID,5114003:LotId,5114016:SlotId,4,5114018:RecipeStepName,5114027:ChamberType,5120001:ChamberName"/*RPTID,SV0,SV1...*/ + "\r\n";
                    lock (Global.ObjRefreshThread)
                    {
                        Global.Queue_Tcp.Enqueue(message);
                    }
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //if (Global.EQ_Name_Status == false)
            //{
            //    if (SECS.EQ.Equipment_Control.IsConnected == true && Global.EQ_Name_Status==false)
            //    {
            //        SIF3_Count++;
            //        switch (SIF3)
            //        {

            //        }


            //    }
            //}
            //if (Global.TCP_IP_Status == true /*&& Global.TCP_IPSV_Status == true*/)
            //{
            //    CNCVision.CtrDisplay.ShowText(statusStrip1, "Online", Color.Lime, 5);
            //}
            //else
            //{
            //    CNCVision.CtrDisplay.ShowText(statusStrip1, "Offline", Color.Red, 5);
            //}
            //if (Global.TCP_IP_Status == true && Global.EQ_Name_Status == true && SECS.EQ.CEID_Status == false)
            //{
            //    Form1.Form.send_TCPmeg_HC();
            //}

            Form1.Form.send_TCPmeg_HC();
            if (Global.RecipeName_Flag == true)
            {
                Global.RecipeName_Flag = false;
                var messageList = new List<string> { };
                messageList.Add("RNR");
                //string Meg = "RNR,";
                for (int i = 0; i < SECS.EQ.gS7F20_Message.Count; i++)
                    messageList.Add(SECS.EQ.gS7F20_Message[i].ToString());
                lock (Global.ObjRefreshThread)
                {
                    Global.Queue_Tcp.Enqueue(String.Join(",", messageList) + "\r\n");
                }
                //Communicate.SendMessage(Meg);
            }

            if (Global.Recipe == true)
            {
                Global.Recipe = false;
                SECS.EQ.S7F19_SendCommand();
            }
        }
        private void brnHOST_GemSave_Click(object sender, EventArgs e)
        {
            Host_DisConnect();
            SECS.Host.Host_Control.T1 = Convert.ToUInt32(txt_HOST_T1.Text);
            SECS.Host.Host_Control.T2 = Convert.ToUInt32(txt_HOST_T2.Text);
            SECS.Host.Host_Control.T3 = Convert.ToUInt32(txt_HOST_T3.Text);
            SECS.Host.Host_Control.T4 = Convert.ToUInt32(txt_HOST_T4.Text);
            SECS.Host.Host_Control.T5 = Convert.ToUInt32(txt_HOST_T5.Text);
            SECS.Host.Host_Control.T6 = Convert.ToUInt32(txt_HOST_T6.Text);
            SECS.Host.Host_Control.T7 = Convert.ToUInt32(txt_HOST_T7.Text);
            SECS.Host.Host_Control.T8 = Convert.ToUInt32(txt_HOST_T8.Text);
            SECS.Host.Host_Control.DeviceId = Convert.ToInt32(txt_HOST_DeviceID.Text);
            if (Combo_HOST_LinkType.SelectedIndex == 0)
            { SECS.Host.Host_Control.LinkType = KSecsWrapperDotNet.KSecsLinkType.slSECS; }
            else
            { SECS.Host.Host_Control.LinkType = KSecsWrapperDotNet.KSecsLinkType.slHSMS; }
            SECS.Host.Host_Control.IP = txt_HOST_IP.Text;
            SECS.Host.Host_Control.Port = Convert.ToInt32(txt_HOST_Port.Text);
            if (Combo_HOST_Entity.SelectedIndex == 0)
            { SECS.Host.Host_Control.Entity = KSecsWrapperDotNet.KHsmsEntity.etActive; }
            else
            { SECS.Host.Host_Control.Entity = KSecsWrapperDotNet.KHsmsEntity.etPassive; }
            SECS.Host.Host_Control.Line = Combo_HOST_Line.SelectedItem.ToString();
            SECS.Host.Host_Control.BaudRate = Convert.ToInt32(Combo_HOST_BaudRate.SelectedItem);
            SECS.Host.Host_Control.Retry = Convert.ToInt32(txt_HOST_Retry.Text);
            if (chHS_LogEnbled.Checked == true)
            {
                SECS.Host.Host_Control.LogToFile = true;
                SECS.Host.Host_Control.LogTransaction = true;
            }
            else
            {
                SECS.Host.Host_Control.LogToFile = false;
                SECS.Host.Host_Control.LogTransaction = false;
            }
            SECS.Host.Host_Control.TxLogName = txt_HSlog_path.Text;
            SECS.Host.Host_Control.MessageLogName = txt_HSlog_path.Text;
            SECS.Host.Host_Control.MaxQueueMessages = Convert.ToInt32(num_HSMaxQueue.Value);
            SECS.Host.Host_Control.SaveToIni(Global.WorkDir + "Parameter/Host_Gem.ini", "SECS");
            Host_Connect();
        }
        private void brnEQ_GemSave_Click(object sender, EventArgs e)
        {
            Equipment_DisConnect();

            SECS.EQ.Equipment_Control.T1 = Convert.ToUInt32(txt_EQ_T1.Text);
            SECS.EQ.Equipment_Control.T2 = Convert.ToUInt32(txt_EQ_T2.Text);
            SECS.EQ.Equipment_Control.T3 = Convert.ToUInt32(txt_EQ_T3.Text);
            SECS.EQ.Equipment_Control.T4 = Convert.ToUInt32(txt_EQ_T4.Text);
            SECS.EQ.Equipment_Control.T5 = Convert.ToUInt32(txt_EQ_T5.Text);
            SECS.EQ.Equipment_Control.T6 = Convert.ToUInt32(txt_EQ_T6.Text);
            SECS.EQ.Equipment_Control.T7 = Convert.ToUInt32(txt_EQ_T7.Text);
            SECS.EQ.Equipment_Control.T8 = Convert.ToUInt32(txt_EQ_T8.Text);
            SECS.EQ.Equipment_Control.DeviceId = Convert.ToInt32(txt_EQ_DeviceID.Text);
            if (Combo_EQ_LinkType.SelectedIndex == 0)
            { SECS.EQ.Equipment_Control.LinkType = KSecsWrapperDotNet.KSecsLinkType.slSECS; }
            else
            { SECS.EQ.Equipment_Control.LinkType = KSecsWrapperDotNet.KSecsLinkType.slHSMS; }
            SECS.EQ.Equipment_Control.IP = txt_EQ_IP.Text;
            SECS.EQ.Equipment_Control.Port = Convert.ToInt32(txt_EQ_Port.Text);
            SECS.EQ.Equipment_Control.Line = Combo_EQ_Line.SelectedItem.ToString();
            SECS.EQ.Equipment_Control.BaudRate = Convert.ToInt32(Combo_EQ_BaudRate.SelectedItem);
            SECS.EQ.Equipment_Control.Retry = Convert.ToInt32(txt_EQ_Retry.Text);
            if (chEQ_LogEnbled.Checked == true)
            {
                SECS.EQ.Equipment_Control.LogToFile = true;
                SECS.EQ.Equipment_Control.LogTransaction = true;
            }
            else
            {
                SECS.EQ.Equipment_Control.LogToFile = false;
                SECS.EQ.Equipment_Control.LogTransaction = false;
            }
            SECS.EQ.Equipment_Control.TxLogName = txt_EQlog_path.Text;
            SECS.EQ.Equipment_Control.MessageLogName = txt_EQlog_path.Text;
            if (Combo_EQ_Entity.SelectedIndex == 0)
            {
                SECS.EQ.Equipment_Control.Entity = KSecsWrapperDotNet.KHsmsEntity.etActive;
            }
            else
            {
                SECS.EQ.Equipment_Control.Entity = KSecsWrapperDotNet.KHsmsEntity.etPassive;
            }
            SECS.EQ.Equipment_Control.MaxQueueMessages = Convert.ToInt32(num_EQMaxQueue.Value);
            SECS.EQ.Equipment_Control.SaveToIni(Global.WorkDir + "Parameter/EQ_Gem.ini", "SECS");
            Equipment_Connect();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //定义报告
            SECS.EQ.S2F33_SendCommand(Global.DATAID, Global.RecipeStartRPTID, Global.RecipeStartLinkedItemID, true);
            //链接报告
            SECS.EQ.S2F35_SendCommand(Global.DATAID, Global.RecipeStartEventID, Global.RecipeStartRPTID, true);
            //启用报告
            SECS.EQ.S2F37_SendCommand(true, Global.RecipeStartEventID, true);
        }


        #region    Secs 回传事件
        /// <summary>
        /// 发送Host后，host响应事件
        /// </summary>
        /// <param name="kSecsMessage"></param>
        public void HostchangeKSecsMessageEvent(KSecsWrapperDotNet.KSecsMessage kSecsMessage)
        {
            try
            {
                //txtSendHost.Invoke(new Action(() => { txtSendHost.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + kSecsMessage.Text + "\r\n"); }));              
            }
            catch (Exception ex)
            { }

        }

        private void HostTransactionLog(string value)
        {
            try
            {
                //txtReciveHost.Invoke(new Action(() => { txtReciveHost.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":"+value + "\r\n"); }));
            }
            catch (Exception ex)
            { }
        }


        public int num = 0;
        /// <summary>
        /// 发送EQ后，EQ响应事件
        /// </summary>
        /// <param name="kSecsMessage"></param>
        public void EQchangeKSecsMessageEvent(KSecsWrapperDotNet.KSecsMessage kSecsMessage)
        {
            try
            {
                num++;
                // txt_Send.Invoke(new Action(() => { txt_Send.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + kSecsMessage.Text + "\r\n"); }));
                Console.WriteLine(num.ToString());
            }
            catch (Exception ex)
            { }
        }

        private void EQTransactionLog(string value)
        {
            try
            {
                if (value == "Warning! T1 Timeout!")
                { Global.SECS_RGA[3] = "1"; }
                else if (value == "Warning! T2 Timeout!")
                { Global.SECS_RGA[4] = "1"; }
                else if (value == "Warning! T3 Timeout!")
                { Global.SECS_RGA[5] = "1"; }
                else if (value == "Warning! T4 Timeout!")
                { Global.SECS_RGA[6] = "1"; }
                else if (value == "Warning! T5 Timeout!")
                { Global.SECS_RGA[7] = "1"; }
                else if (value == "Warning! T6 Timeout!")
                { Global.SECS_RGA[8] = "1"; }
                else if (value == "Warning! T7 Timeout!")
                { Global.SECS_RGA[9] = "1"; }
                else if (value == "Warning! T8 Timeout!")
                { Global.SECS_RGA[10] = "1"; }
                //txt_TransactionLog.Invoke(new Action(() => { txt_TransactionLog.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + value + "\r\n"); })); 
            }
            catch (Exception ex)
            { }
        }


        void _tcpcomm_ConnectedResult(object sender, CNCVision.Communicate.ResultEventArgs e)
        {
            if (e.conResult)
            {
                Global.TCP_IP_Status = true;
            }
            else
            {
                Global.TCP_IP_Status = false;
            }
        }

        void _tcpcomm_ConnectedResultSV(object sender, CNCVision.Communicate.ResultEventArgs e)
        {
            if (e.conResult)
            {
                Global.TCP_IPSV_Status = true;
            }
            else
            {
                Global.TCP_IPSV_Status = false;
            }
        }

        void _tcpcomm_MessageReceived(object sender, CNCVision.Communicate.MessageEventArgs e)
        {
            try
            {
                //txt_TCP_Riceve.Clear();
                //txt_TCP_Riceve.Invoke(new Action(() => { txt_TCP_Riceve.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss:ffff") + ":" + e.Message + "\r\n"); }));
                Global.TCP_Messagelist = e.Message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string line in Global.TCP_Messagelist)
                {
                    Global.TCP_Message = line.Split(',');
                    int lengthMSG = Global.TCP_Message.Length;
                    #region DataGridView 显示速度测试方法1  约： 50ms
                    if (Global.TCP_Message[0].ToString() == "RND")
                    {
                        String strCMD = Global.TCP_Message[1].ToString();
                        if (strCMD == "SVID")
                        {
                            for (int i = 0; i < Global.RGA_SVID_CH1.Length; i++)
                            {
                                if (i == 16 || i == 17 || i == 18)
                                {
                                }
                                else
                                {
                                    Global.RGA_SVID_CH1[i] = Global.TCP_Message[i + 2];
                                    Global.RGA_SVID_CH2[i] = Global.TCP_Message[i + 700 * 1 + 2];
                                    Global.RGA_SVID_CH3[i] = Global.TCP_Message[i + 700 * 2 + 2]; //CH3 CH4 在北方华创机台上为CH5 CH6
                                    Global.RGA_SVID_CH4[i] = Global.TCP_Message[i + 700 * 3 + 2];
                                    Global.RGA_SVID_CH5[i] = Global.TCP_Message[i + 700 * 4 + 2];
                                    Global.RGA_SVID_CH6[i] = Global.TCP_Message[i + 700 * 5 + 2];
                                    Global.RGA_SVID_CHC[i] = Global.TCP_Message[i + 700 * 6 + 2];
                                    Global.RGA_SVID_CHD[i] = Global.TCP_Message[i + 700 * 7 + 2];
                                    Global.RGA_SVID_CHE[i] = Global.TCP_Message[i + 700 * 8 + 2];
                                    Global.RGA_SVID_CHF[i] = Global.TCP_Message[i + 700 * 9 + 2];
                                    Global.RGA_SVID_TM1[i] = Global.TCP_Message[i + 700 * 10 + 2];
                                    Global.RGA_SVID_TM2[i] = Global.TCP_Message[i + 700 * 11 + 2];

                                    Global.Gem_SVID_List_CH1[i].Value = Global.TCP_Message[i + 2];
                                    Global.Gem_SVID_List_CH2[i].Value = Global.TCP_Message[i + 700 * 1 + 2];
                                    Global.Gem_SVID_List_CH3[i].Value = Global.TCP_Message[i + 700 * 2 + 2];
                                    Global.Gem_SVID_List_CH4[i].Value = Global.TCP_Message[i + 700 * 3 + 2];
                                    Global.Gem_SVID_List_CH5[i].Value = Global.TCP_Message[i + 700 * 4 + 2];
                                    Global.Gem_SVID_List_CH6[i].Value = Global.TCP_Message[i + 700 * 5 + 2];
                                    Global.Gem_SVID_List_CHC[i].Value = Global.TCP_Message[i + 700 * 6 + 2];
                                    Global.Gem_SVID_List_CHD[i].Value = Global.TCP_Message[i + 700 * 7 + 2];
                                    Global.Gem_SVID_List_CHE[i].Value = Global.TCP_Message[i + 700 * 8 + 2];
                                    Global.Gem_SVID_List_CHF[i].Value = Global.TCP_Message[i + 700 * 9 + 2];
                                    Global.Gem_SVID_List_TM1[i].Value = Global.TCP_Message[i + 700 * 10 + 2];
                                    Global.Gem_SVID_List_TM2[i].Value = Global.TCP_Message[i + 700 * 11 + 2];
                                }
                            }
                        }
                        else if (strCMD == "ALID")
                        {
                            #region   20250615 Al警报版本
                            //for (int A = 0; A < 612; A++)
                            //{
                            //    Global.Gem_ALID_List[A].Value = Global.TCP_Message[A  + 2];
                            //    Global.RGA_ALID[A] = Global.TCP_Message[A +2];
                            //    if (Global.Gem_ALID_List[A].Value == "1")
                            //    {
                            //        Console.WriteLine("AL:" + A.ToString() + "=1");
                            //    }
                            //}
                            //AlSend_Host();  //Hose Alarm Send
                            #endregion
                            //20250707
                            for (int i = 2; i < Global.TCP_Message.Length; i++)
                            {
                                if (i % 2 == 0)
                                {
                                    SECS.Host.S5F1_SendAlarm(0x80, Convert.ToInt32(Global.TCP_Message[i]), Global.TCP_Message[i + 1]);
                                    SECS.EQ.S5F1_SendAlarm(0x80, Convert.ToInt32(Global.TCP_Message[i]), Global.TCP_Message[i + 1]);//20260126zx
                                }  
                            }

                        }
                    }
                    else if (Global.TCP_Message[0].ToString() == "RNR")
                    {
                        Global.Recipe = true;
                    }
                    else if (Global.TCP_Message[0].ToString() == "OnLine")
                    {
                        EventRead_Host();  //读取RPTID
                        eventReport = false;//机台RPT也重新更新 20260123zx 待测试
                    }
                    else if (Global.TCP_Message[0].ToString() == "S2F41")//停机台 S2F41,Stop,StationID,chamber,1;
                    {
                        if (Global.TCP_MessageSV[4] == "1")
                            SECS.EQ.S2F41_SendCommand(Global.TCP_MessageSV[1], Global.TCP_MessageSV[2], Global.TCP_MessageSV[3]);
                    }else if(Global.TCP_Message[0].ToString() == "S6F11")
                    {
                        if (lengthMSG < 4)
                            return;
                        ArrayList tmpSVID_list = new ArrayList();
                        for (int ii = 3; ii< lengthMSG; ii+=2){
                            tmpSVID_list.Add(Global.TCP_Message[ii].ToString());
                        }
                        SECS.Host.S6F11_SendCommand(60008, Convert.ToUInt32(Global.TCP_Message[1]), Convert.ToUInt32(Global.TCP_Message[1]), tmpSVID_list);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            { Global.DisplayInfo("TCP 接受数据解析失败，" + ex.ToString()); }
        }

        void _tcpcomm_MessageReceivedSV(object sender, CNCVision.Communicate.MessageEventArgs e)
        {
            try
            {
                Global.TCP_MessagelistSV = e.Message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string line in Global.TCP_MessagelistSV)
                {
                    Global.TCP_MessageSV = line.Split(',');
                    if (Global.TCP_MessageSV[0] == "S2F41" && Global.TCP_MessageSV[4] == "1")
                    {
                        SECS.EQ.S2F41_SendCommand(Global.TCP_MessageSV[1], Global.TCP_MessageSV[2], Global.TCP_MessageSV[3]);
                    }
                }

            }
            catch (Exception ex)
            { Global.DisplayInfo("TCP 接受数据解析失败，" + ex.ToString()); }
        }
        #endregion

        #region  "数据更新方法"
        public void Refresh_DataGridView(DataGridView dataGridView, string[] data, int Cell)
        {
            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    CNCVision.CtrDisplay.ShowText(dataGridView, i, Cell, data[i]);
                }
            }
            catch (Exception ex)
            { }
        }
        #endregion

        /// <summary>
        /// RAG对Host 警报发送测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid_RGA_ALID_Table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1 && e.ColumnIndex == 4)
            {
                Boolean flag = Convert.ToBoolean(this.dataGrid_RGA_ALID_Table.Rows[e.RowIndex].Cells[e.ColumnIndex].EditedFormattedValue);
                if (flag == true)
                {
                    SECS.Host.S5F1_SendAlarm(0x80, System.Convert.ToInt32(this.dataGrid_RGA_ALID_Table.Rows[e.RowIndex].Cells[0].Value), "test");
                }
            }
        }

        /// <summary>
        /// RGA对机台回去的事件讯息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid_RGA_CEID_Table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGrid_EQ_CEID_Table.Columns[e.ColumnIndex].Name == "Column12" && e.RowIndex >= 0)
            {
                Point screenPoint = Control.MousePosition;
                // 将屏幕坐标转换为相对于控件的坐标
                // Point controlPoint = this.PointToScreen(screenPoint);
                // 显示坐标 Console.WriteLine($"X: {controlPoint.X}, Y: {controlPoint.Y}");
                //说明点击的列是DataGridViewButtonColumn列
                string mes = SECS.EQ.Gem_CEID_List[e.RowIndex].Message;
                DisplayMessage displayMessage = new DisplayMessage();
                displayMessage.Location = screenPoint;
                displayMessage.MyFormLoad(mes);
                displayMessage.ShowDialog();
            }
        }
        #region  通讯测试
        /// <summary>
        /// HOST SECS 测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void button10_Click(object sender, EventArgs e)
        //{
        //    ArrayList CEidlist = new ArrayList(); 
        //    CEidlist.Add("113600001");
        //    CEidlist.Add("113600002");
        //    Global.SendRGA_SVID(CEidlist);
        //}

        /// <summary>
        /// TCP 测试发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_TCP_Send_Click(object sender, EventArgs e)
        {
            //Communicate.SendMessage(txt_TCP_Send.Text);
        }

        private void btn_HostSend_Click(object sender, EventArgs e)
        {
            switch (com_HostCommand.Text)
            {
                case "S1F1":
                    {
                        SECS.Host.S1F1_SendCommand();
                        break;
                    }
                case "S1F2":
                    {

                        break;
                    }
                case "S1F3":
                    {
                        SECS.Host.S1F3_SendCommand();
                        break;
                    }
                case "S1F4":
                    {
                        break;
                    }
                case "S1F13":
                    {
                        SECS.Host.S1F13_SendCommand(/*SECS.Host.MDLN, SECS.Host.SOFTREV*/);
                        break;
                    }
                case "S1F14":
                    {
                        break;
                    }
                case "S1F15":
                    {
                        break;
                    }
                case "S1F16":
                    {
                        break;
                    }
                case "S1F17":
                    {
                        break;
                    }
                case "S1F18":
                    {
                        break;
                    }
                case "S2F13":
                    {
                        break;
                    }
                case "S2F14":
                    {
                        break;
                    }
                case "S2F17":
                    {
                        SECS.Host.S2F17_SendCommand();
                        break;
                    }
                case "SF18":
                    {
                        break;
                    }
                case "S2F23":
                    {
                        break;
                    }
                case "S2F24":
                    {
                        break;
                    }
                case "S2F29":
                    {
                        break;
                    }
                case "S2F30":
                    {
                        break;
                    }
                case "S2F31":
                    {
                        break;
                    }
                case "S2F32":
                    {
                        break;
                    }
                case "S2F33":
                    {
                        break;
                    }
                case "S2F34":
                    {
                        break;
                    }
                case "S2F35":
                    {
                        break;
                    }
                case "S2F36":
                    {
                        break;
                    }
                case "S2F37":
                    {
                        break;
                    }
                case "S2F38":
                    {
                        break;
                    }
                case "S5F1":
                    {
                        break;
                    }
                case "S5F2":
                    {
                        break;
                    }
                case "S5F3":
                    {
                        break;
                    }
                case "S5F4":
                    {
                        break;
                    }
                case "S5F7":
                    {
                        break;
                    }
                case "S5F8":
                    {
                        break;
                    }
                case "S6F1":
                    {
                        break;
                    }
                case "S6F2":
                    {
                        break;
                    }
                case "S6F11":
                    {
                        //Global.ReadRPTID_File("D:\\EVID");
                        //for (int S = 0; S < Global.RGA_RPTID_CEIDlist.Count; S++)
                        //{
                        //    var filteredRecords = Global.Gem_CEID_List.Where(r => r.ID == Convert.ToInt32(Global.RGA_RPTID_CEIDlist[S])).ToList();
                        //    if (filteredRecords.Count > 0)
                        //    {
                        //        if (filteredRecords[0].Status == "1" && Global.RGA_RPTID_CEID_bool[S] == false)
                        //        {
                        //            Global.ReadRPTID_CEID_File("D:\\EVID\\" + filteredRecords[0].ID, Global.RGA_RPTID_CEIDlist);
                        //            Global.RGA_RPTID_CEID_bool[S] = true;
                        //            Global.RGA_RPTID_CEIDlist.Add(1360003);
                        //            SECS.Host.S6F11_SendCommand(60008, Convert.ToUInt32(Global.RGA_RPTID_CEIDlist[S]), Global.RGA_RPTID_list, Global.RGA_RPTID_SVID_list);
                        //        }
                        //        else if (filteredRecords[0].Status == "0")
                        //        {
                        //            Global.RGA_RPTID_CEID_bool[S] = false;
                        //        }
                        //    }

                        //}

                        //Global.RGA_RPTID_CEIDlist.Clear();
                        //Global.RGA_RPTID_SVID_list.Clear();
                        //Global.RGA_RPTID_list.Clear();
                        break;
                    }
                case "S6F12":
                    {
                        break;
                    }
                case "S6F15":
                    {
                        break;
                    }
                case "S6F16":
                    {
                        break;
                    }
                case "S6F19":
                    {
                        break;
                    }
                case "S6F20":
                    {
                        break;
                    }
                case "S9F1":
                    {
                        break;
                    }
                case "S9F3":
                    {
                        break;
                    }
                case "S9F5":
                    {
                        break;
                    }
                case "S9F7":
                    {
                        break;
                    }
                case "S9F9":
                    {
                        break;
                    }
            }
        }

        private void btn_EQSend_Click(object sender, EventArgs e)
        {
            switch (com_EQCommand.Text)
            {
                case "S1F1":
                    {
                        SECS.EQ.S1F1_SendCommand();
                        break;
                    }
                case "S1F2":
                    {
                        SECS.EQ.S2F17_SendCommand();
                        break;
                    }
                case "S1F3":
                    {
                        SECS.EQ.S1F3_SendCommand(Global.SECS_RGA_SV.ToArray());
                        break;
                    }
                case "S1F4":
                    {
                        break;
                    }
                case "S1F13":
                    {
                        SECS.EQ.S1F13_SendCommand(/*Global.MDLN, Global.SOFTREV*/);
                        break;
                    }
                case "S1F14":
                    {
                        break;
                    }
                case "S1F15":
                    {
                        SECS.EQ.S1F15_SendCommand();
                        break;
                    }
                case "S1F16":
                    {
                        break;
                    }
                case "S1F17":
                    {
                        SECS.EQ.S1F17_SendCommand();
                        break;
                    }
                case "S1F18":
                    {
                        break;
                    }
                case "S2F13":
                    {
                        SECS.EQ.S2F13_SendCommand();
                        break;
                    }
                case "S2F14":
                    {
                        break;
                    }
                case "S2F17":
                    {
                        SECS.EQ.S2F17_SendCommand();
                        break;
                    }
                case "SF18":
                    {
                        break;
                    }
                case "S2F23":
                    {
                        break;
                    }
                case "S2F24":
                    {
                        break;
                    }
                case "S2F29":
                    {
                        break;
                    }
                case "S2F30":
                    {
                        break;
                    }
                case "S2F31":
                    {
                        break;
                    }
                case "S2F32":
                    {
                        break;
                    }
                case "S2F33":
                    {
                        ArrayList array = new ArrayList();
                        array.Add(500001);
                        array.Add(500002);
                        array.Add(500003);
                        array.Add(500004);
                        array.Add(500005);
                        //SECS.EQ.S2F33_SendCommand(1001,2000, array, txt_HostTestSecs);
                        array.Clear();
                        break;
                    }
                case "S2F34":
                    {
                        break;
                    }
                case "S2F35":
                    {
                        ArrayList ceid = new ArrayList();
                        ArrayList rptid = new ArrayList();
                        ceid.Add(20212215);
                        rptid.Add(2000);
                        //SECS.EQ.S2F35_SendCommand(1001, ceid, rptid, txt_HostTestSecs);
                        ceid.Clear();
                        rptid.Clear();
                        break;
                    }
                case "S2F36":
                    {
                        break;
                    }
                case "S2F37":
                    {
                        ArrayList ceid = new ArrayList();
                        ceid.Add(20212215);
                        // SECS.EQ.S2F37_SendCommand(false, ceid,txt_HostTestSecs);
                        ceid.Clear();
                        break;
                    }
                case "S2F38":
                    {
                        break;
                    }
                case "S5F1":
                    {
                        break;
                    }
                case "S5F2":
                    {
                        break;
                    }
                case "S5F3":
                    {
                        SECS.EQ.S5F3_SendAlarm(0, 0);
                        break;
                    }
                case "S5F4":
                    {
                        break;
                    }
                case "S5F7":
                    {
                        break;
                    }
                case "S5F8":
                    {
                        break;
                    }
                case "S6F1":
                    {
                        break;
                    }
                case "S6F2":
                    {
                        break;
                    }
                case "S6F11":
                    {
                        break;
                    }
                case "S6F12":
                    {
                        break;
                    }
                case "S6F15":
                    {
                        break;
                    }
                case "S6F16":
                    {
                        break;
                    }
                case "S6F19":
                    {
                        break;
                    }
                case "S6F20":
                    {
                        break;
                    }
                case "S7F19":
                    {
                        SECS.EQ.S7F19_SendCommand();
                        break;
                    }
                case "S9F1":
                    {
                        break;
                    }
                case "S9F3":
                    {
                        break;
                    }
                case "S9F5":
                    {
                        break;
                    }
                case "S9F7":
                    {
                        break;
                    }
                case "S9F9":
                    {
                        break;
                    }
            }
        }
        #endregion



        #region 回传 EQ 讯息
        //public void send_TCPmeg()
        //{
        //    int S = 0;
        //    Global.SECS_RGA[0] = "RND,";
        //    string message = "";

        //    Global.SECS_RGA[19] = "0";
        //    for (int i = 0; i < 20; i++)
        //    {
        //        if (SECS.EQ.Gem_CEID_List[i*2].State == "1" && SECS.EQ.Gem_CEID_List[i * 2+1].State == "0")
        //        {
        //            Global.SECS_RGA[i+3] = "1";
        //        }
        //        else if(SECS.EQ.Gem_CEID_List[i * 2].State == "0" && SECS.EQ.Gem_CEID_List[i * 2 + 1].State == "0")
        //        { Global.SECS_RGA[i + 3] = "0"; }
        //        if (SECS.EQ.Gem_CEID_List[i * 2].State == "0" && SECS.EQ.Gem_CEID_List[i * 2+1].State == "1")
        //        {
        //            Global.SECS_RGA[i+3] = "0";
        //        }else if(SECS.EQ.Gem_CEID_List[i * 2].State == "0" && SECS.EQ.Gem_CEID_List[i * 2 + 1].State == "0")
        //        { Global.SECS_RGA[i + 3] = "0"; }
        //    }
        //    for (int i=23; i< Global.SECS_RGA.Length;i++)
        //    {

        //        if(i>=23 && i<81)
        //        {
        //            if (i == 26 || i == 27 || i == 32 || i == 33 || i == 38 || i == 39 || i == 44 || i == 45 || i == 52 || i == 53 || i == 58 || i == 59 || i == 26 || i == 27 || i == 62 || i == 63 || i == 67 || i == 68
        //                 || i == 25 || i == 31 || i == 37 || i == 43 || i == 47 || i == 48 || i == 51 || i == 57 || i == 61 || i == 66)
        //            {
        //                //Global.SECS_RGA[62] = "AAAA";
        //            }
        //            else 
        //            {
        //                Global.SECS_RGA[i] = SECS.EQ.Gem_SVID_List[i - 23].Value.ToString();
        //            }

        //        }
        //        else if (i >= 122 && i < 135)
        //        {
        //            Global.SECS_RGA[i] = SECS.EQ.Gem_SVID_List[i - 64].Value.ToString();
        //        }

        //    }


        //    if (SECS.EQ.Report==true)
        //    {

        //    }
        //    if (aaa == 1)
        //    {
        //        Global.Gem_SVID_List_CHE[16].Value = "L123a";
        //        Global.Gem_SVID_List_CHE[17].Value = "SL123a";
        //        Global.Gem_SVID_List_CHE[18].Value = "W123a";
        //        Global.RGA_SVID_CHE[16] = "L123a";
        //        Global.RGA_SVID_CHE[17] = "SL123a";
        //        Global.RGA_SVID_CHE[18] = "W123a";
        //        Global.SECS_RGA[19] = "1";
        //        Global.SECS_RGA[61] = "P-DMD15S.Degas.rcp";
        //        Global.SECS_RGA[89] = "-1";
        //    }else if(aaa == 2)
        //    {
        //        Global.Gem_SVID_List_CHE[16].Value = "L123";
        //        Global.Gem_SVID_List_CHE[17].Value = "SL123";
        //        Global.Gem_SVID_List_CHE[18].Value = "W123";
        //        Global.RGA_SVID_CHE[16] = "L123";
        //        Global.RGA_SVID_CHE[17] = "SL123";
        //        Global.RGA_SVID_CHE[18] = "W123";
        //        Global.SECS_RGA[19] = "0";
        //        Global.SECS_RGA[61] = "P-DMD15S.Degas.rcp";
        //        Global.SECS_RGA[89] = "-2";
        //    }
        //    aaa = 0;
        //    for (int i = 0; i < Global.SECS_RGA.Length; i++)
        //    {
        //        if (i == 0)
        //        {
        //            message = Global.SECS_RGA[i];
        //        }
        //        else if (i == 134)
        //        {
        //            message = message + Global.SECS_RGA[i].ToString();
        //        }
        //        else
        //        {
        //            message = message + Global.SECS_RGA[i].ToString() + ",";
        //        }
        //    }

        //    Communicate.SendMessage(message);
        //}

        public void send_TCPmegSV(ArrayList sv)
        {
            var messageList = new List<string> { };
            messageList.Add("S1F4");
            int LengthSV = sv.Count;
            for (int i = 0; i < LengthSV; i++)
                messageList.Add(Convert.ToString(sv[i].ToString()));
            lock (Global.ObjRefreshThread)
            {
                Global.Queue_Tcp.Enqueue(String.Join(",", messageList) + "\r\n");
            }
            //Communicate.SendMessage(message);
        }

        public void send_TCPmeg_HC()
        {
            var messageList = new List<string> { };
            int LengthSECS_RGA = Global.SECS_RGA.Length;
            for (int i = 0; i < LengthSECS_RGA; i++)
                messageList.Add(Global.SECS_RGA[i]);
            Global.Queue_Tcp.Enqueue(String.Join(",", messageList) + "\r\n");
        }
        #endregion
        private void button11_Click(object sender, EventArgs e)
        {
            string path = "C:\\Users\\86178\\Desktop\\GemDriver20250509\\Report";
            Global.ReadRPTID_File(path);
            Global.ReadRPTID_CEID_File(path, Global.RGA_RPTID_list);
        }

        private void btn_RGA_EQ_SVID_Click(object sender, EventArgs e)
        {
            //    Global.SECS_RGA[0] = "RND,";
            //    string message = "";
            //    send_TCPmeg();
            //    for (int i=0;i < Global.SECS_RGA.Length; i++)
            //    {
            //        if(i==0)
            //        {
            //            message = Global.SECS_RGA[i];
            //        }
            //        else if(i==111)
            //        {
            //            message = message + Global.SECS_RGA[i].ToString() ;
            //        }
            //        else
            //        {
            //            message = message + Global.SECS_RGA[i].ToString()+",";
            //        }
            //    }
            //    Communicate.SendMessage(message);
        }

        private void btn_EQlogPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
            {
                txt_EQlog_path.Text = dilog.SelectedPath;
            }
        }

        private void btn_HSlogPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
            {

                txt_HSlog_path.Text = dilog.SelectedPath;

            }
        }

        /// <summary>
        /// 报告创建，链接，使能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CreateReport_Click(object sender, EventArgs e)
        {
            ////定义报告
            //SECS.EQ.S2F33_SendCommand(Global.DATAID, Global.RecipeStartRPTID, Global.RecipeStartLinkedItemID, true);
            //SECS.EQ.S2F33_SendCommand(Global.DATAID, Global.RecipeEndRPTID, Global.RecipeEndLinkedItemID, true);
            //SECS.EQ.S2F33_SendCommand(Global.DATAID, Global.RecipeStepStartRPTID, Global.RecipeStepStartLinkedItemID, true);
            //SECS.EQ.S2F33_SendCommand(Global.DATAID, Global.RecipeStepEndRPTID, Global.RecipeStepEndLinkedItemID, true);
            ////链接报告
            //SECS.EQ.S2F35_SendCommand(Global.DATAID, Global.RecipeStartEventID, Global.RecipeStartRPTID, true);
            //SECS.EQ.S2F35_SendCommand(Global.DATAID, Global.RecipeEndEventID, Global.RecipeEndRPTID, true);
            //SECS.EQ.S2F35_SendCommand(Global.DATAID, Global.RecipeStepStartEventID, Global.RecipeStepStartRPTID, true);
            //SECS.EQ.S2F35_SendCommand(Global.DATAID, Global.RecipeStepEndEventID, Global.RecipeStepEndRPTID, true);
            ////启用报告
            //SECS.EQ.S2F37_SendCommand(true, Global.RecipeStartEventID, true);
            //SECS.EQ.S2F37_SendCommand(true, Global.RecipeEndEventID, true);
            //SECS.EQ.S2F37_SendCommand(true, Global.RecipeStepStartEventID, true);
            //SECS.EQ.S2F37_SendCommand(true, Global.RecipeStepEndEventID, true);

            DefineReport();
            System.Threading.Thread.Sleep(4000);
            LinkEventReport();
            System.Threading.Thread.Sleep(4000);
            EnableDisableEventReport();
            //    if (count == 3)
            //    { DefineReport(); }
            //    if (count == 6)
            //    { LinkEventReport(); }
            //    if (count == 9)
            //    { EnableDisableEventReport(); }
        }

        /// <summary>
        /// 去使能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ClearReport_Click(object sender, EventArgs e)
        {
            ClearEvent();
        }


        public void EventRead_Host()
        {
            Global.ReadRPTID_File("D:\\EVID");
        }
        //public void EventSend_Host()
        //{
        //    try
        //    {
        //        Global.DisplayInfo("FDC事件上报开始");
        //        for (int S = 0; S < Global.RGA_RPTID_CEIDlist.Count; S++)
        //        {
        //            var filteredRecords = Global.Gem_CEID_List.Where(r => r.ID == Convert.ToInt32(Global.RGA_RPTID_CEIDlist[S])).ToList();
        //            if (filteredRecords.Count > 0)
        //            {
        //                if (filteredRecords[0].Status == "1")    // && Global.RGA_RPTID_CEID_bool[S] == false
        //                {
        //                    //1360004
        //                    Global.ReadRPTID_CEID_File("D:\\EVID\\" + filteredRecords[0].ID, Global.RGA_RPTID_CEIDlist);
        //                    //Global.ReadRPTID_CEID_File("D:\\EVID\\" + "1360004", Global.RGA_RPTID_CEIDlist);
        //                    Global.RGA_RPTID_CEID_bool[S] = true;
        //                    SECS.Host.S6F11_SendCommand(60008, Convert.ToUInt32(Global.RGA_RPTID_CEIDlist[S]), Global.RGA_RPTID_list, Global.RGA_RPTID_SVID_list);
        //                }
        //                //else if (filteredRecords[0].Status == "0")
        //                //{
        //                //    Global.RGA_RPTID_CEID_bool[S] = false;
        //                //}
        //            }
        //        }
        //        Global.RGA_RPTID_SVID_list.Clear();
        //        Global.RGA_RPTID_list.Clear();
        //        Global.DisplayInfo("FDC事件上报完成");
        //    }
        //    catch (Exception ex)
        //    {
        //        Global.DisplayInfo("FDC事件上报异常：" + ex.ToString());
        //    }
        //}


        /// <summary>
        /// 北方华创报告ID定义
        /// </summary>
        //public void ReportID()
        //{
        //    string path = Global.WorkDir +"RPTID";
        //    string[] files = Directory.GetFiles(path, "*.csv");
        //    Global.RecipeChamberRPTID_HC = new string[files.Length];
        //    for (int i = 0; i < files.Length; i++){
        //        Global.RecipeChamberRPTID_HC[i] = files[i];
        //    }
        //}

        /// <summary>
        /// 清除定义的报告
        /// </summary>
        public void ClearEvent()
        {
            if (Global.EQ_Name_Status == false)//XKL不清除？
                return;
            for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
            {
                if (SECS.EQ.gRPTID_EQ[i].isDefind == false)
                {
                    SECS.EQ.S2F33_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], false);
                    SECS.EQ.gRPTID_EQ[i].isDefind = true;
                }
            }
        }

        /// <summary>
        /// 定义报告
        /// </summary>
        public void DefineReport()
        {
            for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
            {
                SECS.EQ.S2F33_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], true);
            }
        }

        /// <summary>
        /// 链接事件
        /// </summary>
        public void LinkEventReport()
        {
            for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
            {
                SECS.EQ.S2F35_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], true);
            }
        }

        /// <summary>
        /// 使能事件
        /// </summary>
        public void EnableDisableEventReport()
        {
            for (int i = 0; i < SECS.EQ.gRPTID_EQ.Count; i++)
            {
                SECS.EQ.S2F37_SendCommandNEW(SECS.EQ.gRPTID_EQ[i], true);
            }
        }
        //public void AlSend_Host()
        //{
        //    if (Hostsif13 == true)
        //    {
        //        for (int i = 0; i < Global.Gem_ALID_List.Count; i++)
        //        {
        //            if (Global.Gem_ALID_List[i].Value == "1" && Global.Gem_ALID_Status[i] == false)
        //            {
        //                Global.Gem_ALID_Status[i] = true;
        //                //SECS.Host.S5F1_SendAlarm(1, System.Convert.ToInt32(this.dataGrid_RGA_ALID_Table.Rows[e.RowIndex].Cells[0].Value), txtSendHost);
        //                SECS.Host.S5F1_SendAlarm(1, Global.Gem_ALID_List[i].AlarmId,"test");
        //            }
        //            else if (Global.Gem_ALID_List[i].Value == "0" && Global.Gem_ALID_Status[i] == true)
        //            {
        //                Global.Gem_ALID_Status[i] = false;
        //                SECS.Host.S5F1_SendAlarm(0, Global.Gem_ALID_List[i].AlarmId,"test");
        //            }
        //        }
        //    }
        //}
        private void button12_Click(object sender, EventArgs e)
        {
            aaa = 1;
        }
        private void button13_Click(object sender, EventArgs e)
        {
            aaa = 2;
        }
        private void com_EQ_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_EQ_Name.SelectedIndex == 0)
            {
                Global.EQ_Name = com_EQ_Name.Text;
                Global.SystemParameterIni.WriteString("Cofig", "EQ_Name", Global.EQ_Name);
            }
            else
            {
                Global.EQ_Name = com_EQ_Name.Text;
                Global.SystemParameterIni.WriteString("Cofig", "EQ_Name", Global.EQ_Name);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Global.EQ_SIF3_Time = textBox3.Text;
                Global.SystemParameterIni.WriteString("Cofig", "EQ_SIF3_Time", Global.EQ_SIF3_Time);
            }
        }
    }

    public static class CSVReaderHelper
    {
        /// <summary>
        /// 缓冲以使滑动滚轮时不卡
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="setting"></param>
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            pi.SetValue(dgv, setting, null);
        }

    }
}
