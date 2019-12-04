﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShareInvest.BackTesting.Analysis;
using ShareInvest.Communication;
using ShareInvest.Information;
using ShareInvest.Log.Message;
using ShareInvest.MassProcessingTechnology;
using ShareInvest.RetrieveOptions;

namespace ShareInvest.BackTesting.SettingsScreen
{
    public partial class GoblinBatScreen : UserControl
    {
        public GoblinBatScreen(int count, IAsset asset)
        {
            this.count = count;
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
            try
            {
                set = new StrategySetting
                {
                    ShortTick = SetValue(50, ran.Next(5, 20), ran.Next(asset.ShortTickPeriod < 50 ? 70 : asset.ShortTickPeriod, asset.ShortTickPeriod * 5)),
                    LongTick = SetValue(200, ran.Next(50, 200), ran.Next(asset.LongTickPeriod < 200 ? 210 : asset.LongTickPeriod, asset.LongTickPeriod * 5)),
                    ShortDay = SetValue(2, ran.Next(1, 3), ran.Next(5, asset.ShortDayPeriod * 5)),
                    LongDay = SetValue(5, ran.Next(5, 15), ran.Next(20, asset.LongDayPeriod * 5)),
                    Reaction = SetValue(ran.Next(15, 30), ran.Next(1, 5), ran.Next(85, 100)),
                    Hedge = SetValue(0, 1, ran.Next(1, 5)),
                    Capital = asset.Assets
                };
                return set.EstimatedTime();
            }
            catch (Exception ex)
            {
                new LogMessage().Record("Exception", ex.ToString());
            }
            return 0;
        }
        private void StartBackTesting(IStrategySetting set)
        {
            Max = set.EstimatedTime();
            button.Text = string.Concat("Estimated Back Testing Time is ", pro.Rate(Max, count).ToString("N0"), " Minutes.");
            button.ForeColor = Color.Maroon;
            checkBox.ForeColor = Color.DarkRed;
            checkBox.Text = "BackTesting";
            new Transmit(asset.Account, set.Capital);
            string path = string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Log\", DateTime.Now.Hour > 23 || DateTime.Now.Hour < 9 ? DateTime.Now.AddDays(-1).ToString("yyMMdd") : DateTime.Now.ToString("yyMMdd"), @"\");
            IOptions options = new Options();
            GC.Collect();
            Count = Process.GetCurrentProcess().Threads.Count;

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
                                        if (Max <= ++pro.ProgressBarValue && InterLink == false)
                                        {
                                            button.ForeColor = Color.Ivory;
                                            SetMarketTick(GC.GetTotalMemory(true));
                                        }
                                    }).Start();
                                    Application.DoEvents();
                                }
        }
        private void SetMarketTick(long wait)
        {
            InterLink = true;
            GC.Collect();

            for (long i = 0; i < wait; i++)
                if (Count > Process.GetCurrentProcess().Threads.Count)
                    break;

            pro.Maximum = SetMaximum();
            pro.Retry();
            GC.Collect();
            new BulkProcessing(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Statistics\", DateTime.Now.Ticks, ".csv"));
            GC.Collect();

            if (TimerBox.Show(string.Concat("Do You Want to Continue with Trading??\n\nIf You don't Want to Proceed,\nPress 'No'.\n\nAfter ", pro.Maximum / 60000, " Minutes the Program is Terminated."), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (uint)pro.Maximum).Equals((DialogResult)6))
                Process.Start(string.Concat(Application.StartupPath, @"\Kospi200.exe"));

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
                    button.Text = string.Concat("Estimated Back Testing Time is ", pro.Rate(set.EstimatedTime(), count).ToString("N0"), " Minutes.");
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
            else if (CheckCurrent == false)
            {
                checkBox.Text = GC.GetTotalMemory(false).ToString("N0");
                checkBox.Font = new Font(checkBox.Font.Name, 11.75F, FontStyle.Regular);

                if (TimerBox.Show("Do You want to Clean Up the Accumulated Memory?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, 1325).Equals(DialogResult.OK))
                    GC.Collect();
            }
            else if (CheckCurrent)
            {
                checkBox.Text = string.Concat("Parallel ", Process.GetCurrentProcess().Threads.Count - Count > 0 ? (Process.GetCurrentProcess().Threads.Count - Count).ToString("N0") : "END");
                checkBox.Font = new Font(checkBox.Font.Name, 15.75F, FontStyle.Regular);
            }
        }
        private void ButtonClick(object sender, EventArgs e)
        {
            if (CheckCurrent && button.ForeColor.Equals(Color.Gold))
                StartBackTesting(set);

            else if (InterLink == false && button.ForeColor.Equals(Color.Ivory) && TimerBox.Show("Do You Want to Store Only Existing Data\nWithout Back Testing?\n\nIf Not Selected,\nIt will be Saved after 30 Seconds and the Program will Exit.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 32735).Equals((DialogResult)6))
                SetMarketTick(0);

            else if (button.ForeColor.Equals(Color.Maroon))
                button.Text = string.Concat(((Max - pro.ProgressBarValue) / count).ToString("N0"), " Minutes left to Complete.");
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
            timer.Dispose();

            if (TimerBox.Show("Start Back Testing.\n\nClick 'No' to Do this Manually.\n\nIf Not Selected,\nIt will Automatically Proceed after 20 Seconds.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 25617).Equals((DialogResult)7))
                return;

            do
            {
                setting = SetOptimize(asset);
            }
            while (setting < count * (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) ? 2880 + 935 : 935) || setting > count * (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) ? 2880 + 985 : 985));

            StartBackTesting(set);
        }
        private void TimerStorageTick(object sender, EventArgs e)
        {
            if (InterLink && pro.Maximum - 20 > pro.ProgressBarValue)
                pro.ProgressBarValue += 15;

            else if (InterLink && pro.Maximum > pro.ProgressBarValue)
                pro.ProgressBarValue++;

            Application.DoEvents();
        }
        private int SetMaximum()
        {
            string[] temp;
            int date = 0;

            try
            {
                foreach (string val in Directory.GetDirectories(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Log\")))
                {
                    temp = val.Split('\\');
                    int recent = int.Parse(temp[temp.Length - 1]);

                    if (recent > date)
                        date = recent;
                }
                timerStorage.Interval = 15;
                timerStorage.Start();
            }
            catch (Exception ex)
            {
                new LogMessage().Record("Exception", ex.ToString());
                MessageBox.Show(string.Concat(ex.ToString(), "\n\nQuit the Program."), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
            return Directory.GetFiles(string.Concat(Path.Combine(Application.StartupPath, @"..\"), @"\Log\", date), "*.csv", SearchOption.AllDirectories).Length;
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
        private int Count
        {
            get; set;
        }
        private Progress pro;
        private IStrategySetting set;
        private readonly int count;
        private readonly IAsset asset;
        private readonly Random ran;
    }
}