﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using ShareInvest.Catalog;
using ShareInvest.Client;
using ShareInvest.EventHandler;
using ShareInvest.Interface;

namespace ShareInvest.Analysis.SecondaryIndicators
{
    public class HoldingStocks : Holding
    {
        public override void OnReceiveConclusion(string[] param) => Console.WriteLine(param[0] + " Q_" + param[1] + " P_" + param[2] + " C_" + param[3] + " R_" + param[4] + " N_" + param[5]);
        public override void OnReceiveBalance(string[] param) => Console.WriteLine(param[0] + " N_" + param[1] + " P_" + param[2]);
        public override void OnReceiveEvent(string[] param) => Console.WriteLine(param[0] + " B_" + param[1] + " S_" + param[2]);
        internal void OnReceiveTrendsInPrices(SendConsecutive e, double gap, double sShort, double sLong, double trend)
        {
            var date = e.Date.Substring(6, 4);

            switch (strategics)
            {
                case TrendFollowingBasicFutures tf:
                    if ((uint)trend < 0x5A0)
                        Secondary = gap;

                    else
                    {
                        if (date.CompareTo(start) > 0 && date.CompareTo(end) < 0 && (gap > 0 ? tf.QuantityLong - Quantity > 0 : tf.QuantityShort + Quantity > 0) && (gap > 0 ? e.Volume > tf.ReactionLong : e.Volume < -tf.ReactionShort) && (gap > 0 ? e.Volume + Secondary > e.Volume : e.Volume + Secondary < e.Volume))
                        {
                            Quantity += gap > 0 ? 1 : -1;
                            CumulativeFee += (uint)(e.Price * transactionMultiplier * Commission);
                            var liquidation = SetLiquidation(e.Price);
                            Purchase = SetPurchasePrice(e.Price);
                            Revenue += (long)(liquidation * transactionMultiplier);
                            VerifyAmount = Quantity;
                        }
                        else if (date.CompareTo(cme) < 0 && date.CompareTo(end) > 0 && uint.TryParse(e.Date.Substring(0, 6), out uint remain))
                        {
                            if (tf.RollOver == false || Temporary.RemainingDay.Contains(remain))
                                while (Quantity != 0)
                                {
                                    Quantity += Quantity > 0 ? -1 : 1;
                                    CumulativeFee += (uint)(e.Price * transactionMultiplier * Commission);
                                    var liquidation = SetLiquidation(e.Price);
                                    Purchase = SetPurchasePrice(e.Price);
                                    Revenue += (long)(liquidation * transactionMultiplier);
                                    VerifyAmount = Quantity;
                                }
                            long revenue = Revenue - CumulativeFee, unrealize = (long)((e.Price - (Purchase ?? 0D)) * Quantity * transactionMultiplier);
                            var avg = EMA.Make(++Accumulative, revenue - TodayRevenue + unrealize - TodayUnrealize, Before);
                            SendMessage = new Statistics
                            {
                                Date = e.Date.Substring(0, 6),
                                Cumulative = revenue + unrealize,
                                Base = avg,
                                Statistic = Quantity
                            };
                            SendStocks?.Invoke(this, new SendHoldingStocks(e.Date, e.Price, sShort, sLong, revenue + unrealize));
                            Before = avg;
                            TodayRevenue = revenue;
                            TodayUnrealize = unrealize;
                        }
                        else
                        {

                        }
                        Base = gap;
                    }
                    break;

                case ScenarioAccordingToTrend st:
                    if (e.Date.Length > 8 && date.CompareTo(start) > 0 && date.CompareTo(transmit) < 0 && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime interval))
                    {
                        if (NextOrderTime == null)
                            NextOrderTime = interval;

                        else if (Quantity > st.Quantity - 1 && OrderNumber.Any(o => o.Key.StartsWith("2") && o.Value == e.Price - GetQuoteUnit(e.Price, Market)))
                        {
                            CumulativeFee += (uint)(e.Price * st.Quantity * (Commission + tax));
                            Revenue += (long)((e.Price - (Purchase ?? 0D)) * st.Quantity);
                            Quantity -= st.Quantity;
                            var profit = OrderNumber.First(o => o.Key.StartsWith("2") && o.Value == e.Price - GetQuoteUnit(e.Price, Market));

                            if (OrderNumber.Remove(profit.Key) && Verify)
                                OnReceiveBalance(new string[]
                                {
                                    string.Concat(interval.ToShortDateString(), " ", interval.ToLongTimeString()),
                                    profit.Key,
                                    profit.Value.ToString("N0")
                                });
                            Base -= profit.Value * st.Quantity;
                        }
                        else if (OrderNumber.Any(o => o.Key.StartsWith("1") && o.Value == e.Price + GetQuoteUnit(e.Price, Market)))
                        {
                            CumulativeFee += (uint)(e.Price * Commission * st.Quantity);
                            Purchase = (double)((e.Price * st.Quantity + (Purchase ?? 0D) * Quantity) / (Quantity + st.Quantity));
                            Quantity += st.Quantity;
                            var profit = OrderNumber.First(o => o.Key.StartsWith("1") && o.Value == e.Price + GetQuoteUnit(e.Price, Market));

                            if (OrderNumber.Remove(profit.Key) && Verify)
                                OnReceiveBalance(new string[]
                                {
                                    string.Concat(interval.ToShortDateString(), " ", interval.ToLongTimeString()),
                                    profit.Key,
                                    profit.Value.ToString("N0")
                                });
                            Base += profit.Value * st.Quantity;
                        }
                        else if (Quantity > st.Quantity - 1 && OrderNumber.ContainsValue(e.Price) == false && e.Price > trend * (1 + st.ErrorRange) && e.Price > (Purchase ?? 0D) && gap < 0 && (st.IntervalInSeconds == 0 || st.IntervalInSeconds > 0 && interval.CompareTo(NextOrderTime) > 0))
                        {
                            var unit = GetQuoteUnit(e.Price, Market);

                            if (Verify && VerifyAmount > Quantity && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                            {
                                OnReceiveConclusion(new string[]
                                {
                                    string.Concat(OpenOrderType.신규매도, " ", dt.ToShortDateString(), " ", dt.ToLongTimeString(), " ", NextOrderTime),
                                    Quantity.ToString("N0"),
                                    (Purchase ?? 0D).ToString("N2"),
                                    (e.Price + unit).ToString("N0"),
                                    (Revenue - CumulativeFee).ToString("C0"),
                                    OrderNumber.Max(o => o.Key)
                                });
                                VerifyAmount = Quantity;
                            }
                            if (OrderNumber.ContainsValue(e.Price + unit) == false)
                                OrderNumber[GetOrderNumber((int)OpenOrderType.신규매도)] = e.Price + unit;

                            if (st.IntervalInSeconds > 0)
                                NextOrderTime = MeasureTheDelayTime(st.IntervalInSeconds, interval);
                        }
                        else if (OrderNumber.ContainsValue(e.Price) == false && e.Price < trend * (1 - st.ErrorRange) && gap > 0 && (st.IntervalInSeconds == 0 || st.IntervalInSeconds > 0 && interval.CompareTo(NextOrderTime) > 0))
                        {
                            var unit = GetQuoteUnit(e.Price, Market);

                            if (Verify && VerifyAmount < Quantity && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                            {
                                OnReceiveConclusion(new string[]
                                {
                                    string.Concat(OpenOrderType.신규매수, " ", dt.ToShortDateString(), " ", dt.ToLongTimeString(), " ", NextOrderTime),
                                    Quantity.ToString("N0"),
                                    (Purchase ?? 0D).ToString("N2"),
                                    (e.Price - unit).ToString("N0"),
                                    (Revenue - CumulativeFee).ToString("C0"),
                                    OrderNumber.Where(o=> o.Key.StartsWith("1")).Max(o => o.Key)
                                });
                                VerifyAmount = Quantity;
                            }
                            if (OrderNumber.ContainsValue(e.Price - unit) == false)
                                OrderNumber[GetOrderNumber((int)OpenOrderType.신규매수)] = e.Price - unit;

                            if (st.IntervalInSeconds > 0)
                                NextOrderTime = MeasureTheDelayTime(st.IntervalInSeconds, interval);
                        }
                    }
                    else if (date.CompareTo(transmit) > 0)
                    {
                        OrderNumber.Clear();
                        Count = 0;
                        long revenue = Revenue - CumulativeFee, unrealize = (long)((e.Price - (Purchase ?? 0D)) * Quantity);
                        var avg = EMA.Make(++Accumulative, revenue - TodayRevenue + unrealize - TodayUnrealize, Before);
                        SendMessage = new Statistics
                        {
                            Date = e.Date.Substring(0, 6),
                            Cumulative = (revenue + unrealize) / st.Quantity,
                            Base = SendMessage.Base > Base / st.Quantity ? SendMessage.Base : Base / st.Quantity,
                            Statistic = (int)(avg / st.Quantity),
                            Price = e.Price
                        };
                        SendStocks?.Invoke(this, new SendHoldingStocks(e.Date, e.Price, sShort, sLong, trend, revenue + unrealize, (long)(Base > 0 ? Base : 0)));
                        Before = avg;
                        TodayRevenue = revenue;
                        TodayUnrealize = unrealize;
                    }
                    break;

                case TrendsInStockPrices ts:
                    if (e.Date.Length > 8 && date.CompareTo(start) > 0 && date.CompareTo(transmit) < 0)
                    {
                        if (Quantity > ts.Quantity - 1 && OrderNumber.Any(o => o.Key.StartsWith("2") && o.Value == e.Price - GetQuoteUnit(e.Price, Market)))
                        {
                            CumulativeFee += (uint)(e.Price * ts.Quantity * (Commission + tax));
                            Revenue += (long)((e.Price - (Purchase ?? 0D)) * ts.Quantity);
                            Quantity -= ts.Quantity;
                            var profit = OrderNumber.First(o => o.Key.StartsWith("2") && o.Value == e.Price - GetQuoteUnit(e.Price, Market));

                            if (OrderNumber.Remove(profit.Key) && Verify && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                                OnReceiveBalance(new string[]
                                {
                                    string.Concat(dt.ToShortDateString(), " ", dt.ToLongTimeString()),
                                    profit.Key,
                                    profit.Value.ToString("N0")
                                });
                            Base -= profit.Value * ts.Quantity;
                        }
                        else if (OrderNumber.Any(o => o.Key.StartsWith("1") && o.Value == e.Price + GetQuoteUnit(e.Price, Market)))
                        {
                            CumulativeFee += (uint)(e.Price * Commission * ts.Quantity);
                            Purchase = (double)((e.Price * ts.Quantity + (Purchase ?? 0D) * Quantity) / (Quantity + ts.Quantity));
                            Quantity += ts.Quantity;
                            var profit = OrderNumber.First(o => o.Key.StartsWith("1") && o.Value == e.Price + GetQuoteUnit(e.Price, Market));

                            if (OrderNumber.Remove(profit.Key) && Verify && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                                OnReceiveBalance(new string[]
                                {
                                    string.Concat(dt.ToShortDateString(), " ", dt.ToLongTimeString()),
                                    profit.Key,
                                    profit.Value.ToString("N0")
                                });
                            Base += profit.Value * ts.Quantity;
                        }
                        else if (Quantity > ts.Quantity - 1 && OrderNumber.ContainsValue(e.Price) == false && e.Price > trend * (1 + ts.RealizeProfit) && e.Price > (Purchase ?? 0D) && gap < 0)
                        {
                            var quote = 0;

                            for (int i = 0; i < ts.QuoteUnit; i++)
                                quote += GetQuoteUnit(e.Price, Market);

                            if (Verify && VerifyAmount > Quantity && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                            {
                                OnReceiveConclusion(new string[]
                                {
                                    string.Concat(OpenOrderType.신규매도, " ", dt.ToShortDateString(), " ", dt.ToLongTimeString()),
                                    Quantity.ToString("N0"),
                                    (Purchase ?? 0D).ToString("N2"),
                                    (e.Price + quote).ToString("N0"),
                                    (Revenue - CumulativeFee).ToString("C0"),
                                    OrderNumber.Max(o => o.Key)
                                });
                                VerifyAmount = Quantity;
                            }
                            if (OrderNumber.ContainsValue(e.Price + quote) == false)
                                OrderNumber[GetOrderNumber((int)OpenOrderType.신규매도)] = e.Price + quote;
                        }
                        else if (OrderNumber.ContainsValue(e.Price) == false && e.Price < trend * (1 - ts.AdditionalPurchase) && gap > 0)
                        {
                            var quote = 0;

                            for (int i = 0; i < ts.QuoteUnit; i++)
                                quote += GetQuoteUnit(e.Price, Market);

                            if (Verify && VerifyAmount < Quantity && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                            {
                                OnReceiveConclusion(new string[]
                                {
                                    string.Concat(OpenOrderType.신규매수, " ", dt.ToShortDateString(), " ", dt.ToLongTimeString()),
                                    Quantity.ToString("N0"),
                                    (Purchase ?? 0D).ToString("N2"),
                                    (e.Price - quote).ToString("N0"),
                                    (Revenue - CumulativeFee).ToString("C0"),
                                    OrderNumber.Where(o=> o.Key.StartsWith("1")).Max(o => o.Key)
                                });
                                VerifyAmount = Quantity;
                            }
                            if (OrderNumber.ContainsValue(e.Price - quote) == false)
                                OrderNumber[GetOrderNumber((int)OpenOrderType.신규매수)] = e.Price - quote;
                        }
                    }
                    else if (date.CompareTo(transmit) > 0)
                    {
                        OrderNumber.Clear();
                        Count = 0;
                        long revenue = Revenue - CumulativeFee, unrealize = (long)((e.Price - (Purchase ?? 0D)) * Quantity);
                        var avg = EMA.Make(++Accumulative, revenue - TodayRevenue + unrealize - TodayUnrealize, Before);

                        if (ts.Setting.Equals(Setting.Reservation) && Quantity > ts.Quantity - 1)
                        {
                            var stock = Market;
                            int quantity = Quantity / ts.Quantity, price = e.Price, sell = (int)((Purchase ?? 0D) * (1 + ts.RealizeProfit)), buy = (int)((Purchase ?? 0D) * (1 - ts.AdditionalPurchase)), upper = (int)(price * 1.3), lower = (int)(price * 0.7), bPrice = GetStartingPrice(lower, stock), sPrice = GetStartingPrice(sell, stock);
                            sPrice = sPrice < lower ? lower + GetQuoteUnit(sPrice, stock) : sPrice;

                            while (sPrice < upper && quantity-- > 0)
                            {
                                OrderNumber[GetOrderNumber((int)OpenOrderType.신규매도)] = sPrice;

                                for (int i = 0; i < ts.QuoteUnit; i++)
                                    sPrice += GetQuoteUnit(sPrice, stock);
                            }
                            while (bPrice < upper && bPrice < buy)
                            {
                                OrderNumber[GetOrderNumber((int)OpenOrderType.신규매수)] = bPrice;

                                for (int i = 0; i < ts.QuoteUnit; i++)
                                    bPrice += GetQuoteUnit(bPrice, stock);
                            }
                            if (Verify && DateTime.TryParseExact(e.Date.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                                OnReceiveEvent(new string[]
                                {
                                    string.Concat(dt.ToShortDateString(), " ", dt.ToLongTimeString()),
                                    OrderNumber.Where(o=> o.Key.StartsWith("1")).Max(o=> o.Key),
                                    OrderNumber.Where(o=> o.Key.StartsWith("2")).Max(o=> o.Key)
                                });
                        }
                        SendMessage = new Statistics
                        {
                            Date = e.Date.Substring(0, 6),
                            Cumulative = (revenue + unrealize) / ts.Quantity,
                            Base = SendMessage.Base > Base / ts.Quantity ? SendMessage.Base : Base / ts.Quantity,
                            Statistic = (int)(avg / ts.Quantity)
                        };
                        SendStocks?.Invoke(this, new SendHoldingStocks(e.Date, e.Price, sShort, sLong, trend, revenue + unrealize, (long)(Base > 0 ? Base : 0)));
                        Before = avg;
                        TodayRevenue = revenue;
                        TodayUnrealize = unrealize;
                    }
                    break;
            }
        }
        public void StartProgress(double commission)
        {
            switch (strategics)
            {
                case ScenarioAccordingToTrend st:
                    Commission = commission > 0 ? commission : 1.5e-4;

                    if (StartProgress(strategics.Code as string) > 0)
                    {
                        var price = SendMessage.Price;
                        var estimate = EstimatedPrice.Where(o => o.Key.ToString(format.Substring(0, 6)).CompareTo(SendMessage.Date) > 0);
                        var find = FindTheNearestQuarter(SendMessage.Date);
                        var key = string.Concat("ST.", st.Calendar.Substring(0, 4), "15.", st.Trend, '.', st.CheckSales.ToString().Substring(0, 1), '.', st.Sales * 0x64, '.', st.CheckOperatingProfit.ToString().Substring(0, 1), '.', st.OperatingProfit * 0x64, '.', st.CheckNetIncome.ToString().Substring(0, 1), '.', st.NetIncome * 0x64);

                        if (client.PutContext(new Catalog.Request.Consensus
                        {
                            Code = st.Code,
                            Strategics = key,
                            Date = SendMessage.Date,
                            FirstQuarter = (estimate.Last(o => o.Key.ToString(format.Substring(0, 6)).Equals(find[0])).Value - price) / price,
                            SecondQuarter = (estimate.Last(o => o.Key.ToString(format.Substring(0, 6)).Equals(find[1])).Value - price) / price,
                            ThirdQuarter = (estimate.Last(o => o.Key.ToString(format.Substring(0, 6)).Equals(find[2])).Value - price) / price,
                            Quarter = (estimate.Last(o => o.Key.ToString(format.Substring(0, 6)).Equals(find[find.Length - 2])).Value - price) / price,
                            TheNextYear = (estimate.LastOrDefault(o => o.Key.ToString(format.Substring(0, 6)).Equals(find[find.Length - 1])).Value - price) / price,
                            TheYearAfterNext = (estimate.Last().Value - price) / price
                        }).Result > 0)
                            SendStocks?.Invoke(this, new SendHoldingStocks(EstimatedPrice, SendMessage.Date));

                        SendMessage = new Statistics
                        {
                            Base = SendMessage.Base,
                            Cumulative = SendMessage.Cumulative,
                            Date = SendMessage.Date,
                            Statistic = SendMessage.Statistic,
                            Price = (int)estimate.Max(o => o.Value),
                            Key = key
                        };
                        consecutive.Dispose();
                    }
                    break;

                case TrendsInStockPrices ts:
                    Commission = commission > 0 ? commission : 1.5e-4;

                    if (StartProgress(strategics.Code as string) > 0)
                    {
                        SendMessage = new Statistics
                        {
                            Base = SendMessage.Base,
                            Cumulative = SendMessage.Cumulative,
                            Date = SendMessage.Date,
                            Statistic = SendMessage.Statistic,
                            Key = string.Concat("TS.", ts.Short, '.', ts.Long, '.', ts.Trend, '.', (int)(ts.RealizeProfit * 0x2710), '.', (int)(ts.AdditionalPurchase * 0x2710), '.', ts.QuoteUnit, '.', (char)ts.LongShort, '.', (char)ts.TrendType, '.', (char)ts.Setting)
                        };
                        consecutive.Dispose();
                    }
                    break;

                case TrendFollowingBasicFutures _:
                    Commission = commission > 0 ? commission : 3e-5;

                    if (StartProgress(strategics.Code as string) > 0)
                    {
                        SendMessage = new Statistics
                        {
                            Base = SendMessage.Base,
                            Cumulative = SendMessage.Cumulative,
                            Date = SendMessage.Date,
                            Statistic = SendMessage.Statistic
                        };
                        foreach (var con in Consecutive)
                            con.Dispose();
                    }
                    break;

                default:
                    return;
            }
            SendBalance?.Invoke(this, new SendSecuritiesAPI(strategics, SendMessage));
        }
        public override string FindStrategicsCode(string code) => base.FindStrategicsCode(code);
        public HoldingStocks(TrendFollowingBasicFutures strategics) : base(strategics)
        {
            OrderNumber = new Dictionary<string, dynamic>();
            this.strategics = strategics;
        }
        public HoldingStocks(TrendsInStockPrices strategics) : base(strategics)
        {
            OrderNumber = new Dictionary<string, dynamic>();
            this.strategics = strategics;
        }
        public HoldingStocks(ScenarioAccordingToTrend strategics, Tuple<List<ConvertConsensus>, List<ConvertConsensus>> consensus, GoblinBatClient client) : base(strategics)
        {
            OrderNumber = new Dictionary<string, dynamic>();
            Consensus = consensus;
            this.strategics = strategics;
            this.client = client;
        }
        public override Tuple<List<ConvertConsensus>, List<ConvertConsensus>> Consensus
        {
            get; set;
        }
        public override string Code
        {
            get; set;
        }
        public override int Quantity
        {
            get; set;
        }
        public override dynamic Purchase
        {
            get; set;
        }
        public override dynamic Current
        {
            get; set;
        }
        public override dynamic Bid
        {
            get; set;
        }
        public override dynamic Offer
        {
            get; set;
        }
        public override long Revenue
        {
            get; set;
        }
        public override double Rate
        {
            get; set;
        }
        public override double Base
        {
            get; protected internal set;
        }
        public override double Secondary
        {
            get; protected internal set;
        }
        public override bool WaitOrder
        {
            get; set;
        }
        public override dynamic FindStrategics
        {
            get;
        }
        public override Dictionary<string, dynamic> OrderNumber
        {
            get;
        }
        protected internal override bool Market
        {
            get; set;
        }
        internal override Dictionary<DateTime, double> EstimatedPrice
        {
            get; set;
        }
        string[] FindTheNearestQuarter(string date)
        {
            if (int.TryParse(date.Substring(0, 2), out int year) && int.TryParse(date.Substring(2, 2), out int month))
                switch (month)
                {
                    case int first when first > 0 && first < 4:
                        return new string[] { string.Concat(date.Substring(0, 2), "0331"), string.Concat(date.Substring(0, 2), "0630"), string.Concat(date.Substring(0, 2), "0930"), string.Concat(date.Substring(0, 2), "1231"), string.Concat(year + 1, "1231") };

                    case int second when second > 3 && second < 7:
                        return new string[] { string.Concat(date.Substring(0, 2), "0630"), string.Concat(date.Substring(0, 2), "0930"), string.Concat(date.Substring(0, 2), "1231"), string.Concat(year + 1, "0331"), string.Concat(year + 2, "0331") };

                    case int third when third > 6 && third < 0xA:
                        return new string[] { string.Concat(date.Substring(0, 2), "0930"), string.Concat(date.Substring(0, 2), "1231"), string.Concat(year + 1, "0331"), string.Concat(year + 1, "0630"), string.Concat(year + 2, "0630") };

                    case int quarter when quarter > 9 && quarter < 0xD:
                        return new string[] { string.Concat(date.Substring(0, 2), "1231"), string.Concat(year + 1, "0331"), string.Concat(year + 1, "0630"), string.Concat(year + 1, "0930"), string.Concat(year + 2, "0930") };
                }
            return null;
        }
        double SetPurchasePrice(double price)
        {
            if (Math.Abs(VerifyAmount) < Math.Abs(Quantity) && Math.Abs(Quantity) > 0)
                Purchase = ((Purchase ?? 0D) * Math.Abs(VerifyAmount) + price) / Math.Abs(Quantity);

            return Quantity == 0 ? 0D : (Purchase ?? 0D);
        }
        double SetLiquidation(double price)
        {
            if (VerifyAmount > Quantity && Quantity > -1)
                return price - (Purchase ?? 0D);

            else if (VerifyAmount < Quantity && Quantity < 1)
                return (Purchase ?? 0D) - price;

            else
                return 0;
        }
        DateTime MeasureTheDelayTime(int delay, DateTime time) => time.AddSeconds(delay);
        string GetOrderNumber(int type) => string.Concat(type, Count++.ToString("D4"));
        [Conditional("DEBUG")]
        void IsDebugging() => Verify = true;
        EMA EMA
        {
            get;
        }
        Statistics SendMessage
        {
            get; set;
        }
        DateTime NextOrderTime
        {
            get; set;
        }
        int Accumulative
        {
            get; set;
        }
        int VerifyAmount
        {
            get; set;
        }
        uint Count
        {
            get; set;
        }
        uint CumulativeFee
        {
            get; set;
        }
        long TodayRevenue
        {
            get; set;
        }
        long TodayUnrealize
        {
            get; set;
        }
        bool Verify
        {
            get; set;
        }
        double Before
        {
            get; set;
        }
        double Commission
        {
            get; set;
        }
        const double tax = 2.5e-3;
        const string start = "0859";
        const string transmit = "1529";
        const string end = "1544";
        const string cme = "1759";
        const string format = "yyMMddHHmmss";
        readonly dynamic strategics;
        readonly GoblinBatClient client;
        public override event EventHandler<SendSecuritiesAPI> SendBalance;
        public override event EventHandler<SendHoldingStocks> SendStocks;
    }
}