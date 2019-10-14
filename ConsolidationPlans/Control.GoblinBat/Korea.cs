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
        public Korea()
        {
            InitializeComponent();
            Confirm.Get().Show();
            api = Futures.Get();
            new RealInvest(24);
            new Temporary(24);
            api.SetAPI(axAPI);
            api.StartProgress(24);
            api.SendExit += OnReceiveExit;
        }
        public Korea(int type)
        {
            InitializeComponent();
            dr = Choose.Show("Please Select the Button You Want to Proceed. . .", "Choose", "Trading", "BackTest", "Exit");

            if (dr == DialogResult.Yes)
            {
                Confirm.Get().Show();
                api = Futures.Get();
                dr = Choose.Show("Please Select the Button You Want to Proceed. . .", "Choose", "Manual", "Revenue", "Exit");

                if (dr == DialogResult.Yes)
                    new Statistics(type);

                else if (dr == DialogResult.No)
                    new Revenue(type);

                else
                    OnReceiveExit();

                new Temporary(type);
                api.SetAPI(axAPI);
                api.StartProgress(type);
                api.SendExit += OnReceiveExit;
            }
            else if (dr == DialogResult.No)
            {
                axAPI.Dispose();
                dr = Choose.Show("Please Select the Button You Want to Proceed. . .", "Choose", "Shallow", "Revenue", "Exit");

                int i, l = type > 0 ? 50 : 100, j, h = 20;

                for (i = type > 0 ? 1 : 10; i < l; i++)
                {
                    if (dr == DialogResult.Yes)
                        new Statistics(i, type);

                    else if (dr == DialogResult.No)
                        for (j = 2; j < h; j++)
                            new Revenue(i, j, type);

                    else
                        OnReceiveExit();
                }
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