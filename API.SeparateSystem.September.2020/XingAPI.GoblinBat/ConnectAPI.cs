﻿using System;
using System.Drawing;
using System.Windows.Forms;

using ShareInvest.Catalog;
using ShareInvest.Catalog.XingAPI;
using ShareInvest.Controls;
using ShareInvest.EventHandler;
using ShareInvest.FindByName;

namespace ShareInvest.XingAPI
{
    public sealed partial class ConnectAPI : UserControl, ISecuritiesAPI
    {
        void ButtonStartProgressClick(object sender, EventArgs e)
        {
            if (textCertificate.Text.Length > 9 && textIdentity.Text.Length < 9 && textPassword.Text.Length < 9)
                BeginInvoke(new Action(() =>
                {
                    API = Connect.GetInstance(new Privacy
                    {
                        Identity = textIdentity.Text,
                        Password = textPassword.Text,
                        Certificate = textCertificate.Text
                    },
                    new LoadServer
                    {
                        Server = checkDemo.Checked ? demo : hts,
                        Date = labelMessage.Text
                    });
                    if (API is Connect api)
                        Send?.Invoke(this, new SendSecuritiesAPI(FormWindowState.Minimized, new Accounts(api.Accounts)));
                }));
            else
                buttonStartProgress.Text = error;
        }
        void OnReceiveControls(object sender, EventArgs e)
        {
            if (sender is TextBox text && text.ForeColor.Equals(Color.DarkGray))
            {
                if (e is PreviewKeyDownEventArgs key && key.KeyData.Equals(Keys.Tab))
                    return;

                text.UseSystemPasswordChar = text.Name.Equals(identity) == false;
                text.ForeColor = Color.Black;

                if (e is MouseEventArgs || text.UseSystemPasswordChar)
                    text.Text = string.Empty;

                text.MouseDown -= OnReceiveControls;
                text.PreviewKeyDown -= OnReceiveControls;
            }
        }
        void FindControlRecursive(Control control)
        {
            foreach (Control sub in control.Controls)
                if (sub.Controls.Count == 0 && sub.GetType().Name.Equals(text))
                {
                    var con = sub.Name.FindByName<TextBox>(this);
                    con.MouseDown += OnReceiveControls;
                    con.PreviewKeyDown += OnReceiveControls;
                }
                else if (sub.Controls.Count > 0)
                    FindControlRecursive(sub);
        }
        public dynamic API
        {
            get; private set;
        }
        public ConnectAPI()
        {
            InitializeComponent();

            foreach (Control control in Controls)
                FindControlRecursive(control);
        }
        public event EventHandler<SendSecuritiesAPI> Send;
    }
}