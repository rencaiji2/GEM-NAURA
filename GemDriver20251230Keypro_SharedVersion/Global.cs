using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper.Configuration.Attributes;
using System.Windows.Forms;

namespace GemDriver
{
    class Global
    {
        public static string WorkDir;

        public static IniLib.IniFile SystemParameterIni = new IniLib.IniFile();
        public static bool GUI_Refresh_Flag = false;
        public static bool S1F3_Refresh_Flag = false;
        public static bool RecipeName_Flag = false;

        public static bool TCP_IP_Status = false;
        //public static bool TCP_IPSV_Status = false;

        public static string EQ_Name = "";
        public static string EQ_SIF3_Time = "";
        public static bool EQ_Name_Status = false; //false:XKL

        public static string Time;                    // Global.Time = "yyMMddHHmmss";   Global.Time = "yyyyMMddHHmmssss";  Global.Time = "yyyy-MM-dd'T'HH:mm:ssXXX";
        public static uint DATAID = 11000;

        public static bool Recipe = false;

        public static ArrayList SendRGA_SV = new ArrayList();

        public static string[] EQ_EVID_CH1 = new string[3];

        public static string Log_Path;

        public static object ObjRefreshThread = new object();//RefreshThread线程互锁
        public static Queue<string> Queue_Tcp = new Queue<string>();

        public static int RPTID_Count = 0;

        public static string MDLN = "0";          //机台型号
        public static string SOFTREV = "0";       //软件版本号
        //public static KSecsWrapperDotNet.KSecsWrapper Equipment_Control = new KSecsWrapperDotNet.KSecsWrapper();
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

        public class RGA_TCP
        {
            [Name("SN")]
            public int SN { get; set; }
            [Name("ID")]
            public string ID { get; set; }

            [Name("Description")]
            public string Description { get; set; }

            [Name("Value")]
            public string Value { get; set; }

            [Name("Status")]
            public string Status { get; set; }
        }
        public class SVID
        {
            [Name("ID")]
            public int ID { get; set; }
            [Name("Name")]
            public string Name { get; set; }

            [Name("Format")]
            public string Format { get; set; }

            [Name("Description")]
            public string Description { get; set; }

            [Name("Value")]
            public string Value { get; set; }

            [Name("Unit")]
            public string Unit { get; set; }
            [Name("Range")]
            public string Range { get; set; }
            [Name("hostCollectionFrequency")]
            public string hostCollectionFrequency { get; set; }
        }
        public class CEID
        {
            [Name("ID")]
            public int ID { get; set; }
            [Name("Name")]
            public string Name { get; set; }
            [Name("Description")]
            public string Description { get; set; }

            [Name("Status")]
            public string Status { get; set; }

            [Name("LinkedItemID")]
            public string LinkedItemID { get; set; }

            [Name("Event")]
            public string Event { get; set; }
        }

        //EQ报告定义
        public class RPTID
        {
            [Name("ID")]
            public int ID { get; set; }
        }


     
        public class RPTID_SVID
        {
            [Name("ID")]
            public int ID { get; set; }
        }
        #region  新凯来报告的定义，链接，使能
        public static string[] RecipeStartLinkedItemID = new string[9] { "5000001", "5000002", "5110004", "5110006", "5112001", "5114003", "5114016", "5114027", "5120001" };
        public static string[] RecipeEndLinkedItemID = new string[12] { "5000001", "5000002", "5110004", "5110006", "5112001", "5114003", "5114016", "5114027", "5114029","5114030","5114033","5120001" };
        public static string[] RecipeStepStartLinkedItemID = new string[12] { "5000001", "5000002", "5110003","5110004", "5110006", "5112001", "5114003", "5114016", "5114017", "5114018","5114027","5120001" };
        public static string[] RecipeStepEndLinkedItemID = new string[14] { "5000001", "5000002","5110003", "5110004", "5110006", "5112001", "5114003", "5114016", "5114017", "5114018","5114020","5114027","5114032","5120001" };

        public static string[] RecipeStartEventID = new string[10] { "2330000", "2330004", "2330008", "2330012", "2330016", "2330020", "2330500", "2330504", "2330508", "2330512" };
        public static string[] RecipeEndEventID = new string[10] { "2330001", "2330005", "2330009", "2330013", "2330017", "2330021", "2330501", "2330505", "2330509", "2330513" };
        public static string[] RecipeStepStartEventID = new string[10] { "2330002", "2330006", "2330010", "2330014", "2330018", "2330022", "2330502", "2330506", "2330510", "2330514" };
        public static string[] RecipeStepEndEventID = new string[10] { "2330003", "2330007", "2330011", "2330015", "2330019", "2330023", "2330503", "2330507", "2330511", "2330515" };

        public static string[] RecipeStartRPTID = new string[10] { "700100", "700101", "700102", "700103", "700104", "700105", "700106", "700107", "700108", "700109" };
        public static string[] RecipeEndRPTID = new string[10] { "700200", "700201", "700202", "700203", "700204", "700205", "700206", "700207", "700208", "700209" };
        public static string[] RecipeStepStartRPTID = new string[10] { "700300", "700301", "700302", "700303", "700304", "700305", "700306", "700307", "700308", "700309" };
        public static string[] RecipeStepEndRPTID = new string[10] { "700400", "700401", "700402", "700403", "700404", "700405", "700406", "700407", "700408", "700409" };
        #endregion

        #region #region  北方华创报告的定义，链接，使能
        public static string[] RecipepChamberEventID_HC;
        public static string[] RecipeSlotVlvEventID_HC = new string[16];
        public static string[] RPTID_EQ;

        //RecipeName,LotID,Slot,MaterialName,RcpStepCounter,MaterialSrcPort
        public static string[] RecipeChamber1_SVID_LinkedItemID_HC = new string[6] { "11004", "11002", "11263", "11003", "11006", "11262" };
        public static string[] RecipeChamber2_SVID_LinkedItemID_HC = new string[6] { "11042", "11040", "11265", "11041", "11044", "11264" };
        public static string[] RecipeChamber5_SVID_LinkedItemID_HC = new string[6] { "11080", "11078", "11267", "11079", "11082" , "11266" };
        public static string[] RecipeChamber6_SVID_LinkedItemID_HC = new string[6] { "11113", "11111", "11269", "11112", "11115", "11268" };
        public static string[] RecipeChamberA_SVID_LinkedItemID_HC = new string[6] { "11146", "11144", "11271", "11145", "11148", "11270" };
        public static string[] RecipeChamberB_SVID_LinkedItemID_HC = new string[6] { "11155", "11153", "11273", "11154", "11157", "11272" };
        public static string[] RecipeChamberC_SVID_LinkedItemID_HC = new string[6] { "11164", "11162", "11275", "11163", "11166", "11274" };
        public static string[] RecipeChamberD_SVID_LinkedItemID_HC = new string[6] { "11183", "11181", "11277", "11182", "11185", "11276" };
        public static string[] RecipeChamberE_SVID_LinkedItemID_HC = new string[6] { "11202", "11200", "11279", "11201", "11204", "11278" };
        public static string[] RecipeChamberF_SVID_LinkedItemID_HC = new string[6] { "11224", "11222", "11281", "11223", "11226", "11280" };

        public static string[] RecipeSlotVlv_SVID_LinkedItemID_HC = new string[16] { "11020", "11020", "11058", "11058", "11096", "11096", "11129", "11129", "11171", "11171","11190","11190","11434","11434","11435","11435"};

        //public static string[] RecipeChamberRPTID_HC ;
        public static string[] RecipeSlotVlvRPTID_HC = new string[16];      
        #endregion

        public static ArrayList RGA_RPTID_CEIDlist = new ArrayList();
        public static bool[] RGA_RPTID_CEID_bool=new bool[1];
        public static ArrayList RGA_RPTID_list=new ArrayList();
        public static ArrayList RGA_RPTID_SVID_list = new ArrayList();

        public static bool[] Gem_ALID_Status;
        public static bool[] RGA_CEID_Status;
        public static List<ALID> Gem_ALID_List;
        public static List<SVID> Gem_SVID_List_CH1;
        public static List<SVID> Gem_SVID_List_CH2;
        public static List<SVID> Gem_SVID_List_CH3;
        public static List<SVID> Gem_SVID_List_CH4;
        public static List<SVID> Gem_SVID_List_CH5;
        public static List<SVID> Gem_SVID_List_CH6;
        //public static List<SVID> Gem_SVID_List_CHA;
        //public static List<SVID> Gem_SVID_List_CHB;
        public static List<SVID> Gem_SVID_List_CHC;
        public static List<SVID> Gem_SVID_List_CHD;
        public static List<SVID> Gem_SVID_List_CHE;
        public static List<SVID> Gem_SVID_List_CHF;


        public static List<SVID> Gem_SVID_List_TM1;
        public static List<SVID> Gem_SVID_List_TM2;
        public static List<CEID> Gem_CEID_List;

        //通用版
        public static List<RPTID_SVID> Gem_RPTID_SVID_List;
        public static List<RPTID> Gem_RPTIID_SVID;
        public static List<ArrayList> Gem_RPTIID;

        public static string[] TCP_Messagelist;
        public static string[] TCP_MessagelistSV;
        public static string[] TCP_Message;
        public static string[] TCP_MessageSV;

        public static string[] RGA_SVID_CH1=new string[700];
        public static string[] RGA_SVID_CH2 = new string[700];
        public static string[] RGA_SVID_CH3 = new string[700];
        public static string[] RGA_SVID_CH4 = new string[700];
        public static string[] RGA_SVID_CH5 = new string[700];
        public static string[] RGA_SVID_CH6 = new string[700];
        public static string[] RGA_SVID_CHC = new string[700];
        public static string[] RGA_SVID_CHD = new string[700];
        public static string[] RGA_SVID_CHE = new string[700];
        public static string[] RGA_SVID_CHF = new string[700];
        public static string[] RGA_SVID_TM1 = new string[700];
        public static string[] RGA_SVID_TM2 = new string[700];

        public static string[] RGA_CEID=new string[240];
        public static string[] RGA_ALID=new string[612];

        public static string[] SECS_RGA_EQ = new string[135];
        public static string[] SECS_RGA = new string[51];
        public static List<string> SECS_RGA_SV;
        public static string[] EQ_CEID_sting=new string[96];


        public class MapRPT
        {
            public bool isDefind = false;
            public string RPTID = "";
            public string CEID = "";
            public ArrayList SVID_LIST = new ArrayList();
        }
        //public static string[] EQ_SVID_string = new string[71];

        // public static string[] EQ_SVID = new string[510];
        //public class EQ_SVID
        //{
        //    [Name("ID")]
        //    public int ID { get; set; }
        //    [Name("Name")]
        //    public string Name { get; set; }

        //    [Name("Unit")]
        //    public string Unit { get; set; }
        //}

        //public static List<EQ_SVID> EQ_SVID_List;
        //public static List<RGA_TCP> RGA_TCP_List;
        public static void  Read_SVID(string path_csv,string CH)
        {
            // var file = Global.WorkDir + "Parameter/EQ_SVID.csv";
            var file = Global.WorkDir + path_csv;
            try
            {
                using (var reader = new System.IO.StreamReader(file))
                {
                    var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                    config.HasHeaderRecord = true;//有頭部
                    using (var csv = new CsvHelper.CsvReader(reader, config))
                    {
                        if (CH == "CH1"){
                            Gem_SVID_List_CH1 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CH1)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }else if (CH == "CH2"){
                            Gem_SVID_List_CH2 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CH2)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }else if (CH == "CH3"){
                            Gem_SVID_List_CH3 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CH3)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }else if (CH == "CH4"){
                            Gem_SVID_List_CH4 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CH4)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }else if (CH == "CH5"){
                            Gem_SVID_List_CH5 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CH5)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }else if (CH == "CH6"){
                            Gem_SVID_List_CH6 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CH6)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }else if (CH == "CHC"){
                            Gem_SVID_List_CHC = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CHC)
                            {
                                //dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }
                        else if (CH == "CHD")
                        {
                            Gem_SVID_List_CHD = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CHD)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }
                        else if (CH == "CHE")
                        {
                            Gem_SVID_List_CHE = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CHE)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //         record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }
                        else if (CH == "CHF")
                        {
                            Gem_SVID_List_CHF = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_CHF)
                            {
                                // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //        record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }
                        else if (CH == "TM1")
                        {
                            Gem_SVID_List_TM1 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_TM1)
                            {
                                //  dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //         record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }
                        else if (CH == "TM2")
                        {
                            Gem_SVID_List_TM2 = csv.GetRecords<SVID>().ToList();
                            //快速查詢
                            //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();
                            foreach (var record in Gem_SVID_List_TM2)
                            {
                                //dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Format.ToString(), record.Description.ToString(), "0", record.Unit.ToString(),
                                //         record.Range.ToString(), record.hostCollectionFrequency.ToString());
                            }
                        }

                    }
                }
            }
            catch(Exception EX)
            {
                Console.WriteLine(EX.ToString());
            }
        }
        public static void Read_CEID( string path_csv)
        {
            // var file = Global.WorkDir + "Parameter/EQ_SVID.csv";
            var file = Global.WorkDir + path_csv;
            using (var reader = new System.IO.StreamReader(file))
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;//有頭部
                using (var csv = new CsvHelper.CsvReader(reader, config))
                {
                    Gem_CEID_List = csv.GetRecords<CEID>().ToList();
                    RGA_CEID_Status = new bool[Gem_CEID_List.Count];
                    //快速查詢
                    //var filteredRecords = Gem_CEID_List.Where(r => r.ID == 241245).ToList();

                    //foreach (var record in Gem_CEID_List)
                    //{
                    //   // dataGridView.Rows.Add(record.ID.ToString(), record.Name.ToString(), record.Description.ToString(), record.Status.ToString(), record.LinkedItemID.ToString());
                    //}
                }
            }
        }

        //public static void Read_RGA_TCP(string path_csv)
        //{
        //    // var file = Global.WorkDir + "Parameter/EQ_SVID.csv";
        //    var file = Global.WorkDir + path_csv;
        //    using (var reader = new System.IO.StreamReader(file))
        //    {
        //        var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        //        config.HasHeaderRecord = true;//有頭部
        //        using (var csv = new CsvHelper.CsvReader(reader, config))
        //        {
        //            RGA_TCP_List = csv.GetRecords<RGA_TCP>().ToList();
        //            //Global.SECS_RGA = new string[RGA_TCP_List.Count-1];

        //        }

        //        for(int i=0;i< 51/*RGA_TCP_List.Count-1*/;i++)
        //        {
        //            Global.SECS_RGA[i] = RGA_TCP_List[i].Value;
        //        }
        //    }
        //}
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

                    Gem_ALID_Status = new bool[Gem_ALID_List.Count];
                    //快速查詢
                    //var filteredRecords = Gem_ALID_List.Where(r => r.AlarmId == 1082785813).ToList();

                    foreach (var record in Gem_ALID_List)
                    {
                        if (record.Value=="FALSE")
                        {
                            //dataGridView.Rows.Add(record.AlarmId.ToString(), record.AlarmName.ToString(), record.AlarmText.ToString(), false);
                        }
                        else
                        {
                           // dataGridView.Rows.Add(record.AlarmId.ToString(), record.AlarmName.ToString(), record.AlarmText.ToString(), true);
                        }                       
                    }
                }
            }
        }

        public static void ReadRPTID_File(string path)
        {
            Global.RGA_RPTID_CEIDlist.Clear();
            string[] files = Directory.GetDirectories(path);
            if (RGA_RPTID_CEID_bool.Length != files.Length)
            {
                RGA_RPTID_CEID_bool = new bool[files.Length];
            }
            
            for (int i = 0; i < files.Length; i++)
            {
                //文件夹路径中获取文件夹名称存到数组
               Global.RGA_RPTID_CEIDlist.Add(Path.GetFileName(files[i]));

            }
        }

        public static void ReadRPTID_CEID_File(string path, ArrayList CEID)
        {
            for (int i = 0; i < 1; i++)
            {
                string directoryPath = path  ;


                string[] csvFiles = Directory.GetFiles(directoryPath, "*.csv");

                // 输出每个CSV文件的名称
                foreach (string file in csvFiles)
                {
                    //Console.WriteLine(Path.GetFileName(file));
                    Global.RGA_RPTID_list.Add( (Path.GetFileName(file)));
                    ReadPPID_SVID(csvFiles[0], Gem_SVID_List_CH1);
                }
               
            }        
        }

        /// <summary>
        /// RGA报警事件
        /// </summary>
        /// <param name="path_csv"></param>
        /// <param name="RGA_SVID"></param>
        public static void ReadPPID_SVID(string path_csv, List<SVID> RGA_SVID)
        {
            var file = path_csv;
            using (var reader = new System.IO.StreamReader(file))
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                config.HasHeaderRecord = true;//有頭部
                using (var csv = new CsvHelper.CsvReader(reader, config))
                {
                    Gem_RPTID_SVID_List = csv.GetRecords<RPTID_SVID>().ToList();
                    string CH = "CH1";
                    for (int i = 0; i < 12; i++)
                    {                        
                        switch(CH)
                        {
                            case "CH1":
                                {
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CH1.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CH2.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CH3.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CH4.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CH5.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CH6.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CHC.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CHD.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CHE.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_CHF.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
                                        }
                                        else
                                        {
                                            CH = "TM1";
                                        }
                                    }
                                    CH = "TM1";
                                    break;
                                }
                            case "TM1":
                                {
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_TM1.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
                                        }
                                        else
                                        {
                                            CH = "TM2";
                                        }
                                    }
                                    CH = "TM2";
                                    break;
                                }
                            case "TM2":
                                {
                                    for (int A = 0; A < Gem_RPTID_SVID_List.Count; A++)
                                    {
                                        var filteredRecords = Gem_SVID_List_TM2.Where(r => r.ID == Gem_RPTID_SVID_List[A].ID).ToList();
                                        if (filteredRecords.Count > 0)
                                        {
                                            Global.RGA_RPTID_SVID_list.Add(filteredRecords[0].Value.ToString());
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
                    
                }
            }
        }


        /// <summary>
        /// EQ定义报告
        /// </summary>
        /// <param name="path_csv"></param>
        public static void ReadPPID_SVID_New(string path_csv)
        {
            try
            {
                ArrayList arrayList = new ArrayList();
                var file = path_csv;
                using (var reader = new System.IO.StreamReader(file))
                {
                    var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                    config.HasHeaderRecord = true;//有頭部
                    using (var csv = new CsvHelper.CsvReader(reader, config))
                    {
                        Gem_RPTIID_SVID = csv.GetRecords<RPTID>().ToList();

                        foreach (var record in Gem_RPTIID_SVID)
                        {
                            //if (record.Value == "FALSE")
                            //{
                            //    //dataGridView.Rows.Add(record.AlarmId.ToString(), record.AlarmName.ToString(), record.AlarmText.ToString(), false);
                            //}
                            //else
                            //{
                            //    // dataGridView.Rows.Add(record.AlarmId.ToString(), record.AlarmName.ToString(), record.AlarmText.ToString(), true);
                            //}
                        }

                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public static void SendRGA_SVID(ArrayList arrayList)
        {
            for (int S = 0; S < arrayList.Count; S++)
            {
                Search_SVID(Global.Gem_SVID_List_CHE, Convert.ToInt32(arrayList[S]),SendRGA_SV);
            }
        }


        public static void Search_SVID(List<SVID> List, int ID, ArrayList Arraylist_Value)
        {

            var filteredRecords = List.Where(r => r.ID == Convert.ToInt32(ID)).ToList();

            if (filteredRecords.Count > 0)
            {
                Arraylist_Value.Add(filteredRecords[0].Value.ToString());
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

        public static void DisplayInfo(string str, bool isRed = false)//新消息置顶
        {
            lock (new object())
            {
                //richTextBox1.Invoke(new Action(() =>
                //{
                //    richTextBox1.SelectionStart = 0;
                //    if (isRed)
                //    {
                //        richTextBox1.SelectionColor = Color.Red;
                //    }
                //    else
                //    {
                //        richTextBox1.SelectionColor = Color.Black;
                //    }
                //    richTextBox1.SelectedText = DateTime.Now.ToString("HH/mm/ss") + ": " + str + "\r\n";

                //}));
                //save
                if (true)
                {
                    RunLog.Instance.SaveLog(str);
                }

            }
        }
    }

    public class CEID
    {
        public struct DataInfo
        {
            public int CEID ;
            public int RPTID;
            public System.Collections.ArrayList SVID;
        }
    }
}
