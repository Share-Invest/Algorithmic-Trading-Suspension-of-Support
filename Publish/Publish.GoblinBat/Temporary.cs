﻿using System;
using System.Collections.Generic;
using System.IO;
using ShareInvest.AutoMessageBox;
using ShareInvest.EventHandler;

namespace ShareInvest.Publish
{
    public class Temporary
    {
        public Temporary()
        {
            memo = new List<string>(32768);
            act = new Action(() => Save());
            api = PublicFutures.Get();
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
            string path = string.Concat(Environment.CurrentDirectory, @"\TickChart\");

            try
            {
                DirectoryInfo di = new DirectoryInfo(path);

                if (di.Exists == false)
                    di.Create();

                using StreamWriter sw = new StreamWriter(string.Concat(path, api.Code, ".csv"), true);
                foreach (string val in memo)
                    if (val.Length > 0)
                        sw.WriteLine(val);
            }
            catch (Exception ex)
            {
                Box.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", 3750);
                Environment.Exit(0);
            }
            Send?.Invoke(this, new ForceQuit(1));
        }
        private readonly PublicFutures api;
        private readonly Action act;
        private readonly List<string> memo;
        public event EventHandler<ForceQuit> Send;
    }
}