﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using AxKHOpenAPILib;

using ShareInvest.Catalog;
using ShareInvest.Catalog.OpenAPI;
using ShareInvest.Controls;
using ShareInvest.EventHandler;
using ShareInvest.OpenAPI.Catalog;

namespace ShareInvest.OpenAPI
{
    public sealed partial class ConnectAPI : UserControl, ISecuritiesAPI
    {
        void ButtonStartProgressClick(object sender, EventArgs e) => BeginInvoke(new Action(() =>
        {
            Start = true;
            axAPI.OnEventConnect += OnEventConnect;
            axAPI.OnReceiveMsg += OnReceiveMsg;
            API = Connect.GetInstance(axAPI);
        }));
        void OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e) => BeginInvoke(new Action(() =>
        {
            if (e.nErrCode == 0 && (string.IsNullOrEmpty(privacy.SecurityAPI) == false || string.IsNullOrEmpty(axAPI.KOA_Functions(showAccountWindow, string.Empty))))
                Send?.Invoke(this, new SendSecuritiesAPI(FormWindowState.Minimized, securites != null && securites.Length == 0xA ? new Accounts(securites) : new Accounts(axAPI.GetLoginInfo(account))));

            else
                (API as Connect)?.SendErrorMessage(e.nErrCode);
        }));
        void OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (Connect.Chejan.TryGetValue(e.sGubun, out Chejan chejan))
                chejan.OnReceiveChejanData(e);
        }
        void OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e) => Connect.TR.FirstOrDefault(o => (o.RQName != null ? o.RQName.Equals(e.sRQName) : o.PrevNext.ToString().Equals(e.sPrevNext)) && o.GetType().Name.Substring(1).Equals(e.sTrCode.Substring(1)))?.OnReceiveTrData(e);
        void OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e) => Connect.Real.FirstOrDefault(o => o.GetType().Name.Replace('_', ' ').Equals(e.sRealType))?.OnReceiveRealData(e);
        void OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e) => Send?.BeginInvoke(this, new SendSecuritiesAPI(string.Concat("[", e.sRQName, "] ", e.sMsg.Substring(9), "(", e.sScrNo, ")")), null, null);
        [Conditional("DEBUG")]
        void SendMessage(string code, string message) => Console.WriteLine(code + "\t" + message);
        TR GetRequestTR(string name) => Connect.TR.FirstOrDefault(o => o.GetType().Name.Equals(name)) ?? null;
        public AccountInformation SetPrivacy(Privacies privacy)
        {
            if (Connect.TR.Add(new OPT50010 { PrevNext = 0, API = axAPI }) && Connect.TR.Add(new Opw00005 { Value = string.Concat(privacy.AccountNumber, password), PrevNext = 0, API = axAPI }))
            {
                axAPI.OnReceiveTrData += OnReceiveTrData;
                axAPI.OnReceiveRealData += OnReceiveRealData;
                axAPI.OnReceiveChejanData += OnReceiveChejanData;
            }
            string mServer = axAPI.GetLoginInfo(server), log = axAPI.GetLoginInfo(name);
            checkAccount.CheckState = mServer.Equals(mock) && checkAccount.Checked ? CheckState.Unchecked : CheckState.Checked;
            Invoke(new Action(async () =>
            {
                if (checkAccount.Checked && await new Security().Encrypt(this.privacy, privacy.AccountNumber, checkAccount.Checked) == 0xC8)
                    Console.WriteLine(log);
            }));
            var aInfo = new AccountInformation
            {
                Identity = axAPI.GetLoginInfo(user),
                Account = privacy.AccountNumber,
                Name = string.Empty,
                Server = mServer.Equals(mock),
                Nick = log
            };
            switch (privacy.AccountNumber.Substring(privacy.AccountNumber.Length - 2))
            {
                case "31":
                    aInfo.Name = "선물옵션";
                    break;

                default:
                    aInfo.Name = "위탁종합";
                    break;
            }
            return aInfo;
        }
        public IEnumerable<string> InputValueRqData()
        {
            if (Connect.TR.Add(new OPTKWFID { PrevNext = 0, API = axAPI }) && API is Connect api)
                foreach (var code in api.GetInformationOfCode(new List<string> { axAPI.GetFutureCodeByIndex(0) }, axAPI.GetCodeListByMarket(string.Empty).Split(';')))
                    yield return code;
        }
        public ISendSecuritiesAPI InputValueRqData(string name, string sArrCode, int nCodeCount)
        {
            var ctor = GetRequestTR(name);

            if (string.IsNullOrEmpty(sArrCode) == false && nCodeCount > 0 && API is Connect api)
            {
                ctor.Value = sArrCode;
                api.InputValueRqData(nCodeCount, ctor);
            }
            return ctor ?? null;
        }
        public ISendSecuritiesAPI InputValueRqData(string name, string param)
        {
            TR ctor;

            if (name.Length > 0x1F)
            {
                ctor = Assembly.GetExecutingAssembly().CreateInstance(name) as TR;

                if (Connect.TR.Add(ctor))
                {
                    ctor.Value = param;
                    ctor.RQName = param;
                    ctor.API = axAPI;
                    (API as Connect)?.InputValueRqData(ctor);
                }
            }
            else
            {
                ctor = Connect.TR.FirstOrDefault(o => o.GetType().Name.Substring(1).Equals(name.Substring(1)) && o.RQName.Contains(param));

                if (Connect.TR.Remove(ctor))
                    SendMessage(param, name);
            }
            return ctor ?? null;
        }
        public ISendSecuritiesAPI InputValueRqData(bool input, string name)
        {
            var ctor = GetRequestTR(name);

            if (input)
                BeginInvoke(new Action(() => (API as Connect)?.InputValueRqData(ctor)));

            return ctor ?? null;
        }
        public void StartProgress() => buttonStartProgress.PerformClick();
        public void SetForeColor(Color color, string remain)
        {
            labelOpenAPI.ForeColor = color;
            labelMessage.Text = remain;
        }
        public ISendSecuritiesAPI OnConnectErrorMessage => API as Connect ?? null;
        public IEnumerable<Holding> HoldingStocks
        {
            get
            {
                foreach (var ctor in Connect.HoldingStock)
                    yield return ctor.Value ?? null;
            }
        }
        public dynamic API
        {
            get; private set;
        }
        public bool Start
        {
            get; private set;
        }
        public ConnectAPI(Privacies privacy)
        {
            this.privacy = privacy;
            InitializeComponent();

            if (string.IsNullOrEmpty(privacy.SecurityAPI) == false)
            {
                securites = new Security().Decipher(privacy.Security, privacy.SecuritiesAPI, privacy.SecurityAPI);
                checkAccount.CheckState = CheckState.Checked;
            }
        }
        readonly StringBuilder securites;
        readonly Privacies privacy;
        public event EventHandler<SendSecuritiesAPI> Send;
    }
}