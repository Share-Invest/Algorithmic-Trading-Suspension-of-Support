﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ShareInvest.Kospi200HedgeVersion
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string[] temp;
            bool operation = false;

            foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "*.lnk", SearchOption.TopDirectoryOnly))
            {
                temp = file.Split('\\');
                temp = temp[temp.Length - 1].Split('.');

                if (temp[0].Equals("Kospi200"))
                    operation = true;
            }
            if (operation)
            {
                while (DateTime.Now.Hour < 8)
                    Thread.Sleep(60000);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Kospi200());

                return;
            }
            MessageBox.Show("You didn't Agree to the 'GoblinBat' program\nTerms and Conditions.\n\nAccept the Terms and Conditions\non the Installation Screen.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Process.Start(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Install.exe"));
            Application.Exit();
        }
    }
}