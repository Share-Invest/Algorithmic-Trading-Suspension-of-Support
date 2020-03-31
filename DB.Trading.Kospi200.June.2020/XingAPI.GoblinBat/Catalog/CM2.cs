﻿using System;
using ShareInvest.Catalog;
using ShareInvest.EventHandler;

namespace ShareInvest.XingAPI.Catalog
{
    internal class CM2 : Real, IReals, IStates<State>
    {
        internal CM2() : base()
        {
            Console.WriteLine(GetType().Name);
        }
        protected override void OnReceiveRealData(string szTrCode)
        {
            string[] arr = Enum.GetNames(typeof(CMO)), temp = new string[arr.Length];

            for (int i = 0; i < arr.Length - 1; i++)
                temp[i] = GetFieldData(OutBlock, arr[i]);

            if (temp[56].Equals(sell) && uint.TryParse(temp[45], out uint number) && double.TryParse(temp[60], out double price))
                switch (temp[55])
                {
                    case sell:
                        API.SellOrder[number.ToString()] = price;
                        break;

                    case buy:
                        API.BuyOrder[number.ToString()] = price;
                        break;
                }
            SendState?.Invoke(this, new State(API.OnReceiveBalance = true, API.SellOrder.Count, API.Quantity, API.BuyOrder.Count, API.AvgPurchase, API.MaxAmount));
        }
        public void OnReceiveRealTime(string code)
        {
            if (LoadFromResFile(new Secret().GetResFileName(GetType().Name)))
                AdviseRealData();
        }
        public event EventHandler<State> SendState;
    }
}