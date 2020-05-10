﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
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
            if (ShowWindow(GetConsoleWindow(), secret.Hide) && secret.GetIdentify(str))
            {
                var registry = Registry.CurrentUser.OpenSubKey(new Secret().Path);
                var classfication = secret.GetPort(str).Equals((char)Port.Trading) && (DateTime.Now.Hour == 15 && DateTime.Now.Minute >= 45 || DateTime.Now.Hour > 4 && DateTime.Now.Hour < 6);
                var remaining = secret.GetIsSever(str) ? secret.GetExternal(str) ? ran.Next(3, classfication ? 10 : 5) : 2 : 1;
                var path = Path.Combine(Application.StartupPath, secret.Indentify);
                var initial = secret.GetPort(str);
                var cts = new CancellationTokenSource();
                var retrieve = new Strategy.Retrieve(str);
                var count = secret.GetProcessorCount(str);
                var info = new Information(str);

                if (secret.GetDirectoryInfoExists(path))
                {
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
                                retrieve.SetInitialzeTheCode();
                                info.GetUserIdentity(initial);
                                var catalog = info.GetStatistics(count);
                                int length = 0;

                                if (secret.GetIsSever(str) == false)
                                {
                                    count = 0.5;
                                    info.SetInsertBaseStrategy(secret.strategy, secret.rate, secret.commission);
                                    var priority = info.GetBestStrategy();
                                    length = priority.Count;

                                    foreach (var best in priority)
                                        catalog.Insert(0, best);
                                }
                                var recent = retrieve.RecentDate;
                                var po = new ParallelOptions
                                {
                                    CancellationToken = cts.Token,
                                    MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * count)
                                };
                                try
                                {
                                    Parallel.ForEach(catalog, po, new Action<Models.ImitationGames>((number) =>
                                    {
                                        if (cts.IsCancellationRequested)
                                            po.CancellationToken.ThrowIfCancellationRequested();

                                        if (retrieve.GetDuplicateResults(recent, number) == false)
                                        {
                                            new BackTesting(number, str);
                                            Count++;

                                            if (length == Count)
                                                recent = retrieve.RecentDate;
                                        }
                                    }));
                                }
                                catch (OperationCanceledException ex)
                                {
                                    catalog.Clear();
                                    new ExceptionMessage(ex.StackTrace);
                                }
                                catch (Exception ex)
                                {
                                    new ExceptionMessage(ex.StackTrace, ex.TargetSite.Name);
                                }
                            }).Start();
                    while (TimerBox.Show(secret.StartProgress, secret.GetIdentify(), MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2, 30000U).Equals(DialogResult.Cancel))
                        if (secret.GetHoliday(DateTime.Now) == false && DateTime.Now.DayOfWeek.Equals(DayOfWeek.Saturday) == false && DateTime.Now.DayOfWeek.Equals(DayOfWeek.Sunday) == false)
                        {
                            if (initial.Equals((char)Port.Collecting) && (DateTime.Now.Hour == 8 || DateTime.Now.Hour == 17) && DateTime.Now.Minute > 35 && ran.Next(0, 10) == 9)
                                break;

                            if ((DateTime.Now.Hour == 8 || DateTime.Now.Hour == 17) && (DateTime.Now.Minute > 55 || DateTime.Now.Minute > 49 && ran.Next(0, 5) == 3))
                                break;
                        }
                    if (initial.Equals((char)126) == false)
                    {
                        if (initial.Equals((char)Port.Trading) && cts.IsCancellationRequested == false)
                        {
                            try
                            {
                                cts.Cancel();
                            }
                            catch (Exception ex)
                            {
                                new ExceptionMessage(ex.StackTrace);
                            }
                            finally
                            {
                                cts.Dispose();
                            }
                            var catalog = info.GetStatistics(count);
                            var recent = retrieve.RecentDate;
                            cts = new CancellationTokenSource();
                            new Task(() =>
                            {
                                var po = new ParallelOptions
                                {
                                    CancellationToken = cts.Token,
                                    MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * count * 0.4)
                                };
                                try
                                {
                                    Parallel.ForEach(catalog, po, new Action<Models.ImitationGames>((number) =>
                                    {
                                        if (cts.IsCancellationRequested)
                                            po.CancellationToken.ThrowIfCancellationRequested();

                                        if (retrieve.GetDuplicateResults(recent, number) == false)
                                        {
                                            new BackTesting(number, str);
                                            Count++;
                                        }
                                    }));
                                }
                                catch (OperationCanceledException ex)
                                {
                                    catalog.Clear();
                                    new ExceptionMessage(ex.StackTrace);
                                }
                                catch (Exception ex)
                                {
                                    new ExceptionMessage(ex.StackTrace, ex.TargetSite.Name);
                                }
                            }).Start();
                        }
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new GoblinBat(initial, secret, str, cts));
                    }
                    else
                        new ExceptionMessage(str);
                }
                else if (ShowWindow(GetConsoleWindow(), secret.Show) == false)
                {
                    while (secret.SetIndentify(path, str) == false)
                        Thread.Sleep(ran.Next(1, 100));

                    Process.Start("shutdown.exe", "-r");
                }
            }
        }
        static readonly Random ran = new Random(Guid.NewGuid().GetHashCode());
        static readonly Secret secret = new Secret();
        static readonly string str = KeyDecoder.GetWindowsProductKeyFromRegistry();
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        internal static uint Count
        {
            get; set;
        }
    }
}