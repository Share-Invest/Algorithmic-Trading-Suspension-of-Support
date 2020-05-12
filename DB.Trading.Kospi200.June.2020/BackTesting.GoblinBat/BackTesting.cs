﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShareInvest.Catalog;
using ShareInvest.EventHandler.BackTesting;
using ShareInvest.GoblinBatContext;
using ShareInvest.Message;
using ShareInvest.Strategy.Statistics;

namespace ShareInvest.Strategy
{
    public class BackTesting : CallUpStatisticalAnalysis
    {
        EMA EMA
        {
            get;
        }
        int Amount
        {
            get; set;
        }
        int Accumulative
        {
            get; set;
        }
        int TodayCommission
        {
            get; set;
        }
        uint Count
        {
            get; set;
        }
        uint Commission
        {
            get; set;
        }
        long CumulativeRevenue
        {
            get; set;
        }
        long Revenue
        {
            get; set;
        }
        long TodayRevenue
        {
            get; set;
        }
        double Before
        {
            get; set;
        }
        Dictionary<uint, int> Residue
        {
            get;
        }
        int StartProgress()
        {
            foreach (var quotes in Retrieve.Quotes)
            {
                if (quotes.Price != null && quotes.Volume != null)
                {
                    SendDatum?.Invoke(this, new Datum(quotes.Time, quotes.Price, quotes.Volume));

                    continue;
                }
                SendQuotes?.Invoke(this, new Quotes(quotes.Time, quotes.SellPrice, quotes.BuyPrice, quotes.SellQuantity, quotes.BuyQuantity, quotes.SellAmount, quotes.BuyAmount));
            }
            if (games.Count > 0 && SetStatisticalStorage(games) == false)
                Message = new Secrets().Message;

            return statement == null ? 0 : statement.Count;
        }
        void SetConclusion(double price)
        {
            Commission += (uint)(price * Const.TransactionMultiplier * game.Commission);
            var liquidation = SetLiquidation(price);
            PurchasePrice = SetPurchasePrice(price);
            CumulativeRevenue += (long)(liquidation * Const.TransactionMultiplier);
            Amount = Quantity;
        }
        double SetWeight(long param)
        {
            if (param > 0)
                return param * 0.925;

            return param * 1.075;
        }
        double SetPurchasePrice(double price)
        {
            if (Math.Abs(Amount) < Math.Abs(Quantity) && Quantity != 0)
                PurchasePrice = (PurchasePrice * Math.Abs(Amount) + price) / Math.Abs(Quantity);

            return Quantity == 0 ? 0 : PurchasePrice;
        }
        double SetLiquidation(double price)
        {
            if (Amount > Quantity && Quantity > -1)
                return price - PurchasePrice;

            else if (Amount < Quantity && Quantity < 1)
                return PurchasePrice - price;

            else
                return 0;
        }
        string ConvertDateTime(string time)
        {
            if (time.Length > 6 && DateTime.TryParseExact(time.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date))
                return string.Concat(date.ToShortDateString(), " ", date.ToShortTimeString());

            return DateTime.Parse(time).ToLongDateString();
        }
        internal void Max(double trend, Catalog.XingAPI.Specify specify)
        {
            Judge[specify.Time] = trend;
            double temp = 0;

            foreach (var kv in Judge)
                temp += kv.Value;

            Classification = temp == 0 ? string.Empty : temp > 0 ? Analysis.buy : Analysis.sell;
        }
        internal bool SendClearingOrder(string time, uint number)
        {
            if (SellOrder.ContainsValue(number) && SellOrder.Remove(SellOrder.First(o => o.Value == number).Key) && Residue.Remove(number))
            {
                if (verify)
                    statement.Enqueue(new Conclusion
                    {
                        Time = ConvertDateTime(time),
                        Division = string.Concat(sell, cancel),
                        Price = string.Empty,
                        OrderNumber = number.ToString("N0")
                    });
                return true;
            }
            if (BuyOrder.ContainsValue(number) && BuyOrder.Remove(BuyOrder.First(o => o.Value == number).Key) && Residue.Remove(number))
            {
                if (verify)
                    statement.Enqueue(new Conclusion
                    {
                        Time = ConvertDateTime(time),
                        Division = string.Concat(buy, cancel),
                        Price = string.Empty,
                        OrderNumber = number.ToString("N0")
                    });
                return true;
            }
            return false;
        }
        internal bool SendCorrectionOrder(string time, string price, uint number, int residue)
        {
            if (SellOrder.ContainsValue(number) && SellOrder.Remove(SellOrder.First(o => o.Value == number).Key) && Residue.Remove(number))
            {
                SellOrder[price] = Count;
                Residue[Count++] = residue;

                if (verify)
                    statement.Enqueue(new Conclusion
                    {
                        Time = ConvertDateTime(time),
                        Division = string.Concat(sell, correction),
                        Price = price,
                        OrderNumber = SellOrder[price].ToString("N0")
                    });
                return true;
            }
            if (BuyOrder.ContainsValue(number) && BuyOrder.Remove(BuyOrder.First(o => o.Value == number).Key) && Residue.Remove(number))
            {
                BuyOrder[price] = Count;
                Residue[Count++] = residue;

                if (verify)
                    statement.Enqueue(new Conclusion
                    {
                        Time = ConvertDateTime(time),
                        Division = string.Concat(buy, correction),
                        Price = price,
                        OrderNumber = BuyOrder[price].ToString("N0")
                    });
                return true;
            }
            return false;
        }
        internal bool SendNewOrder(string time, string price, string classification, int residue)
        {
            switch (classification)
            {
                case Analysis.sell:
                    SellOrder[price] = Count;
                    Residue[Count++] = residue;

                    if (verify)
                        statement.Enqueue(new Conclusion
                        {
                            Time = ConvertDateTime(time),
                            Division = string.Concat(sell, order),
                            Price = price,
                            OrderNumber = SellOrder[price].ToString("N0")
                        });
                    return true;

                case Analysis.buy:
                    BuyOrder[price] = Count;
                    Residue[Count++] = residue;

                    if (verify)
                        statement.Enqueue(new Conclusion
                        {
                            Time = ConvertDateTime(time),
                            Division = string.Concat(buy, order),
                            Price = price,
                            OrderNumber = BuyOrder[price].ToString("N0")
                        });
                    return true;
            }
            return false;
        }
        internal void SetSellConclusion(string time, double price, int residue)
        {
            var key = price.ToString("F2");

            if (SellOrder.TryGetValue(key, out uint number))
            {
                if (Residue[number] - residue > 0)
                {
                    Residue[number] = Residue[number] - residue;

                    return;
                }
                if (SellOrder.Remove(key) && Residue.Remove(number))
                {
                    if (verify)
                        statement.Enqueue(new Conclusion
                        {
                            Time = ConvertDateTime(time),
                            Division = string.Concat(sell, conclusion),
                            Price = price.ToString("F2"),
                            OrderNumber = number.ToString("N0")
                        });
                    Quantity -= 1;
                    SetConclusion(price);
                }
            }
        }
        internal void SetBuyConclusion(string time, double price, int residue)
        {
            var key = price.ToString("F2");

            if (BuyOrder.TryGetValue(key, out uint number))
            {
                if (Residue[number] + residue > 0)
                {
                    Residue[number] = Residue[number] + residue;

                    return;
                }
                if (BuyOrder.Remove(key) && Residue.Remove(number))
                {
                    if (verify)
                        statement.Enqueue(new Conclusion
                        {
                            Time = ConvertDateTime(time),
                            Division = string.Concat(buy, conclusion),
                            Price = price.ToString("F2"),
                            OrderNumber = number.ToString("N0")
                        });
                    Quantity += 1;
                    SetConclusion(price);
                }
            }
        }
        internal void SetStatisticalStorage(string date, double price, bool over)
        {
            if (over)
                while (Quantity != 0)
                {
                    if (verify)
                        statement.Enqueue(new Conclusion
                        {
                            Time = ConvertDateTime(date),
                            Division = string.Concat(Quantity > 0 ? sell : buy, conclusion),
                            Price = price.ToString("F2"),
                            OrderNumber = Count.ToString("N0")
                        });
                    Quantity += Quantity > 0 ? -1 : 1;
                    SetConclusion(price);
                }
            Revenue = CumulativeRevenue - Commission;
            long revenue = Revenue - TodayRevenue, unrealized = (long)(Quantity == 0 ? 0 : (Quantity > 0 ? price - PurchasePrice : PurchasePrice - price) * Const.TransactionMultiplier * Math.Abs(Quantity));
            var avg = EMA.Make(++Accumulative, SetWeight(revenue + unrealized), Before);
            games.Enqueue(new Models.ImitationGames
            {
                Assets = game.Assets,
                Code = game.Code,
                Commission = game.Commission,
                MarginRate = game.MarginRate,
                Strategy = game.Strategy,
                RollOver = game.RollOver,
                BaseTime = game.BaseTime,
                BaseShort = game.BaseShort,
                BaseLong = game.BaseLong,
                NonaTime = game.NonaTime,
                NonaShort = game.NonaShort,
                NonaLong = game.NonaLong,
                OctaTime = game.OctaTime,
                OctaShort = game.OctaShort,
                OctaLong = game.OctaLong,
                HeptaTime = game.HeptaTime,
                HeptaShort = game.HeptaShort,
                HeptaLong = game.HeptaLong,
                HexaTime = game.HexaTime,
                HexaShort = game.HexaShort,
                HexaLong = game.HexaLong,
                PentaTime = game.PentaTime,
                PentaShort = game.PentaShort,
                PentaLong = game.PentaLong,
                QuadTime = game.QuadTime,
                QuadShort = game.QuadShort,
                QuadLong = game.QuadLong,
                TriTime = game.TriTime,
                TriShort = game.TriShort,
                TriLong = game.TriLong,
                DuoTime = game.DuoTime,
                DuoShort = game.DuoShort,
                DuoLong = game.DuoLong,
                MonoTime = game.MonoTime,
                MonoShort = game.MonoShort,
                MonoLong = game.MonoLong,
                Date = date,
                Unrealized = unrealized,
                Revenue = revenue,
                Cumulative = CumulativeRevenue - Commission,
                Fees = (int)(Commission - TodayCommission),
                Statistic = (int)avg
            });
            if (Count > 5000)
                new ExceptionMessage(game.Strategy, string.Concat(date, '_', Count));

            Before = avg;
            TodayCommission = (int)Commission;
            TodayRevenue = Revenue;
            SellOrder.Clear();
            BuyOrder.Clear();
            Residue.Clear();
            Count = 0;
        }
        internal int Quantity
        {
            get; set;
        }
        internal double PurchasePrice
        {
            get; private set;
        }
        internal string Classification
        {
            get; set;
        }
        internal Dictionary<string, uint> BuyOrder
        {
            get;
        }
        internal Dictionary<string, uint> SellOrder
        {
            get;
        }
        internal Dictionary<uint, double> Judge
        {
            get;
        }
        public string Message
        {
            get; private set;
        }
        public BackTesting(char verify, Models.ImitationGames game, string key) : base(key)
        {
            this.verify = verify.Equals((char)86);
            this.game = game;
            Residue = new Dictionary<uint, int>();
            SellOrder = new Dictionary<string, uint>();
            BuyOrder = new Dictionary<string, uint>();
            Judge = new Dictionary<uint, double>();
            games = new Queue<Models.ImitationGames>();
            Parallel.ForEach(Retrieve.GetCatalog(game), new Action<Catalog.XingAPI.Specify>((param) =>
            {
                switch (param.Strategy)
                {
                    case basic:
                        new Base(this, param);
                        break;

                    case bantam:
                        new Bantam(this, param);
                        break;

                    case feather:
                        new Feather(this, param);
                        break;

                    case fly:
                        new Fly(this, param);
                        break;

                    case heavy:
                        new Heavy(this, param);
                        break;
                }
            }));
            if (this.verify)
                statement = new Queue<Conclusion>(32);

            if (StartProgress() > 0)
                using (var sw = new StreamWriter(new Secrets().Path(game.Strategy, games.Last().Statistic), true))
                    try
                    {
                        while (statement.Count > 0)
                        {
                            var str = statement.Dequeue();
                            sw.WriteLine(new StringBuilder(str.Time).Append(',').Append(str.Division).Append(',').Append(str.Price).Append(',').Append(str.OrderNumber));
                        }
                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessage(ex.StackTrace);
                    }
        }
        const string format = "yyMMddHHmmss";
        const string sell = "매도";
        const string buy = "매수";
        const string order = "주문";
        const string correction = "정정";
        const string cancel = "취소";
        const string conclusion = "체결";
        const string basic = "Base";
        const string bantam = "Bantam";
        const string feather = "Feather";
        const string fly = "Fly";
        const string heavy = "Heavy";
        readonly bool verify;
        readonly Models.ImitationGames game;
        readonly Queue<Models.ImitationGames> games;
        readonly Queue<Conclusion> statement;
        public event EventHandler<Datum> SendDatum;
        public event EventHandler<Quotes> SendQuotes;
    }
}