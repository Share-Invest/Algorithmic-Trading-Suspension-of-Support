﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using ShareInvest.Catalog;
using ShareInvest.Client;
using ShareInvest.Controls;
using ShareInvest.EventHandler;

namespace ShareInvest
{
    partial class SecuritiesAPI : Form
    {
        internal SecuritiesAPI(Privacies privacy, ISecuritiesAPI com)
        {
            this.com = com;
            this.privacy = privacy;
            random = new Random();
            InitializeComponent();
            icon = new string[] { mono, duo, tri, quad };
            colors = new Color[] { Color.Maroon, Color.Ivory, Color.DeepSkyBlue };
            infoCodes = new Dictionary<string, Codes>();
            strip.ItemClicked += OnItemClick;
            timer.Start();
        }
        void StartProgress()
        {
            var control = (Control)com;
            Controls.Add(control);
            control.Dock = DockStyle.Fill;
            control.Show();
            Size = new Size(0x177, com is XingAPI.ConnectAPI ? 0x127 : 0xC3);
            Opacity = 0.8135;
            BackColor = Color.FromArgb(0x79, 0x85, 0x82);
            FormBorderStyle = FormBorderStyle.None;
            CenterToScreen();
            com.Send += OnReceiveSecuritiesAPI;
        }
        void OnReceiveSecuritiesAPI(object sender, SendSecuritiesAPI e)
        {
            if (e.Accounts == null && Balance != null)
                BeginInvoke(new Action(() =>
                {
                    switch (e.Convey)
                    {
                        case string message:
                            Balance.OnReceiveMessage(message);
                            return;

                        case Tuple<string, string, int, dynamic, dynamic, long, double> balance:
                            SuspendLayout();
                            var strategics = string.Empty;

                            switch (com)
                            {
                                case XingAPI.ConnectAPI x:
                                    strategics = x.Strategics.First(o => o.Code.Equals(balance.Item1)).GetType().Name;
                                    break;

                                case OpenAPI.ConnectAPI o:
                                    break;
                            }
                            Size = new Size(0x3B8, 0x63 + 0x28 + Balance.OnReceiveBalance(balance, strategics));
                            ResumeLayout();
                            break;

                        case long available:
                            Balance.OnReceiveDeposit(available);
                            break;

                        case Tuple<long, long> tuple:
                            Balance.OnReceiveDeposit(tuple);
                            return;

                        case Tuple<int, string> kw:
                            if (com is OpenAPI.ConnectAPI open)
                            {
                                var connect = open.InputValueRqData(optkwFID, kw.Item2, kw.Item1);

                                if (connect != null)
                                    connect.Send += OnReceiveSecuritiesAPI;
                            }
                            return;

                        case Tuple<string, string, string> code:
                            infoCodes[code.Item1] = new Codes
                            {
                                Code = code.Item1,
                                Name = code.Item2,
                                Price = code.Item3
                            };
                            return;

                        case Dictionary<string, Tuple<string, string>> dictionary:
                            foreach (var kv in dictionary)
                                if (infoCodes.TryGetValue(kv.Key, out Codes info) && double.TryParse(kv.Value.Item2, out double rate) && com is XingAPI.ConnectAPI xing)
                                {
                                    info.MarginRate = rate * 1e-2;
                                    info.Name = kv.Value.Item1;
                                    infoCodes[kv.Key] = info;
                                    xing.StartProgress(info);
                                }
                            return;

                        case Tuple<byte, byte> tuple:
                            switch (tuple)
                            {
                                case Tuple<byte, byte> tp when tp.Item2 == 21:
                                    if (WindowState.Equals(FormWindowState.Minimized))
                                        strip.Items.Find(st, false).First(o => o.Name.Equals(st)).PerformClick();

                                    break;

                                case Tuple<byte, byte> tp when tp.Item2 == 41 && tp.Item1 == 5:
                                    var chart = (com as XingAPI.ConnectAPI)?.Charts[0];
                                    chart.Send += OnReceiveSecuritiesAPI;
                                    chart?.QueryExcute(GoblinBatClient.GetContext(new Futures()));
                                    break;
                            }
                            break;

                        case Tuple<string, Stack<string>> charts:
                            switch (charts.Item1.Length)
                            {
                                case 6:
                                    break;

                                case int length when length == 8 && charts.Item1.StartsWith("1"):
                                    var content = GoblinBatClient.PostContext(new Catalog.Convert().ToStoreInFutures(charts.Item1, charts.Item2));
                                    break;
                            }
                            break;
                    }
                }));
            else if (e.Convey is FormWindowState state)
            {
                WindowState = state;
                com.Send -= OnReceiveSecuritiesAPI;
                ((Control)com).Hide();
                Controls.Add(e.Accounts);
                e.Accounts.Dock = DockStyle.Fill;
                e.Accounts.Show();
                Size = new Size(0x13B, 0x7D);
                Visible = true;
                ShowIcon = true;
                notifyIcon.Visible = false;
                WindowState = FormWindowState.Normal;
                CenterToScreen();

                if (e.Accounts is Accounts accounts)
                    accounts.Send += OnReceiveSecuritiesAPI;
            }
            else if (e.Convey is string str && e.Accounts is Accounts accounts)
            {
                Opacity = 0;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                WindowState = FormWindowState.Minimized;
                strategy.Text = balance;
                accounts.Hide();
                accounts.Send -= OnReceiveSecuritiesAPI;
                var param = str.Split(';');
                var info = com.SetPrivacy(com is OpenAPI.ConnectAPI ? new Privacies { AccountNumber = param[0] } : new Privacies
                {
                    AccountNumber = param[0],
                    AccountPassword = param[1]
                });
                Balance = new Balance(info);
                Controls.Add(Balance);
                Balance.Dock = DockStyle.Fill;
                Text = info.Nick;
                notifyIcon.Text = info.Nick;
                Opacity = 0.79315;

                if (com is XingAPI.ConnectAPI connect)
                {
                    foreach (var ctor in connect.ConvertTheCodeToName)
                    {
                        ctor.Send += OnReceiveSecuritiesAPI;
                        ctor.QueryExcute();
                        Connect = int.MaxValue;
                    }
                    foreach (var strategics in privacy.CodeStrategics.Split(';'))
                    {
                        IStrategics temp = null;
                        var stParam = strategics.Split('.');

                        if (stParam[0].Length > 0xC)
                        {
                            switch (strategics.Substring(0, 2))
                            {
                                case "TF":
                                    if (int.TryParse(stParam[0].Substring(0xB), out int ds) & int.TryParse(stParam[1], out int dl) & int.TryParse(stParam[2], out int m) & int.TryParse(stParam[3], out int ms) & int.TryParse(stParam[4], out int ml) & int.TryParse(stParam[5], out int rs) & int.TryParse(stParam[6], out int rl) & int.TryParse(stParam[7], out int qs) & int.TryParse(stParam[8], out int ql))
                                        temp = new TrendFollowingBasicFutures
                                        {
                                            Code = strategics.Substring(2, 8),
                                            RollOver = stParam[0].Substring(0xA, 1).Equals("1"),
                                            DayShort = ds,
                                            DayLong = dl,
                                            Minute = m,
                                            MinuteShort = ms,
                                            MinuteLong = ml,
                                            ReactionShort = rs,
                                            ReactionLong = rl,
                                            QuantityShort = qs,
                                            QuantityLong = ql
                                        };
                                    break;
                            }
                            if (temp != null && connect.Strategics.Add(temp) && connect.SetStrategics(temp) > 0)
                                foreach (var real in connect.Reals)
                                    real.OnReceiveRealTime(temp.Code);
                        }
                        else
                        {

                        }
                    }
                    foreach (var conclusion in connect.Conclusion)
                        conclusion.OnReceiveRealTime(string.Empty);

                    var alarm = connect.JIF;
                    alarm.Send += OnReceiveSecuritiesAPI;
                    alarm.QueryExcute();
                }
            }
        }
        void GoblinBatResize(object sender, EventArgs e)
        {
            if (WindowState.Equals(FormWindowState.Minimized))
            {
                SuspendLayout();
                Visible = false;
                ShowIcon = false;
                notifyIcon.Visible = true;

                if (strategy.Text.Equals(balance) && Balance != null)
                {
                    Balance.Hide();

                    if (com is OpenAPI.ConnectAPI openAPI)
                    {
                        openAPI.OnConnectErrorMessage.Send -= OnReceiveSecuritiesAPI;
                        openAPI.Send -= OnReceiveSecuritiesAPI;
                        openAPI.InputValueRqData(false, opw00005).Send -= OnReceiveSecuritiesAPI;

                        var connect = openAPI.InputValueRqData(optkwFID, null, 0);

                        if (connect != null)
                            connect.Send -= OnReceiveSecuritiesAPI;

                        foreach (var ctor in openAPI.HoldingStocks)
                        {
                            Balance.SetDisconnectHoldingStock(ctor);
                            ctor.SendBalance -= OnReceiveSecuritiesAPI;
                        }
                    }
                    else if (com is XingAPI.ConnectAPI xing)
                    {
                        foreach (var ctor in xing.HoldingStocks)
                        {
                            Balance.SetDisconnectHoldingStock(ctor);
                            ctor.SendBalance -= OnReceiveSecuritiesAPI;
                        }
                        foreach (var ctor in xing.querys)
                            ctor.Send -= OnReceiveSecuritiesAPI;
                    }
                }
                ResumeLayout();
            }
        }
        void GoblinBatFormClosing(object sender, FormClosingEventArgs e)
        {
            switch (MessageBox.Show(rExit, notifyIcon.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3))
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    WindowState = FormWindowState.Minimized;
                    return;

                case DialogResult.Yes:
                    var code = GoblinBatClient.DeleteContext<Privacies>(privacy);

                    if (code > 0xC8)
                    {
                        notifyIcon.Text = OnReceiveErrorMessage(code);
                        e.Cancel = true;
                        WindowState = FormWindowState.Minimized;

                        return;
                    }
                    break;
            }
            timer.Stop();
            strip.ItemClicked -= OnItemClick;
            Dispose();
        }
        void TimerTick(object sender, EventArgs e)
        {
            if (com == null)
            {
                timer.Stop();
                strip.ItemClicked -= OnItemClick;
                Dispose();
            }
            else if (FormBorderStyle.Equals(FormBorderStyle.Sizable) && WindowState.Equals(FormWindowState.Minimized) == false)
                WindowState = FormWindowState.Minimized;

            else if (Controls.Contains((Control)com) == false && WindowState.Equals(FormWindowState.Minimized))
                strip.Items.Find(st, false).First(o => o.Name.Equals(st)).PerformClick();

            else if (Visible && ShowIcon && notifyIcon.Visible == false && FormBorderStyle.Equals(FormBorderStyle.None) && WindowState.Equals(FormWindowState.Normal) && (com is XingAPI.ConnectAPI || com is OpenAPI.ConnectAPI))
            {
                var now = DateTime.Now;
                var day = 0;

                switch (now.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        day = now.AddDays(1).Day;
                        break;

                    case DayOfWeek.Saturday:
                        day = now.AddDays(2).Day;
                        break;

                    case DayOfWeek weeks when weeks.Equals(DayOfWeek.Friday) && now.Hour > 8:
                        day = now.AddDays(3).Day;
                        break;

                    default:
                        day = (now.Hour > 8 || Array.Exists(holidays, o => o.Equals(now.ToString(dFormat))) ? now.AddDays(1) : now).Day;
                        break;
                }
                var remain = new DateTime(now.Year, now.Month, day, 9, 0, 0) - DateTime.Now;
                com.SetForeColor(colors[DateTime.Now.Second % 3], GetRemainingTime(remain));

                if (remain.TotalMinutes < 0x1F && com.Start == false && DateTime.Now.Hour == 8 && DateTime.Now.Minute > 0x1E && (Connect > 0x4B0 || random.Next(Connect++, 0x4B1) == 0x4B0))
                    com.StartProgress();
            }
            else if (Visible == false && ShowIcon == false && notifyIcon.Visible && WindowState.Equals(FormWindowState.Minimized))
                notifyIcon.Icon = (Icon)resources.GetObject(icon[DateTime.Now.Second % 4]);
        }
        void OnItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name.Equals(st))
            {
                if (strategy.Text.Equals(balance) && Balance != null)
                {
                    if (com is XingAPI.ConnectAPI xingAPI)
                    {
                        foreach (var ctor in xingAPI.querys)
                        {
                            ctor.Send += OnReceiveSecuritiesAPI;
                            ctor.QueryExcute();
                        }
                        if (Connect == int.MaxValue)
                            foreach (var convert in xingAPI.ConvertTheCodeToName)
                            {
                                convert.Send -= OnReceiveSecuritiesAPI;
                                Connect = int.MinValue;
                            }
                        foreach (var ctor in xingAPI.HoldingStocks)
                        {
                            Balance.SetConnectHoldingStock(ctor);
                            ctor.SendBalance += OnReceiveSecuritiesAPI;
                        }
                    }
                    else if (com is OpenAPI.ConnectAPI openAPI)
                    {
                        openAPI.OnConnectErrorMessage.Send += OnReceiveSecuritiesAPI;
                        openAPI.Send += OnReceiveSecuritiesAPI;
                        openAPI.InputValueRqData(true, opw00005).Send += OnReceiveSecuritiesAPI;

                        foreach (var ctor in openAPI.HoldingStocks)
                        {
                            Balance.SetConnectHoldingStock(ctor);
                            ctor.SendBalance += OnReceiveSecuritiesAPI;
                        }
                    }
                    Size = new Size(0x3B8, 0x63 + 0x28);
                    Balance.Show();
                }
                Visible = true;
                ShowIcon = true;
                notifyIcon.Visible = false;
                WindowState = FormWindowState.Normal;

                if (com is XingAPI.ConnectAPI xing && xing.API == null || com is OpenAPI.ConnectAPI open && open.API == null)
                    StartProgress();
            }
            else
                Close();
        }
        Balance Balance
        {
            get; set;
        }
        int Connect
        {
            get; set;
        }
        readonly Random random;
        readonly Dictionary<string, Codes> infoCodes;
        readonly Privacies privacy;
        readonly ISecuritiesAPI com;
        readonly Color[] colors;
        readonly string[] icon;
    }
}