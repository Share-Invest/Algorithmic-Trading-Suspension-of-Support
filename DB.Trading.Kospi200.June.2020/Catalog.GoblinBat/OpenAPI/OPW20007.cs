﻿using System.Collections;

namespace ShareInvest.Catalog
{
    public class OPW20007 : ScreenNumber, ITR, IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            int i, l = output.Length;

            for (i = 0; i < l; i++)
                yield return output[i];
        }
        public string ID
        {
            get
            {
                return id;
            }
        }
        public string Value
        {
            get; set;
        }
        public string RQName
        {
            get
            {
                return name;
            }
            set
            {

            }
        }
        public string TrCode
        {
            get
            {
                return code;
            }
        }
        public int PrevNext
        {
            get; set;
        }
        public string ScreenNo
        {
            get
            {
                return GetScreenNumber();
            }
        }
        private readonly string[] output =
        {
            "종목코드",
            "종목명",
            "매도매수구분",
            "수량",
            "매입단가",
            "현재가",
            "평가손익",
            "청산가능수량",
            "약정금액",
            "평가금액"
        };
        private const string id = "계좌번호;비밀번호;비밀번호입력매체구분";
        private const string name = "선옵잔고현황정산가기준요청";
        private const string code = "OPW20007";
    }
}