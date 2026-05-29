using System;
using System.Threading;
using System.Windows.Forms;

namespace FocusBlocker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, "FocusBlocker_v2", out bool isNew);
            if (!isNew) { MessageBox.Show("FocusBlocker is already running."); return; }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.MainForm());
        }
    }
}
