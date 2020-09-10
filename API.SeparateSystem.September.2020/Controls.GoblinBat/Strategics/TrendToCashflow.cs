﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using ShareInvest.FindByName;

namespace ShareInvest.Controls
{
    partial class TrendToCashflow : UserControl
    {
        internal TrendToCashflow(Catalog.Codes codes)
        {
            InitializeComponent();
            random = new Random();
            boxTrend.Text = codes.Name;
            code = codes.Code;

            foreach (var radio in panel.Controls)
                if (radio is RadioButton button)
                    button.Checked = random.Next(0, 4) == 0;
        }
        internal void TransmuteStrategics(string[] strategics)
        {
            for (int i = 0; i < strategics.Length; i++)
                if (decimal.TryParse(strategics[i], out decimal value))
                    string.Concat(numeric, this.strategics[i]).FindByName<NumericUpDown>(this).Value = value;
        }
        internal bool TransmuteStrategics()
        {
            var pass = false;

            foreach (var radio in panel.Controls)
                if (radio is RadioButton button && button.Checked)
                    pass = true;

            return numericShort.Value < numericLong.Value && pass;
        }
        internal string TransmuteStrategics(string code)
        {
            if (this.code.Equals(code))
            {
                var sb = new StringBuilder("TC|").Append(code);

                foreach (var str in strategics)
                    sb.Append('|').Append(string.Concat(numeric, str).FindByName<NumericUpDown>(this).Value);

                return sb.ToString();
            }
            return string.Empty;
        }
        internal IEnumerable<RadioButton> RadioButtons
        {
            get
            {
                foreach (var radio in panel.Controls)
                    if (radio is RadioButton button)
                        yield return button;
            }
        }
        readonly string code;
        readonly Random random;
    }
}