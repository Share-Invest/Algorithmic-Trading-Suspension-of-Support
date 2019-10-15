﻿using System;
using System.Windows.Forms;
using ShareInvest.Analysis;
using ShareInvest.AutoMessageBox;
using ShareInvest.BackTest;
using ShareInvest.EventHandler;
using ShareInvest.Identify;
using ShareInvest.Reservoir;
using ShareInvest.SelectableMessageBox;

namespace ShareInvest.Control
{
    public partial class Korea : UserControl
    {
        public Korea(int type)
        {
            InitializeComponent();
            dr = Choose.Show("Please Select the Button You Want to Proceed. . .", "Choose", "Trading", "BackTest", "Exit");

            if (dr == DialogResult.Yes)
            {
                Confirm.Get().Show();
                api = Futures.Get();
                new Statistics(type);
                new Temporary(type);
                api.SetAPI(axAPI);
                api.StartProgress(type);
                api.SendExit += OnReceiveExit;
            }
            else if (dr == DialogResult.No)
            {
                axAPI.Dispose();
                int i, l = type > 0 ? 50 : 100;

                for (i = type > 0 ? 1 : 10; i < l; i++)
                    new Statistics(i, type);

                new Storage(type);
                Box.Show("Complete. . .♬", "Notice", 3750);
                OnReceiveExit();
            }
            else
                OnReceiveExit();
        }
        private void OnReceiveExit()
        {
            Dispose();
            Environment.Exit(0);
        }
        private void OnReceiveExit(object sender, ForceQuit e)
        {
            if (e.Quit == 1)
            {
                Box.Show("How was Your Day Today. . .??", "Notice", 7950);
                Dispose();
                Environment.Exit(0);
            }
        }
        private readonly Futures api;
        private readonly DialogResult dr;
    }
}