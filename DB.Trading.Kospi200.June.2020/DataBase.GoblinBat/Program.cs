﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using ShareInvest.Catalog.XingAPI;
using ShareInvest.Message;
using ShareInvest.Strategy;
using ShareInvest.Verify;

namespace ShareInvest
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var secret = new Secret();
            var handle = GetConsoleWindow();
            var str = KeyDecoder.GetWindowsProductKeyFromRegistry();
            ShowWindow(handle, secret.Hide);

            if (secret.GetIdentify(str))
            {
                var registry = Registry.CurrentUser.OpenSubKey(new Secret().Path);
                var classfication = secret.GetPort(str).Equals((char)Port.Trading) && DateTime.Now.Hour > 4 && DateTime.Now.Hour < 6;
                var remaining = new Random(new Random().Next(0, Application.StartupPath.Length * secret.GetIdentify().Length)).Next(classfication ? 35 : 5, classfication ? 51 : 21);
                var path = Path.Combine(Application.StartupPath, secret.Indentify);
                Stack<Specify[]> stack = null;

                if (secret.GetDirectoryInfoExists(path))
                {
                    var initial = secret.GetPort(str);

                    if (registry.GetValue(secret.GoblinBat) == null || DateTime.Now.Date.Equals(new DateTime(2020, 4, 3)))
                    {
                        registry.Close();
                        registry = Registry.CurrentUser.OpenSubKey(new Secret().Path, true);
                        registry.SetValue(secret.GoblinBat, Array.Find(Directory.GetFiles(Application.StartupPath, "*.exe", SearchOption.AllDirectories), o => o.Contains(string.Concat(secret.GodSword, ".exe"))));
                    }
                    while (remaining > 0)
                        if (TimerBox.Show(new Secret(remaining--).RemainingTime, secret.GetIdentify(), MessageBoxButtons.OK, MessageBoxIcon.Information, 60000U).Equals(DialogResult.OK) && remaining == 0)
                            new Task(() =>
                            {
                                stack = new Strategy.Retrieve(str).SetInitialzeTheCode();

                                while (stack.Count > 0)
                                    new BackTesting(stack.Pop(), str);
                            }).Start();
                    while (TimerBox.Show(secret.StartProgress, secret.GetIdentify(), MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2, 30000U).Equals(DialogResult.Cancel))
                        if ((DateTime.Now.Hour == 8 || DateTime.Now.Hour == 17) && DateTime.Now.Minute > 45 && DateTime.Now.DayOfWeek.Equals(DayOfWeek.Saturday) == false && DateTime.Now.DayOfWeek.Equals(DayOfWeek.Sunday) == false)
                            break;

                    if (initial.Equals((char)126) == false)
                    {
                        if (initial.Equals((char)Port.Trading) && stack != null && stack.Count > 0)
                        {
                            stack.Clear();
                            GC.Collect();
                        }
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new GoblinBat(initial, secret));
                    }
                    else
                        new ExceptionMessage(str);
                }
                else
                {
                    ShowWindow(handle, secret.Show);
                    secret.SetIndentify(path, str);
                }
            }
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}