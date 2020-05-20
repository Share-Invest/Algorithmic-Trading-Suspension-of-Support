﻿using System;
using System.Drawing;
using System.Windows.Forms;
using ShareInvest.EventHandler;
using ShareInvest.Catalog;
using ShareInvest.EventHandler.OpenAPI;
using ShareInvest.Message;

namespace ShareInvest.GoblinBatControls
{
    public partial class BalanceControl : UserControl
    {
        public BalanceControl()
        {
            InitializeComponent();
            balGrid.ColumnCount = 7;
            balGrid.BackgroundColor = Color.FromArgb(121, 133, 130);

            for (int i = 0; i < columns.Length; i++)
                balGrid.Columns[i].Name = columns[i];
        }
        public void OnRealTimeCurrentPriceReflect(object sender, Current e)
        {
            if (e.Quantity != 0 && balGrid.Rows.Count > 0)
                BeginInvoke(new Action(() =>
                {
                    foreach (DataGridViewRow row in balGrid.Rows)
                    {
                        var type = row.Cells[6];
                        var current = row.Cells[5];
                        long revenue = 0;
                        string str = e.Price.ToString("N2");

                        if (str.Equals(current.Value))
                            break;

                        if (row.Cells[0].Value.ToString().Contains("101") && double.TryParse(row.Cells[4].Value.ToString(), out double purchase))
                        {
                            current.Value = str;
                            int absolute = e.Quantity > 0 ? e.Quantity : -e.Quantity;
                            revenue = (long)((e.Price - purchase) * e.Quantity * Const.TransactionMultiplier - purchase * absolute * Const.TransactionMultiplier * Const.Commission - e.Price * absolute * Const.TransactionMultiplier * Const.Commission);
                            type.Value = revenue.ToString("N0");
                        }
                        if (revenue > 0)
                        {
                            type.Style.ForeColor = Color.Maroon;
                            type.Style.SelectionForeColor = Color.FromArgb(0xB9062F);
                        }
                        else if (revenue < 0)
                        {
                            type.Value = type.Value.ToString().Replace("-", string.Empty);
                            type.Style.ForeColor = Color.Navy;
                            type.Style.SelectionForeColor = Color.DeepSkyBlue;
                        }
                    }
                    Application.DoEvents();
                }));
        }
        public void OnReceiveBalance(object sender, Balance e) => BeginInvoke(new Action(() =>
        {
            balGrid.SuspendLayout();
            balGrid.Rows.Clear();
            balGrid.AutoSize = true;

            try
            {
                foreach (string info in e.Hold)
                {
                    string[] arr = new string[7];
                    int i = 0;

                    foreach (string val in info.Split(';'))
                    {
                        if (string.IsNullOrEmpty(val))
                            break;

                        switch (i)
                        {
                            case 0:
                            case 1:
                                arr[i++] = val;
                                break;

                            case 2:
                                arr[i++] = val.Equals("1") ? sell : buy;
                                break;

                            case 3:
                            case 6:
                                if (int.TryParse(val, out int num))
                                    arr[i++] = num.ToString("N0");

                                break;

                            case 4:
                            case 5:
                                if (double.TryParse(val, out double dot))
                                    arr[i++] = (val.Contains(".") ? dot : (dot / 100)).ToString("N2");

                                break;
                        }
                    }
                    if (arr[0] != null)
                        balGrid.Rows.Add(arr);
                }
                foreach (DataGridViewRow row in balGrid.Rows)
                {
                    var type = row.Cells[2];

                    if (type.Value.Equals(sell))
                    {
                        type.Style.ForeColor = Color.Navy;
                        type.Style.SelectionForeColor = Color.DeepSkyBlue;
                    }
                    else if (type.Value.Equals(buy))
                    {
                        type.Style.ForeColor = Color.Maroon;
                        type.Style.SelectionForeColor = Color.FromArgb(0xB9062F);
                    }
                    type = row.Cells[6];
                    var str = type.Value.ToString();

                    if (string.IsNullOrEmpty(str) == false && long.TryParse(str.Replace(",", string.Empty), out long revenue))
                        if (revenue > 0)
                        {
                            type.Style.ForeColor = Color.Maroon;
                            type.Style.SelectionForeColor = Color.FromArgb(0xB9062F);
                        }
                        else if (revenue < 0)
                        {
                            type.Value = type.Value.ToString().Replace("-", string.Empty);
                            type.Style.ForeColor = Color.Navy;
                            type.Style.SelectionForeColor = Color.DeepSkyBlue;
                        }
                }
            }
            catch (Exception ex)
            {
                new ExceptionMessage(ex.StackTrace);
            }
            balGrid.Show();
            balGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            balGrid.Cursor = Cursors.Hand;
            balGrid.AutoResizeRows();
            balGrid.AutoResizeColumns();
            SendReSize?.Invoke(this, new GridResize(balGrid.Rows.GetRowsHeight(DataGridViewElementStates.None), balGrid.Rows.Count));
            balGrid.ResumeLayout();
        }));
        const string sell = "매도";
        const string buy = "매수";
        readonly string[] columns = { "종목코드", "종목명", "구분", "수량", "매입가", "현재가", "평가손익" };
        public event EventHandler<GridResize> SendReSize;
    }
}