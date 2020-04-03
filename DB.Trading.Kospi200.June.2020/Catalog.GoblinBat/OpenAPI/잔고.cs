﻿using System.Collections;

namespace ShareInvest.Catalog
{
    public struct 잔고 : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            foreach (var index in new int[]
            {
                9201,
                9001,
                917,
                916,
                302,
                10,
                930,
                931,
                932,
                933,
                945,
                946,
                950,
                951,
                27,
                28,
                307,
                8019,
                957,
                958,
                918,
                990,
                991,
                992,
                993,
                959,
                924
            })
                yield return index;
        }
    }
}