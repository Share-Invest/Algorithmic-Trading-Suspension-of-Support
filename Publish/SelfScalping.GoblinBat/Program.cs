﻿using System;
using System.Windows.Forms;

namespace ShareInvest.SelfScalping
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SelfScalping());
        }
    }
}