﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ShareInvest.Communication
{
    public class Enumerate : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            string[] arr;
            int i = 0;

            try
            {
                foreach (string val in Directory.GetDirectories(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Log\")))
                {
                    arr = val.Split('\\');
                    int recent = int.Parse(arr[arr.Length - 1]);

                    if (recent > RecentDate)
                        RecentDate = recent;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
            foreach (string file in Directory.GetFiles(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Log\", RecentDate), "*.csv", SearchOption.AllDirectories))
            {
                if (i++ > 5500)
                {
                    Application.DoEvents();
                    GC.Collect();
                    i = 0;
                }
                arr = file.Split('\\');
                arr = arr[arr.Length - 1].Split('.');

                foreach (string val in ReadCSV(file, new List<string>(64)))
                {
                    if (val.Split(',').Length < 4)
                        break;

                    yield return string.Concat(arr[0], ",", val);
                }
                Application.DoEvents();
            }
        }
        private List<string> ReadCSV(string file, List<string> list)
        {
            try
            {
                using StreamReader sr = new StreamReader(file);
                if (sr != null)
                    while (sr.EndOfStream == false)
                        list.Add(sr.ReadLine());
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
            return list;
        }
        private int RecentDate
        {
            get; set;
        }
    }
}