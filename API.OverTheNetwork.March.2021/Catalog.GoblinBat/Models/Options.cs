﻿namespace ShareInvest.Catalog.Models
{
    public struct Options
    {
        public string Code
        {
            get; set;
        }
        public string Retention
        {
            get; set;
        }
        public string Date
        {
            get; set;
        }
        public string Price
        {
            get; set;
        }
        public int Volume
        {
            get; set;
        }
    }
}