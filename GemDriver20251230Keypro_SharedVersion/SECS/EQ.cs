using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using System.Windows.Forms;
using KSecsWrapperDotNet;

namespace GemDriver.SECS
{
    class EQ
    {
        public static KSecsWrapperDotNet.KSecsWrapper Equipment_Control = new KSecsWrapperDotNet.KSecsWrapper();

        public delegate void ChangeKSecsMessage(KSecsWrapperDotNet.KSecsMessage value);
        public static event ChangeKSecsMessage ChangeKSecsMessageEvent;


        public delegate void TransactionLogEvent(string value);
        public static event TransactionLogEvent EQ_TransactionLog;

        private static string Model_Number = "", Revision = "";
        public static string TransactionErrorMessage = "";

        #region  发送参数定义
        public static byte COMMACK = 0;
        public static string MDLN="0";          //机台型号
        public static string SOFTREV="0";       //软件版本号

        //public static int gS2F34_Count = 0;
        //public static int gS2F36_Count = 0;
        //public static int gS2F38_Count = 0;

        public static int gCount = 0;

        //public static bool gS1F1 = false;
        public static bool gStatusS1F3 = true;
        public static bool gStatusS6F11 = true;
        #endregion
        //public static List<ALID> Gem_ALID_List;
        //public static List<SVID> Gem_SVID_List;
        public static List<CEID> Gem_CEID_List;
        //public static List<RGA_TCP> RGA_TCP_List;
        public static List<Global.MapRPT> gRPTID_EQ =new List<Global.MapRPT> { };

        public static ArrayList gS7F20_Message = new ArrayList();
        public static bool SearchCEID { get; private set; }
        //public class ALID
        //{
        //    [Name("AlarmId")]
        //    public int AlarmId { get; set; }

        //    [Name("Level")]
        //    public string Level { get; set; }
        //    [Name("AlarmName")]
        //    public string AlarmName { get; set; }
        //    [Name("AlarmText")]
        //    public string AlarmText { get; set; }
        //}
        //public class SVID
        //{
        //    [Name("ID")]
        //    public int ID { get; set; }
        //    [Name("Name")]
        //    public string Name { get; set; }

        //    [Name("Format")]
        //    public string Format { get; set; }

        //    [Name("Description")]
        //    public string Description { get; set; }

        //    [Name("Value")]
        //    public string Value { get; set; }

        //    [Name("Unit")]
        //    public string Unit { get; set; }
        //    [Name("Range")]
        //    public string Range { get; set; }
        //    [Name("hostCollectionFrequency")]
        //    public string hostCollectionFrequency { get; set; }
        //}
        public class CEID
        {
            [Name("RPTID")]
            public int RPTID { get; set; }
            [Name("ID")]
            public int ID { get; set; }
            [Name("Name")]
            public string Name { get; set; }
            [Name("Description")]
            public string Description { get; set; }
            //Index
            [Name("Index")]
            public string Index { get; set; }

            [Name("State")]
            public string State { get; set; }
            [Name("SVID")]
            public string Message { get; set; }
        }

        

        //public class RGA_TCP
        //{
        //    [Name("SN")]
        //    public int SN { get; set; }
        //    [Name("ID")]
        //    public string ID { get; set; }

        //    [Name("Description")]
        //    public string Description { get; set; }

        //    [Name("Value")]
        //    public string Value { get; set; }

        //    [Name("Status")]
        //    public string Status { get; set; }
        //}
        public static void Host_Connect()
        {
            try
            {
                Equipment_Control.LoadFromIni(Global.WorkDir + "Parameter/EQ_Gem.ini", "SECS");
                Equipment_Control.SxmlFile = Global.WorkDir + "Parameter/GemEQ.sxml";
                Equipment_Control.OnSecondaryReceived += SECS.EQ.OnSecondaryReceived;
                Equipment_Control.OnPrimaryReceived += SECS.EQ.OnPrimaryReceived;
                Equipment_Control.OnTransactionLog += SECS.EQ.OnTransactionLog;
                Equipment_Control.OnTransactionError += SECS.EQ.OnTransactionError;
                Equipment_Control.Open();
            }
            catch(Exception EX)
            { }
        }

        public static void Host_Close()
        {
            Equipment_Control.Close();
        }

        //public static void Read_Alarn(DataGridView dataGridView)
        //{
        //    var file = Global.WorkDir + "Parameter/RGA_ALID.csv";
        //    using (var reader = new System.IO.StreamReader(file))
        //    {
        //        var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        //        config.HasHeaderRecord = true;//有頭部
        //        using (var csv = new CsvHelper.CsvReader(reader, config))
        //        {
        //             Gem_ALID_List = csv.GetRecords<ALID>().ToList();
        //        }
        //    }
        //}

        //public static void Read_SVID()
        //{
        //    var file = Global.WorkDir + "Parameter/EQ_SVID.csv";
        //    using (var reader = new System.IO.StreamReader(file))
        //    {
        //        var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        //        config.HasHeaderRecord = true;//有頭部
        //        using (var csv = new CsvHelper.CsvReader(reader, config))
        //        {
        //            Gem_SVID_List = csv.GetRecords<SVID>().ToList();
        //            //快速查詢
        //            var filteredRecords = Gem_SVID_List.Where(r => r.ID == 213600090).ToList();
        //        }
        //    }
        //}

        //public static void Read_RGA_TCP()
        //{
        //    var file = Global.WorkDir + "Parameter/RGA_TCP.csv";
        //    using (var reader = new System.IO.StreamReader(file))
        //    {
        //        var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        //        config.HasHeaderRecord = true;//有頭部
        //        using (var csv = new CsvHelper.CsvReader(reader, config))
        //        {
        //            RGA_TCP_List = csv.GetRecords<RGA_TCP>().ToList();
        //        }
        //    }
        //}

        /// <summary>
        /// 读取CEID文件夹中的csv文件名称，csv文件名称为机台事件ID
        /// </summary>
        public static void Read_CEID()
        {
            gRPTID_EQ.Clear();
            string path = Global.WorkDir+"CEID";
            string[] files = Directory.GetFiles(path, "*.csv");
            Global.RPTID_Count = files.Length;
            for (int i = 0; i < files.Length; i++){
                string name = files[i].Split('\\').Last();
                //Global.RecipeChamberRPTID_HC[i] = name.Split('.').FirstOrDefault();
                Global.MapRPT tmpMapRPT= new Global.MapRPT { };
                tmpMapRPT.CEID= name.Split('.').FirstOrDefault();
                
                string  file = files[i];
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (!File.Exists(file))
                    continue;
                string Line;
                Encoding encoder = Encoding.UTF8;
                using (StreamReader reader=new StreamReader(file, encoder)){
                    if ((Line = reader.ReadLine()) != null){
                        string[] tmpList = Line.Split(',');
                        tmpMapRPT.RPTID = tmpList[0];//meg = meg+ Line + ",";
                    }
                    //Global.RecipeChamberRPTID_HC[i] = Global.RecipeChamberRPTID_HC[i] + "," + meg;
                }
                
                string pathReport = Global.WorkDir + "RPTID\\" + tmpMapRPT.RPTID + ".csv";
                if (!File.Exists(pathReport))
                    continue;
                using (StreamReader reader = new StreamReader(pathReport, encoder))
                {//读取报告的SVID
                    while ((Line = reader.ReadLine()) != null) {
                        string[] tmpList = Line.Split(',');
                        tmpMapRPT.SVID_LIST.Add(tmpList[0]);
                    }
                }
                //SECS.EQ.S2F33_SendCommandNEW(Global.DATAID, MES[1], SVID, false);
                gRPTID_EQ.Add(tmpMapRPT);
            }

            var fileNAME = Global.WorkDir + "Parameter/EQ_CEID.csv";
            using (var reader = new System.IO.StreamReader(fileNAME)){
                var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;//有頭部
                using (var csv = new CsvHelper.CsvReader(reader, config)){
                    Gem_CEID_List = csv.GetRecords<CEID>().ToList();
                }
            }
        }

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
                                    if (m_Message .RootItem.ItemNumber== 0)
                                    {
                                        //S1F1 = true;
                                       // MessageBox.Show("The host sends a zero-length list to the equipment.");
                                    }
                                    okEQ_S1F1 = true;


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
                                    //    //if (!SetLocalTime(ref systemTime))
                                    //    //{ Console.WriteLine("無法設定系統時間"); }
                                    //}
                                    //UpDataResponseTimeEvent(responseTime);
                                }
                                break;
                            case 4:
                                {
                                    ArrayList SIF4_Message = new ArrayList();
                                    MessageParseS1F4(m_Transaction, m_Message, SIF4_Message, Form1.Form.dataGrid_EQ_SVID_Table);
                                    if (Global.TCP_IP_Status == true)
                                    {
                                        Form1.Form.send_TCPmegSV(SIF4_Message);
                                    }
                                }
                                break;
                            case 14:
                                {
                                    ArrayList SIF14_Message = new ArrayList();
                                    MessageParse(m_Transaction, m_Message, SIF14_Message);
                                    if (SIF14_Message[0].ToString() == "00")
                                    {
                                        okEQ_S1F13 = true;
                                        string[] Model_NumberS = SIF14_Message[1].ToString().Split(',');
                                        Model_Number = Model_NumberS[0];
                                        Revision= Model_NumberS[1];
                                    }    
                                }
                                break;
                            case 16:
                                {
                                    ArrayList SIF16_Message = new ArrayList();
                                    MessageParse(m_Transaction, m_Message, SIF16_Message);
                                }
                                break;
                            case 18:
                                {
                                    ArrayList SIF18_Message = new ArrayList();
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

                                }
                                break;
                            case 34:
                                {
                                    ArrayList S2F34_Message = new ArrayList();
                                    MessageParse(m_Transaction, m_Message, S2F34_Message);
                                    byte Value = m_Transaction.Secondary.RootItem.AsByte;
                                    if (Value == 00)
                                        okEQ_S2F33 = true;//++gS2F34_Count;
                                }
                                break;
                            case 36:
                                {
                                    //ArrayList S2F36_Message = new ArrayList();
                                    //MessageParse(m_Transaction, m_Message, S2F36_Message);
                                    byte Value = m_Transaction.Secondary.RootItem.AsByte;
                                    if (Value == 00)
                                        okEQ_S2F35 = true;
                                    //{
                                    //    Global.SECS_RGA[20] = "1";
                                    //}
                                    //else
                                    //{
                                    //    Global.SECS_RGA[20] = "0";
                                    //}
                                }
                                break;
                            case 38:
                                {
                                    //ArrayList S2F38_Message = new ArrayList();
                                    //MessageParse(m_Transaction, m_Message, S2F38_Message);
                                    byte Value = m_Transaction.Secondary.RootItem.AsByte;
                                    if (Value == 00)
                                        okEQ_S2F37 = true;
                                    //{
                                        
                                    //    Global.SECS_RGA[21] = "1";
                                    //    //Report = true;
                                    //}
                                    //else
                                    //{
                                    //    Global.SECS_RGA[21] = "0";
                                    //    gS2F38_Count++;
                                    //}
                                }
                                break;
                            case 42:
                                {
                                    byte Value = m_Transaction.Secondary.RootItem.Items[0].AsByte;
                                    if(Value==3)
                                    {
                                        Global.SECS_RGA[22] = "1";
                                        Global.DisplayInfo("S2F42 数据格式错误！。");
                                    }
                                    else
                                    {
                                        Global.SECS_RGA[22] = "0";
                                    }
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

                            case 20:
                                {
                                    if(m_Transaction.Secondary.RootItem.ItemNumber>0)
                                    {

                                    }
                                    else
                                    {
                                        MessageBox.Show("RPTID is not defined!");
                                    }
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
                                    //ArrayList S7F20_Message = new ArrayList();
                                    MessageParse(m_Transaction, m_Message, gS7F20_Message);
                                    Global.RecipeName_Flag = true;
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
                            case 13:
                                { S1F14_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                            case 15:
                                { S1F16_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
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
                                   // S2F14_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, data);
                                }
                                break;
                            case 17:
                                {
                                    string time = DateTime.Now.ToString("yyyyMMddHHmmssff");
                                    S2F18_SendCommand(m_Transaction, m_Message, time, m_IsDefined);
                                }
                                break;
                            case 23:
                                {
                                    //S2F24_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 29:
                                {
                                    ArrayList data = new ArrayList();
                                    data.Add("100");
                                    data.Add("101");
                                    data.Add("102");
                                    data.Add("103");
                                   // S2F30_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, data);
                                }
                                break;
                            case 31:
                                {
                                    string responseTime = m_Message.RootItem.AsString;
                                    if (responseTime.Length == 12)
                                    {
                                        responseTime = "20" + responseTime + "00";
                                    }

                                    //DateTimeformat = "yyyyMMddHHmmssff";
                                    ////string responseTime = m_Transaction.Secondary.RootItem.ItemsByName["ActualTime"].AsString;
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
                                    //    try
                                    //    {
                                    //        if (!SetLocalTime(ref systemTime))
                                    //        {
                                    //            S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                    //        }
                                    //        else
                                    //        { S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    S2F32_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                    //}
                                }
                                break;
                            case 33:
                                {
                                    //S2F34_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 35:
                                {
                                    //S2F36_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                            case 37:
                                {
                                   // S2F38_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                }
                                break;
                                //case 41:
                                //    {
                                //        S2F42_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined, Form1.Form.txtSendHost);
                                //    }
                                //    break;
                        }
                        break;
                    }
                case 5:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                { S5F2_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined); }
                                break;
                            case 13:
                                { }
                                break;
                            case 15:
                                { }
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
                                    gStatusS6F11 = true;//S6F11Result = true;
                                    Global.DisplayInfo("EQ上报S6F11.");
                                    //CEID_Status = true; 
                                    ArrayList S6F11_Message = new ArrayList();
                                    MessageParseRecv(m_Transaction, m_Message, S6F11_Message);
                                    SECS.EQ.Search_CEID_HC(/*RGA_TCP_List, */S6F11_Message);
                                    ArrayList message = new ArrayList();
                                    if (S6F11_Message.Count == 3)
                                    {
                                        message.Add(S6F11_Message[2].ToString());
                                    }
                                    else
                                    {
                                        message.Add("0");
                                    }
                                    //S6F11_Quest.Enqueue(S6F11_Message);                      
                                    S6F12_SendCommand(m_Transaction, m_Message, m_SmlTag, m_IsDefined);
                                    Global.DisplayInfo("RGA回复S6F12.");
                                }
                                break;
                            case 13:
                                { }
                                break;
                            case 15:
                                { }
                                break;
                        }
                    }
                    break;
                case 9:
                    {
                        switch (m_Message.Function)
                        {
                            case 1:
                                {
                                    //MessageBox.Show("Unrecognized Device ID!");
                                }
                                break;
                            case 3:
                                {
                                    //MessageBox.Show("Unrecognized Stream Type!");
                                }
                                break;
                            case 5:
                                {
                                   // MessageBox.Show("Unrecognized Function!");
                                }
                                break;
                            case 7:
                                {
                                   //// MessageBox.Show("Illegal Data!");
                                }
                                break;
                            case 9:
                                {
                                    //MessageBox.Show("Transaction Timer Timeout!");
                                }
                                break;
                            case 11:
                                {
                                   // MessageBox.Show("Data Too Long!");
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
            EQ_TransactionLog(sender);
        }

        private static void OnTransactionError(KSecsMessage m_Message, uint m_Ticket,int m_ErrorCode,string m_SmlTag,bool m_IsDefined)
        {
           switch(m_ErrorCode)
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
        public static bool okEQ_S1F1 = true;
        public static void S1F1_SendCommand( )
        {
            okEQ_S1F1 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Are You There Request"); 
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S1F2_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Online Data");
            m_Transaction.Secondary.RootItem.ItemsByName["MDLN"].AsString = Model_Number;
            m_Transaction.Secondary.RootItem.ItemsByName["SOFTREV"].AsString = Revision;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S1F3_SendCommand( string[] SVID )
        {
            gStatusS1F3 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Selected Equipment Status Request");
            for (int i = 0; i < SVID.Length; i++){
                message.RootItem.Items[i].AsU4= Convert.ToUInt32(SVID[i]);
            }
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S1F4_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined, ArrayList DataList )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Selected Equipment Status Data");
            for (int i = 0; i < DataList.Count; i++)
            {
                m_Transaction.Secondary.RootItem.Items[i].AsU4 = Convert.ToUInt32(DataList[i]);
            }
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }
        public static bool okEQ_S1F13 = true;
        public static void S1F13_SendCommand(/*string MDLN ,string SOFTREV */)
        {
            okEQ_S1F13 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Establish Communication Request");
            message.RootItem.ItemsByName["MDLN"].AsString = Global.MDLN;
            message.RootItem.ItemsByName["SOFTREV"].AsString = Global.SOFTREV;
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S1F14_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Establish Communication Request ACK");
            m_Transaction.Secondary.RootItem.ItemsByName["COMMACK"].AsByte = COMMACK;
            m_Transaction.Secondary.RootItem.ItemsByName["MDLN"].AsString = MDLN;
            m_Transaction.Secondary.RootItem.ItemsByName["SOFTREV"].AsString = SOFTREV;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S1F15_SendCommand()
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Request Offline");
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S1F16_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Offline Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["OFLACK"].AsByte = 0;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);          
        }

        public static void S1F17_SendCommand( )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Request Online");
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S1F18_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Online Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ONLACK"].AsU4 = 0;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }
        public static void S2F13_SendCommand()
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Equipment Constant Request");
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S2F17_SendCommand( )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Date And Time Request");
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S2F18_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Date And Time Data");
            m_Transaction.Secondary.RootItem.ItemsByName["Time"].AsString = m_SmlTag;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S2F33_SendCommand( uint DATAID, string[] RPTID,string[] SVID,bool Enable )
        {           
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Define Report");
            message.RootItem.Items[0].AsU4 = DATAID;
            do{
                if (Global.EQ_Name_Status == false){
                    if (Enable == false)
                        break;
                    for (int i = 0; i < RPTID.Length; i++){
                        message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                        for (int A = 0; A < SVID.Length; A++)
                            message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(SVID[A]);
                    }
                    break;
                }
                if (Enable == false){
                    for (int i = 0; i < RPTID.Length; i++){
                        //message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                        message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                        message.RootItem.Items[1].Items[i].AddItem();
                    }
                    break;
                }
                if (RPTID.Length != 60){
                    for (int i = 0; i < RPTID.Length; i++){
                        message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                        message.RootItem.Items[1].Items[i].Items[1].Items[0].AsU4 = Convert.ToUInt32(SVID[i]);
                    }
                    break;
                }
                for (int i = 0; i < RPTID.Length; i++){
                            message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                            if (i < 6){
                                for (int A = 0; A < Global.RecipeChamber1_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamber1_SVID_LinkedItemID_HC[A]);
                            }else if (i > 5 && i < 12){
                                for (int A = 0; A < Global.RecipeChamber2_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamber2_SVID_LinkedItemID_HC[A]);
                            }else if (i > 11 && i < 18){
                                for (int A = 0; A < Global.RecipeChamber5_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamber5_SVID_LinkedItemID_HC[A]);
                            }else if (i > 17 && i < 24){
                                for (int A = 0; A < Global.RecipeChamber6_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamber6_SVID_LinkedItemID_HC[A]);
                            }else if (i > 23 && i < 30){
                                for (int A = 0; A < Global.RecipeChamberA_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamberA_SVID_LinkedItemID_HC[A]);
                            }else if (i > 29 && i < 36){
                                for (int A = 0; A < Global.RecipeChamberB_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamberB_SVID_LinkedItemID_HC[A]);
                            }else if (i > 35 && i < 42){
                                for (int A = 0; A < Global.RecipeChamberC_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamberC_SVID_LinkedItemID_HC[A]);
                            }else if (i > 41 && i < 48){
                                for (int A = 0; A < Global.RecipeChamberD_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamberD_SVID_LinkedItemID_HC[A]);
                            }else if (i > 47 && i < 54){
                                for (int A = 0; A < Global.RecipeChamberE_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamberE_SVID_LinkedItemID_HC[A]);
                            }else if (i > 53 && i < 60){
                                for (int A = 0; A < Global.RecipeChamberF_SVID_LinkedItemID_HC.Length; A++)
                                    message.RootItem.Items[1].Items[i].Items[1].Items[A].AsU4 = Convert.ToUInt32(Global.RecipeChamberF_SVID_LinkedItemID_HC[A]);
                            }
                        }
            } while (false);
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static bool okEQ_S2F33 = true;
        public static void S2F33_SendCommandNEW(/*string RPTID, ArrayList SVID, */Global.MapRPT pMapRPT, bool Enable)
        {
            okEQ_S2F33 = false; //gS2F34_Count = 0;
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Define Report");
            message.RootItem.Items[0].AsU4 = Global.DATAID;// DATAID;
            //do { 
                if (Enable == false){
                    message.RootItem.Items[1].Items[0].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.RPTID);
                    message.RootItem.Items[1].Items[0].AddItem();
                //break;
            }
            else
            {
                message.RootItem.Items[1].Items[0].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.RPTID);
                for (int A = 0; A < pMapRPT.SVID_LIST.Count; A++)
                    message.RootItem.Items[1].Items[0].Items[1].Items[A].AsU4 = Convert.ToUInt32(pMapRPT.SVID_LIST[A]);
            }
                //if (Global.EQ_Name_Status == false){//XKL
                //    message.RootItem.Items[1].Items[0].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.RPTID);
                //    for (int A = 0; A < pMapRPT.SVID_LIST.Count; A++)
                //        message.RootItem.Items[1].Items[0].Items[1].Items[A].AsU4 = Convert.ToUInt32(pMapRPT.SVID_LIST[A]);
                //    break;
                //}
                //else
                //{
                //    message.RootItem.Items[1].Items[0].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.RPTID);
                //    for (int A = 0; A < pMapRPT.SVID_LIST.Count; A++)
                //        message.RootItem.Items[1].Items[0].Items[1].Items[A].AsU4 = Convert.ToUInt32(pMapRPT.SVID_LIST[A]);
                //}
            //} while (false);
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S2F35_SendCommand(uint DATAID, string[] CEID, string[] RPTID, bool Enable )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Link Event Report");
            message.RootItem.Items[0].AsU4 = DATAID;
            if (Enable == true){
                for (int i = 0; i < CEID.Length; i++)
                {
                    message.RootItem.Items[1].Items[i].Items[0].AsU4 = Convert.ToUInt32(CEID[i]);
                    message.RootItem.Items[1].Items[i].Items[1].Items[0].AsU4 = Convert.ToUInt32(RPTID[i]);
                }
            }else{ 
                message.RootItem.AddItem(); 
            }
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static bool okEQ_S2F35 = true;
        public static void S2F35_SendCommandNEW(Global.MapRPT pMapRPT, bool Enable)
        {
            okEQ_S2F35 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Link Event Report");
            message.RootItem.Items[0].AsU4 = Global.DATAID;// DATAID;
            if (Enable == true){
                message.RootItem.Items[1].Items[0].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.CEID);
                message.RootItem.Items[1].Items[0].Items[1].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.RPTID);
            }else{ 
                message.RootItem.AddItem(); 
            }
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S2F37_SendCommand(bool Enable,string[] CEID, bool Enable1 )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Enable/Disable Event Report");
            message.RootItem.Items[0].AsBoolean = Enable;
            for (int i = 0; i < CEID.Length; i++)
                message.RootItem.Items[1].Items[i].AsU4 = Convert.ToUInt32(CEID[i]);
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }
        public static bool okEQ_S2F37 = true;
        public static void S2F37_SendCommandNEW(Global.MapRPT pMapRPT, bool Enable)
        {
            okEQ_S2F37 = false;
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Enable/Disable Event Report");
            message.RootItem.Items[0].AsBoolean = Enable;
            message.RootItem.Items[1].Items[0].AsU4 = Convert.ToUInt32(pMapRPT.CEID);
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S2F41_SendCommand(string Status, string StationID, string chamber)
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("S2F41");
            message.RootItem.ItemsByName["Status"].AsString = Status;
            message.RootItem.ItemsByName["StationID"].AsString = StationID;
            message.RootItem.ItemsByName["chamber"].AsString = chamber;
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        //public static void S5F1_SendAlarm(int ALID )
        //{
        //    //KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Alarm Report Send");
        //    //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == ALID).ToList();
        //    //message.RootItem.ItemsByName["ALCD"].AsByte = 1;
        //    //message.RootItem.ItemsByName["ALID"].AsI4 = ALID;
        //    //message.RootItem.ItemsByName["ALTX"].AsString = filteredRecords[0].AlarmText;
        //    //SECS.EQ.Equipment_Control.SendPrimary(message);
        //}
        public static void S5F1_SendAlarm(byte ALCD, int ALID, string AlarmText)
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Alarm Report Send");
            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == ALID).ToList();
            //message.RootItem.ItemsByName["ALCD"].AsByte = ALCD;
            //message.RootItem.ItemsByName["ALID"].AsI4 = ALID;
            //message.RootItem.ItemsByName["ALTX"].AsString = filteredRecords[0].AlarmText;
            //SECS.Host.Host_Control.SendPrimary(message);

            message.RootItem.ItemsByName["ALCD"].AsByte = ALCD;
            message.RootItem.ItemsByName["ALID"].AsI4 = ALID;
            message.RootItem.ItemsByName["ALTX"].AsString = AlarmText;
            SECS.EQ.Equipment_Control.SendPrimary(message);

            // textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }
        public static void S5F2_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Alarm Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC5"].AsByte = 0;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S5F3_SendAlarm(byte ALED, uint ALID )
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Enable/Disable Alarm Send");
            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == ALID).ToList();
            message.RootItem.ItemsByName["ALED"].AsByte = ALED;
            //message.RootItem.ItemsByName["ALID"].AsU4 = ALID;
            SECS.EQ.Equipment_Control.SendPrimary(message);
        }

        public static void S6F12_SendCommand(KSecsTransaction m_Transaction, KSecsMessage m_Message, string m_SmlTag, bool m_IsDefined )
        {
            m_Transaction.Secondary = SECS.EQ.Equipment_Control.LoadMessage("Event Report Acknowledge");
            m_Transaction.Secondary.RootItem.ItemsByName["ACKC6"].AsByte =00;
            SECS.EQ.Equipment_Control.SendReplyByTransaction(m_Transaction);
        }

        public static void S6F19_SendCommand(uint RPTID)
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Individual Report Request");
            message.RootItem.ItemsByName["ALED"].AsU4 = RPTID;
            SECS.EQ.Equipment_Control.SendPrimary(message);
           // textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }

        public static void S7F19_SendCommand()
        {
            KSecsWrapperDotNet.KSecsMessage message = SECS.EQ.Equipment_Control.LoadMessage("Current EPPD Request");
            SECS.EQ.Equipment_Control.SendPrimary(message);
            //textBox.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" + message.Text + "\r\n");
        }
        #endregion


        #region  指令解析
        public static void MessageParse(KSecsTransaction m_Transaction, KSecsMessage m_Message,ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Transaction.Secondary.RootItem.ItemNumber;
            string Meg = "";
            for (int i = 0; i < Count; i++){
                if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itList"){
                    if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber > 1){
                        for (int j = 0; j < m_Transaction.Secondary.RootItem.Items[i].ItemNumber; j++){
                            if (j == 0){
                                Meg = m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                            }else if (j == m_Transaction.Secondary.RootItem.Items[i].ItemNumber - 1){
                                Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString;
                            }else{
                                Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                            }
                        }
                    }else if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber == 0){
                        Meg = "Null";
                    }else{
                        Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                    }
                }else{
                    Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                }
                mesParse.Add(Meg);
                Meg = "";
            }
        }

        public static void MessageParseS1F4(KSecsTransaction m_Transaction, KSecsMessage m_Message, ArrayList mesParse, DataGridView dataGridView)
        {
            mesParse.Clear();
            gStatusS1F3 = true;
            int Count = m_Transaction.Secondary.RootItem.ItemNumber;
            if (Count < 1) {
                MessageBox.Show("Unable to respond");//无法做出响应
                return;
            }
            int row = 0;
                string Meg = "";
                for (int i = 0; i < Count; i++){
                    if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber != 0){
                        if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itList"){
                            if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber > 1)
                            {
                                for (int j = 0; j < m_Transaction.Secondary.RootItem.Items[i].ItemNumber; j++){
                                    if (j == 0){
                                        Meg = m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                                    }else if (j == m_Transaction.Secondary.RootItem.Items[i].ItemNumber - 1){
                                        Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString;
                                    }else{
                                        Meg = Meg + m_Transaction.Secondary.RootItem.Items[i].Items[j].AsString + ",";
                                    }

                                }
                            }else if (m_Transaction.Secondary.RootItem.Items[i].ItemNumber == 0){
                                Meg = "Null";
                            }else{
                                Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                            }
                        }else{
                            if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary"){
                                if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true){
                                    Meg = "True";
                                }else{
                                    Meg = "False";
                                }
                            }else{
                                Meg = m_Transaction.Secondary.RootItem.Items[i].AsString;
                            }
                        }
                    }else{
                        Meg = "Null";
                    }
                    mesParse.Add(Meg);
                    row++;
                    Meg = "";
                }          
        }

        public static void MessageParseRecv(KSecsTransaction m_Transaction, KSecsMessage m_Message, ArrayList mesParse)
        {
            mesParse.Clear();
            int Count = m_Message.RootItem.ItemNumber;
            string Meg = "";
            try{
                Global.DisplayInfo("EQ上报S6F11开始解析.");
                for (int i = 0; i < Count; i++){
                    if (m_Message.RootItem.Items[i].ItemType.ToString() == "itList"){
                        if (m_Message.RootItem.Items[i].ItemNumber > 0){                        
                            for (int j = 0; j < m_Message.RootItem.Items[i].ItemNumber; j++){
                                if (m_Message.RootItem.Items[i].Items[j].ItemType.ToString() == "itU4"){
                                    Meg = m_Message.RootItem.Items[i].Items[j].AsU4.ToString() + ",";
                                }else if(m_Message.RootItem.Items[i].Items[j].ItemType.ToString() == "itU8"){
                                    Meg = m_Message.RootItem.Items[i].Items[j].AsU8.ToString() + ",";
                                }else if (m_Message.RootItem.Items[i].Items[j].ItemType.ToString() == "itList"){
                                    for (int S = 0; S < m_Message.RootItem.Items[i].Items[j].ItemNumber; S++){
                                        if(m_Message.RootItem.Items[i].Items[j].Items[S].ItemType.ToString() == "itList"){
                                            for (int a = 0; a < m_Message.RootItem.Items[i].Items[j].Items[S].ItemNumber; a++)
                                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].Items[S].Items[a].AsString.ToString() + ",";
                                        }else if(m_Message.RootItem.Items[i].Items[j].Items[S].ItemType.ToString() == "itU8"){
                                            Meg = Meg + m_Message.RootItem.Items[i].Items[j].Items[S].AsString.ToString() + ",";
                                        }else if (m_Message.RootItem.Items[i].Items[j].Items[S].ItemType.ToString() == "itU4"){
                                            Meg = Meg + m_Message.RootItem.Items[i].Items[j].Items[S].AsString.ToString() + ",";
                                        }
                                    }
                                }
                            }
                        }else if (m_Message.RootItem.Items[i].ItemNumber == 0){
                            Meg = "Null";
                        }else{
                            if (m_Transaction.Secondary.RootItem.Items[i].ItemType.ToString() == "itBinary"){
                                if (m_Transaction.Secondary.RootItem.Items[i].AsBoolean == true){
                                    Meg = "True";
                                }else{
                                    Meg = "False";
                                }
                            }else{
                                Meg = m_Transaction.Secondary.RootItem.Items[i].AsString.ToString();
                            }
                        }
                    }else if(m_Message.RootItem.Items[i].ItemType.ToString() == "itU4"){
                        Meg = m_Message.RootItem.Items[i].AsU4.ToString();
                    }else if (m_Message.RootItem.Items[i].ItemType.ToString() == "itU8"){
                        Meg = m_Message.RootItem.Items[i].AsU8.ToString();
                    }
                    mesParse.Add(Meg);
                    Meg = "";
                }
                Global.DisplayInfo("EQ上报S6F11解析完成.");
            }catch(Exception EX){
                Global.DisplayInfo("EQ上报S6F11解析失败.");
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
                                Meg = m_Message.RootItem.Items[i].Items[j].AsString.ToString() + ",";
                            }
                            else if (j == m_Message.RootItem.Items[i].ItemNumber - 1)
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString.ToString();
                            }
                            else
                            {
                                Meg = Meg + m_Message.RootItem.Items[i].Items[j].AsString.ToString() + ",";
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
                            Meg = m_Transaction.Secondary.RootItem.Items[i].AsString.ToString();
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
                        Meg = m_Transaction.Secondary.RootItem.Items[i].AsString.ToString();
                    }
                }
                mesParse.Add(Meg);
                Meg = "";
            }
        }
        #endregion

        #region     ID查找
        /// <summary>
        /// 北方华创S6F11内容填写到TCP发送中
        /// </summary>
        /// <param name="List"></param>
        /// <param name="CEID_ArrayList"></param>
        public static void Search_CEID_HC(/*List<RGA_TCP> List,*/ ArrayList CEID_ArrayList)
        {
            if (CEID_ArrayList.Count != 3){
                Global.DisplayInfo("EQ上报Search_CEID_HC 解析失败.");
                SearchCEID = false;
                return;
            }
            SearchCEID = true;
            Global.DisplayInfo("EQ上报Search_CEID_HC 解析完成.");
            if (Global.TCP_IP_Status == true ){
                var messageList = new List<String>(Global.SECS_RGA);
                string message= String.Join(",", messageList) + ","+ CEID_ArrayList[1].ToString()/*CEID*/ + ","
                    + CEID_ArrayList[2].ToString()/*RPTID,SV0,SV1...*/ + "\r\n";
                Global.Queue_Tcp.Enqueue(message);
            }  
        }


        public static int FindRowIndexByString(DataGridView dgv, string searchString)
        {
            for (int rowIndex = 0; rowIndex < dgv.Rows.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < dgv.Columns.Count; colIndex++)
                {
                    if (dgv.Rows[rowIndex].Cells[colIndex].Value != null &&
                        dgv.Rows[rowIndex].Cells[colIndex].Value.ToString().Contains(searchString))
                    {
                        return rowIndex;
                    }
                }
            }
            return -1; // 未找到返回-1
        }

        public static int FindRowIndexByString(List<CEID> ceid_list, string searchString)
        {

            for (int colIndex = 0; colIndex < ceid_list.Count; colIndex++)
            {
                if (ceid_list[colIndex].State != null &&
                    ceid_list[colIndex].ID.ToString().Contains(searchString))
                {
                    return colIndex;
                }
            }

            return -1; // 未找到返回-1
        }
        #endregion

        #region     PPID 操作


        #endregion
    }
}
