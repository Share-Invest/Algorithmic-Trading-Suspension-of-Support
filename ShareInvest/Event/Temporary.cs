﻿using System;
using System.Collections.Generic;
using System.IO;
using ShareInvest.EventHandler;

namespace ShareInvest
{
    public class Temporary
    {
        public Temporary()
        {
            memo = new List<string>(32768);
            act = new Action(() => Save());
            api = Futures.Get();

            api.SendMemorize += OnReceiveMemorize;
        }
        private void OnReceiveMemorize(object sender, Memorize e)
        {
            if (memo.Count > 0 && e.SPrevNext == null)
            {
                memo.Insert(0, string.Concat(e.Date, ",", e.Price, ",", e.Volume));

                return;
            }
            if (memo.Count == 0 && e.SPrevNext == null)
            {
                memo.Add(string.Concat(e.Date, ",", e.Price, ",", e.Volume));

                return;
            }
            if (e.SPrevNext.Equals("0"))
                act.BeginInvoke(act.EndInvoke, null);
        }
        private void Save()
        {
            string path = Environment.CurrentDirectory + @"\TickChart\", file = api.Code + ".csv";

            try
            {
                di = new DirectoryInfo(path);

                if (di.Exists == false)
                    di.Create();

                using (sw = new StreamWriter(path + file, true))
                {
                    foreach (string val in memo)
                        if (val.Length > 0)
                            sw.WriteLine(val);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private DirectoryInfo di;
        private StreamWriter sw;

        private readonly Futures api;
        private readonly Action act;
        private readonly List<string> memo;
    }
}