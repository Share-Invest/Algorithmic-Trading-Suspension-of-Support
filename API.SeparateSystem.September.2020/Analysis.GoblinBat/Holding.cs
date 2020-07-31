﻿using System;
using System.Collections.Generic;
using System.Linq;

using ShareInvest.Catalog;
using ShareInvest.EventHandler;

namespace ShareInvest.Analysis
{
    public abstract class Holding
    {
        Queue<Charts> Days
        {
            get; set;
        }
        IEnumerable<Queue<Charts>> FindTheOldestDueDate(string code)
        {
            var temporary = new Temporary(code.Length);

            if (code.Length == 8 && Temporary.CodeStorage != null && Temporary.CodeStorage.Any(o => o.Code.StartsWith(code.Substring(0, 3)) && o.Code.EndsWith(code.Substring(5))))
            {
                var stack = new Stack<Codes>();
                var days = temporary.CallUpTheChartAsync(code);

                foreach (var arg in Temporary.CodeStorage.Where(o => o.Code.StartsWith(code.Substring(0, 3)) && o.Code.EndsWith(code.Substring(5))).OrderByDescending(o => o.MaturityMarketCap.Length == 8 ? o.MaturityMarketCap.Substring(2) : o.MaturityMarketCap))
                    stack.Push(arg);

                Days = days.Result;

                while (stack.Count > 0)
                    yield return temporary.CallUpTheChartAsync(stack.Pop()).Result;
            }
            else if (code.Length == 6)
            {

            }
        }
        long StartProgress(string code)
        {
            foreach (var queue in FindTheOldestDueDate(code))
            {
                var enumerable = queue.OrderBy(o => o.Date);

                if (Days.Count > 0)
                {
                    var before = enumerable.First().Date.Substring(0, 6);

                    foreach (var day in Days.OrderBy(o => o.Date))
                        if (string.Compare(day.Date.Substring(2), before) < 0)
                            Send?.Invoke(this, new SendConsecutive(day));

                    Days.Clear();
                }
                foreach (var consecutive in enumerable)
                    Send?.Invoke(this, new SendConsecutive(consecutive));
            }
            return GC.GetTotalMemory(true);
        }
        public event EventHandler<SendConsecutive> Send;
        public Holding(TrendFollowingBasicFutures strategics)
        {
            TF = strategics;
            var catalog = strategics.SetCatalog(strategics);
            Consecutive = new Consecutive[catalog.Length];

            for (int i = 0; i < catalog.Length; i++)
                Consecutive[i] = new Consecutive(catalog[i], this);

            if (StartProgress(strategics.Code) > 0)
                foreach (var con in Consecutive)
                    con.Dispose();
        }
        public Holding(TrendsInStockPrices strategics)
        {
            TS = strategics;
        }
        public abstract string Code
        {
            get; set;
        }
        public abstract int Quantity
        {
            get; set;
        }
        public abstract dynamic Purchase
        {
            get; set;
        }
        public abstract dynamic Current
        {
            get; set;
        }
        public abstract long Revenue
        {
            get; set;
        }
        public abstract double Rate
        {
            get; set;
        }
        public abstract bool WaitOrder
        {
            get; set;
        }
        public abstract Dictionary<string, dynamic> OrderNumber
        {
            get;
        }
        public abstract void OnReceiveEvent(string[] param);
        public abstract void OnReceiveBalance(string[] param);
        public abstract void OnReceiveConclusion(string[] param);
        public abstract event EventHandler<SendSecuritiesAPI> SendBalance;
        public abstract event EventHandler<SendHoldingStocks> SendStocks;
        internal Consecutive[] Consecutive
        {
            get;
        }
        protected internal TrendFollowingBasicFutures TF
        {
            get;
        }
        protected internal TrendsInStockPrices TS
        {
            get;
        }
        protected internal const string conclusion = "체결";
        protected internal const string acceptance = "접수";
        protected internal const string confirmation = "확인";
        protected internal const string cancellantion = "취소";
        protected internal const string correction = "정정";
        protected internal const uint transactionMutiplier = 0x3D090;
    }
    enum TR
    {
        SONBT001 = 0,
        SONBT002 = 1,
        SONBT003 = 2,
        CONET801 = 3,
        CONET002 = 4,
        CONET003 = 5
    }
}