﻿using System.Collections;

namespace ShareInvest.Catalog
{
    public class Opt10079 : ScreenNumber, ITRs, IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            int i, l = output.Length;

            for (i = 0; i < l; i++)
                yield return output[i];
        }
        public string Value
        {
            get
            {
                return string.Concat(Code, ";1;1");
            }
            set
            {
                Code = value;
            }
        }
        public string RQName
        {
            get; set;
        }
        public int PrevNext
        {
            get; set;
        }
        public string ScreenNo => GetScreenNumber();
        public string ID => id;
        public string TrCode => code;
        string Code
        {
            get; set;
        }
        readonly string[] output = { "현재가", "거래량", "체결시간", "시가", "고가", "저가", "수정주가구분", "수정비율", "대업종구분", "소업종구분", "종목정보", "수정주가이벤트", "전일종가" };
        const string code = "opt10079";
        const string id = "종목코드;틱범위;수정주가구분";
    }
}