using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ShareInvest.Communication;
using ShareInvest.Log.Message;
using ShareInvest.Models;
using ShareInvest.RemainingDate;

namespace ShareInvest.BackTesting.Analysis
{
    public class Analysize
    {
        public Analysize(Remaining remaining, IStrategy st)
        {
            ema = new EMA();
            days = st.ShortDayPeriod > 1 && st.LongDayPeriod > 2 ? true : false;
            count = st.Quantity + 1;
            Headway = st.Time;

            if (days)
            {
                shortDay = new List<double>(512);
                longDay = new List<double>(512);
            }
            shortTick = new List<double>(2097152);
            longTick = new List<double>(2097152);
            Send += Analysis;
            this.st = st;
            info = new Information(st);
            this.remaining = remaining;
            GetChart();
            info.Log();
        }
        private int Analysis(double price)
        {
            int sc = shortTick.Count, lc = longTick.Count;
            shortTick.Add(sc > 0 ? ema.Make(st.ShortTickPeriod, sc, price, shortTick[sc - 1]) : ema.Make(price));
            longTick.Add(lc > 0 ? ema.Make(st.LongTickPeriod, lc, price, longTick[lc - 1]) : ema.Make(price));

            return (sc < 2 || lc < 2) ? 0 : shortTick[sc] - longTick[lc] - (shortTick[sc - 1] - longTick[lc - 1]) > 0 ? 1 : -1;
        }
        private int Analysis(string time, double price)
        {
            int sc = shortDay.Count, lc = longDay.Count;

            if ((time.Length == 6 && !time.Equals(initiation) ? false : time.Length == 2 ? true : ConfirmDate(time.Substring(0, 6))) == false)
            {
                shortDay[sc - 1] = ema.Make(st.ShortDayPeriod, sc, price, sc > 1 ? shortDay[sc - 2] : 0);
                longDay[lc - 1] = ema.Make(st.LongDayPeriod, lc, price, lc > 1 ? longDay[lc - 2] : 0);

                return shortDay[sc - 1] - longDay[lc - 1] - (shortDay[sc - 2] - longDay[lc - 2]) > 0 ? 1 : -1;
            }
            shortDay.Add(sc > 0 ? ema.Make(st.ShortDayPeriod, sc, price, shortDay[sc - 1]) : ema.Make(price));
            longDay.Add(lc > 0 ? ema.Make(st.LongDayPeriod, lc, price, longDay[lc - 1]) : ema.Make(price));

            return sc > 1 && lc > 1 ? shortDay[sc] - longDay[lc] - (shortDay[sc - 1] - longDay[lc - 1]) > 0 ? 1 : -1 : 0;
        }
        private void Analysis(object sender, Datum e)
        {
            int i, quantity = days ? Order(Analysis(e.Price), Analysis(e.Time, e.Price)) : Order(Analysis(e.Price));
            double max = st.BasicAssets / count / (e.Price * st.TransactionMultiplier * st.MarginRate);

            if (e.Time.Length > 2 && e.Time.Substring(6, 4).Equals("1545") || Array.Exists(info.Kospi, o => o.Equals(e.Time)))
            {
                info.Save(e.Time, e.Price);

                return;
            }
            if ((e.Volume > st.Reaction || e.Volume < -st.Reaction) && Math.Abs(e.Volume) < Math.Abs(e.Volume + quantity) && Interval())
            {
                if (Math.Abs(info.Quantity + quantity) < max)
                    for (i = 0; i < count; i++)
                        info.Operate(e.Price, quantity);

                else if (Math.Abs(info.Quantity) > max)
                    info.Operate(e.Price, info.Quantity > 0 ? -count : count);

                return;
            }
            if (Array.Exists(remaining.Date, o => o.Equals(e.Time)) && Math.Abs(info.Quantity) > 0)
                for (i = info.Quantity; i > 0; i--)
                    info.Operate(e.Price, info.Quantity > 0 ? -1 : 1);
        }
        private bool Interval()
        {
            if (Headway-- > 0)
                return false;

            Headway = st.Time;

            return true;
        }
        private int Order(int tick, int day)
        {
            return tick > 0 && day > 0 ? 1 : tick < 0 && day < 0 ? -1 : 0;
        }
        private int Order(int tick)
        {
            return tick > 0 ? 1 : tick < 0 ? -1 : 0;
        }
        private void GetChart()
        {
            foreach (var temp in new Fetch(code))
            {
                Console.WriteLine(temp.GetType());
            }
        }
        private bool ConfirmDate(string date)
        {
            if (date.Equals(Register))
                return false;

            Register = date;

            return true;
        }
        private string Register
        {
            get; set;
        }
        private int Headway
        {
            get; set;
        }
        private const string initiation = "090000";
        private readonly string code;
        private readonly int count;
        private readonly bool days;
        private readonly IStrategy st;
        private readonly EMA ema;
        private readonly Information info;
        private readonly Remaining remaining;
        private readonly List<double> shortDay;
        private readonly List<double> longDay;
        private readonly List<double> shortTick;
        private readonly List<double> longTick;
        public event EventHandler<Datum> Send;
    }
}