﻿using System;
using System.Diagnostics;
using ShareInvest.DelayRequest;
using ShareInvest.EventHandler;
using ShareInvest.Struct;
using XA_DATASETLib;
using System.Threading.Tasks;
using System.Text;

namespace ShareInvest.XingAPI
{
    public class ConnectXingAPI : Catalog
    {
        public void StartProgress(string path, string[] accounts)
        {
            if (Query != null && Real != null && Query.LoadFromResFile(string.Concat(path, "t9943.res")))
            {
                foreach (var block in new T9943().GetInBlock(Query.GetResData()))
                    Query.SetFieldData(block.Name, block.Field, block.Occurs, block.Property);

                Account = accounts[0];
                Delay.delay = 1000 / Query.GetTRCountPerSec("t9943");
                request.RequestTrData(new Task(() =>
                {
                    if (Query.Request(false) < 0)
                        Console.WriteLine("request");
                }));
                return;
            }
            Process.Start("shutdown.exe", "-r");
            Dispose();
        }
        public void SetAPI(XAQueryClass query)
        {
            Query = query;
            query.ReceiveData += OnReceiveData;
            query.ReceiveMessage += OnReceiveMessage;
        }
        public void SetAPI(XARealClass real)
        {
            Real = real;
            real.ReceiveRealData += OnReceiveRealData;
        }
        public static ConnectXingAPI GetInstance()
        {
            if (api == null)
                api = new ConnectXingAPI();

            return api;
        }
        private void OnReceiveRealData(string szTrCode)
        {

        }
        private void OnReceiveMessage(bool bIsSystemError, string nMessageCode, string szMessage)
        {

        }
        private void OnReceiveData(string szTrCode)
        {
            Sb = new StringBuilder(128);

            foreach (var block in Array.Find(catalog, o => o.ToString().Contains(szTrCode.Substring(1))).GetOutBlock(Query.GetResData()))
                for (int i = 0; i < Query.GetBlockCount(block.Name); i++)
                    Sb.Append(Query.GetFieldData(block.Name, block.Field, i)).Append(';');

            switch (szTrCode)
            {
                case "t9943":
                    foreach (var str in Sb.ToString().Split(';'))
                        if (str.Substring(0, 3).Equals("101"))
                        {
                            Code = str;
                            SendCount?.Invoke(this, new NotifyIconText(Query.GetAccountName(Account), Query.GetAcctDetailName(Account), Query.GetAcctNickname(Account), str));

                            return;
                        }
                    return;
            }
        }
        private void Dispose()
        {

        }
        private string Code
        {
            get; set;
        }
        private string Account
        {
            get; set;
        }
        private StringBuilder Sb
        {
            get; set;
        }
        private ConnectXingAPI()
        {
            request = Delay.GetInstance(501);
            request.Run();
        }
        private XARealClass Real
        {
            get; set;
        }
        private XAQueryClass Query
        {
            get; set;
        }
        private static ConnectXingAPI api;
        private readonly Delay request;
        public event EventHandler<Datum> SendDatum;
        public event EventHandler<Memorize> SendMemorize;
        public event EventHandler<NotifyIconText> SendCount;
        public event EventHandler<Quotes> SendQuotes;
        public event EventHandler<Deposit> SendDeposit;
        public event EventHandler<Balance> SendBalance;
        public event EventHandler<Current> SendCurrent;
        public event EventHandler<State> SendState;
        public event EventHandler<Trends> SendTrend;
    }
}