using System;
using System.Diagnostics;

using XA_DATASETLib;

namespace ShareInvest.XingAPI
{
    abstract class Real : XARealClass
    {
        string GetField(string code)
        {
            var now = DateTime.Now;

            if (code.Length == 8 && (now.Hour > 0xF || now.Hour < 5))
                switch (code[2])
                {
                    case '1':
                        return field;

                    case '5':
                        return "optcode";
                }
            return field;
        }
        [Conditional("DEBUG")]
        protected internal void SendMessage(string message) => Console.WriteLine(message);
        protected internal InBlock GetInBlock(string code) => new InBlock
        {
            Block = inBlock,
            Field = GetField(code),
            Data = code
        };
        protected internal abstract void OnReceiveRealData(string szTrCode);
        protected internal string GetInBlock() => inBlock;
        protected internal string OutBlock => outBlock;
        protected internal Real() => ReceiveRealData += OnReceiveRealData;
        protected internal const string sell = "1";
        protected internal const string buy = "2";
        protected internal const string cancel = "3";
        protected internal const string avg = "000.00";
        const string field = "futcode";
        const string inBlock = "InBlock";
        const string outBlock = "OutBlock";
    }
    enum C
    {
        chetime = 1,
        sign = 2,
        change = 3,
        drate = 4,
        price = 5,
        open = 6,
        high = 7,
        low = 8,
        cgubun = 9,
        cvolume = 10,
        volume = 11,
        value = 12,
        mdvolume = 13,
        mdchecnt = 14,
        msvolume = 15,
        mschecnt = 16,
        cpower = 17,
        offerho1 = 18,
        bidho1 = 19,
        openyak = 20,
        k200jisu = 21,
        theoryprice = 22,
        kasis = 23,
        sbasis = 24,
        ibasis = 25,
        openyakcha = 26,
        jgubun = 27,
        jnilvolume = 28,
        futcode = 29,
        chetime1 = 30
    }
    enum EC
    {
        chetime = 1,
        sign = 2,
        change = 3,
        drate = 4,
        price = 5,
        open = 6,
        high = 7,
        low = 8,
        cgubun = 9,
        cvolume = 10,
        volume = 11,
        value = 12,
        mdvolume = 13,
        mdchecnt = 14,
        msvolume = 15,
        mschecnt = 16,
        cpower = 17,
        offerho1 = 18,
        bidho1 = 19,
        openyak = 20,
        k200jisu = 21,
        eqva = 22,
        theoryprice = 23,
        impv = 24,
        openyakcha = 25,
        timevalue = 26,
        jgubun = 27,
        jnilvolume = 28,
        optcode = 29,
        chetime1 = 30
    }
    enum H
    {
        hotime = 12,
        offerho = 1,
        bidho = 2,
        offerrem = 3,
        bidrem = 4,
        offercnt = 5,
        bidcnt = 6,
        totofferrem = 7,
        totbidrem = 8,
        totoffercnt = 9,
        totbidcnt = 10,
        futcode = 11,
        danhochk = 'H',
        alloc_gubun = 'G'
    }
    enum EH
    {
        hotime = 12,
        offerho = 1,
        bidho = 2,
        offerrem = 3,
        bidrem = 4,
        offercnt = 5,
        bidcnt = 6,
        totofferrem = 7,
        totbidrem = 8,
        totoffercnt = 9,
        totbidcnt = 10,
        optcode = 11,
        danhochk = 'H'
    }
    enum Attribute
    {
        Common = 0,
        코스피 = 1,
        코스닥 = 2,
        선물옵션 = 5,
        CME야간선물 = 7,
        EUREX야간옵션선물 = 8,
        미국주식 = 9,
        중국주식오전 = 'A',
        중국주식오후 = 'B',
        홍콩주식오전 = 'C',
        홍콩주식오후 = 'D',
        장전동시호가개시 = 11,
        장시작 = 21,
        장개시10초전 = 22,
        장개시1분전 = 23,
        장개시5분전 = 24,
        장개시10분전 = 25,
        장후동시호가개시 = 31,
        장마감 = 41,
        장마감10초전 = 42,
        장마감1분전 = 43,
        장마감5분전 = 44,
        시간외종가매매개시 = 51,
        시간외종가매매종료 = 52,
        시간외단일가매매종료 = 54,
        서킷브레이크1단계발동_코스피관련파생상품_당일장종료 = 61,
        서킷브레이크1단계해제_호가접수개시 = 62,
        서킷브레이크1단계_동시호가종료_서킷브레이크_장중동시마감 = 63,
        사이드카매도발동 = 64,
        사이드카매도해제 = 65,
        사이드카매수발동 = 66,
        사이드카매수해제 = 67,
        서킷브레이크2단계발동 = 68,
        서킷브레이크3단계발동_당일장종료 = 69,
        서킷브레이크2단계해제_호가접수개시_2단계상한가_5분후확대예정 = 70,
        서킷브레이크2단계_동시호가종료_2단계하한가_5분후확대예정 = 71,
        _3단계상한가_5분후확대예정 = 72,
        _3단계하한가_5분후확대예정 = 73,
        _2단계상한가_확대적용 = 74,
        _2단계하한가_확대적용 = 75,
        _3단계상한가_확대적용 = 76,
        _3단계하한가_확대적용 = 77
    }
    enum J
    {
        jangubun = 0,
        jstatus = 1
    }
    enum C1
    {
        lineseq = 0,
        accno = 1,
        user = 2,
        seq = 3,
        trcode = 4,
        megrpno = 5,
        boardid = 6,
        memberno = 7,
        bpno = 8,
        ordno = 9,
        ordordno = 10,
        expcode = 11,
        yakseq = 12,
        cheprice = 13,
        chevol = 14,
        sessionid = 15,
        chedate = 16,
        chetime = 17,
        spdprc1 = 18,
        spdprc2 = 19,
        dosugb = 20,
        accno1 = 21,
        sihogagb = 22,
        jakino = 23,
        daeyong = 24,
        mem_filler = 25,
        mem_accno = 26,
        mem_filler1 = 27
    }
    enum H1
    {
        lineseq = 0,
        accno = 1,
        user = 2,
        seq = 3,
        trcode = 4,
        megrpno = 5,
        boardid = 6,
        memberno = 7,
        bpno = 8,
        ordno = 9,
        orgordno = 10,
        expcode = 11,
        dosugb = 12,
        mocagb = 13,
        accno1 = 14,
        qty2 = 15,
        price = 16,
        ordgb = 17,
        hogagb = 18,
        sihogagb = 19,
        tradid = 20,
        treacode = 21,
        askcode = 22,
        creditcode = 23,
        jakigb = 24,
        trustnum = 25,
        ptgb = 26,
        substonum = 27,
        accgb = 28,
        accmarggb = 29,
        nationcode = 30,
        investgb = 31,
        forecode = 32,
        medcode = 33,
        ordid = 34,
        macid = 35,
        orddate = 36,
        rcvtime = 37,
        mem_filler = 38,
        mem_accno = 39,
        mem_filler1 = 40,
        ordacpttm = 41,
        qty = 42,
        autogb = 43,
        rejcode = 44,
        prgordde = 45
    }
    enum CMO
    {
        lineseq = 0,
        accno = 1,
        user = 2,
        len = 3,
        gubun = 4,
        compress = 5,
        encrypt = 6,
        offset = 7,
        trcode = 8,
        comid = 9,
        userid = 10,
        media = 11,
        ifid = 12,
        seq = 13,
        trid = 14,
        pubip = 15,
        prvip = 16,
        pcbpno = 17,
        bpno = 18,
        termno = 19,
        lang = 20,
        proctm = 21,
        msgcode = 22,
        outgu = 23,
        compreq = 24,
        funckey = 25,
        reqcnt = 26,
        filler = 27,
        cont = 28,
        contkey = 29,
        varlen = 30,
        varhdlen = 31,
        varmsglen = 32,
        trsrc = 33,
        eventid = 34,
        ifinfo = 35,
        filler1 = 36,
        trcode1 = 37,
        firmno = 38,
        acntno = 39,
        acntno1 = 40,
        acntnm = 41,
        brnno = 42,
        ordmktcode = 43,
        ordno1 = 44,
        ordno = 45,
        orgordno1 = 46,
        orgordno = 47,
        prntordno = 48,
        prntordno1 = 49,
        isuno = 50,
        fnoIsuno = 51,
        fnoIsunm = 52,
        pdgrpcode = 53,
        fnoIsuptntp = 54,
        bnstp = 55,
        mrctp = 56,
        ordqty = 57,
        hogatype = 58,
        mmgb = 59,
        ordprc = 60,
        unercqty = 61,
        commdacode = 62,
        peeamtcode = 63,
        mgempno = 64,
        fnotrdunitamt = 65,
        trxtime = 66,
        strtgcode = 67,
        grpId = 68,
        ordseqno = 69,
        ptflno = 70,
        bskno = 71,
        trchno = 72,
        Itemno = 73,
        userId = 74,
        opdrtnno = 75,
        rjtcode = 76,
        mrccnfqty = 77,
        orgordunercqty = 78,
        orgordmrcqty = 79,
        ctrcttime = 80,
        ctrctno = 81,
        execprc = 82,
        execqty = 83,
        newqty = 84,
        qdtqty = 85,
        lastqty = 86,
        lallexecqty = 87,
        allexecamt = 88,
        fnobalevaltp = 89,
        bnsplamt = 90,
        fnoIsuno1 = 91,
        bnstp1 = 92,
        execprc1 = 93,
        newqty1 = 94,
        qdtqty1 = 95,
        allexecamt1 = 96,
        fnoIsuno2 = 97,
        bnstp2 = 98,
        execprc2 = 99,
        newqty2 = 100,
        lqdtqty2 = 101,
        allexecamt2 = 102,
        dps = 103,
        ftsubtdsgnamt = 104,
        mgn = 105,
        mnymgn = 106,
        ordableamt = 107,
        mnyordableamt = 108,
        fnoIsuno_1 = 109,
        bnstp_1 = 110,
        unsttqty_1 = 111,
        lqdtableqty_1 = 112,
        avrprc_1 = 113,
        fnoIsuno_2 = 114,
        bnstp_2 = 115,
        unsttqty_2 = 116,
        lqdtableqty_2 = 117,
        avrprc_2 = 118
    }
}