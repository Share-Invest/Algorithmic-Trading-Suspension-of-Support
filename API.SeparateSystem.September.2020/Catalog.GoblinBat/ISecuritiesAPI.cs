﻿using System;

using ShareInvest.EventHandler;

namespace ShareInvest.Catalog
{
    public interface ISecuritiesAPI
    {
        dynamic API
        {
            get;
        }
        event EventHandler<SendSecuritiesAPI> Send;
    }
    public enum SecuritiesCOM
    {
        OpenAPI = 'O',
        XingAPI = 'X'
    }
}