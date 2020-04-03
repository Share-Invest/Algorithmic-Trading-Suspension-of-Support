﻿namespace ShareInvest.Strategy
{
    internal partial struct EMA
    {
        internal double Make(int period, int count, double price, double before)
        {
            var k = K(count, period);

            return price * k + before * (1 - k);
        }
        internal double Make(double price) => price;
        double K(int count, int period) => count > period ? 2 / (double)(period + 1) : 2 / (double)(count + 1);
    }
}