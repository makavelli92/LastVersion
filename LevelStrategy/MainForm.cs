using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LevelStrategy.Model;
using System.Runtime.InteropServices;
using LevelStrategy.DAL;

namespace LevelStrategy
{
    public partial class MainForm : Form
    {
        private static Mutex mtx;

        public  DataReception data;

        public static DataGridView grid;

        private bool cycleRead = true;
        private bool cycleWrite = true;

        private Task first;
        private Task two;

        public MainForm()
        {
            InitializeComponent();

            mtx = new Mutex();

            grid = this.dataGridView1;

            AddItemToCb();

            data = new DataReception();
            first = Task.Run(() => {
                                       data.Start(ref cycleRead);
            });
            two = Task.Run(() =>
            {
                data.CycleSetCommand(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, ref cycleWrite);
            });
        }

        
        private void AddItemToCb()
        {
            foreach (var item in Enum.GetValues(typeof(ClassCod)))
            {
                cbClass.Items.Add(item);
            }
            foreach (var item in Enum.GetValues(typeof(TimeFrame)))
            {
                cbTimeFrame.Items.Add(item);
            }
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            if (cbClass.Text != String.Empty && cbSecurity.Text != String.Empty && cbTimeFrame.Text != String.Empty && !this.data.listBars.Any(x => x.Name == cbSecurity.Text && x.TimeFrame == (int)Enum.GetValues(typeof(TimeFrame)).Cast<TimeFrame>().First(y => y.ToString() == cbTimeFrame.Text)))
            {
                TimeFrame frame = Enum.GetValues(typeof(TimeFrame)).Cast<TimeFrame>().First(x => x.ToString() == cbTimeFrame.Text);
                string classCod = cbClass.Text;
                string security = cbSecurity.Text;
                Task.Run(() =>
                {
                    data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, DataReception.GetCommandStringCb(classCod, security, frame), "GetCandle");
                });
                listSecurity.Text += cbSecurity.Text + "\n";
                listSecurity.AppendText(Environment.NewLine);
                cbClass.Text = String.Empty;
                cbSecurity.Text = String.Empty;
                cbTimeFrame.Text = String.Empty;
            }
        }
        private void cbClass_Leave(object sender, EventArgs e)
        {
            if (cbClass.Text != String.Empty)
            {
                if (cbClass.Text == "SPBFUT")
                {
                    cbSecurity.Items.Clear();
                    foreach (var item in Enum.GetValues(typeof(Futures)))
                    {
                        cbSecurity.Items.Add(item);
                    }
                }
                if (cbClass.Text == "TQBR")
                {
                    cbSecurity.Items.Clear();
                    foreach (var item in Enum.GetValues(typeof(Security)))
                    {
                        cbSecurity.Items.Add(item);
                    }
                }
             //   cbSecurity.Enabled = true;
            }
            else
            {
                cbSecurity.Items.Clear();
            }
        }
        public enum ClassCod
        {
            SPBFUT = 1,
            TQBR = 2
        }
        public enum Futures
        {
            GZZ7,
            SRZ7,
            EuZ7,
            GDZ7,
            RIZ7,
            SiZ7,
            BRF8
        }
        public enum Security
        {
            SBER,
            SBERP,
            GAZP,
            LKOH,
            MTSS,
            MGNT,
            MOEX,
            NVTK,
            NLMK,
            RASP,
            VTBR,
            RTKM,
            ROSN,
            AFLT,
            AKRN,
            AFKS,
            PHOR,
            GMKN,
            CHMF,
            SNGS,
            URKA,
            FEES,
            ALRS,
            APTK,
            YNDX,
            MTLRP,
            MAGN,
            BSPB,
            MTLR
        }
        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (data.listBars.Count > 0)
            {
                Bars temp = this.data.listBars.OfType<Bars>().FirstOrDefault(x => (x.Name + " " + x.TimeFrame) == (string)dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                if(temp != null)
                {
                    FormAllSignal form = new FormAllSignal(data);
                    form.Text = temp.Name;
                    form.Show();
                    foreach (SignalData i in temp.listSignal)
                    {
                        form.dataGridView1.Rows.Add(String.Format($"{temp.Name} {temp.TimeFrame}"), i.SignalType, String.Format($"{i.pointsBars[0]} - {i.DateBsy.Day}  {i.DateBsy.ToShortTimeString()}"), i.DateBpy1.ToShortTimeString(), i.DateBpy2.ToShortTimeString(), i.Level, i.Lyft, i.CancelSignal, i.TimeNow.ToShortTimeString());
                        form.dataGridView1.Rows[form.dataGridView1.RowCount - 1].MinimumHeight = 35;
                    }
                }
            }
        }
        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cycleRead = false;
            cycleWrite = false;
            while(!first.IsCompleted && !two.IsCompleted)
                Thread.Sleep(100);
        }

        private List<ApplicationItem> listItem;

        private void button2_Click(object sender, EventArgs e)
        {
            if (listItem == null)
            {
                listItem = new List<ApplicationItem>();
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.BRF8, TimeFrame.INTERVAL_M15));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.EuZ7, TimeFrame.INTERVAL_M15));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.GDZ7, TimeFrame.INTERVAL_M15));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.GZZ7, TimeFrame.INTERVAL_M5));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.RIZ7, TimeFrame.INTERVAL_M15));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.SRZ7, TimeFrame.INTERVAL_M15));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.SPBFUT, global::LevelStrategy.DAL.Futures.SiZ7, TimeFrame.INTERVAL_M15));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.TQBR, global::LevelStrategy.DAL.Security.SBER, TimeFrame.INTERVAL_M5));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.TQBR, global::LevelStrategy.DAL.Security.GAZP, TimeFrame.INTERVAL_M5));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.TQBR, global::LevelStrategy.DAL.Security.LKOH, TimeFrame.INTERVAL_M5));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.TQBR, global::LevelStrategy.DAL.Security.MOEX, TimeFrame.INTERVAL_M5));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.TQBR, global::LevelStrategy.DAL.Security.ROSN, TimeFrame.INTERVAL_M5));
                listItem.Add(new ApplicationItem(global::LevelStrategy.DAL.ClassCod.TQBR, global::LevelStrategy.DAL.Security.GMKN, TimeFrame.INTERVAL_M5));
                AddAllCb(listItem);
            }

        }

        private void AddAllCb(List<ApplicationItem> listApp)
        {
            foreach (ApplicationItem i in listApp)
            {
                cbClass.Text = i.classCod;
                cbSecurity.Text = i.security;
                cbTimeFrame.Text = i.timeFrame.ToString();
                this.button1_Click(this, EventArgs.Empty);
            }
        }
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int width, int height, uint flags);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            IntPtr calc = FindWindow(null, "FindLevel");
            if (calc != null)
            {
                if (checkBox1.Checked)
                    SetWindowPos(calc, (IntPtr)(-1), 0, 0, 0, 0, 0x0003);
                else
                    SetWindowPos(calc, (IntPtr)(-2), 0, 0, 0, 0, 0x0003);
            }
        }

        private SignalItem itemSign;

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.ColumnIndex == 0)
            {
                itemSign = new SignalItem();
                int temp = 0;
                foreach (DataGridViewCell cell in this.dataGridView1.Rows[e.RowIndex].Cells)
                {
                    itemSign.dataGridView1.Rows[0].Cells[temp++].Value = cell.Value;
                }
                itemSign.Show();
            }
            if (e.ColumnIndex == 1)
            {
                if(this.dataGridView1.Rows.Count > 1)
                {
                    string[] temp = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Split();
                    Bars bars = (Bars)data.listBars.FirstOrDefault(x => x.Name == temp[0] && x.TimeFrame == Int32.Parse(temp[1]));
                    if(bars.listSignal != null && bars.listSignal.Count > 0)
                    {
                        SignalData signal = bars.listSignal.Last();
                        if (bars != null)
                        {
                            Chart window = new Chart(bars, signal, data);
                            window.Show();
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count > 0 && this.dataGridView1.SelectedRows[0].Cells[0].Value != null)
            {
                mtx.WaitOne();

                string[] temp = this.dataGridView1.SelectedRows[0].Cells[0].Value.ToString().Split();
                Data tmp = this.data.listBars.FirstOrDefault(x => x.Name == temp[0] && x.TimeFrame.ToString() == temp[1]);
                data.listBars.Remove(tmp);
                this.dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);

                mtx.ReleaseMutex();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            mtx.WaitOne();
            for (int i = 0; i < this.dataGridView1.SelectedRows.Count; i++)
            {
                string[] temp = this.dataGridView1.SelectedRows[i].Cells[0].Value.ToString().Split();

                Bars tmp = (Bars)this.data.listBars.FirstOrDefault(x => x.Name == temp[0] && x.TimeFrame.ToString() == temp[1]);

                tmp.fractalPeriod = Int32.Parse(textBox1.Text);
            }
            mtx.ReleaseMutex();
            textBox1.Text = String.Empty;
        }
    }
}

