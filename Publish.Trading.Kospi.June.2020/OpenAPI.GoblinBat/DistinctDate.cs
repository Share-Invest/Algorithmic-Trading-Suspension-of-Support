﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ShareInvest.Catalog;
using ShareInvest.RetrieveInformation;

namespace ShareInvest.OpenAPI
{
    public class DistinctDate
    {
        protected string GetDistinctDate(int usWeekNumber)
        {
            DayOfWeek dt = DateTime.Now.AddDays(1 - DateTime.Now.Day).DayOfWeek;
            int check = dt.Equals(DayOfWeek.Friday) || dt.Equals(DayOfWeek.Saturday) ? 3 : 2;

            return usWeekNumber > check || usWeekNumber == check && (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) || DateTime.Now.DayOfWeek.Equals(DayOfWeek.Saturday)) ? DateTime.Now.AddMonths(1).ToString("yyyyMM") : DateTime.Now.ToString("yyyyMM");
        }
        protected string Retention(string code)
        {
            foreach (string retention in code.Substring(0, 3).Equals("101") ? Retrieve.Get().ReadCSV(Array.Find(Directory.GetFiles(Path.Combine(Application.StartupPath, @"..\"), "*.csv", SearchOption.AllDirectories), o => o.Contains("Tick")), new List<string>(2097152)) : Retrieve.Get().ReadCSV(Array.Find(Directory.GetFiles(Path.Combine(Application.StartupPath, @"..\"), string.Concat(code, ".csv"), SearchOption.AllDirectories), o => o.Contains(code)), new List<string>(1280)))
                code = retention.Substring(0, 12);

            return code;
        }
        protected bool GetConclusion(string[] param)
        {
            if (param[18].Equals(string.Empty))
                return false;

            return true;
        }
        protected readonly Dictionary<string, string> table = new Dictionary<string, string>()
        {
            { "Code", "BEGIN CREATE TABLE [Code] ([Code] VARCHAR(8) NOT NULL CONSTRAINT pkCode PRIMARY KEY,[Name] VARCHAR(12) NOT NULL) END" },
            { "Futures", "BEGIN CREATE TABLE [Futures] ([Code] VARCHAR(8) NOT NULL,[Date] BIGINT NOT NULL,[Price] FLOAT NOT NULL,[Volume] INT NOT NULL,CONSTRAINT pkFutures PRIMARY KEY ([Code],[Date])) END" },
            { "Options", "BEGIN CREATE TABLE [Options] ([Code] VARCHAR(8) NOT NULL,[Date] BIGINT NOT NULL,[Price] FLOAT NOT NULL,[Volume] INT NOT NULL,CONSTRAINT pkOptions PRIMARY KEY ([Code],[Date])) END" },
            { "Stocks", "BEGIN CREATE TABLE [Stocks] ([Code] VARCHAR(8) NOT NULL,[Date] BIGINT NOT NULL,[Price] INT NOT NULL,[Volume] INT NOT NULL,CONSTRAINT pkStocks PRIMARY KEY ([Code],[Date])) END" }
        };
        protected readonly IEnumerable[] catalog =
        {
            new Opt50001(),
            new OPW20010(),
            new OPW20007()
        };
        protected const string it = "Information that already Exists";
    }
}