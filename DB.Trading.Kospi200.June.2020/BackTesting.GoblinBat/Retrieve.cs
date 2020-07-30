﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using ShareInvest.Catalog;
using ShareInvest.Catalog.XingAPI;
using ShareInvest.GoblinBatContext;
using ShareInvest.Message;

namespace ShareInvest.Strategy
{
    public partial class Retrieve : CallUpStatisticalAnalysis
    {
        public Retrieve(string key, char initial) : base(key)
        {
            secret = new Secret();
            Initial = initial;
        }
        public Dictionary<DateTime, string> OnReceiveInformation(Catalog.DataBase.ImitationGame number) => GetInformation(number, Code);
        public bool OnReceiveRepositoryID(Catalog.DataBase.ImitationGame specifies) => GetRepositoryID(specifies, RecentDate);
        public Models.Strategics GetBestStrategy() => GetBestStrategyRecommend(Information.Statistics);
        public Models.Strategics OnReceiveMyStrategy() => GetMyStrategy();
        public void SetIsMirror() => SetInitialzeTheCode();
        public Catalog.XingAPI.Specify[] GetUserStrategy()
        {
            var game = new Models.Strategics();
            var recommend = GetBestStrategyRecommend(Information.Statistics, game);
            var rank = secret.GetRank(recommend.Item3);

            if (recommend.Item5 == null || TimerBox.Show(secret.GetMessage(recommend.Item4, recommend.Item1, recommend.Item4 / (double)recommend.Item1), rank, MessageBoxButtons.YesNo, MessageBoxIcon.Question, 0 > recommend.Item4 ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2, 13975U).Equals(DialogResult.Yes))
                game = recommend.Item2;

            else if (recommend.Item1 == int.MinValue)
            {
                var user = GetUserStrategics();

                if (user != null)
                    return GetCatalog(user);

                else
                    GetUserStrategy();
            }
            else
                game = recommend.Item5;

            new Task(() => new ExceptionMessage(game, rank)).Start();
            SetIdentify(new Models.Identify
            {
                Assets = game.Assets,
                Strategy = game.Strategy,
                Commission = game.Commission,
                RollOver = game.RollOver ? CheckState.Checked : CheckState.Unchecked,
                Code = game.Code,
                BaseShort = game.BaseShort,
                BaseLong = game.BaseLong,
                NonaTime = game.NonaTime,
                NonaShort = game.NonaShort,
                NonaLong = game.NonaLong,
                OctaTime = game.OctaTime,
                OctaShort = game.OctaShort,
                OctaLong = game.OctaLong,
                HeptaTime = game.HeptaTime,
                HeptaShort = game.HeptaShort,
                HeptaLong = game.HeptaLong,
                HexaTime = game.HexaTime,
                HexaShort = game.HexaShort,
                HexaLong = game.HexaLong,
                PentaTime = game.PentaTime,
                PentaShort = game.PentaShort,
                PentaLong = game.PentaLong,
                QuadTime = game.QuadTime,
                QuadShort = game.QuadShort,
                QuadLong = game.QuadLong,
                TriTime = game.TriTime,
                TriShort = game.TriShort,
                TriLong = game.TriLong,
                DuoTime = game.DuoTime,
                DuoShort = game.DuoShort,
                DuoLong = game.DuoLong,
                MonoTime = game.MonoTime,
                MonoShort = game.MonoShort,
                MonoLong = game.MonoLong
            });
            return GetCatalog(game);
        }
        public void GetInitialzeTheCode()
        {
            Code = GetStrategy();
            SetInitialzeTheCode();
        }
        public void SetInitializeTheChart()
        {
            if (Chart != null)
            {
                Chart.Clear();
                Chart = null;
            }
            if (Charts != null)
                Charts = null;

            if (Quotes != null)
            {
                Quotes.Clear();
                Quotes = null;
            }
            if (QuotesEnumerable != null)
                QuotesEnumerable = null;
        }
        public bool GetDuplicateResults(Models.Strategics game)
        {
            if (string.IsNullOrEmpty(RecentDate))
                GetRecentDate(DateTime.Now);

            return GetDuplicateResults(game, RecentDate);
        }
        public string GetDate(string code)
        {
            if (DateTime.TryParseExact(SetDate(code).Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date))
                return string.Concat(date.ToLongDateString(), " ", date.ToShortTimeString());

            return string.Empty;
        }
        public Models.Strategics GetImitationModel(Catalog.DataBase.ImitationGame game) => new Models.Strategics
        {
            Assets = game.Assets,
            Code = game.Code,
            Commission = game.Commission,
            MarginRate = game.MarginRate,
            Strategy = game.Strategy,
            RollOver = game.RollOver,
            BaseTime = game.BaseTime,
            BaseShort = game.BaseShort,
            BaseLong = game.BaseLong,
            NonaTime = game.NonaTime,
            NonaShort = game.NonaShort,
            NonaLong = game.NonaLong,
            OctaTime = game.OctaTime,
            OctaShort = game.OctaShort,
            OctaLong = game.OctaLong,
            HeptaTime = game.HeptaTime,
            HeptaShort = game.HeptaShort,
            HeptaLong = game.HeptaLong,
            HexaTime = game.HexaTime,
            HexaShort = game.HexaShort,
            HexaLong = game.HexaLong,
            PentaTime = game.PentaTime,
            PentaShort = game.PentaShort,
            PentaLong = game.PentaLong,
            QuadTime = game.QuadTime,
            QuadShort = game.QuadShort,
            QuadLong = game.QuadLong,
            TriTime = game.TriTime,
            TriShort = game.TriShort,
            TriLong = game.TriLong,
            DuoTime = game.DuoTime,
            DuoShort = game.DuoShort,
            DuoLong = game.DuoLong,
            MonoTime = game.MonoTime,
            MonoShort = game.MonoShort,
            MonoLong = game.MonoLong
        };
        protected override string GetConvertCode(string code)
        {
            if (Code.Substring(0, 3).Equals(code.Substring(0, 3)) && Code.Substring(5).Equals(code.Substring(3)))
                return Code;

            return code;
        }
        public void SetStatistics(Setting setting, List<string> list, double rate)
        {
            var queue = new Queue<Models.Statistics>();

            for (int i = 20; i < 100; i++)
                list.Add(i.ToString());

            foreach (var str in list)
                if (str.Equals("Auto") == false)
                {
                    queue.Enqueue(new Models.Statistics
                    {
                        Assets = setting.Assets,
                        Code = setting.Code,
                        Commission = setting.Commission,
                        MarginRate = rate,
                        Strategy = str,
                        RollOver = true
                    });
                    queue.Enqueue(new Models.Statistics
                    {
                        Assets = setting.Assets,
                        Code = setting.Code,
                        Commission = setting.Commission,
                        MarginRate = rate,
                        Strategy = str,
                        RollOver = false
                    });
                }
            SetInsertBaseStrategy(queue);
        }
        public int SetIdentify(Setting setting) => SetIdentify(new Models.Identify
        {
            Assets = setting.Assets,
            Strategy = setting.Strategy,
            Commission = setting.Commission,
            RollOver = setting.RollOver,
            Code = setting.Code,
            BaseShort = setting.BaseShort,
            BaseLong = setting.BaseLong,
            NonaTime = setting.NonaTime,
            NonaShort = setting.NonaShort,
            NonaLong = setting.NonaLong,
            OctaTime = setting.OctaTime,
            OctaShort = setting.OctaShort,
            OctaLong = setting.OctaLong,
            HeptaTime = setting.HeptaTime,
            HeptaShort = setting.HeptaShort,
            HeptaLong = setting.HeptaLong,
            HexaTime = setting.HexaTime,
            HexaShort = setting.HexaShort,
            HexaLong = setting.HexaLong,
            PentaTime = setting.PentaTime,
            PentaShort = setting.PentaShort,
            PentaLong = setting.PentaLong,
            QuadTime = setting.QuadTime,
            QuadShort = setting.QuadShort,
            QuadLong = setting.QuadLong,
            TriTime = setting.TriTime,
            TriShort = setting.TriShort,
            TriLong = setting.TriLong,
            DuoTime = setting.DuoTime,
            DuoShort = setting.DuoShort,
            DuoLong = setting.DuoLong,
            MonoTime = setting.MonoTime,
            MonoShort = setting.MonoShort,
            MonoLong = setting.MonoLong
        });
        public void GetRecentDate(DateTime now)
        {
            for (int i = -1; i > int.MinValue; i--)
            {
                var date = now.AddDays(i);
                var max = GetRecentDate(date.ToString(recent));

                if (string.IsNullOrEmpty(max) == false)
                {
                    RecentDate = max;

                    return;
                }
            }
        }
        public static string Code
        {
            get; private set;
        }
        public static string Date
        {
            get
            {
                var time = Initial.Equals((char)Port.Trading) && QuotesEnumerable != null ? QuotesEnumerable.Last().Value.Last().Time : Charts.Last().Value.Last().Date.ToString();

                if (DateTime.TryParseExact(time.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date))
                    return string.Concat(date.ToLongDateString(), " ", date.ToShortTimeString());

                else
                    return string.Empty;
            }
        }
        internal static Catalog.XingAPI.Specify[] GetCatalog(Models.Strategics find)
        {
            var temp = new Catalog.XingAPI.Specify[10];
            int i = 0;

            while (i < temp.Length)
                switch (i)
                {
                    case 0:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.BaseTime,
                            Short = find.BaseShort,
                            Long = find.BaseLong
                        };
                        break;

                    case 1:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.NonaTime,
                            Short = find.NonaShort,
                            Long = find.NonaLong
                        };
                        break;

                    case 2:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.OctaTime,
                            Short = find.OctaShort,
                            Long = find.OctaLong
                        };
                        break;

                    case 3:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.HeptaTime,
                            Short = find.HeptaShort,
                            Long = find.HeptaLong
                        };
                        break;

                    case 4:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.HexaTime,
                            Short = find.HexaShort,
                            Long = find.HexaLong
                        };
                        break;

                    case 5:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.PentaTime,
                            Short = find.PentaShort,
                            Long = find.PentaLong
                        };
                        break;

                    case 6:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.QuadTime,
                            Short = find.QuadShort,
                            Long = find.QuadLong
                        };
                        break;

                    case 7:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.TriTime,
                            Short = find.TriShort,
                            Long = find.TriLong
                        };
                        break;

                    case 8:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.DuoTime,
                            Short = find.DuoShort,
                            Long = find.DuoLong
                        };
                        break;

                    case 9:
                        temp[i++] = new Catalog.XingAPI.Specify
                        {
                            Assets = (ulong)find.Assets,
                            Code = find.Code,
                            Commission = find.Commission,
                            MarginRate = find.MarginRate,
                            Strategy = find.Strategy,
                            RollOver = find.RollOver,
                            Time = (uint)find.MonoTime,
                            Short = find.MonoShort,
                            Long = find.MonoLong
                        };
                        break;
                }
            return temp;
        }
        protected internal static Queue<Chart> Chart
        {
            get; private set;
        }
        protected internal static Queue<Quotes> Quotes
        {
            get; private set;
        }
        protected internal static IOrderedEnumerable<KeyValuePair<long, Queue<Chart>>> Charts
        {
            get; private set;
        }
        protected internal static IOrderedEnumerable<KeyValuePair<long, Queue<Quotes>>> QuotesEnumerable
        {
            get; private set;
        }
        static char Initial
        {
            get; set;
        }
        static string RecentDate
        {
            get; set;
        }
        void SetInitialzeTheCode()
        {
            do
            {
                if (Code == null)
                    Code = GetStrategy();

                if (Charts == null)
                    Charts = GetChart(new Dictionary<long, Queue<Chart>>(1048576), Code).OrderBy(o => o.Key);

                if (Chart == null)
                    Chart = GetChart(Code, new Queue<Chart>(1048576), new System.IO.DirectoryInfo(Path));
            }
            while (Code == null || Charts == null || Chart == null);
        }
        readonly Secret secret;
        const string format = "yyMMddHHmmss";
    }
}