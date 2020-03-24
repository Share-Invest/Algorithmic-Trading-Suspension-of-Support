﻿using System;
using ShareInvest.Catalog;
using ShareInvest.EventHandler;

namespace ShareInvest.XingAPI.Catalog
{
    internal class O01 : Real, IReals, IStates<State>
    {
        internal O01() : base()
        {
            Console.WriteLine(GetType().Name);
        }
        protected override void OnReceiveRealData(string szTrCode)
        {
            string[] arr = Enum.GetNames(typeof(CMO)), temp = new string[arr.Length];

            for (int i = 0; i < arr.Length - 1; i++)
                temp[i] = GetFieldData(OutBlock, arr[i]);

            if (temp[8].Equals(Enum.GetName(typeof(TR), TR.SONBT001)) && double.TryParse(temp[60], out double price))
            {
                switch (temp[55])
                {
                    case sell:
                        API.SellOrder[temp[45]] = price;
                        break;

                    case buy:
                        API.BuyOrder[temp[45]] = price;
                        break;
                }
                SendState?.Invoke(this, new State(API.OnReceiveBalance = true, API.SellOrder.Count, API.Quantity, API.BuyOrder.Count, API.AvgPurchase));
            }
        }
        public void OnReceiveRealTime(string code)
        {
            if (LoadFromResFile(new Secret().GetResFileName(GetType().Name)))
                AdviseRealData();
        }
        public event EventHandler<State> SendState;
    }
}