﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ShareInvest.BackTest
{
    public class Information
    {
        public void Operate(double price, int quantity)
        {
            if (quantity != 0)
            {
                Quantity += quantity;
                Commission += (int)(price * tm * commission);
                Liquidation = price;
                PurchasePrice = price;
                Amount = Quantity;
                CumulativeRevenue += (int)(Liquidation * tm);
            }
        }
        public void Save(string time)
        {
            Revenue = CumulativeRevenue - Commission;
            TodayCommission = Commission - TempCommission;

            if (TodayCommission != 0)
                list.Add(string.Concat(DateTime.ParseExact(time.Substring(0, 6), "yyMMdd", CultureInfo.CurrentCulture).ToString("yy-MM-dd"), ',', TodayCommission, ',', Revenue - TodayRevenue, ',', CumulativeRevenue - Commission));

            TempCommission = Commission;
            TodayRevenue = Revenue;
        }
        public void Log(int param)
        {
            string path = Environment.CurrentDirectory + @"\Log\" + DateTime.Now.ToString("yyMMdd") + @"\", file = param + ".csv";

            try
            {
                di = new DirectoryInfo(path);

                if (di.Exists == false)
                    di.Create();

                using (sw = new StreamWriter(path + file))
                {
                    foreach (string val in list)
                        if (val.Length > 0)
                            sw.WriteLine(val);
                }
                list.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public string[] Remaining
        {
            get; private set;
        } =
            {
                "190911151959"
            };
        public int Quantity
        {
            get; private set;
        }
        private int Amount
        {
            get; set;
        }
        private double PurchasePrice
        {
            get
            {
                return purchase;
            }
            set
            {
                if (Math.Abs(Amount) < Math.Abs(Quantity) && Quantity != 0)
                {
                    purchase = (purchase * Math.Abs(Amount) + value) / Math.Abs(Quantity);
                    Amount = Quantity;
                }
                else if (Quantity == 0)
                    purchase = 0;
            }
        }
        private long CumulativeRevenue
        {
            get; set;
        }
        private double Liquidation
        {
            get
            {
                return liquidation;
            }
            set
            {
                if (Amount > Quantity && Quantity > -1)
                    liquidation = value - PurchasePrice;

                else if (Amount < Quantity && Quantity < 1)
                    liquidation = PurchasePrice - value;
            }
        }
        private long Revenue
        {
            get; set;
        }
        private long TodayRevenue
        {
            get; set;
        }
        private int Commission
        {
            get; set;
        }
        private int TodayCommission
        {
            get; set;
        }
        private int TempCommission
        {
            get; set;
        }
        private readonly List<string> list = new List<string>(64);
        private const int tm = 250000;
        private const double commission = 3e-5;
        private DirectoryInfo di;
        private StreamWriter sw;
        private double purchase;
        private double liquidation;
    }
}