﻿using System.Collections.Generic;

namespace ShareInvest.Models
{
    public static class Registry
    {
        public static Dictionary<string, string> Retentions = new Dictionary<string, string>();
        public static Queue<string> Codes = new Queue<string>();
    }
}