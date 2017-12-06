using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelStrategy
{
    public partial class SignalItem : Form
    {
        public SignalItem()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int width, int height, uint flags);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.Close();
        }

        private void SignalItem_Load(object sender, EventArgs e)
        {
            IntPtr calc = FindWindow(null, "SignalItem");
            if (calc != null)
                SetWindowPos(calc, (IntPtr)(-1), 0, 0, 0, 0, 0x0003);
        }

        private void SignalItem_MouseDown(object sender, MouseEventArgs e)
        {
            base.Capture = false;
            Message m = Message.Create(base.Handle, 0xa1, new IntPtr(2), IntPtr.Zero);
            this.WndProc(ref m);
        }
    }
}
