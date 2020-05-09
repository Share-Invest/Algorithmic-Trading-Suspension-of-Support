﻿using System;
using System.Linq;
using ShareInvest.Catalog;

namespace ShareInvest.Strategy.Statistics
{
    class Heavy : Analysis
    {
        protected internal override bool ForTheLiquidationOfBuyOrder(string price, double[] selling, int quantity)
        {
            if (double.TryParse(price, out double sAvg) && sAvg == selling[selling.Length - 1] && bt.SendNewOrder(price, sell, quantity))
            {
                var cPrice = sAvg - Const.ErrorRate;
                var oPrice = cPrice.ToString("F2");

                if (bt.BuyOrder.Count == 0 && bt.Quantity + 1 < specify.Assets / (cPrice * Const.TransactionMultiplier * specify.MarginRate))
                    return bt.SendNewOrder(oPrice, buy, quantity);

                if (bt.BuyOrder.Count > 0 && bt.BuyOrder.ContainsKey(oPrice) == false)
                    return bt.SendCorrectionOrder(oPrice, bt.BuyOrder.OrderBy(o => o.Key).First().Value, quantity);
            }
            return false;
        }
        protected internal override bool ForTheLiquidationOfSellOrder(string price, double[] bid, int quantity)
        {
            if (double.TryParse(price, out double bAvg) && bAvg == bid[bid.Length - 1] && bt.SendNewOrder(price, buy, quantity))
            {
                var cPrice = bAvg + Const.ErrorRate;
                var oPrice = cPrice.ToString("F2");

                if (bt.SellOrder.Count == 0 && Math.Abs(bt.Quantity - 1) < specify.Assets / (cPrice * Const.TransactionMultiplier * specify.MarginRate))
                    return bt.SendNewOrder(oPrice, sell, quantity);

                if (bt.SellOrder.Count > 0 && bt.SellOrder.ContainsKey(oPrice) == false)
                    return bt.SendCorrectionOrder(oPrice, bt.SellOrder.OrderByDescending(o => o.Key).First().Value, quantity);
            }
            return false;
        }
        protected internal override bool ForTheLiquidationOfBuyOrder(double[] selling)
        {
            var sell = bt.SellOrder.OrderBy(o => o.Key).First();

            return double.TryParse(sell.Key, out double csp) && selling[bt.SellOrder.Count == 1 ? 3 : (selling.Length - 1)] < csp ? bt.SendClearingOrder(sell.Value) : false;
        }
        protected internal override bool ForTheLiquidationOfSellOrder(double[] bid)
        {
            var buy = bt.BuyOrder.OrderByDescending(o => o.Key).First();

            return double.TryParse(buy.Key, out double cbp) && bid[bt.BuyOrder.Count == 1 ? 3 : (bid.Length - 1)] > cbp ? bt.SendClearingOrder(buy.Value) : false;
        }
        protected internal override void SendNewOrder(double[] param, string classification, int quantity)
        {
            var check = classification.Equals(buy);
            var price = param[5];
            string key = price.ToString("F2"), oPrice = param[param.Length - 1].ToString("F2");

            if (check && bt.Quantity < 0 && param[param.Length - 1] > 0 && bt.BuyOrder.Count < -bt.Quantity && bt.BuyOrder.ContainsKey(oPrice) == false && bt.SendNewOrder(oPrice, buy, quantity))
                return;

            else if (check == false && bt.Quantity > 0 && param[param.Length - 1] > 0 && bt.SellOrder.Count < bt.Quantity && bt.SellOrder.ContainsKey(oPrice) == false && bt.SendNewOrder(oPrice, sell, quantity))
                return;

            else if (price > 0 && GetPermission(price) && (check ? bt.Quantity + bt.BuyOrder.Count : bt.SellOrder.Count - bt.Quantity) < Max(specify.Assets / (price * Const.TransactionMultiplier * specify.MarginRate), check ? XingAPI.Classification.Buy : XingAPI.Classification.Sell) && (check ? bt.BuyOrder.ContainsKey(key) : bt.SellOrder.ContainsKey(key)) == false && bt.SendNewOrder(key, classification, quantity))
                return;
        }
        protected internal override bool SetCorrectionBuyOrder(string avg, double buy, int quantity)
        {
            var price = string.Empty;
            var gap = double.TryParse(avg, out double bAvg) && bAvg > buy ? Const.ErrorRate * 2 : Const.ErrorRate * 3;

            foreach (var kv in bt.BuyOrder.OrderByDescending(o => o.Key))
            {
                if (string.IsNullOrEmpty(price) == false && double.TryParse(kv.Key, out double oPrice) && (oPrice + Const.ErrorRate).ToString("F2").Equals(price) && double.TryParse(bt.BuyOrder.OrderBy(o => o.Key).First().Key, out double cPrice))
                    return bt.SendCorrectionOrder((cPrice - gap).ToString("F2"), kv.Value, quantity);

                price = kv.Key;
            }
            return false;
        }
        protected internal override bool SetCorrectionSellOrder(string avg, double sell, int quantity)
        {
            var price = string.Empty;
            var gap = double.TryParse(avg, out double sAvg) && sAvg < sell ? Const.ErrorRate * 2 : Const.ErrorRate * 3;

            foreach (var kv in bt.SellOrder.OrderBy(o => o.Key))
            {
                if (string.IsNullOrEmpty(price) == false && double.TryParse(kv.Key, out double oPrice) && (oPrice - Const.ErrorRate).ToString("F2").Equals(price) && double.TryParse(bt.SellOrder.OrderByDescending(o => o.Key).First().Key, out double cPrice))
                    return bt.SendCorrectionOrder((cPrice + gap).ToString("F2"), kv.Value, quantity);

                price = kv.Key;
            }
            return false;
        }
        bool GetPermission(double price)
        {
            if (bt.BuyOrder.Count > 0 && bt.Quantity > 0)
            {
                var cPrice = (price - Const.ErrorRate).ToString("F2");

                if (double.TryParse(cPrice, out double bPrice) && double.TryParse(bt.BuyOrder.OrderBy(o => o.Key).First().Key, out double bKey) && bPrice > bKey && bt.BuyOrder.ContainsKey(cPrice))
                    return false;
            }
            if (bt.SellOrder.Count > 0 && bt.Quantity < 0)
            {
                var cPrice = (price + Const.ErrorRate).ToString("F2");

                if (double.TryParse(cPrice, out double sPrice) && double.TryParse(bt.SellOrder.OrderByDescending(o => o.Key).First().Key, out double sKey) && sPrice < sKey && bt.SellOrder.ContainsKey(cPrice))
                    return false;
            }
            return true;
        }
        double Max(double max, XingAPI.Classification classification)
        {
            var num = 4.5D;

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
        internal Heavy(BackTesting bt, Catalog.XingAPI.Specify specify) : base(bt, specify) => Console.WriteLine(specify.Strategy);
    }
}