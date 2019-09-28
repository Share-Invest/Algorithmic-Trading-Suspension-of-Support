﻿using System.Collections;

namespace ShareInvest.Catalog
{
    public class 주문체결 : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            int i, l = output.Length;

            for (i = 0; i < l; i++)
                yield return output[i];
        }
        private readonly int[] output =
        {
            9201,
            9203,
            9205,
            9001,
            912,
            913,
            302,
            900,
            901,
            902,
            903,
            904,
            905,
            906,
            907,
            908,
            909,
            910,
            911,
            10,
            27,
            28,
            914,
            915,
            938,
            939,
            919,
            920,
            921,
            922,
            923
        };
    }
}