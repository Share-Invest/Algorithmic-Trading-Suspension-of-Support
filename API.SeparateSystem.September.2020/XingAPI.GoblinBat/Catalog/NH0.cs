﻿using System;

using ShareInvest.Interface.XingAPI;

namespace ShareInvest.XingAPI.Catalog
{
    class NH0 : Real, IReals
    {
        protected internal override void OnReceiveRealData(string szTrCode)
        {
            int index = 0;
            string str;
            var price = new double[10];
            var quantity = new int[20];
            var time = new string[6];

            foreach (var param in Enum.GetValues(typeof(H)))
            {
                int temp = (int)param, i = temp % 2 == 1 ? 5 : 1;

                if (temp < 7)
                {
                    if (temp == 3)
                        index = 0;

                    while (i < 6 && i > 0)
                    {
                        str = GetFieldData(OutBlock, string.Concat(param, temp % 2 == 1 ? i-- : i++));

                        if (temp < 3 && double.TryParse(str, out double pr))
                            price[index++] = pr;

                        else if (int.TryParse(str, out int qt))
                            quantity[index++] = qt;
                    }
                    continue;
                }
                str = GetFieldData(OutBlock, param.ToString());

                if (temp == 7)
                    index = 0;

                if (str.Equals(string.Empty) == false && index < 6)
                    time[index++] = str;
            }
        }
        public void OnReceiveRealTime(string code)
        {
            if (LoadFromResFile(Secrecy.GetResFileName(GetType().Name)))
            {
                var param = GetInBlock(code);
                SetFieldData(param.Block, param.Field, param.Data);
                AdviseRealData();
            }
        }
    }
}