using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace CNCVision
{
    class CtrDisplay
    {
        private delegate void ShowTextEventHandler(Control control, string s);

        private delegate void ShowTextWithColorEventHandler(Control control, string s, Color color);

        private delegate void ClearRichTextBoxEventHandler(RichTextBox rtBox);

        private delegate void AddTextToRichTextBoxEventHandler(RichTextBox rtBox, string s, bool addTime);

        private delegate void AddTextWithColorToRichTextBoxEventHandler(RichTextBox rtBox, string s, Color color, bool addTime);

        private delegate void AddTextWithFontToRichTextBoxEventHandler(RichTextBox rtBox, string s, Font font, bool addTime);

        private delegate void AddTextWithColorFontToRichTextBoxEventHandler(RichTextBox rtBox, string s, Color color, Font font, bool addTime);

        private delegate void GetLinesFromRichTextBoxEventHandler(RichTextBox rtBox,out string[] stringLines);
       
        private delegate void ShowTextToolStripEventHandler(ToolStrip toolStrip, string text, Color color, int index);

        private delegate void ShowTextButtonTextEventHandler(Button buttonText, string text, Color color);

        private delegate void ShowTextOnDgvWithColorEventHandler(DataGridView dgv, int row, int cell, string s, Color c);

        private delegate void ShowTextOnDgvEventHandler(DataGridView dgv, int row, int cell, string s);

        private delegate void AddRowOnDgvEventHandler(DataGridView dgv, object[] objs);

        private delegate void AddRowOnDgvColorEventHandler(DataGridView dgv, object[] objs, Color c);

        private delegate void ChangeCellColorOnEventHandler(DataGridView dgv, int row, int cell, Color c);


        private delegate void ChangeCellAndColorEventHandler(DataGridView dgv, int row, int cell, string content, Color c);

        private delegate void ChangeCellContentEventHandler(DataGridView dgv, int row, int cell, string content);

        private delegate void ChangeRowWithColorEventHandler(DataGridView dgv, int row, object[] objs, Color c);

        private delegate void ChangeRowEventHandler(DataGridView dgv, int row, object[] objs);

        private delegate void ADDDataGridViewValueEventHandler(DataGridView dgv,int row, string cell0, string cell1, string cell2, string cell3, string cell4,string cell5);

        private delegate void AddDataGirdViewRowEventHandler(DataGridView dgv, int row);

        private delegate void AddNumUpDownValueEventHandler(NumericUpDown num, Decimal value);

        private delegate void AddProcessBarValueEventHandler(ProgressBar bar, int value);

        public static void AddProgress(ProgressBar bar, int value)
        {
            if (bar.InvokeRequired)
            {
                bar.Invoke(new AddProcessBarValueEventHandler(AddProgress), new object[] { bar, value });
                return;
            }
            bar.Value = value;
        }


        public static void AddNumeric(NumericUpDown num, decimal value)
        {
            if (num.InvokeRequired)
            {
                num.Invoke(new AddNumUpDownValueEventHandler(AddNumeric), new object[] { num, value });
                return;
            }
            num.Value = value;
        }
        public static void AddRow(DataGridView dgv, int row)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new AddDataGirdViewRowEventHandler(AddRow), new object[] { dgv, row });
                return;
            }
            dgv.Rows.Insert(row);

        }


        public  static void ShowText(DataGridView dgv,int row, string cell0, string cell1, string cell2, string cell3, string cell4,string cell5)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ADDDataGridViewValueEventHandler(ShowText),new object[]{dgv,row,cell0,cell1,cell2,cell3,cell4,cell5});
                    return;
            }
            dgv.Rows[row].Cells[0].Value=cell0;
            dgv.Rows[row].Cells[1].Value=cell1;
            dgv.Rows[row].Cells[2].Value=cell2;
            dgv.Rows[row].Cells[3].Value=cell3;
            dgv.Rows[row].Cells[4].Value=cell4;
            dgv.Rows[row].Cells[5].Value = cell5;

        }

        public static void ShowText(Button buttontext, string text, Color color)
        {
            if (buttontext.InvokeRequired)
            {
                buttontext.Invoke(new ShowTextButtonTextEventHandler(ShowText), new object[] { buttontext, text, color });
                return;
            }
            buttontext.BackColor = color;
            buttontext.Text = text;
        }
    

        public static void ShowText(ToolStrip toolStrip, string text, Color color, int index)
        {
            if (toolStrip.InvokeRequired)
            {
                toolStrip.Invoke(new ShowTextToolStripEventHandler(ShowText), new object[] { toolStrip, text, color, index });
                return;
            }
            
            toolStrip.Items[index].Text = text;
            toolStrip.Items[index].BackColor = color;
            
        }

        public static void ShowText(Control control, string s)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new ShowTextEventHandler(ShowText), new object[] { control, s });
                return;
            }
            control.Text = s;

        }

        public static void ShowText(Control control, string s, Color color)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new ShowTextWithColorEventHandler(ShowText), new object[] { control, s, color });
                return;
            }
            control.Text = s;
            control.BackColor = color;
        }

        public static void AddTextToRichTextBox(RichTextBox rtbox, string s, bool addTime)
        {
            if (rtbox.InvokeRequired)
            {
                rtbox.Invoke(new AddTextToRichTextBoxEventHandler(AddTextToRichTextBox), new object[] { rtbox, s, addTime });
                return;
            }

            if (addTime)
            {
                s = string.Format("【{0}】--{1}", DateTime.Now.ToString(), s);
            }

            rtbox.AppendText(s + "\n");

            rtbox.ScrollToCaret();
        }

        public static void AddTextToRichTextBox(RichTextBox rtbox, string s, Color color,bool addTime)
        {
            if (rtbox.InvokeRequired)
            {
                rtbox.Invoke(new AddTextWithColorToRichTextBoxEventHandler(AddTextToRichTextBox), new object[] { rtbox, s, color, addTime });
                return;
            }

            if (addTime)
            {
                s = string.Format("【{0}】--{1}", DateTime.Now.ToString(), s);
            }

            rtbox.SelectionColor = color;

            rtbox.AppendText(s + "\n");

            rtbox.ScrollToCaret();
        }

        public static void AddTextToRichTextBox(RichTextBox rtbox, string s, Font font,bool addTime)
        {
            if (rtbox.InvokeRequired)
            {
                rtbox.Invoke(new AddTextWithFontToRichTextBoxEventHandler(AddTextToRichTextBox), new object[] { rtbox, s, font,addTime });
                return;
            }

            if (addTime)
            {
                s = string.Format("【{0}】--{1}", DateTime.Now.ToString(), s);
            }

            rtbox.SelectionFont = font;

            rtbox.AppendText(s + "\n");

            rtbox.ScrollToCaret();
        }

        public static void AddTextToRichTextBox(RichTextBox rtbox, string s, Color color, Font font,bool addTime)
        {
            if (rtbox.InvokeRequired)
            {
                rtbox.Invoke(new AddTextWithColorFontToRichTextBoxEventHandler(AddTextToRichTextBox), new object[] { rtbox, s, color, font ,addTime});
                return;
            }

            if (addTime)
            {
                s = string.Format("【{0}】--{1}", DateTime.Now.ToString(), s);
            }

            rtbox.SelectionColor = color;

            rtbox.SelectionFont = font;

            rtbox.AppendText(s + "\n");

            rtbox.ScrollToCaret();
        }

        public static void ClearRichTextBox(RichTextBox rtbox)
        {
            if (rtbox.InvokeRequired)
            {
                rtbox.Invoke(new ClearRichTextBoxEventHandler(ClearRichTextBox), new object[] { rtbox });
                return;
            }

            rtbox.ResetText();
        }

        public static void GetAllText(RichTextBox rtbox ,out string[] stringLines)
        {
            if (rtbox.InvokeRequired)
            { 
                stringLines = null;
                rtbox.Invoke(new GetLinesFromRichTextBoxEventHandler(GetAllText), new object[] { rtbox });
                return;
            }

            stringLines = rtbox.Lines;
        }
        public static void ShowTextBackColor(DataGridView dgv, int row, int cell, string s, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ShowTextOnDgvWithColorEventHandler(ShowTextForeColor), new object[] { dgv, row, cell, s, c });
                return;
            }
            dgv.Rows[row].Cells[cell].Value = s;
            dgv.Rows[row].Cells[cell].Style.BackColor = c;
        }

        public static void ShowTextBackColor(DataGridView dgv, int row, int cell, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ShowTextOnDgvWithColorEventHandler(ShowTextForeColor), new object[] { dgv, row, cell, c });
                return;
            }
            //dgv.Rows[row].Cells[cell].Value = s;
            dgv.Rows[row].Cells[cell].Style.BackColor = c;
        }
        public static void ShowTextForeColor(DataGridView dgv, int row, int cell, string s, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ShowTextOnDgvWithColorEventHandler(ShowTextForeColor), new object[] { dgv, row, cell, s, c });
                return;
            }
            dgv.Rows[row].Cells[cell].Value = s;
            dgv.Rows[row].Cells[cell].Style.ForeColor = c;
        }

        public static void ShowText(DataGridView dgv, int row, int cell, string s)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ShowTextOnDgvEventHandler(ShowText), new object[] { dgv, row, cell, s, });
                return;
            }
            dgv.Rows[row].Cells[cell].Value = s;
        }

        public static void AddRowOnDgv(DataGridView dgv, object[] objs)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new AddRowOnDgvEventHandler(AddRowOnDgv), new object[] { dgv, objs });
                return;
            }
            dgv.Rows.Add();
            for (int i = 0; i < objs.Length; i++)
            {
                if (i < objs.Length)
                {
                    dgv.Rows[dgv.RowCount - 1].Cells[i].Value = objs[i].ToString();
                }
            }

            dgv.FirstDisplayedScrollingRowIndex = dgv.RowCount - 1;
        }

        public static void AddRowOnDgvColor(DataGridView dgv, object[] objs, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new AddRowOnDgvColorEventHandler(AddRowOnDgvColor), new object[] { dgv, objs, c });
                return;
            }
            dgv.Rows.Add();
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                if (i < objs.Length)
                {
                    dgv.Rows[dgv.RowCount - 1].Cells[i].Value = objs[i].ToString();
                    dgv.Rows[dgv.RowCount - 1].Cells[i].Style.ForeColor = c;
                }
            }
            dgv.FirstDisplayedScrollingRowIndex = dgv.RowCount - 1;
        }

        public static void ChangeCellColor(DataGridView dgv, int row, int cell, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ChangeCellColorOnEventHandler(ChangeCellColor), new object[] { dgv, row, cell, c });
                return;
            }
            dgv.Rows[row].Cells[cell].Style.ForeColor = c;
        }


        public static void ChangeCellContent(DataGridView dgv, int row, int cell, string content)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ChangeCellContentEventHandler(ChangeCellContent), new object[] { dgv, row, cell, content });
                return;
            }
            dgv.Rows[row].Cells[cell].Value = content;

        }

        public static void ChangeCellAndColor(DataGridView dgv, int row, int cell, string content, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ChangeCellAndColorEventHandler(ChangeCellAndColor), new object[] { dgv, row, cell, content, c });
                return;
            }
            dgv.Rows[row].Cells[cell].Value = content;
            dgv.Rows[row].Cells[cell].Style.BackColor = c;

        }

        public static void ChangeRow(DataGridView dgv, int row, object[] objs)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ChangeRowEventHandler(ChangeRow), new object[] { dgv, row, objs });
                return;
            }
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                if (i < objs.Length)
                {
                    dgv.Rows[row].Cells[i].Value = objs[i].ToString();
                }
            }
        }

        public static void ChangeRowColor(DataGridView dgv, int row, object[] objs, Color c)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new ChangeRowWithColorEventHandler(ChangeRowColor), new object[] { dgv, row, objs, c });
                return;
            }
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                if (i < objs.Length)
                {
                    dgv.Rows[row].Cells[i].Value = objs[i].ToString();
                }
            }
            dgv.Rows[row].DefaultCellStyle.BackColor = c;
        }
    }
}
