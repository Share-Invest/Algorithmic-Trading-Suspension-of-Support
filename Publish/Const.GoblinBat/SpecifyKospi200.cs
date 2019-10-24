﻿using ShareInvest.Communicate;

namespace ShareInvest.Const
{
    public class SpecifyKospi200 : IStrategy
    {
        public SpecifyKospi200()
        {
            Type = (int)IStrategy.Futures.Kospi200;
            TransactionMultiplier = 250000;
            MarginRate = 7.65e-2;
            Commission = 3e-5;
            ErrorRate = 5e-2;
            Activate = true;
        }
        public int Type
        {
            get; private set;
        }
        public int Reaction
        {
            get; set;
        }
        public int ShortMinPeriod
        {
            get; set;
        }
        public int LongMinPeriod
        {
            get; set;
        }
        public int ShortDayPeriod
        {
            get; set;
        }
        public int LongDayPeriod
        {
            get; set;
        }
        public int TransactionMultiplier
        {
            get; private set;
        }
        public int StopLoss
        {
            get; set;
        }
        public int Revenue
        {
            get; set;
        }
        public long BasicAssets
        {
            get; set;
        }
        public bool Division
        {
            get; set;
        }
        public bool Activate
        {
            get; set;
        }
        public double MarginRate
        {
            get; private set;
        }
        public double Commission
        {
            get; private set;
        }
        public double ErrorRate
        {
            get; private set;
        }
        public string Strategy
        {
            get; set;
        }
        public IStopLossAndRevenue.StopLossAndRevenue Stop
        {
            get; set;
        }
        public int SetActivate(int quantity, double price, double purchase)
        {
            if (quantity > 0 && price - purchase > Revenue * ErrorRate && (Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.UseAll) || Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.OnlyRevenue)))
            {
                Activate = false;
                return -1;
            }
            if (quantity > 0 && purchase - price > StopLoss * ErrorRate && (Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.UseAll) || Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.OnlyStopLoss)))
            {
                Activate = false;
                return -1;
            }
            if (quantity < 0 && purchase - price > Revenue * ErrorRate && (Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.UseAll) || Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.OnlyRevenue)))
            {
                Activate = false;
                return 1;
            }
            if (quantity < 0 && price - purchase > StopLoss * ErrorRate && (Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.UseAll) || Stop.Equals(IStopLossAndRevenue.StopLossAndRevenue.OnlyStopLoss)))
            {
                Activate = false;
                return 1;
            }
            return 0;
        }
    }
}