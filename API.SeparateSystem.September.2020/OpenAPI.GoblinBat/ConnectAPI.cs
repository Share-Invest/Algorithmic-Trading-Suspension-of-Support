﻿using System.Windows.Forms;

using ShareInvest.Catalog;

namespace ShareInvest.OpenAPI
{
    public sealed partial class ConnectAPI : UserControl, ISecuritiesAPI
    {
        public ConnectAPI()
        {
            InitializeComponent();
        }
        public dynamic API
        {
            get; private set;
        }
    }
}