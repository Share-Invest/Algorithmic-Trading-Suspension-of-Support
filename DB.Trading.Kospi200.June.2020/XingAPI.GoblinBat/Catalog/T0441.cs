﻿using System;
using System.Text;
using ShareInvest.Catalog;
using ShareInvest.EventHandler;

namespace ShareInvest.XingAPI.Catalog
{
    internal class T0441 : Query, IQuerys, IEvents<Balance>, IMessage<NotifyIconText>
    {
        internal T0441() : base()
        {
            Console.WriteLine(GetType().Name);
        }
        protected override void OnReceiveMessage(bool bIsSystemError, string nMessageCode, string szMessage)
        {
            base.OnReceiveMessage(bIsSystemError, nMessageCode, szMessage);
            SendMessage?.Invoke(this, new NotifyIconText(szMessage));
        }
        protected override void OnReceiveData(string szTrCode)
        {
            var enumerable = GetOutBlocks();
            var temp = new StringBuilder[enumerable.Count];
            string str = string.Empty;

            while (enumerable.Count > 0)
            {
                var param = enumerable.Dequeue();

                for (int i = 0; i < GetBlockCount(param.Block); i++)
                    if (enumerable.Count < 13)
                    {
                        if (temp[i] == null)
                            temp[i] = new StringBuilder();

                        temp[i] = temp[i].Append(GetFieldData(param.Block, param.Field, i)).Append(';');
                    }
            }
            foreach (var sb in temp)
                if (sb != null)
                {
                    var param = sb.ToString().Split(';');
                    str += string.Concat(param[0], ';', API.CodeList[param[0]], ';', param[6], ';', param[2], ';', param[4], ';', param[9], ';', param[11], '*');

                    if (param[0].Length == 8 && param[0].Substring(0, 3).Equals("101"))
                    {
                        API.Quantity = param[6].Equals("1") ? -int.Parse(param[2]) : int.Parse(param[2]);
                        API.AvgPurchase = param[4];
                    }
                }
                else if (API.Quantity == 0)
                    API.AvgPurchase = "000.00";

            Send.Invoke(this, new Balance(str.Split('*')));
        }
        public void QueryExcute()
        {
            if (LoadFromResFile(new Secret().GetResFileName(GetType().Name)))
            {
                foreach (var param in GetInBlocks(GetType().Name))
                    SetFieldData(param.Block, param.Field, param.Occurs, param.Data);

                SendErrorMessage(Request(false));
            }
        }
        public event EventHandler<Balance> Send;
        public event EventHandler<NotifyIconText> SendMessage;
    }
}