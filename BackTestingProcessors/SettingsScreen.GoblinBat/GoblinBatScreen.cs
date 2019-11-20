﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShareInvest.BackTesting.Analysis;
using ShareInvest.Communication;
using ShareInvest.Market;
using ShareInvest.RetrieveOptions;

namespace ShareInvest.BackTesting.SettingsScreen
{
    public partial class GoblinBatScreen : UserControl
    {
        public GoblinBatScreen()
        {
            InitializeComponent();
        }
        public void SetProgress(Progress pro)
        {
            this.pro = pro;
            timer.Start();
        }
        private void StartBackTesting(IStrategySetting set)
        {
            Max = set.EstimatedTime();
            button.Text = string.Concat("Estimated Back Testing Time is ", pro.Rate(Max).ToString("N0"), " Minutes.");
            button.ForeColor = Color.Maroon;
            checkBox.ForeColor = Color.DarkRed;
            checkBox.Text = "BackTesting";
            string path = string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Log\", DateTime.Now.Hour > 23 || DateTime.Now.Hour < 9 ? DateTime.Now.AddDays(-1).ToString("yyMMdd") : DateTime.Now.ToString("yyMMdd"), @"\");
            IOptions options = Options.Get();
            new ReceiveOptions(new Dictionary<string, double>(256));

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
                                        pro.ProgressBarValue++;

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
                                        if (Max.Equals(pro.ProgressBarValue) && TimerBox.Show("Back Testing is Complete.\n\nDo You Want to Store the Data?\n\nIf Not Selected,\nIt will be Saved after 30 Seconds and the Program will Exit.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 32735).Equals((DialogResult)6))
                                        {
                                            new Storage(string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Statistics\", DateTime.Now.Hour > 23 || DateTime.Now.Hour < 9 ? DateTime.Now.AddDays(-1).ToString("yyMMdd") : DateTime.Now.ToString("yyMMdd"), ".csv"));
                                            SetMarketTick();
                                        }
                                    }).Start();
                                    Application.DoEvents();
                                }
        }
        private void SetMarketTick()
        {
            if (TimerBox.Show("Do You Want to Continue with Trading??\n\nIf You don't Want to Proceed,\nPress 'No'.\n\nAfter 30 Seconds the Program is Terminated.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 32735).Equals((DialogResult)6))
                Process.Start("Kospi200.exe");

            SendMarket?.Invoke(this, new OpenMarket(0));
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
                button.Text = string.Concat("Click to Recommend ", DateTime.Now.DayOfWeek.Equals(DayOfWeek.Friday) ? 3650 : 900, " Minutes and Save Existing Data.");
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
                pro.Maximum = SetMaximum();

                new Task(() =>
                {
                    new Storage(string.Concat(Path.Combine(Environment.CurrentDirectory, @"..\"), @"\Statistics\", DateTime.Now.Hour > 23 || DateTime.Now.Hour < 9 ? DateTime.Now.AddDays(-1).ToString("yyMMdd") : DateTime.Now.ToString("yyMMdd"), ".csv"));
                    SetMarketTick();
                }).Start();
            }
            else if (button.ForeColor.Equals(Color.Maroon))
                button.Text = string.Concat(((Max - pro.ProgressBarValue) / 210).ToString("N0"), " Minutes left to Complete.");
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

            if (TimerBox.Show("Start Back Testing.\n\nClick 'No' to Do this Manually.\n\nIf Not Selected,\nIt will Automatically Proceed after 20 Seconds.", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 25617).Equals((DialogResult)7))
                return;

            StartBackTesting(new StrategySetting
            {
                ShortTick = SetValue((int)numericPST.Value, (int)numericIST.Value, (int)numericDST.Value),
                ShortDay = SetValue((int)numericPSD.Value, (int)numericISD.Value, (int)numericDSD.Value),
                LongTick = SetValue((int)numericPLT.Value, (int)numericILT.Value, (int)numericDLT.Value),
                LongDay = SetValue((int)numericPLD.Value, (int)numericILD.Value, (int)numericDLD.Value),
                Reaction = SetValue((int)numericPR.Value, (int)numericIR.Value, (int)numericDR.Value),
                Hedge = SetValue((int)numericPH.Value, (int)numericIH.Value, (int)numericDH.Value),
                Capital = (long)numericCapital.Value
            });
        }
        private void TimerStorageTick(object sender, EventArgs e)
        {
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
                timerStorage.Start();
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
        private int Max
        {
            get; set;
        }
        private Progress pro;
        private IStrategySetting set;
        public event EventHandler<OpenMarket> SendMarket;
    }
}