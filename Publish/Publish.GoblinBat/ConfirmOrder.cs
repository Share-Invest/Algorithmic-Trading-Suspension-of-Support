﻿using System;
using System.Windows.Forms;
using ShareInvest.EventHandler;

namespace ShareInvest.Publish
{
    public partial class ConfirmOrder : Form
    {
        public static ConfirmOrder Get()
        {
            if (cf == null)
                cf = new ConfirmOrder();

            return cf;
        }
        public bool CheckCurrent()
        {
            return checkBox.Checked;
        }
        private void OnReceiveIdentify(object sender, Identify e)
        {
            checkBox.Text = string.Concat(DateTime.Now.ToString("H시 m분 s초  "), e.Confirm);
        }
        private ConfirmOrder()
        {
            InitializeComponent();
            PublicFutures.Get().SendConfirm += OnReceiveIdentify;
        }
        private void ConfirmOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            cf.Dispose();
            Dispose();
            Environment.Exit(0);
        }
        private static ConfirmOrder cf;
    }
}