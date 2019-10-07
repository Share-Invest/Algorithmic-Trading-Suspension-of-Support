﻿using System;
using System.Windows.Forms;
using ShareInvest.Analysis;
using ShareInvest.AutoMessageBox;
using ShareInvest.BackTest;
using ShareInvest.EventHandler;
using ShareInvest.Reservoir;
using ShareInvest.SelectableMessageBox;

namespace ShareInvest.Control
{
    public partial class Kospi200 : UserControl
    {
        public Kospi200()
        {
            InitializeComponent();

            dr = Choose.Show("Please Select the Button You Want to Proceed.", "Choose", "Trading", "BackTest", "Exit");

            if (dr == DialogResult.Yes)
            {
                api = Futures.Get();

                new Statistics();
                new Temporary();

                api.SetAPI(axAPI);
                api.StartProgress();
                api.SendExit += OnReceiveExit;
            }
            else if (dr == DialogResult.No)
            {
                axAPI.Dispose();

                int i, l = 100;

                for (i = 11; i < l; i++)
                    new Statistics(i);

                new Storage();

                Box.Show("Complete...!!", "Notice", 3750);

                Dispose();

                Environment.Exit(0);
            }
            else
            {
                Dispose();

                Environment.Exit(0);
            }
        }
        private void OnReceiveExit(object sender, ForceQuit e)
        {
            if (e.Quit == 1)
            {
                Dispose();

                Environment.Exit(0);
            }
        }
        private readonly Futures api;
        private readonly DialogResult dr;
    }
}