﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ShareInvest.EventHandler;
using ShareInvest.Interface;
using ShareInvest.Interface.OpenAPI;
using ShareInvest.SecondaryIndicators;

namespace ShareInvest.Indicators
{
	public class TrendsToCashflow
	{
		int Accumulative
		{
			get; set;
		}
		double Before
		{
			get; set;
		}
		double Commission
		{
			get; set;
		}
		long TodayRevenue
		{
			get; set;
		}
		long TodayUnrealize
		{
			get; set;
		}
		internal Catalog.Strategics.Statistics SendMessage
		{
			get; private set;
		}
		internal (Stack<double>, Stack<double>, Stack<double>) ReturnTheUsedStack
		{
			get
			{
				Stack<double> st = new Stack<double>(), lg = new Stack<double>(), td = new Stack<double>(),
					short_stack = new Stack<double>(), long_stack = new Stack<double>(), trend_stack = new Stack<double>();

				if (Strategics is Catalog.TrendsToCashflow tc)
				{
					while (st.Count <= tc.Short)
						st.Push(Short.Pop());

					while (lg.Count <= tc.Long)
						lg.Push(Long.Pop());

					while (td.Count <= tc.Trend)
						td.Push(Trend.Pop());

					while (st.Count > 0)
						short_stack.Push(st.Pop());

					while (lg.Count > 0)
						long_stack.Push(lg.Pop());

					while (td.Count > 0)
						trend_stack.Push(td.Pop());
				}
				return (short_stack, long_stack, trend_stack);
			}
		}
		internal void StartProgress(double commission)
		{
			if (Strategics is Catalog.TrendsToCashflow cf)
			{
				Commission = commission;
				Short = new Stack<double>();
				Long = new Stack<double>();
				Trend = new Stack<double>();
				OrderNumber = new Dictionary<string, dynamic>();
				NextOrderTime = DateTime.MinValue;
				Line = new Tuple<int, int, int>(cf.Short, cf.Long, cf.Trend);
			}
		}
		public void OnReceiveDrawChart(object sender, SendConsecutive e)
		{
			if (GetCheckOnDate(e.Date))
			{
				Short.Pop();
				Long.Pop();
				Trend.Pop();
			}
			Trend.Push(Trend.Count > 0 ? EMA.Make(Line.Item3, Trend.Count, e.Price, Trend.Peek()) : EMA.Make(e.Price));
			Short.Push(Short.Count > 0 ? EMA.Make(Line.Item1, Short.Count, e.Price, Short.Peek()) : EMA.Make(e.Price));
			Long.Push(Long.Count > 0 ? EMA.Make(Line.Item2, Long.Count, e.Price, Long.Peek()) : EMA.Make(e.Price));

			if (e.Volume != 0 && e.Date.Length > 8 && Short.Count > 1 && Long.Count > 1)
			{
				double popShort = Short.Pop(), popLong = Long.Pop(), gap = popShort - popLong - (Short.Peek() - Long.Peek());
				Short.Push(popShort);
				Long.Push(popLong);
				var date = e.Date.Substring(6, 4);

				if (date.CompareTo(Base.Start) > 0 && date.CompareTo(Base.Transmit) < 0 && Strategics is Catalog.TrendsToCashflow tc
					&& DateTime.TryParseExact(e.Date.Substring(0, 12), Base.FullDateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime cInterval))
				{
					if (Balance.Quantity > tc.ReservationQuantity - 1 && (Offer ?? int.MaxValue) < e.Price
						&& OrderNumber.Any(o => o.Key[0] == '8' && o.Value == e.Price - Base.GetQuoteUnit(e.Price, Market)))
					{
						CumulativeFee += (uint)(e.Price * tc.ReservationQuantity * (Commission + Base.Tax));
						Balance.Revenue += (long)((e.Price - (Balance.Purchase ?? 0D)) * tc.ReservationQuantity);
						Balance.Quantity -= tc.ReservationQuantity;
						var profit = OrderNumber.First(o => o.Key.StartsWith("8") && o.Value == e.Price - Base.GetQuoteUnit(e.Price, Market));

						if (OrderNumber.Remove(profit.Key))
						{
							Capital -= profit.Value * tc.ReservationQuantity;
							Offer = profit.Value;
						}
					}
					else if ((Bid ?? int.MinValue) > e.Price && OrderNumber.Any(o => o.Key[0] == '7' && o.Value == e.Price + Base.GetQuoteUnit(e.Price, Market)))
					{
						CumulativeFee += (uint)(e.Price * Commission * tc.ReservationQuantity);
						Balance.Purchase
							= (double)((e.Price * tc.ReservationQuantity + (Balance.Purchase ?? 0D) * Balance.Quantity) / (Balance.Quantity + tc.ReservationQuantity));
						Balance.Quantity += tc.ReservationQuantity;
						var profit = OrderNumber.First(o => o.Key.StartsWith("7") && o.Value == e.Price + Base.GetQuoteUnit(e.Price, Market));

						if (OrderNumber.Remove(profit.Key))
						{
							Capital += profit.Value * tc.ReservationQuantity;
							Bid = profit.Value;
						}
					}
					else if (Balance.Quantity > tc.TradingQuantity - 1 && OrderNumber.Any(o => o.Key[0] == '2' && o.Value == e.Price - Base.GetQuoteUnit(e.Price, Market)))
					{
						CumulativeFee += (uint)(e.Price * tc.TradingQuantity * (Commission + Base.Tax));
						Balance.Revenue += (long)((e.Price - (Balance.Purchase ?? 0D)) * tc.TradingQuantity);
						Balance.Quantity -= tc.TradingQuantity;
						var profit = OrderNumber.First(o => o.Key.StartsWith("2") && o.Value == e.Price - Base.GetQuoteUnit(e.Price, Market));

						if (OrderNumber.Remove(profit.Key))
							Capital -= profit.Value * tc.TradingQuantity;
					}
					else if (OrderNumber.Any(o => o.Key.StartsWith("1") && o.Value == e.Price + Base.GetQuoteUnit(e.Price, Market)))
					{
						CumulativeFee += (uint)(e.Price * Commission * tc.TradingQuantity);
						Balance.Purchase = (double)((e.Price * tc.TradingQuantity + (Balance.Purchase ?? 0D) * Balance.Quantity) / (Balance.Quantity + tc.TradingQuantity));
						Balance.Quantity += tc.TradingQuantity;
						var profit = OrderNumber.First(o => o.Key.StartsWith("1") && o.Value == e.Price + Base.GetQuoteUnit(e.Price, Market));

						if (OrderNumber.Remove(profit.Key))
							Capital += profit.Value * tc.TradingQuantity;
					}
					else if (Balance.Quantity > tc.TradingQuantity - 1 && OrderNumber.ContainsValue(e.Price) == false
						&& e.Price > Trend.Peek() * (1 + tc.PositionRevenue) && e.Price > (Balance.Purchase ?? 0D)
						&& gap < 0 && (tc.Interval == 0 || tc.Interval > 0 && cInterval.CompareTo(NextOrderTime) > 0))
					{
						var unit = Base.GetQuoteUnit(e.Price, Market);

						if (OrderNumber.ContainsValue(e.Price + unit) == false)
							OrderNumber[Base.GetOrderNumber((int)OrderType.신규매도)] = e.Price + unit;

						if (tc.Interval > 0)
							NextOrderTime = Base.MeasureTheDelayTime(tc.Interval, cInterval);
					}
					else if (tc.TradingQuantity > 0 && OrderNumber.ContainsValue(e.Price) == false && e.Price < Trend.Peek() * (1 - tc.PositionAddition)
						&& gap > 0 && (tc.Interval == 0 || tc.Interval > 0 && cInterval.CompareTo(NextOrderTime) > 0))
					{
						var unit = Base.GetQuoteUnit(e.Price, Market);

						if (OrderNumber.ContainsValue(e.Price - unit) == false)
							OrderNumber[Base.GetOrderNumber((int)OrderType.신규매수)] = e.Price - unit;

						if (tc.Interval > 0)
							NextOrderTime = Base.MeasureTheDelayTime(tc.Interval, cInterval);
					}
				}
				else if (date.CompareTo(Base.Transmit) > 0 && Strategics is Catalog.TrendsToCashflow cf)
				{
					OrderNumber.Clear();
					long revenue = Balance.Revenue - CumulativeFee, unrealize = (long)((e.Price - (Balance.Purchase ?? 0D)) * Balance.Quantity);
					var avg = EMA.Make(++Accumulative, revenue - TodayRevenue + unrealize - TodayUnrealize, Before);

					if (cf.ReservationQuantity > 0 && Balance.Quantity > cf.ReservationQuantity - 1)
					{
						var stock = Market;
						int quantity = Balance.Quantity / cf.ReservationQuantity, price = e.Price, sell = (int)((Balance.Purchase ?? 0D) * (1 + cf.ReservationRevenue)),
							buy = (int)((Balance.Purchase ?? 0D) * (1 - cf.Addition)), upper = (int)(price * 1.3), lower = (int)(price * 0.7),
							bPrice = Base.GetStartingPrice(lower, stock), sPrice = Base.GetStartingPrice(sell, stock);
						sPrice = sPrice < lower ? lower + Base.GetQuoteUnit(sPrice, stock) : sPrice;

						while (sPrice < upper && quantity-- > 0)
						{
							OrderNumber[Base.GetOrderNumber((int)OrderType.예약매도)] = sPrice;

							for (int i = 0; i < cf.Unit; i++)
								sPrice += Base.GetQuoteUnit(sPrice, stock);
						}
						while (bPrice < upper && bPrice < buy)
						{
							OrderNumber[Base.GetOrderNumber((int)OrderType.예약매수)] = bPrice;

							for (int i = 0; i < cf.Unit; i++)
								bPrice += Base.GetQuoteUnit(bPrice, stock);
						}
						Bid = OrderNumber.Count > 0 && OrderNumber.Any(o => o.Key.StartsWith("7")) ? OrderNumber.Where(o => o.Key.StartsWith("7")).Max(o => o.Value) : 0;
						Offer = OrderNumber.Count > 0 && OrderNumber.Any(o => o.Key.StartsWith("8")) ? OrderNumber.Where(o => o.Key.StartsWith("8")).Min(o => o.Value) : 0;
					}
					SendMessage = new Catalog.Strategics.Statistics
					{
						Key = string.Concat("TC.", cf.AnalysisType),
						Date = e.Date.Substring(0, 6),
						Cumulative = revenue + unrealize,
						Base = SendMessage.Base > Capital ? SendMessage.Base : Capital,
						Statistic = (int)avg,
						Price = (int)Trend.Peek()
					};
					Before = avg;
					TodayRevenue = revenue;
					TodayUnrealize = unrealize;
				}
			}
		}
		public bool Market
		{
			get; set;
		}
		public string Code
		{
			get; set;
		}
		dynamic Offer
		{
			get; set;
		}
		dynamic Bid
		{
			get; set;
		}
		double Capital
		{
			get; set;
		}
		public Balance Balance
		{
			get; set;
		}
		public IStrategics Strategics
		{
			get; set;
		}
		Dictionary<string, dynamic> OrderNumber
		{
			get; set;
		}
		Stack<double> Short
		{
			get; set;
		}
		Stack<double> Long
		{
			get; set;
		}
		Stack<double> Trend
		{
			get; set;
		}
		Tuple<int, int, int> Line
		{
			get; set;
		}
		DateTime NextOrderTime
		{
			get; set;
		}
		string DateChange
		{
			get; set;
		}
		uint CumulativeFee
		{
			get; set;
		}
		bool GetCheckOnDeadline(string time)
		{
			var date = time.Substring(0, 6);
			var change = string.IsNullOrEmpty(DateChange) == false && DateChange.Equals(date);
			DateChange = date;

			return change;
		}
		bool GetCheckOnDate(string date) => date.Length > 8 && GetCheckOnDeadline(date);
	}
}