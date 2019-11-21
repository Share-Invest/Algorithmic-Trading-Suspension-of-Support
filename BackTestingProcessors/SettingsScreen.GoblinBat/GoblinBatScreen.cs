﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShareInvest.BackTesting.Analysis;
using ShareInvest.Communication;
using ShareInvest.Information;
using ShareInvest.Market;
using ShareInvest.RetrieveOptions;

namespace ShareInvest.BackTesting.SettingsScreen
{
    public partial class GoblinBatScreen : UserControl
    {
        public GoblinBatScreen(IAsset asset)
        {
            this.asset = asset;
            InitializeComponent();
            BackColor = Color.FromArgb(121, 133, 130);
            numericCapital.Value = asset.Assets;
            labelH.Text = asset.Hedge.ToString();
            labelR.Text = asset.Reaction.ToString();
            labelSD.Text = asset.ShortDayPeriod.ToString();
            labelST.Text = asset.ShortTickPeriod.ToString();
            labelLD.Text = asset.LongDayPeriod.ToString();
            labelLT.Text = asset.LongTickPeriod.ToString();
            ran = new Random();
        }
        public void SetProgress(Progress pro)
        {
            this.pro = pro;
            timer.Start();
        }
        private int SetOptimize(IAsset asset)
        {
            set = new StrategySetting
            {
                ShortTick = SetValue(asset.ShortTickPeriod - ran.Next(asset.ShortTickPeriod - 1), ran.Next(1, asset.ShortTickPeriod / 2), asset.ShortTickPeriod + ran.Next(asset.ShortTickPeriod)),
                ShortDay = SetValue(asset.ShortDayPeriod - ran.Next(asset.ShortDayPeriod - 1), ran.Next(1, asset.ShortDayPeriod / 2), asset.ShortDayPeriod + ran.Next(asset.ShortDayPeriod)),
                LongTick = SetValue(asset.LongTickPeriod - ran.Next(asset.LongTickPeriod - 1), ran.Next(1, asset.LongTickPeriod / 2), asset.LongTickPeriod + ran.Next(asset.LongTickPeriod)),
                LongDay = SetValue(asset.LongDayPeriod - ran.Next(asset.LongDayPeriod - 1), ran.Next(1, asset.LongDayPeriod / 2), asset.LongDayPeriod + ran.Next(asset.LongDayPeriod)),
                Reaction = SetValue(asset.Reaction - ran.Next(10, 50), ran.Next(1, 3), asset.Reaction + ran.Next(0, 30)),
                Hedge = SetValue(0, 1, ran.Next(1, 5)),
                Capital = asset.Assets
            };
            return set.EstimatedTime();
        }
        private void StartBackTesting(IStrategySetting set)
        {
            Max = set.EstimatedTime();
            button.Text = string.Concat("Estimated Back Testing Time is ", pro.Rate(Max).ToString("N0"), " Minutes.");
            button.ForeColor = Color.Maroon;
            checkBox.ForeColor = Color.DarkRed;
            checkBox.Text = "BackTesting";
            new Transmit(asset.Account, set.Capital);
            string path = string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Log\", DateTime.Now.Hour > 23 || DateTime.Now.Hour < 9 ? DateTime.Now.AddDays(-1).ToString("yyMMdd") : DateTime.Now.ToString("yyMMdd"), @"\");
            IOptions options = Options.Get();
            new ReceiveOptions(new Dictionary<string, double>(256));
            timerStorage.Start();

            foreach (int hedge in set.Hedge)
                foreach (int reaction in set.Reaction)
                    foreach (int sTick in set.ShortTick)
                        foreach (int sDay in set.ShortDay)
                            foreach (int lTick in set.LongTick)
                                foreach (int lDay in set.LongDay)
                                {
                                    if (sTick >= lTick || sDay >= lDay)
                                    {
                                        Application.DoEvents();

                                        continue;
                                    }
                                    new Task(() =>
                                    {
                                        new Analysize(new Specify
                                        {
                                            Repository = options.Repository,
                                            ShortTickPeriod = sTick,
                                            ShortDayPeriod = sDay,
                                            LongTickPeriod = lTick,
                                            LongDayPeriod = lDay,
                                            Reaction = reaction,
                                            Hedge = hedge,
                                            BasicAssets = set.Capital,
                                            PathLog = path,
                                            Strategy = string.Concat(sDay.ToString("D2"), '^', sTick.ToString("D2"), '^', lDay.ToString("D2"), '^', lTick.ToString("D2"), '^', reaction.ToString("D2"), '^', hedge.ToString("D2"))
                                        });
                                        pro.ProgressBarValue++;
                                    }).Start();
                                    Application.DoEvents();
                                }
        }
        private void SetMarketTick()
        {
            InterLink = true;
            pro.Maximum = pro.Retry(SetMaximum());
            new Storage(string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Statistics\", DateTime.Now.Hour > 23 || DateTime.Now.Hour < 9 ? DateTime.Now.AddDays(-1).ToString("yyMMdd") : DateTime.Now.ToString("yyMMdd"), ".csv"));

            if (TimerBox.Show("Do You Want to Continue with Trading??\n\nIf You don't Want to Proceed,\nPress 'No'.\n\nAfter 5 Minutes the Program is Terminated.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 332735).Equals((DialogResult)6))
                Process.Start("Kospi200.exe");

            SendMarket?.Invoke(this, new OpenMarket(0));
            Application.ExitThread();
            Application.Exit();
        }
        private void CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (!button.ForeColor.Equals(Color.Maroon))
            {
                if (button.ForeColor.Equals(Color.Ivory) && CheckCurrent)
                {
                    set = new StrategySetting
                    {
                        ShortTick = SetValue((int)numericPST.Value, (int)numericIST.Value, (int)numericDST.Value),
                        ShortDay = SetValue((int)numericPSD.Value, (int)numericISD.Value, (int)numericDSD.Value),
                        LongTick = SetValue((int)numericPLT.Value, (int)numericILT.Value, (int)numericDLT.Value),
                        LongDay = SetValue((int)numericPLD.Value, (int)numericILD.Value, (int)numericDLD.Value),
                        Reaction = SetValue((int)numericPR.Value, (int)numericIR.Value, (int)numericDR.Value),
                        Hedge = SetValue((int)numericPH.Value, (int)numericIH.Value, (int)numericDH.Value),
                        Capital = (long)numericCapital.Value
                    };
                    button.Text = string.Concat("Estimated Back Testing Time is ", pro.Rate(set.EstimatedTime()).ToString("N0"), " Minutes.");
                    checkBox.Text = "Reset";
                    checkBox.ForeColor = Color.Yellow;
                    button.ForeColor = Color.Gold;

                    return;
                }
                button.Text = string.Concat("Click to Recommend ", DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) ? 2880 + 870 : 870, " Minutes and Save Existing Data.");
                button.ForeColor = Color.Ivory;
                checkBox.ForeColor = Color.Ivory;
                checkBox.Text = "Process";
            }
        }
        private void ButtonClick(object sender, EventArgs e)
        {
            if (CheckCurrent && button.ForeColor.Equals(Color.Gold))
                StartBackTesting(set);

            else if (button.ForeColor.Equals(Color.Ivory) && TimerBox.Show("Do You Want to Store Only Existing Data\nWithout Back Testing?\n\nIf Not Selected,\nIt will be Saved after 30 Seconds and the Program will Exit.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 32735).Equals((DialogResult)6))
            {
                timerStorage.Start();
                SetMarketTick();
            }
            else if (button.ForeColor.Equals(Color.Maroon))
                button.Text = string.Concat(((Max - pro.ProgressBarValue) / 155).ToString("N0"), " Minutes left to Complete.");
        }
        private int[] SetValue(int sp, int interval, int destination)
        {
            int[] value = new int[(destination - sp) / interval + 1];
            int i;

            for (i = 0; i < value.Length; i++)
                value[i] = sp + interval * i;

            return value;
        }
        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            int setting;

            if (TimerBox.Show("Start Back Testing.\n\nClick 'No' to Do this Manually.\n\nIf Not Selected,\nIt will Automatically Proceed after 20 Seconds.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 25617).Equals((DialogResult)7))
                return;

            do
            {
                setting = SetOptimize(asset);
            }
            while (setting < 155 * (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) ? 2880 + 840 : 840) || setting > 155 * (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) ? 2880 + 900 : 900));

            StartBackTesting(set);
            timer.Dispose();
        }
        private void TimerStorageTick(object sender, EventArgs e)
        {
            if (Max.Equals(pro.ProgressBarValue) && InterLink == false)
                SetMarketTick();

            if (InterLink && pro.Maximum > pro.ProgressBarValue)
                pro.ProgressBarValue++;

            Application.DoEvents();
        }
        private int SetMaximum()
        {
            string[] temp;
            int date = 0;

            try
            {
                foreach (string val in Directory.GetDirectories(string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Log\")))
                {
                    temp = val.Split('\\');
                    int recent = int.Parse(temp[temp.Length - 1]);

                    if (recent > date)
                        date = recent;
                }
                timerStorage.Interval = 15;
            }
            catch (Exception ex)
            {
                TimerBox.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", 3750);
                Environment.Exit(0);
            }
            return Directory.GetFiles(string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Log\", date), "*.csv", SearchOption.AllDirectories).Length;
        }
        private bool CheckCurrent
        {
            get
            {
                return checkBox.Checked;
            }
        }
        private bool InterLink
        {
            get; set;
        }
        private int Max
        {
            get; set;
        }
        private Progress pro;
        private IStrategySetting set;
        private readonly IAsset asset;
        private readonly Random ran;
        public event EventHandler<OpenMarket> SendMarket;
    }
}