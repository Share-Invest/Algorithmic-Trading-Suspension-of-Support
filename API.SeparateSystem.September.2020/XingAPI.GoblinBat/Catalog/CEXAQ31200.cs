using System;
using System.Text;

using ShareInvest.Analysis;
using ShareInvest.EventHandler;
using ShareInvest.Interface.XingAPI;

namespace ShareInvest.XingAPI.Catalog
{
    class CEXAQ31200 : Query, IQuerys<SendSecuritiesAPI>
    {
        protected internal override void OnReceiveMessage(bool bIsSystemError, string nMessageCode, string szMessage)
        {
            base.OnReceiveMessage(bIsSystemError, nMessageCode, szMessage);
            SendMessage(szMessage.Trim());
        }
        protected internal override void OnReceiveData(string szTrCode)
        {
            var enumerable = GetOutBlocks();
            var temp = new StringBuilder[enumerable.Count];
            string deposit = string.Empty, available = string.Empty;

            while (enumerable.Count > 0)
            {
                var param = enumerable.Dequeue();

                for (int i = 0; i < GetBlockCount(param.Block); i++)
                    if (enumerable.Count < 0xC)
                    {
                        if (temp[i] == null)
                            temp[i] = new StringBuilder();

                        temp[i] = temp[i].Append(GetFieldData(param.Block, param.Field, i)).Append(';');
                    }
                    else if (enumerable.Count == 0x23)
                        deposit = GetFieldData(param.Block, param.Field, i);

                    else if (enumerable.Count == 0x1C)
                        available = GetFieldData(param.Block, param.Field, i);
            }
            foreach (var sb in temp)
                if (sb != null)
                {
                    var param = sb.ToString().Split(';');
                    var sAPI
                        = new SendSecuritiesAPI(new string[] { param[0], param[1], param[2], string.Empty, param[4], param[5], param[6], string.Empty, param[8], param[9] });

                    if (sAPI.Convey is Tuple<string, string, int, dynamic, dynamic, long, double> balance
                        && Connect.HoldingStock.TryGetValue(balance.Item1, out Holding hs))
                    {
                        hs.Quantity = balance.Item3;
                        hs.Purchase = (double)balance.Item4;
                        hs.Current = (double)balance.Item5;
                        hs.Revenue = balance.Item6;
                        hs.Rate = balance.Item7;
                        Connect.HoldingStock[balance.Item1] = hs;
                    }
                    Send?.Invoke(this, sAPI);
                }
            Send?.Invoke(this, new SendSecuritiesAPI(deposit, available));
            Send?.Invoke(this, new SendSecuritiesAPI(false));
        }
        public void QueryExcute()
        {
            if (LoadFromResFile(Secrecy.GetResFileName(GetType().Name)))
            {
                foreach (var param in GetInBlocks(GetType().Name))
                    SetFieldData(param.Block, param.Field, param.Occurs, param.Data);

                SendErrorMessage(GetType().Name, Request(false));
            }
        }
        public event EventHandler<SendSecuritiesAPI> Send;
    }
}