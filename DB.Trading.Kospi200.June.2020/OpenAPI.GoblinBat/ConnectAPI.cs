﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxKHOpenAPILib;
using ShareInvest.Catalog;
using ShareInvest.DelayRequest;
using ShareInvest.Interface;
using ShareInvest.Message;

namespace ShareInvest.OpenAPI
{
    public class ConnectAPI : AuxiliaryFunction
    {
        public void SetAPI(AxKHOpenAPI axAPI)
        {
            API = axAPI;
            axAPI.OnEventConnect += OnEventConnect;
            axAPI.OnReceiveTrData += OnReceiveTrData;
            axAPI.OnReceiveRealData += OnReceiveRealData;
            axAPI.OnReceiveMsg += OnReceiveMsg;
        }
        public void StartProgress()
        {
            if (API != null)
            {
                ErrorCode = API.CommConnect();

                if (ErrorCode != 0)
                    new Error(ErrorCode);

                return;
            }
            Environment.Exit(0);
        }
        public static ConnectAPI GetInstance()
        {
            if (api == null)
                api = new ConnectAPI();

            return api;
        }
        private void OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
        }
        private void OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
        }
        private void OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sTrCode.Contains("KOA"))
                return;

            Sb = new StringBuilder(512);
            int i, cnt = API.GetRepeatCnt(e.sTrCode, e.sRQName);

            for (i = 0; i < (cnt > 0 ? cnt : cnt + 1); i++)
            {
                foreach (string item in Array.Find(catalog, o => o.ToString().Contains(e.sTrCode.Substring(1))))
                    Sb.Append(API.GetCommData(e.sTrCode, e.sRQName, i, item).Trim()).Append(';');

                if (cnt > 0)
                    Sb.Append("*");
            }
            switch (Array.FindIndex(catalog, o => o.ToString().Contains(e.sTrCode.Substring(1))))
            {
                case 0:
                    FixUp(Sb.ToString().Split(';'), e.sRQName);
                    break;

                case 1:
                    foreach (string info in Sb.ToString().Split('*'))
                        FixUp(info.Split(';'));

                    break;
            }
        }
        private void OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            int i, l;
            string exclusion, date = GetDistinctDate(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) - CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now.AddDays(1 - DateTime.Now.Day), CalendarWeekRule.FirstDay, DayOfWeek.Sunday) + 1);
            Code = new List<string>
            {
                API.GetFutureCodeByIndex(e.nErrCode)
            };
            for (i = 2; i < 4; i++)
                foreach (var om in API.GetActPriceList().Split(';'))
                {
                    exclusion = API.GetOptionCode(om.Insert(3, "."), i, date);

                    if (Code.Exists(o => o.Equals(exclusion)))
                        continue;

                    Code.Add(exclusion);
                }
            Code[1] = API.GetFutureCodeByIndex(24);
            string[] temp, market = API.GetCodeListByMarket("").Split(';');
            l = market.Length;

            foreach (string output in Code)
                RemainingDay(output);

            foreach (string sMarket in new CodeListByMarket())
            {
                temp = API.GetCodeListByMarket(sMarket).Split(';');

                for (i = 0; i < l; i++)
                    if (Array.Exists(temp, o => o.Equals(market[i])))
                        market[i] = string.Empty;
            }
            for (i = 0; i < l; i++)
            {
                string tempCode = market[i];

                if (API.GetMasterStockState(tempCode).Contains("거래정지"))
                {
                    market[i] = string.Empty;

                    continue;
                }
                if (tempCode.Length > 0)
                {
                    foreach (string ex in new CodeListByExclude())
                        if (API.GetMasterCodeName(tempCode).EndsWith(ex) && !Array.Exists(exclude, o => o.Equals(tempCode)))
                            market[i] = string.Empty;

                    continue;
                }
            }
            foreach (string output in SetCodeStorage(market))
                RemainingDay(output);

            if (DateTime.Now.Hour > 7 && DateTime.Now.Hour < 16 && (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Saturday) || DateTime.Now.DayOfWeek.Equals(DayOfWeek.Sunday)) == false)
                return;

            else if (TimerBox.Show("Waiting to Receive. . .", "Caution", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, (uint)Code.Count * 875).Equals(DialogResult.OK))
            {
                Code.Clear();
                Code = RequestCodeList(Code);
                Delay.delay = 3615;
                Request(GetRandomCode(new Random().Next(0, Code.Count)));
            }
        }
        private string GetRandomCode(int index)
        {
            if (Code.Contains(Code[index]))
            {
                var temp = Code[index];
                Code.RemoveAt(index);

                return temp;
            }
            return GetRandomCode(new Random(index).Next(0, Code.Count));
        }
        private void Request(string code)
        {
            int param = code.Length > 6 ? (code.Contains("101") ? 0 : 0) : 0;
            ITR tr = (ITR)catalog[param];
            tr.Value = code;
            tr.RQName = string.Concat(code, Retention(param, code));
            tr.PrevNext = 0;
            request.RequestTrData(new Task(() => InputValueRqData(tr)));
        }
        private void FixUp(string[] param, string code)
        {
            SetInsertCode(code, param[72], param[63]);

            if (code.Contains("101") && param[63].Equals(DateTime.Now.ToString("yyyyMMdd")))
                RemainingDate = param[63];
        }
        private void RemainingDay(string code)
        {
            if (code.Length < 9 && code.Length > 6)
            {
                request.RequestTrData(new Task(() => InputValueRqData(new Opt50001 { Value = code, RQName = code })));

                return;
            }
            request.RequestTrData(new Task(() =>
            {
                ITR tr = new OPTKWFID { Value = code };
                ErrorCode = API.CommKwRqData(tr.Value, 0, 100, tr.PrevNext, tr.RQName, tr.ScreenNo);

                if (ErrorCode < 0)
                    new Error(ErrorCode);
            }));
        }
        private void InputValueRqData(ITR param)
        {
            string[] count = param.ID.Split(';'), value = param.Value.Split(';');
            int i, l = count.Length;

            for (i = 0; i < l; i++)
                API.SetInputValue(count[i], value[i]);

            ErrorCode = API.CommRqData(param.RQName, param.TrCode, param.PrevNext, param.ScreenNo);

            if (ErrorCode < 0)
                new Error(ErrorCode);
        }
        private int ErrorCode
        {
            get; set;
        }
        private bool DeadLine
        {
            get; set;
        }
        private string RemainingDate
        {
            get; set;
        }
        private ConnectAPI()
        {
            request = Delay.GetInstance(605);
            request.Run();
        }
        private List<string> Code
        {
            get; set;
        }
        private StringBuilder Sb
        {
            get; set;
        }
        private AxKHOpenAPI API
        {
            get; set;
        }
        private static ConnectAPI api;
        private readonly Delay request;
    }
}