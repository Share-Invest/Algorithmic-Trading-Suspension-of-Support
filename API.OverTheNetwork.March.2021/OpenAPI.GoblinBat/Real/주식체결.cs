﻿using System.IO;

using AxKHOpenAPILib;

namespace ShareInvest.OpenAPI.Catalog
{
    class 주식체결 : Real
    {
        internal override AxKHOpenAPI API
        {
            get; set;
        }
        internal override StreamWriter Server
        {
            get; set;
        }
        internal override void OnReceiveRealData(_DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string time = API.GetCommRealData(e.sRealKey, Fid[0]), current = API.GetCommRealData(e.sRealKey, Fid[1]), volume = API.GetCommRealData(e.sRealKey, Fid[6]);

            if (string.IsNullOrEmpty(volume) == false && string.IsNullOrEmpty(current) == false && string.IsNullOrEmpty(time) == false)
                Server.WriteLine(string.Concat(e.sRealType, '|', e.sRealKey, '|', time, ';', current, ';', volume));
        }
        protected internal override int[] Fid => new int[] { 20, 10, 11, 12, 27, 28, 15, 13, 14, 16, 17, 18, 25, 26, 29, 30, 31, 32, 228, 311, 290, 691, 567, 568 };
    }
}