﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

using ShareInvest.Catalog;
using ShareInvest.DelayRequest;
using ShareInvest.Message;

using XA_SESSIONLib;

namespace ShareInvest.XingAPI
{
    class Connect : XASessionClass
    {
        internal Delay Request
        {
            get; private set;
        }
        internal static Dictionary<string, Holding> HoldingStock
        {
            get; private set;
        }
        internal static Connect GetInstance() => API;
        internal static Connect GetInstance(Privacies privacy, ShareInvest.Catalog.XingAPI.LoadServer load)
        {
            if (API == null)
            {
                HoldingStock = new Dictionary<string, Holding>();
                API = new Connect(privacy, load);
            }
            return API;
        }
        internal (string, string, string) SetAccountName(string account, string password)
        {
            Secrecy.Account = account;
            Secrecy.Password = password;

            return (GetAccountName(account), GetAcctDetailName(account), GetAcctNickname(account));
        }
        internal string[] Accounts
        {
            get; private set;
        }
        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        internal void Dispose()
        {
            try
            {
                GC.Collect();
                DisconnectServer();
                API = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.TargetSite.Name + "\n" + ex.StackTrace);
            }
        }
        internal void OnReceiveChapterOperatingInformation(int gubun, int status)
        {
            string jangubun = Enum.GetName(typeof(Attribute), gubun), jstatus = Enum.GetName(typeof(Attribute), status);

            if (TimerBox.Show(jstatus, jangubun, MessageBoxButtons.YesNo, MessageBoxIcon.Information, gubun == 5 && status == 41 ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2, 0x3B7).Equals(DialogResult.Yes))
                SendMessage(jangubun, jstatus);
        }
        void OnEventConnect(string szCode, string szMsg)
        {
            if (secrecy.GetConnectionStatus(szCode, szMsg) && IsConnected())
            {
                Accounts = new string[GetAccountListCount()];

                for (int i = 0; i < Accounts.Length; i++)
                    Accounts[i] = GetAccountList(i);
            }
        }
        [Conditional("DEBUG")]
        void SendMessage(string code, string message) => new Task(() => Console.WriteLine(code + "\t" + message)).Start();
        Connect(Privacies privacy, ShareInvest.Catalog.XingAPI.LoadServer load)
        {
            if (ConnectServer(load.Server, 0x4E21) && Login(privacy.Identity, privacy.Password, privacy.Certificate, 0, string.IsNullOrEmpty(privacy.Certificate) == false) && IsLoadAPI())
            {
                _IXASessionEvents_Event_Login += OnEventConnect;
                Disconnect += Dispose;
                secrecy = new Secrecy();
                Request = Delay.GetInstance(0xCD);

                while (TimerBox.Show(secrecy.Connection, load.Date, MessageBoxButtons.OK, MessageBoxIcon.Information, 0xC57).Equals(DialogResult.OK))
                    if (Accounts != null)
                    {
                        Request.Run();

                        return;
                    }
            }
            else
                Dispose();
        }
        static Connect API
        {
            get; set;
        }
        readonly Secrecy secrecy;
    }
}