using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GemDriver
{
    public partial class DisplayMessage : Form
    {
        public DisplayMessage()
        {
            InitializeComponent();
        }

        public string Mes;
        public void MyFormLoad(string message)
        {
            string[] s = message.Split(',');
            for(int i=0;i< s.Length;i++)
            {
                if(i==0)
                {
                    if(s[i]=="")
                    {
                        Mes = Mes+ "null" + "\r\n";
                    }
                    {
                        Mes = Mes + s[i] + "\r\n";
                    }
                }
                else
                {
                    if (s[i] == "")
                    {
                        Mes = Mes + "null" + "\r\n";
                    }
                    {
                        Mes = Mes + s[i] + "\r\n";
                    }
                }
                
            }
        }
        private void DisplayMessage_Load(object sender, EventArgs e)
        {
            try
            { txt_message.Invoke(new Action(() => { txt_message.AppendText(DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + ":" +"\r\n" + Mes + "\r\n"); })); }
            catch (Exception ex)
            {

            }
        }
    }
}
