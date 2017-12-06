using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LevelStrategy.Model;
using LevelStrategy.DAL;

namespace LevelStrategy
{
    public partial class FormAllSignal : Form
    {
        public DataReception data;

        public FormAllSignal(DataReception data)
        {
            InitializeComponent();
            this.data = data;
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.Empty)
                this.dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
            else
                this.dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                if (this.dataGridView1.Rows.Count > 1)
                {
                    string[] temp = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Split();
                    Bars bars = (Bars)data.listBars.FirstOrDefault(x => x.Name == temp[0] && x.TimeFrame == Int32.Parse(temp[1]));
                    SignalData signal = bars.listSignal.FirstOrDefault(x => x.SignalType == dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() && x.DateBpy2.ToShortTimeString() == dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString());
                    if (bars != null)
                    {
                        Chart window = new Chart(bars, signal, data);
                        window.Show();
                    }
                }
            }

        }
    }
}
