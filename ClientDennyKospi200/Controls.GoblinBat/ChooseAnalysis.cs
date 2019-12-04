﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ShareInvest.Const;
using ShareInvest.EventHandler;
using ShareInvest.FindByName;
using ShareInvest.Interface;
using ShareInvest.Log.Message;
using ShareInvest.Management;

namespace ShareInvest.Controls
{
    public partial class ChooseAnalysis : UserControl
    {
        public string Key
        {
            get; private set;
        }
        public ChooseAnalysis()
        {
            InitializeComponent();
            string[] assets = new Assets().ReadCSV().Split(',');
            Assets = long.Parse(assets[1]);
            CleanUp(SetSecret().Split('^'));
        }
        private void CleanUp(string[] file)
        {
            foreach (IMakeUp val in mp)
                for (Count = 0; Count < 14; Count++)
                {
                    string.Concat(val.FindByName, Count).FindByName<Button>(this).Click += ButtonClick;
                    string.Concat(val.FindByName, Count).FindByName<Button>(this).Font = new Font(string.Concat(val.FindByName, Count).FindByName<Button>(this).Font.Name, 10.25F, FontStyle.Regular);

                    if (string.Concat(val.FindByName, Count).FindByName<Button>(this).Text.Contains("-"))
                    {
                        string.Concat(val.FindByName, Count).FindByName<Button>(this).ForeColor = Color.DeepSkyBlue;
                        string.Concat(val.FindByName, Count).FindByName<Button>(this).Text = string.Concat(val.FindByName, Count).FindByName<Button>(this).Text.Replace("-", string.Empty);

                        continue;
                    }
                    string.Concat(val.FindByName, Count).FindByName<Button>(this).ForeColor = Color.Maroon;
                }
            cumulative.Text = string.Concat(cumulative.Text, " [", file[0], "]");
            recent.Text = string.Concat(recent.Text, " [", file[1], "]");
        }
        private void ButtonClick(object sender, EventArgs e)
        {
            arr = sender.ToString().Split(':');
            SendClose?.Invoke(this, new DialogClose(arr[1].Split(' ')));
        }
        private string SetSecret()
        {
            try
            {
                foreach (string val in Directory.GetFiles(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Statistics\"), "*.csv", SearchOption.AllDirectories))
                {
                    arr = val.Split('\\');
                    arr = arr[arr.Length - 1].Split('.');
                    long count = long.Parse(arr[0]);

                    if (count > RecentDate)
                        RecentDate = count;
                }
                using StreamReader sr = new StreamReader(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Statistics\", RecentDate.ToString(), ".csv"));
                List<string> list = new List<string>(256);

                if (sr != null)
                    while (sr.EndOfStream == false)
                        list.Add(sr.ReadLine());

                foreach (IMakeUp val in mp)
                    MakeUp(list, val);

                return string.Concat(list[1].Substring(0, 8), "^", list[list.Count - 2].Substring(0, 8));
            }
            catch (Exception ex)
            {
                new LogMessage().Record("Error", ex.ToString());
                MessageBox.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                if (i < 1)
                    FindBest(ip.FindByName.Equals("cumulative") ? list.Count - 2 : ip.Turn - 1, kv.Value, kv.Key);

                string.Concat(ip.FindByName, i++).FindByName<Button>(this).Text = string.Concat(kv.Key.Replace('^', '.'), " Day", (kv.Value / Assets / (ip.FindByName.Equals("cumulative") ? list.Count - 2 : ip.Turn - 1)).ToString("P3"));
            }
        }
        private void FindBest(int denominator, long molecule, string key)
        {
            double temp = (double)molecule / denominator;

            if (Quotient < temp && denominator > 1)
            {
                Quotient = temp;
                Key = key;
            }
        }
        private long RecentDate
        {
            get; set;
        }
        private int Count
        {
            get; set;
        }
        private double Assets
        {
            get; set;
        }
        private double Quotient
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
            new MakeUpMonthly(),
            new MakeUpFor3Months()
        };
        public event EventHandler<DialogClose> SendClose;
    }
}