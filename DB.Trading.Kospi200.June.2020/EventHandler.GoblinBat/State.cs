﻿using System;

namespace ShareInvest.EventHandler
{
    public class State : EventArgs
    {
        public bool OnReceive
        {
            get; private set;
        }
        public string SellOrderCount
        {
            get; private set;
        }
        public string Quantity
        {
            get; private set;
        }
        public string BuyOrderCount
        {
            get; private set;
        }
        public State(bool receive, int sell, int quantity, int buy)
        {
            OnReceive = receive;
            SellOrderCount = sell.ToString();
            Quantity = quantity.ToString();
            BuyOrderCount = buy.ToString();
        }
    }
}