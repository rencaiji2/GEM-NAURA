using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using System.Windows.Forms;
using KSecsWrapperDotNet;
using System.IO;

namespace GemDriver.SECS
{
    class Host
    {

        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);


        public  static KSecsWrapperDotNet.KSecsWrapper Host_Control = new KSecsWrapperDotNet.KSecsWrapper();

        public delegate void ChangeKSecsMessage(KSecsWrapperDotNet.KSecsMessage value);
        public static event ChangeKSecsMessage ChangeKSecsMessageEvent;


        public delegate void TransactionLogEvent(string value);
        public static event TransactionLogEvent TransactionLog;

        private static string Model_Number = "", Revision = "";
        public static string TransactionErrorMessage = "";

        // 定义SYSTEMTIME结构体
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        private static string DateTimeformat = "yyyyMMddHHmmssff"; //時間格式

        #region  发送参数定义
        public static byte COMMACK = 0;
        
        #endregion

        public class ALID
        {
            [Name("AlarmId")]
            public int AlarmId { get; set; }

            [Name("Level")]
            public string Level { get; set; }
            [Name("AlarmName")]
            public string AlarmName { get; set; }
            [Name("AlarmText")]
            public string AlarmText { get; set; }

            [Name("Value")]
            public string Value { get; set; }
        }
        public class SVID
        {
            [Name("ID")]
            public int ID { get; set; }
            [Name("Name")]
            public string Name { get; set; }
        }
        public class CEID
        {
            [Name("ID")]
            public int ID { get; set; }
            [Name("Name")]
            public string Name { get; set; }
        }
        public static List<ALID> Gem_ALID_List;
        public static List<SVID> Gem_SVID_List;
        public static List<CEID> Gem_CEID_List;

        public static ArrayList SIF3_Message = new ArrayList();
        public static ArrayList SIF4_Message = new ArrayList();
        public static ArrayList SIF14_Message = new ArrayList();
        public static ArrayList SIF16_Message = new ArrayList();
        public static ArrayList SIF18_Message = new ArrayList();
        public static ArrayList S2F18_Message = new ArrayList();
        public static ArrayList S2F32_Message = new ArrayList();
        public static ArrayList S2F34_Message = new ArrayList();
        public static ArrayList S2F36_Message = new ArrayList();
        public static ArrayList S2F38_Message = new ArrayList();
        public static ArrayList S6F11_Message = new ArrayList();
        public static ArrayList S7F20_Message = new ArrayList();

        //public static List<Global.MapRPT> gRPTID_Host = new List<Global.MapRPT> { };
        //public static void loadReport()
        //{
        //    gRPTID_Host.Clear();
        //    string path = Global.WorkDir + "RPTID\\Host";
        //    string[] files = Directory.GetFiles(path, "*.csv");
        //    Global.RPTID_Count = files.Length;
        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        string name = files[i].Split('\\').Last();
        //        //Global.RecipeChamberRPTID_HC[i] = name.Split('.').FirstOrDefault();
        //        Global.MapRPT tmpMapRPT = new Global.MapRPT { };
        //        tmpMapRPT.RPTID = tmpMapRPT.CEID = name.Split('.').FirstOrDefault();

        //        string file = files[i];
        //        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
        //        if (!File.Exists(file))
        //            continue;
        //        string Line;
        //        Encoding encoder = Encoding.UTF8;
        //        using (StreamReader reader = new StreamReader(file, encoder))
        //        {//读取报告的SVID
        //            while ((Line = reader.ReadLine()) != null)
        //            {
        //                string[] tmpList = Line.Split(',');
        //                tmpMapRPT.SVID_LIST.Add(tmpList[0]);
        //            }
        //        }
        //        //SECS.EQ.S2F33_SendCommandNEW(Global.DATAID, MES[1], SVID, false);
        //        gRPTID_Host.Add(tmpMapRPT);
        //    }
        //}

        public static void Host_Connect()
        {
            Host_Control.LoadFromIni(Global.WorkDir + "Parameter/Host_Gem.ini", "SECS");
            Host_Control.SxmlFile = Global.WorkDir + "Parameter/GenHost.sxml";
            Host_Control.OnSecondaryReceived += SECS.Host.OnSecondaryReceived;
            Host_Control.OnPrimaryReceived += SECS.Host.OnPrimaryReceived;
            Host_Control.OnTransactionLog += SECS.Host.OnTransactionLog;
            Host_Control.OnTransactionError += SECS.Host.OnTransactionError;
            Host_Control.Open();
        }

        public static void Host_Close()
        {
            Host_Control.Close();
        }

        public static void Read_Alarn()
        {
            var file = Global.WorkDir + "Parameter/RGA_ALID.csv";
            using (var reader = new System.IO.StreamReader(file))
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;//有頭部
                using (var csv = new CsvHelper.CsvReader(reader, config))
                {
                    Gem_ALID_List = csv.GetRecords<ALID>().ToList();


                    //快速查詢
                    //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                }
            }
        }

        public static bool okHostS1F1 = true;
        public static bool okHostS1F13 = true;
        private static void OnSecondaryReceived(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined)
        {
            ChangeKSecsMessageEvent(m_Message);
            switch (m_Message.Stream)
            {
                case 1:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                {
                                    if(m_Transaction.Secondary.RootItem.ItemNumber==0)
                                    {
                                       // MessageBox.Show("The host sends a zero-length list to the equipment.");
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (m_Transaction.Secondary.RootItem.ItemNumber == 0)
                                    {
                                        //MessageBox.Show("The host sends a zero-length list to the equipment.");
                                    }
                                    okHostS1F1 = true;
                                }
                                break;
                            case 4:
                                {
                                    MessageParseS1F4(m_Transaction, m_Message, SIF14_Message);
                                }
                                break;
                            case 14:
                                {
                                    MessageParse(m_Transaction, m_Message, SIF14_Message);
                                    okHostS1F13 = true;//Global.SECS_RGA[23] = "1";
                                }
                                break;
                            case 16:
                                {
                                    MessageParse(m_Transaction, m_Message, SIF16_Message);
                                }
                                break;
                            case 18:
                                {
                                    MessageParse(m_Transaction, m_Message, SIF18_Message);
                                }
                                break;
                        }
                    }
                    break;
                case 2:
                    {
                        switch (m_Message.Function)
                        {
                            case 14:
                                {

                                }
                                break;
                            case 18:
                                {
                                    MessageParse(m_Transaction, m_Message, SIF18_Message);
                                    if(SIF18_Message[0].ToString().Length==12)
                                    {
                                        Global.Time = "yyMMddHHmmss"; 
                                    }
                                    else if (SIF18_Message[0].ToString().Length == 16)
                                    {
                                        Global.Time = "yyyyMMddHHmmssss";
                                    }
                                }
                                break;
                            case 32:
                                {
                                    MessageParse(m_Transaction, m_Message, S2F32_Message);
                                }
                                break;
                            case 34:
                                {
                                    MessageParse(m_Transaction, m_Message, S2F34_Message);
                                }
                                break;
                            case 36:
                                {
                                    MessageParse(m_Transaction, m_Message, S2F36_Message);
                                }
                                break;
                            case 38:
                                {
                                    MessageParse(m_Transaction, m_Message, S2F38_Message);
                                }
                                break;
                        }
                    }
                    break;
                case 5:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                {

                                }
                                break;
                            case 4:
                                {
                                    //string responseTime = m_Transaction.Secondary.RootItem.ItemsByName["ActualTime"].AsString;
                                    //bool bTimeFormat = DateTime.TryParseExact(responseTime, DateTimeformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                                    //if (bTimeFormat == true)
                                    //{
                                    //    var systemTime = new SYSTEMTIME();
                                    //    systemTime.wYear = System.Convert.ToUInt16(responseTime.Substring(0, 4));
                                    //    systemTime.wMonth = System.Convert.ToUInt16(responseTime.Substring(4, 2));
                                    //    systemTime.wDay = System.Convert.ToUInt16(responseTime.Substring(6, 2));
                                    //    systemTime.wHour = System.Convert.ToUInt16(responseTime.Substring(8, 2));
                                    //    systemTime.wMinute = System.Convert.ToUInt16(responseTime.Substring(10, 2));
                                    //    systemTime.wSecond = System.Convert.ToUInt16(responseTime.Substring(12, 2));
                                    //    systemTime.wMilliseconds = System.Convert.ToUInt16(responseTime.Substring(14, 2));
                                    //    if (!SetLocalTime(ref systemTime))
                                    //    { Console.WriteLine("無法設定系統時間"); }
                                    //}
                                    //UpDataResponseTimeEvent(responseTime);
                                }
                                break;
                        }
                    }
                    break;
                case 6:
                    {
                        switch (m_Message.Function)
                        {
                            case 11:
                                {

                                }
                                break;
                        }
                    }
                    break;
                case 7:
                    {
                        switch (m_Message.Function)
                        {
                            case 20:
                                {
                                    MessageParse(m_Transaction, m_Message, S7F20_Message);
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private static void OnPrimaryReceived(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined)
        {
            ChangeKSecsMessageEvent(m_Message);
            switch (m_Message.Stream)
            {
                case 1:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                { 
                                    S1F2_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); 
                                }
                                break;
                            case 3:
                                {
                                    MessageParseS1F3(m_Transaction, m_Message, SIF3_Message);
                                    Global.DisplayInfo("FDC发送S1F3.");
                                    string CH = "CH1";
                                    for (int i = 0; i < 10; i++)
                                    {
                                        switch (CH)
                                        {
                                            case "CH1":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CH1.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CH2";
                                                        }
                                                    }
                                                    CH = "CH2";
                                                    break;
                                                }
                                            case "CH2":
                                                {
                                                    for (int A = 0; A < SIF3_Message.Count; A++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CH2.Where(r => r.ID == Convert.ToInt32(SIF3_Message[A])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CH3";
                                                        }
                                                    }
                                                    CH = "CH3";
                                                    break;
                                                }
                                            case "CH3":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CH3.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CH4";
                                                        }
                                                    }
                                                    CH = "CH4";
                                                    break;
                                                }
                                            case "CH4":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CH4.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CHA";
                                                        }
                                                    }
                                                    CH = "CHA";
                                                    break;
                                                }
                                            case "CH5":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CH5.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CH6";
                                                        }
                                                    }
                                                    CH = "CH6";
                                                    break;
                                                }
                                            case "CH6":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CH6.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CHC";
                                                        }
                                                    }
                                                    CH = "CHC";
                                                    break;
                                                }
                                            case "CHC":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CHC.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CHD";
                                                        }
                                                    }
                                                    CH = "CHD";
                                                    break;
                                                }
                                            case "CHD":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CHD.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CHE";
                                                        }
                                                    }
                                                    CH = "CHE";
                                                    break;
                                                }
                                            case "CHE":
                                                {
  
                                                    for (int A = 0; A < SIF3_Message.Count; A++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CHE.Where(r => r.ID == Convert.ToInt32(SIF3_Message[A])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CHF";
                                                        }
                                                    }
                                                    CH = "CHF";
                                                    break;
                                                }
                                            case "CHF":
                                                {
                                                    for (int s = 0; s < SIF3_Message.Count; s++)
                                                    {
                                                        var filteredRecords = Global.Gem_SVID_List_CHF.Where(r => r.ID == Convert.ToInt32(SIF3_Message[s])).ToList();
                                                        if (filteredRecords.Count > 0)
                                                        {
                                                            Global.SendRGA_SV.Add(filteredRecords[0].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            CH = "CH0";
                                                        }
                                                    }
                                                    CH = "CH0";
                                                    break;
                                                }
                                        }

                                    }
                                    S1F4_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, Global.SendRGA_SV);
                                    Global.SendRGA_SV.Clear();
                                    Global.DisplayInfo("RGA回复S1F4完成.");
                                }
                                break;
                            case 13:
                                {
                                
                                    S1F14_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); 
                                }
                                break;
                            case 15:
                                { S1F16_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                            case 17:
                                { S1F18_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                        }
                    }
                    break;
                case 2:
                    {
                        switch (m_Message.Function)
                        {
                            case 13:
                                {
                                    ArrayList data = new ArrayList();
                                    data.Add("100");
                                    data.Add("101");
                                    data.Add("102");
                                    data.Add("103");
                                    S2F14_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, data);
                                }
                                break;
                            case 17:
                                {
                                    S2F18_SendCommand(m_Transaction, m_Message, "202501071159124",m_IsDefined);
                                }
                                break;
                            case 23:
                                {
                                    S2F24_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 29:
                                {
                                    ArrayList data = new ArrayList();
                                    data.Add("100");
                                    data.Add("101");
                                    data.Add("102");
                                    data.Add("103");
                                    S2F30_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, data);
                                }
                                break;
                            case 31:
                                {
                                    string responseTime = m_Message.RootItem.AsString;
                                    if(responseTime.Length==12)
                                    {
                                        responseTime = "20" + responseTime + "00";
                                    }

                                    DateTimeformat = "yyyyMMddHHmmssff";
                                    //string responseTime = m_Transaction.Secondary.RootItem.ItemsByName["ActualTime"].AsString;
                                    bool bTimeFormat = DateTime.TryParseExact(responseTime, DateTimeformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                                    if (bTimeFormat == true)
                                    {
                                        var systemTime = new SYSTEMTIME();
                                        systemTime.wYear = System.Convert.ToUInt16(responseTime.Substring(0, 4));
                                        systemTime.wMonth = System.Convert.ToUInt16(responseTime.Substring(4, 2));
                                        systemTime.wDay = System.Convert.ToUInt16(responseTime.Substring(6, 2));
                                        systemTime.wHour = System.Convert.ToUInt16(responseTime.Substring(8, 2));
                                        systemTime.wMinute = System.Convert.ToUInt16(responseTime.Substring(10, 2));
                                        systemTime.wSecond = System.Convert.ToUInt16(responseTime.Substring(12, 2));
                                        systemTime.wMilliseconds = System.Convert.ToUInt16(responseTime.Substring(14, 2));
                                        try
                                        {
                                            if (!SetLocalTime(ref systemTime))
                                            {
                                                S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                            }
                                            else
                                            { S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                        }
                                        catch(Exception ex)
                                        {
                                            S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                        }
                                    }
                                    else
                                    {
                                        S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                    }                                   
                                }
                                break;
                            case 33:
                                {
                                    S2F34_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 35:
                                {
                                    S2F36_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 37:
                                {
                                    S2F38_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            //case 41:
                            //    {
                            //        S2F42_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, Form1.Form.txtSendHost);
                            //    }
                            //    break;
                        }
                    }
                    break;
                case 5:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                { S5F2_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                            case 3:
                                { S5F4_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                            case 7:
                                {
                                    ArrayList DATA_List = new ArrayList();
                                    DATA_List.Add("1000,altx1");
                                    DATA_List.Add("1001,altx2");
                                    DATA_List.Add("1002,altx3"); 
                                    DATA_List.Add("1003,altx4");
                                    DATA_List.Add("1004,altx5");
                                    DATA_List.Add("1005,altx6");
                                    S5F8_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, DATA_List);
                                }
                                break;
                            case 15:
                                { 
                                    //S6F16_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, Form1.Form.txt_Send); 
                                }
                                break;
                        }
                    }
                    break;
                case 6:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                {
                                    S6F2_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 11:
                                {
                                    MessageParseRecv(m_Transaction, m_Message, S6F11_Message);
                                    S6F12_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); 
                                }
                                break;
                            case 13:
                                { }
                                break;
                            case 15:
                                {
                                    ArrayList RPTidlist = new ArrayList();
                                    RPTidlist.Add("100861");
                                    RPTidlist.Add("100871");
                                    RPTidlist.Add("100881");
                                    RPTidlist.Add("100891");
                                    S6F16_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, RPTidlist);
                                }
                                break;
                            case 19:
                                {
                                    ArrayList RPTidlist = new ArrayList();
                                    RPTidlist.Add("100861");
                                    RPTidlist.Add("100871");
                                    RPTidlist.Add("100881");
                                    RPTidlist.Add("100891");
                                    S6F20_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, RPTidlist);
                                }
                                break;
                        }
                    }
                    break;
                case 7:
                    {
                        switch (m_Message.Function)
                        {
                            case 19:
                                {
                                    ArrayList data = new ArrayList();
                                    data.Add("100");
                                    data.Add("101");
                                    data.Add("102");
                                    data.Add("103");
                                    S7F20_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, data);
                                }
                                break;
                        }
                    }
                    break;
                case 14:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                { S1F14_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                        }
                    }
                    break;

            }
        }

        private static void OnTransactionLog(string sender)
        {
            TransactionLog(sender);
        }

        private static void OnTransactionError(KSecsMessage m_Message, uint m_Ticket, int m_ErrorCode, string m_SmlTag, bool m_IsDefined)
        {
            switch (m_ErrorCode)
            {
                case -1:
                    { TransactionErrorMessage = "FALL_SEND_P:无法发送Primary讯息."; }
                    break;
                case -2:
                    { TransactionErrorMessage = "FALL_SEND_S:无法发送Secondary讯息."; }
                    break;
                case -3:
                    { TransactionErrorMessage = "T3 TIMEOUT:T3 Timeout(已发送Primary讯息，但没有接收到Secondary讯息)."; }
                    break;
                case -4:
                    { TransactionErrorMessage = "RECV_UNKNOWN_DEVICEID:收到的讯息所带的Device ID不正确."; }
                    break;
            }
        }



        #region  指令
        public static void S1F1_SendCommand()
        {
            okHostS1F1 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Are You There Request"); 
            SECS.Host.Host_Control.SendPrimary(message);
           // textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S1F2_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined)
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Online Data");
            m_Transaction.Secondary.RootItem.ItemsByName["MDLN"].AsString = Global.MDLN;
            m_Transaction.Secondary.RootItem.ItemsByName["SOFTREV"].AsString = Global.SOFTREV;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S1F3_SendCommand()
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Selected Equipment Status Request");

            SECS.Host.Host_Control.SendPrimary(message);
        }

        public static void S1F4_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList DataList)
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Selected Equipment Status Data");
            for (int i = 0; i < DataList.Count; i++)
            {
                m_Transaction.Secondary.RootItem.Items[i].AsString = DataList[i].ToString();
            }
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }
        public static void S1F13_SendCommand(/*string MDLN ,string SOFTREV  */)
        {
            //SECS.Host.MDLN, SECS.Host.SOFTREV
            okHostS1F13 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Establish Communication Request");
            message.RootItem.ItemsByName["MDLN"].AsString = Global.MDLN;
            message.RootItem.ItemsByName["SOFTREV"].AsString = Global.SOFTREV;
            SECS.Host.Host_Control.SendPrimary(message);

        }

        public static void S1F14_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Establish Communication Request ACK");
            m_Transaction.Secondary.RootItem.ItemsByName["COMMACK"].AsByte = COMMACK;
            m_Transaction.Secondary.RootItem.ItemsByName["MDLN"].AsString = Global.MDLN;
            m_Transaction.Secondary.RootItem.ItemsByName["SOFTREV"].AsString = Global.SOFTREV;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S1F15_SendCommand( )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Request Offline");
            SECS.Host.Host_Control.SendPrimary(message);

        }

        public static void S1F16_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Offline Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["OFLACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
       
        }

        public static void S1F17_SendCommand( )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Request Online");
            SECS.Host.Host_Control.SendPrimary(message);
            //textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S1F18_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined  )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Online Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["OFLACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S2F13_SendCommand(  )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Equipment Constant Request");
            SECS.Host.Host_Control.SendPrimary(message);
            //textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S2F14_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined,ArrayList list_Data )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Equipment Constant Data");
            for(int i=0;i< list_Data.Count;i++)
            {
                m_Transaction.Secondary.RootItem.Items[i].AsU4 = Convert.ToUInt32(list_Data[i]);
            }
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }
        public static void S2F17_SendCommand( )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Date And Time Request");
            SECS.Host.Host_Control.SendPrimary(message);
            //textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S2F18_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string Time, bool m_IsDefined  )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Date And Time Data");
            m_Transaction.Secondary.RootItem.ItemsByName["TIME"].AsString = Time;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S2F24_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Trace Initialize Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["TIAACK"].AsString = m_SmlTag;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S2F30_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList ECID_list )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Equipment Constant Namelist");
            for(int i=0;i< ECID_list.Count;i++)
            {
                m_Transaction.Secondary.RootItem.Items[i].AsU4 = Convert.ToUInt32(ECID_list[i]);
            }
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S2F32_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Data and Time Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["TIACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }
        public static void S2F33_SendCommand( uint DATAID, uint RPTID, ArrayList SVID_list )
        {           
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Define Report");
            message.RootItem.Items[0].AsU4 = DATAID;
            message.RootItem.Items[1].Items[0].Items[0].AsU4 = RPTID;
            for (int i = 0; i < SVID_list.Count; i++)
            {
                message.RootItem.Items[1].Items[0].Items[1].Items[i].AsU4 =Convert.ToUInt32( SVID_list[i]);
            }

            SECS.Host.Host_Control.SendPrimary(message);
           // textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S2F34_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Define Report Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["DRACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S2F35_SendCommand(uint DATAID, ArrayList CEID_list, ArrayList RPTID_list )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Link Event Report");
            message.RootItem.Items[0].AsU4 = DATAID;
            for (int i = 0; i < CEID_list.Count; i++)
            {
                message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(CEID_list[i]);
                for (int j = 0; j < RPTID_list.Count; j++)
                {
                    message.RootItem.Items[1].Items[i].Items[1].Items[j].AsU4 = Convert.ToUInt32(RPTID_list[j]);
                }
            }

            SECS.Host.Host_Control.SendPrimary(message);
            //textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S2F36_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Link Event Report Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["LRACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }


        public static void S2F37_SendCommand(bool Enable, ArrayList CEID_list )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Enable/Disable Event Report");
            message.RootItem.Items[0].AsBoolean = Enable;
            for (int i = 0; i < CEID_list.Count; i++)
            {
                message.RootItem.Items[1].Items[i].AsU4 = Convert.ToUInt32(CEID_list[i]);

            }
            SECS.Host.Host_Control.SendPrimary(message);
            //textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S2F38_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Enable/Disable Event Report Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ERACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S2F42_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Host Command Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["HCACK"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S5F1_SendAlarm(byte ALCD, int ALID ,string AlarmText)
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Alarm Report Send");
            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == ALID).ToList();
            //message.RootItem.ItemsByName["ALCD"].AsByte = ALCD;
            //message.RootItem.ItemsByName["ALID"].AsI4 = ALID;
            //message.RootItem.ItemsByName["ALTX"].AsString = filteredRecords[0].AlarmText;
            //SECS.Host.Host_Control.SendPrimary(message);
      
            message.RootItem.ItemsByName["ALCD"].AsByte = ALCD;
            message.RootItem.ItemsByName["ALID"].AsI4 = ALID;
            message.RootItem.ItemsByName["ALTX"].AsString = AlarmText;
            SECS.Host.Host_Control.SendPrimary(message);

            // textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S5F2_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Alarm Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC5"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S5F3_SendAlarm(uint ALED, uint ALID )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Enable/Disable Alarm Send");
            var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == ALID).ToList();
            message.RootItem.ItemsByName["ALED"].AsU4 = ALED;
            message.RootItem.ItemsByName["ALID"].AsU4 = ALID;
            SECS.Host.Host_Control.SendPrimary(message);
           // textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S5F4_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Enable/Disable Alarm Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC5"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S5F6_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList ALID_list )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("List Alarm Data");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC5"].AsByte = 0;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S5F8_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList ALID_list )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("List Enabled Alarm Data");
             
            for(int i=0;i< ALID_list.Count;i++)
            {
                string[] Data = ALID_list[i].ToString().Split(',');
                m_Transaction.Secondary.RootItem.Items[i].Items[0].AsByte = 0;
                m_Transaction.Secondary.RootItem.Items[i].Items[1].AsU4 = Convert.ToUInt32(Data[0]); 
                m_Transaction.Secondary.RootItem.Items[i].Items[2].AsString = Data[1].ToString();
            }
            
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S6F1_SendCommand( )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Trace Data Send");
            message.RootItem.ItemsByName["TRID"].AsU4 = 0;
            message.RootItem.ItemsByName["SMPLN"].AsU4 = 0;
            message.RootItem.ItemsByName["STIME"].AsU4 = 0;
            for (int i = 0; i < 10; i++)
            {
                
            }
            SECS.Host.Host_Control.SendPrimary(message);
        }

        public static void S6F2_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Trace Datan Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC6"].AsByte = 01;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S6F11_SendCommand(uint DATAID, uint CEID, uint RPTID, ArrayList SVID )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Event Report Send");
            message.RootItem.ItemsByName["DATAID"].AsU4 = DATAID;
            message.RootItem.ItemsByName["CEID"].AsU4 = CEID;
            //for (int i = 0; i < RPTID.Count; i++)
            //{
            //    string[] name = RPTID[i].ToString().Split('.');

            message.RootItem.Items[2].Items[0].Items[0].AsU4 = RPTID; //Convert.ToUInt32(name[0]);
                for (int S = 0; S < SVID.Count; S++)
                {
                    message.RootItem.Items[2].Items[0].Items[1].Items[S].AsString = Convert.ToString(SVID[S]);
                }
            //}
            SECS.Host.Host_Control.SendPrimary(message);
        }
        public static void S6F12_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Event Report Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC6"].AsByte =01;
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S6F16_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList RPTID)
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Event Report Data");
            m_Transaction.Secondary.RootItem.Items[0].AsU4 = 100000;
            m_Transaction.Secondary.RootItem.Items[1].AsU4 = 20110215;
            for (int i = 0; i < RPTID.Count; i++)
            {
                m_Transaction.Secondary.RootItem.Items[2].Items[i].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                m_Transaction.Secondary.RootItem.Items[2].Items[i].Items[1].Items[0].AsU4 = 100;
                m_Transaction.Secondary.RootItem.Items[2].Items[i].Items[1].Items[1].AsU4 = 101;
            }
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);

        }

        public static void S6F20_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined,ArrayList SVID_Data )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Individual Report Data");

            for (int i = 0; i < SVID_Data.Count; i++)
            {
                m_Transaction.Secondary.RootItem.Items[i].AsU4 = Convert.ToUInt32(SVID_Data[i]);
            }
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }
        public static void S7F19_SendCommand()
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.Host.Host_Control.LoadMessage("Current EPPD Request");
            SECS.Host.Host_Control.SendPrimary(message);
        }

        public static void S7F20_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList SVID_Data )
        {
            m_Transaction.Secondary = SECS.Host.Host_Control.LoadMessage("Current EPPD Data");

            for (int i = 0; i < SVID_Data.Count; i++)
            {
                m_Transaction.Secondary.RootItem.Items[i].AsU4 = Convert.ToUInt32(SVID_Data[i]);
            }
            SECS.Host.Host_Control.SendReplyByTransaction(m_Transaction);
        }
        #endregion


        #region  指令解析
        public static void MessageParse(KSecsTransaction m_Transaction, KSecsMessage m_Message,ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Transaction.Secondary.RootItem.ItemNumber;
            string Meg = "";
            for (int i = 0; i < Count; i++)
            {
                if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itList")
                {
                    if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber > 1)
                    {
                        for (int j = 0; j < m_Transaction.Secondary.RootItem.Items[i].ItemNumber; j++)
                        {
                            if (j == 0)
                            {
                                Meg = m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                            }
                            else if (j == m_Transaction.Secondary.RootItem.Items[i].ItemNumber - 1)
                            {
                                Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString;
                            }
                            else
                            {
                                Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                            }

                        }
                    }
                    else if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber == 0)
                    {
                        Meg = "Null";
                    }
                    else
                    {
                        Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                    }
                }
                else
                {
                    Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                }
                mesParse.Add(Meg);
                Meg = "";
            }
        }

        public static void MessageParseS1F3(KSecsTransaction m_Transaction, KSecsMessage m_Message, ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Message.RootItem.ItemNumber;
            string Meg = "";
            for (int i = 0; i < Count; i++)
            {
                if (m_Message.RootItem.Items[i].ItemType.ToString() == "itList")
                {
                    if (m_Message.RootItem.Items[i].ItemNumber > 1)
                    {
                        for (int j = 0; j < m_Message.RootItem.Items[i].ItemNumber; j++)
                        {
                            if (j == 0)
                            {
                                Meg = m_Message.RootItem.Items[i].Items[j].AsString + ",";
                            }
                            else if (j == m_Message.RootItem.Items[i].ItemNumber - 1)
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString;
                            }
                            else
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString + ",";
                            }

                        }
                    }
                    else if (m_Message.RootItem.Items[i].ItemNumber == 0)
                    {
                        Meg = "Null";
                    }
                    else
                    {
                        Meg = m_Message.RootItem.Items[i].AsString;
                    }
                }
                else
                {
                    Meg = m_Message.RootItem.Items[i].AsString;
                }
                //Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                mesParse.Add(Meg);
                Meg = "";
            }
        }

        public static void MessageParseS1F4(KSecsTransaction m_Transaction, KSecsMessage m_Message, ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Transaction.Secondary.RootItem.ItemNumber;
            
            if(Count>0)
            {
                string Meg = "";
                for (int i = 0; i < Count; i++)
                {
                    if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber != 0)
                    {
                        if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itList")
                        {
                            if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber > 1)
                            {
                                for (int j = 0; j < m_Transaction.Secondary.RootItem.Items[i].ItemNumber; j++)
                                {
                                    if (j == 0)
                                    {
                                        Meg = m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                                    }
                                    else if (j == m_Transaction.Secondary.RootItem.Items[i].ItemNumber - 1)
                                    {
                                        Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString;
                                    }
                                    else
                                    {
                                        Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                                    }

                                }
                            }
                            else if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber == 0)
                            {
                                Meg = "Null";
                            }
                            else
                            {
                                Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                            }
                        }
                        else
                        {
                            if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary")
                            {
                                if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true)
                                {
                                    Meg = "True";
                                }
                                else
                                {
                                    Meg = "False";
                                }
                            }
                            else
                            {
                                Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                            }

                        }
                    }
                    else
                    {
                        Meg = "Null";
                        MessageBox.Show("SVID不存在！");
                    }
                    mesParse.Add(Meg);
                    Meg = "";
                }
            }
            else
            {
                MessageBox.Show("Unable to respond");//无法做出响应
            }           
        }

        public static void MessageParseRecv(KSecsTransaction m_Transaction, KSecsMessage m_Message, ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Message.RootItem.ItemNumber;
            string Meg = "";
            for (int i = 0; i < Count; i++)
            {
                if (m_Message.RootItem.Items[i].ItemType.ToString() == "itList")
                {
                    if (m_Message.RootItem.Items[i].ItemNumber > 1)
                    {
                        for (int j = 0; j < m_Message.RootItem.Items[i].ItemNumber; j++)
                        {
                            if (j == 0)
                            {
                                Meg = m_Message.RootItem.Items[i].Items[j].AsString + ",";
                            }
                            else if (j == m_Message.RootItem.Items[i].ItemNumber - 1)
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString;
                            }
                            else
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString + ",";
                            }

                        }
                    }
                    else if (m_Message.RootItem.Items[i].ItemNumber == 0)
                    {
                        Meg = "Null";
                    }
                    else
                    {
                        if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary")
                        {
                            if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true)
                            {
                                Meg = "True";
                            }
                            else
                            {
                                Meg = "False";
                            }
                        }
                        else
                        {
                            Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                        }
                    }
                }
                else
                {
                    if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary")
                    {
                        if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true)
                        {
                            Meg = "True";
                        }
                        else
                        {
                            Meg = "False";
                        }
                    }
                    else
                    {
                        Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                    }
                }
                mesParse.Add(Meg);
                Meg = "";
            }
        }

        public static void MessageParseS1F14(KSecsTransaction m_Transaction, KSecsMessage m_Message, ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Message.RootItem.ItemNumber;
            string Meg = "";
            for (int i = 0; i < Count; i++)
            {
                if (m_Message.RootItem.Items[i].ItemType.ToString() == "itList")
                {
                    if (m_Message.RootItem.Items[i].ItemNumber > 1)
                    {
                        for (int j = 0; j < m_Message.RootItem.Items[i].ItemNumber; j++)
                        {
                            if (j == 0)
                            {
                                Meg = m_Message.RootItem.Items[i].Items[j].AsString + ",";
                            }
                            else if (j == m_Message.RootItem.Items[i].ItemNumber - 1)
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString;
                            }
                            else
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString + ",";
                            }

                        }
                    }
                    else if (m_Message.RootItem.Items[i].ItemNumber == 0)
                    {
                        Meg = "Null";
                    }
                    else
                    {
                        if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary")
                        {
                            if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true)
                            {
                                Meg = "True";
                            }
                            else
                            {
                                Meg = "False";
                            }
                        }
                        else
                        {
                            Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                        }
                    }
                }
                else
                {
                    if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary")
                    {
                        if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true)
                        {
                            Meg = "True";
                        }
                        else
                        {
                            Meg = "False";
                        }
                    }
                    else
                    {
                        Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                    }
                }
                mesParse.Add(Meg);
                Meg = "";
            }
        }
        #endregion


        #region     ID查找

        //EQ  CEID 查找
        public static void Search_CEID(List<CEID> List,ArrayList CEID_ArrayList,DataGridView dataGridView)
        {
            var filteredRecords = List.Where(r => r.ID == Convert.ToInt32(CEID_ArrayList[1])).ToList();
        }

      
        #endregion
    }
}
