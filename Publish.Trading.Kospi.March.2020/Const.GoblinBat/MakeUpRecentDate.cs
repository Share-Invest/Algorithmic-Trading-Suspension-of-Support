﻿using System.Collections.Generic;
using ShareInvest.Interface;

namespace ShareInvest.Const
{
    public class MakeUpRecentDate : IMakeUp
    {
        public MakeUpRecentDate()
        {
            FindByName = "recent";
            Turn = 2;
            DescendingSort = new Dictionary<string, long>(256);
        }
        public Dictionary<string, long> DescendingSort
        {
            get; set;
        }
        public string FindByName
        {
            get; private set;
        }
        public int Turn
        {
            get; private set;
        }
    }
}