﻿using System;
using System.Text;
using ShareInvest.Catalog;
using ShareInvest.EventHandler;

namespace ShareInvest.XingAPI.Catalog
{
    internal class CCEAQ50600 : Query, IQuerys, IEvents<Balance>, IMessage<NotifyIconText>
    {
        internal CCEAQ50600() : base()
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
                    if (enumerable.Count < 11)
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
                    str += string.Concat(param[0], ';', ConnectAPI.GetInstance(string.Empty).CodeList[param[0]], ';', param[2], ';', param[4], ';', param[5], ';', param[6], ';', param[8], '*');
                }
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