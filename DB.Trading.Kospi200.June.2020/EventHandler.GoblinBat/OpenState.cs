﻿using System;

namespace ShareInvest.EventHandler
{
    public class OpenState : EventArgs
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
        public string ScreenNumber
        {
            get; private set;
        }
        public OpenState(bool receive, int sell, int quantity, int buy, uint number)
        {
            OnReceive = receive;
            SellOrderCount = sell.ToString();
            Quantity = quantity.ToString();
            BuyOrderCount = buy.ToString();
            ScreenNumber = number.ToString();
        }
    }
}