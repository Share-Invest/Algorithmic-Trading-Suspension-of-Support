﻿using System.Linq;
using ShareInvest.Catalog;

namespace ShareInvest.Strategy.Statistics
{
    class Feather : Analysis
    {
        protected internal override bool ForTheLiquidationOfBuyOrder(double[] selling)
        {
            if (bt.Quantity < -1)
                return bt.SendNewOrder(selling[selling.Length - 1]);

            var sell = bt.SellOrder.OrderByDescending(o => o.Key).First();

            if (double.TryParse(sell.Key, out double csp) && selling[bt.SellOrder.Count == 1 ? 5 : (selling.Length - 1)] < csp)
                return bt.SendClearingOrder(sell.Value);

            return false;
        }
        protected internal override bool ForTheLiquidationOfBuyOrder(string price, double[] selling, int quantity)
        {
            if (double.TryParse(price, out double sAvg) && sAvg < selling[5])
                return bt.SendNewOrder(sAvg < selling[selling.Length - 1] ? selling[selling.Length - 1].ToString("F2") : price, sell, quantity);

            return false;
        }
        protected internal override bool ForTheLiquidationOfSellOrder(double[] bid)
        {
            if (bt.Quantity > 1)
                return bt.SendNewOrder(bid[bid.Length - 1]);

            var buy = bt.BuyOrder.OrderBy(o => o.Key).First();

            if (double.TryParse(buy.Key, out double cbp) && bid[bt.BuyOrder.Count == 1 ? 5 : (bid.Length - 1)] > cbp)
                return bt.SendClearingOrder(buy.Value);

            return false;
        }
        protected internal override bool ForTheLiquidationOfSellOrder(string price, double[] bid, int quantity)
        {
            if (double.TryParse(price, out double bAvg) && bAvg > bid[5])
                return bt.SendNewOrder(bAvg > bid[bid.Length - 1] ? bid[bid.Length - 1].ToString("F2") : price, buy, quantity);

            return false;
        }
        protected internal override void SendNewOrder(double[] param, string classification, int quantity)
        {
            var check = classification.Equals(buy);
            var price = param[5];
            var key = price.ToString("F2");

            if (price > 0 && (check ? bt.Quantity + bt.BuyOrder.Count : bt.SellOrder.Count - bt.Quantity) < Max(specify.Assets / (price * Const.TransactionMultiplier * specify.MarginRate), check ? XingAPI.Classification.Buy : XingAPI.Classification.Sell) && (check ? bt.BuyOrder.ContainsKey(key) : bt.SellOrder.ContainsKey(key)) == false)
                bt.SendNewOrder(key, classification, quantity);
        }
        protected internal override bool SetCorrectionBuyOrder(string avg, double buy, int quantity)
        {
            var order = bt.BuyOrder.OrderBy(o => o.Key).First();
            var sb = bt.BuyOrder.OrderByDescending(o => o.Key).First();

            if (bt.SellOrder.Count > 1 && double.TryParse(bt.SellOrder.OrderByDescending(o => o.Key).First().Key, out double bsp))
            {
                var benchmark = bsp + Const.ErrorRate * 2;

                if (benchmark < buy + Const.ErrorRate * 11)
                    return bt.SendCorrectionOrder(benchmark.ToString("F2"), bt.SellOrder.OrderBy(o => o.Key).First().Value, quantity);
            }
            if (double.TryParse(order.Key, out double oPrice) && double.TryParse(sb.Key, out double sPrice) && double.TryParse(avg, out double sAvg) && double.TryParse(GetExactPrice((((sAvg - Const.ErrorRate) * bt.Quantity + sPrice) / (bt.Quantity + 1)).ToString("F2")), out double prospect))
            {
                double check = prospect - Const.ErrorRate, abscond = oPrice - Const.ErrorRate, chase = sPrice + Const.ErrorRate, confirm = check - buy;

                if (0 < confirm && confirm < 0.2 * bt.Quantity && sAvg > check && bt.BuyOrder.ContainsKey(abscond.ToString("F2")) == false && sPrice > buy - Const.ErrorRate * 2)
                    return bt.SendCorrectionOrder(abscond.ToString("F2"), sb.Value, quantity);

                if (buy > check && buy < sAvg && bt.BuyOrder.ContainsKey(chase.ToString("F2")) == false && sPrice < buy - Const.ErrorRate * 5)
                    return bt.SendCorrectionOrder(chase.ToString("F2"), order.Value, quantity);
            }
            return false;
        }
        protected internal override bool SetCorrectionSellOrder(string avg, double sell, int quantity)
        {
            var order = bt.SellOrder.OrderByDescending(o => o.Key).First();
            var sb = bt.SellOrder.OrderBy(o => o.Key).First();

            if (bt.BuyOrder.Count > 1 && double.TryParse(bt.BuyOrder.OrderBy(o => o.Key).First().Key, out double bbp))
            {
                var benchmark = bbp - Const.ErrorRate * 2;

                if (benchmark > sell * Const.ErrorRate * 11)
                    return bt.SendCorrectionOrder(benchmark.ToString("F2"), bt.BuyOrder.OrderByDescending(o => o.Key).First().Value, quantity);
            }
            if (double.TryParse(order.Key, out double oPrice) && double.TryParse(sb.Key, out double sPrice) && double.TryParse(avg, out double bAvg) && double.TryParse(GetExactPrice(((sPrice - (bAvg + Const.ErrorRate) * bt.Quantity) / (1 - bt.Quantity)).ToString("F2")), out double prospect))
            {
                double check = prospect + Const.ErrorRate, abscond = oPrice + Const.ErrorRate, chase = sPrice - Const.ErrorRate, confirm = sell - check;

                if (0 < confirm && confirm < 0.2 * -bt.Quantity && bAvg < check && bt.SellOrder.ContainsKey(abscond.ToString("F2")) == false && sPrice < sell + Const.ErrorRate * 2)
                    return bt.SendCorrectionOrder(abscond.ToString("F2"), sb.Value, quantity);

                if (sell < check && sell > bAvg && bt.SellOrder.ContainsKey(chase.ToString("F2")) == false && sPrice > sell + Const.ErrorRate * 5)
                    return bt.SendCorrectionOrder(chase.ToString("F2"), order.Value, quantity);
            }
            return false;
        }
        internal Feather(BackTesting bt, Catalog.XingAPI.Specify specify) : base(bt, specify)
        {

        }
        double Max(double max, XingAPI.Classification classification)
        {
            var num = 5.5D;

            foreach (var kv in bt.Judge)
                switch (classification)
                {
                    case XingAPI.Classification.Sell:
                        if (kv.Value > 0)
                            num += 0.5;

                        break;

                    case XingAPI.Classification.Buy:
                        if (kv.Value < 0)
                            num += 0.5;

                        break;
                }
            return max * num * 0.1;
        }
    }
}