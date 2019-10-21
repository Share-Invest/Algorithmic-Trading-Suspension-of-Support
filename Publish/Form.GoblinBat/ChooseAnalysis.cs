﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ShareInvest.AutoMessageBox;
using ShareInvest.Communicate;
using ShareInvest.Const;
using ShareInvest.EventHandler;
using ShareInvest.FindByName;

namespace ShareInvest.Control
{
    public partial class ChooseAnalysis : UserControl
    {
        public string TempText
        {
            get; private set;
        }
        public ChooseAnalysis()
        {
            InitializeComponent();
            file = SetSecret().Split('^');
            cumulative.Text = string.Concat(cumulative.Text, " [", file[0], "]");
            recent.Text = string.Concat(recent.Text, " [", file[1], "]");

            foreach (IMakeUp val in mp)
                for (Count = 0; Count < 14; Count++)
                {
                    string.Concat(val.FindByName, Count).FindByName<Button>(this).Click += ButtonClick;

                    if (string.Concat(val.FindByName, Count).FindByName<Button>(this).Text.Contains("-"))
                    {
                        string.Concat(val.FindByName, Count).FindByName<Button>(this).ForeColor = Color.DeepSkyBlue;
                        string.Concat(val.FindByName, Count).FindByName<Button>(this).Text = string.Concat(val.FindByName, Count).FindByName<Button>(this).Text.Replace("-", string.Empty);

                        continue;
                    }
                    string.Concat(val.FindByName, Count).FindByName<Button>(this).ForeColor = Color.Maroon;
                }
            Size = new Size(350, 150);
        }
        private void ButtonClick(object sender, EventArgs e)
        {
            arr = sender.ToString().Split(':');
            arr = arr[1].Split('￦');
            TempText = arr[0].Trim();
            SendQuit?.Invoke(this, new ForceQuit(1));
        }
        private string SetSecret()
        {
            foreach (string val in Directory.GetFiles(string.Concat(Environment.CurrentDirectory, @"\Statistics\"), "*.csv", SearchOption.AllDirectories))
            {
                arr = val.Split('\\');
                arr = arr[arr.Length - 1].Split('.');
                int count = int.Parse(arr[0]);

                if (count > RecentDate)
                    RecentDate = count;
            }
            try
            {
                using (StreamReader sr = new StreamReader(string.Concat(Environment.CurrentDirectory, @"\Statistics\", RecentDate.ToString(), ".csv")))
                {
                    List<string> list = new List<string>(256);

                    if (sr != null)
                        while (sr.EndOfStream == false)
                            list.Add(sr.ReadLine());

                    foreach (IMakeUp val in mp)
                        MakeUp(list, val);

                    return string.Concat(list[1].Substring(0, 8), "^", list[list.Count - 2].Substring(0, 8));
                }
            }
            catch (Exception ex)
            {
                Box.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", 3750);
                Environment.Exit(0);
            }
            return string.Empty;
        }
        private void MakeUp(List<string> list, IMakeUp ip)
        {
            file = list[0].Split(',');
            Count = ip.Turn;
            count = new long[file.Length - 1];
            int i;

            do
            {
                arr = list[list.Count - Count].Split(',');
                Count--;

                for (i = 0; i < count.Length; i++)
                    count[i] += long.Parse(arr[i + 1]);
            }
            while (Count > 1);

            for (i = 0; i < count.Length; i++)
                ip.DescendingSort[file[i + 1]] = count[i];

            i = 0;

            foreach (KeyValuePair<string, long> kv in ip.DescendingSort.OrderByDescending(o => o.Value))
            {
                if (i > 13)
                    break;

                string.Concat(ip.FindByName, i++).FindByName<Button>(this).Text = string.Concat(kv.Key.Replace('^', '.'), " ￦", kv.Value.ToString("N0"));
            }
        }
        private int RecentDate
        {
            get; set;
        }
        private int Count
        {
            get; set;
        }
        private long[] count;
        private string[] arr, file;
        private readonly IMakeUp[] mp =
        {
            new MakeUpCumulative(),
            new MakeUpRecentDate(),
            new MakeUpWeekly(),
            new MakeUpBiweekly(),
            new MakeUpMonthly()
        };
        public event EventHandler<ForceQuit> SendQuit;
    }
}