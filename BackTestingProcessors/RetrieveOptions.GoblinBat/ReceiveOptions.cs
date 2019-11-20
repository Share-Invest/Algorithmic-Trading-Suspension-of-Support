﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ShareInvest.RetrieveOptions
{
    public class ReceiveOptions
    {
        public ReceiveOptions(Dictionary<string, double> list)
        {
            this.list = list;
            temp = new Dictionary<string, Dictionary<string, double>>(512);
            options = Options.Get();
            options.SendRepository += OnReceiveOptions;

            foreach (string file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, @"..\Chart\"), "*.csv", SearchOption.AllDirectories))
                if (!file.Contains("Day") && !file.Contains("Tick"))
                    options.ReadCSV(file);
        }
        private void OnReceiveOptions(object sender, OptionsRepository e)
        {
            if (e.Code.Equals(Code) || Code == null)
            {
                if (Code == null)
                {
                    Code = e.Code;
                    FileName = e.FileName;
                }
                list[e.Date] = e.Price;

                if (e.EndOfStream)
                {
                    temp[Code] = list;
                    options.Repository[FileName] = temp;
                }
                return;
            }
            temp[Code] = list;

            if (!e.Code.Equals(Code) && !e.FileName.Equals(FileName))
            {
                options.Repository[FileName] = temp;
                temp = new Dictionary<string, Dictionary<string, double>>(512);
                FileName = e.FileName;
            }
            list = new Dictionary<string, double>(512)
            {
                { e.Date, e.Price }
            };
            Code = e.Code;
        }
        private string Code
        {
            get; set;
        }
        private string FileName
        {
            get; set;
        }
        private readonly Options options;
        private Dictionary<string, Dictionary<string, double>> temp;
        private Dictionary<string, double> list;
    }
}