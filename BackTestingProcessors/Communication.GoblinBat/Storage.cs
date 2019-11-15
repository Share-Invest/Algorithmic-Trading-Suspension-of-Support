﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShareInvest.Communication
{
    public class Storage
    {
        public Storage(string path)
        {
            list = new List<Storage>();
            analysis = new Dictionary<string, string>();

            foreach (string val in new Enumerate())
            {
                string[] arr = val.Split(',');

                list.Add(new Storage(arr));
            }
            int i = 0, count = list.Count;
            string[] squence = new string[count];
            string[] total = new string[count];

            foreach (Storage val in list)
            {
                total[i] = val.cumulative;
                squence[i++] = val.strategy;
                analysis[val.date] = val.commission;
            }
            sb = new StringBuilder();
            sb_analysis = new StringBuilder();

            for (i = 0; i < count - 1; i++)
            {
                if (!squence[i].Equals(squence[i + 1]))
                {
                    sb.Append(',').Append(squence[i]);
                    sb_analysis.Append(',').Append(total[i]);
                }
                if (i == count - 2)
                {
                    sb.Append(',').Append(squence[i + 1]);
                    sb_analysis.Append(',').Append(total[i + 1]);
                }
            }
            Statistics(sb, path);

            foreach (KeyValuePair<string, string> kv in analysis)
            {
                sb = new StringBuilder();
                sb.Append(kv.Key).Append(',');

                foreach (Storage val in list.FindAll(o => o.date.Equals(kv.Key)))
                    sb.Append(val.revenue).Append(',');

                Statistics(sb, path);
            }
            Statistics(sb_analysis, path);
        }
        private void Statistics(StringBuilder sb, string path)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Statistics\"));

                if (di.Exists == false)
                    di.Create();

                using StreamWriter sw = new StreamWriter(path, true);
                sw.WriteLine(sb);
            }
            catch (Exception ex)
            {
                TimerBox.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", 3750);
                Environment.Exit(0);
            }
        }
        private Storage(string[] arr)
        {
            date = arr[1];
            revenue = arr[3];
            cumulative = arr[4];
            strategy = arr[0];
            commission = arr[2];
        }
        private readonly string commission;
        private readonly string revenue;
        private readonly string cumulative;
        private readonly string date;
        private readonly string strategy;
        private readonly List<Storage> list;
        private readonly Dictionary<string, string> analysis;
        private readonly StringBuilder sb;
        private readonly StringBuilder sb_analysis;
    }
}