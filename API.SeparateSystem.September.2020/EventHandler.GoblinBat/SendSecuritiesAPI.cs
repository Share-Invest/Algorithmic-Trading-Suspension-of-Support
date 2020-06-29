﻿using System;
using System.Windows.Forms;

namespace ShareInvest.EventHandler
{
    public class SendSecuritiesAPI : EventArgs
    {
        public object Convey
        {
            get; private set;
        }
        public Control Accounts
        {
            get; private set;
        }
        public SendSecuritiesAPI(FormWindowState state, Control accounts)
        {
            Convey = state;
            Accounts = accounts;
        }
        public SendSecuritiesAPI(Control control, string account, string password)
        {
            Accounts = control;
            Convey = string.Concat(account.Replace("-", string.Empty), ";", password);
        }
        public SendSecuritiesAPI(string sDeposit, string sAvailable)
        {
            if (long.TryParse(sDeposit, out long deposit) && long.TryParse(sAvailable, out long available))
                Convey = new Tuple<long, long>(deposit, available);
        }
        public SendSecuritiesAPI(string message) => Convey = message;
    }
}