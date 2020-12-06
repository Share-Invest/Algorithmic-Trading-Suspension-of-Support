﻿using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

using ShareInvest.Catalog.Models;
using ShareInvest.EventHandler;
using ShareInvest.Interface.OpenAPI;
using ShareInvest.SecondaryIndicators;

namespace ShareInvest.OpenAPI
{
	public class Stocks : Analysis
	{
		public override event EventHandler<SendConsecutive> Send;
		public override bool Market
		{
			get; set;
		}
		public override dynamic SellPrice
		{
			protected internal get; set;
		}
		public override dynamic BuyPrice
		{
			protected internal get; set;
		}
		[SupportedOSPlatform("windows")]
		public override void OnReceiveDrawChart(object sender, SendConsecutive e)
		{
			if (e.Matrix is null)
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

				if (GetCheckOnDeadline(e.Date) && Short.Count > 1 && Long.Count > 1 && Strategics is Catalog.SatisfyConditionsAccordingToTrends sc)
				{
					double popShort = Short.Pop(), popLong = Long.Pop(), gap = popShort - popLong - (Short.Peek() - Long.Peek());
					Short.Push(popShort);
					Long.Push(popLong);
					var interval = new DateTime(NextOrderTime.Year, NextOrderTime.Month, NextOrderTime.Day,
						int.TryParse(e.Date.Substring(0, 2), out int cHour) ? cHour : DateTime.Now.Hour,
							int.TryParse(e.Date.Substring(2, 2), out int cMinute) ? cMinute : DateTime.Now.Minute,
								int.TryParse(e.Date[4..], out int cSecond) ? cSecond : DateTime.Now.Second);

					if (sc.TradingBuyQuantity > 0 && Bid < Trend.Peek() * (1 - sc.TradingBuyRate) && gap > 0 && OrderNumber.ContainsValue(Bid) == false
						&& Wait && (sc.TradingBuyInterval == 0 || sc.TradingBuyInterval > 0 && interval.CompareTo(NextOrderTime) > 0))
					{
						Pipe.Server.WriteLine(string.Concat(order,
							ShareInvest.Strategics.SetOrder(sc.Code, (int)OrderType.신규매수, Bid, sc.TradingBuyQuantity, ((int)HogaGb.지정가).ToString("D2"), string.Empty)));
						Wait = false;

						if (sc.TradingBuyInterval > 0)
							NextOrderTime
								= Base.MeasureTheDelayTime(sc.TradingBuyInterval * (Balance.Purchase > 0 && Bid > 0 ? Balance.Purchase / (double)Bid : 1), interval);
					}
					else if (Balance.Quantity > 0)
					{
						if (sc.TradingSellQuantity > 0 && Offer > Trend.Peek() * (1 + sc.TradingSellRate) && Offer > Balance.Purchase + tax * Offer
							&& gap < 0 && OrderNumber.ContainsValue(Offer) == false && Wait
							&& (sc.TradingSellInterval == 0 || sc.TradingSellInterval > 0 && interval.CompareTo(NextOrderTime) > 0))
						{
							Pipe.Server.WriteLine(string.Concat(order,
								ShareInvest.Strategics.SetOrder(sc.Code, (int)OrderType.신규매도, Offer, sc.TradingSellQuantity, ((int)HogaGb.지정가).ToString("D2"), string.Empty)));
							Wait = false;

							if (sc.TradingSellInterval > 0)
								NextOrderTime
									= Base.MeasureTheDelayTime(sc.TradingSellInterval * (Balance.Purchase > 0 && Offer > 0 ? Offer / (double)Balance.Purchase : 1), interval);
						}
						else if (SellPrice > 0 && sc.ReservationSellQuantity > 0 && Offer > SellPrice && OrderNumber.ContainsValue(Offer) == false && Wait)
						{
							for (int i = 0; i < sc.ReservationSellUnit; i++)
								SellPrice += GetQuoteUnit(SellPrice, Market);

							Pipe.Server.WriteLine(string.Concat(order,
								ShareInvest.Strategics.SetOrder(sc.Code, (int)OrderType.신규매도, Offer, sc.ReservationSellQuantity, ((int)HogaGb.지정가).ToString("D2"), string.Empty)));
							Wait = false;
						}
						else if (BuyPrice > 0 && sc.ReservationBuyQuantity > 0 && Bid < BuyPrice && OrderNumber.ContainsValue(Bid) == false && Wait)
						{
							for (int i = 0; i < sc.ReservationBuyUnit; i++)
								BuyPrice -= GetQuoteUnit(BuyPrice, Market);

							Pipe.Server.WriteLine(string.Concat(order,
								ShareInvest.Strategics.SetOrder(sc.Code, (int)OrderType.신규매수, Bid, sc.ReservationBuyQuantity, ((int)HogaGb.지정가).ToString("D2"), string.Empty)));
							Wait = false;
						}
						else if (SellPrice == 0 && Balance.Purchase > 0)
							SellPrice = Base.GetStartingPrice((int)((1 + sc.ReservationSellRate) * Balance.Purchase), Market);

						else if (BuyPrice == 0 && Balance.Purchase > 0)
							BuyPrice = Base.GetStartingPrice((int)(Balance.Purchase * (1 - sc.ReservationBuyRate)), Market);
					}
				}
				return;
			}
		}
		public override int GetQuoteUnit(int price, bool info) => base.GetQuoteUnit(price, info);
		public override async Task<Catalog.Models.Balance> OnReceiveBalance<T>(T param) where T : struct
		{
			if (param is Catalog.OpenAPI.Balance bal && int.TryParse(bal.Purchase, out int purchase) && int.TryParse(bal.Quantity, out int quantity)
				&& int.TryParse(bal.Current[0] is '-' ? bal.Current[1..] : bal.Current, out int current)
				&& int.TryParse(bal.Offer[0] is '-' ? bal.Offer[1..] : bal.Offer, out int offer)
				&& int.TryParse(bal.Bid[0] is '-' ? bal.Bid[1..] : bal.Bid, out int bid))
				try
				{
					await Slim.WaitAsync();
					Current = current;
					Balance.Quantity = quantity;
					Balance.Purchase = purchase;
					Balance.Revenue = (current - purchase) * quantity;
					Balance.Rate = current / (double)purchase - 1;
					Bid = bid;
					Offer = offer;
					Wait = true;
				}
				catch (Exception ex)
				{
					Base.SendMessage(bal.Name, ex.StackTrace, param.GetType());
				}
				finally
				{
					if (Slim.Release() > 0)
						Base.SendMessage(bal.Name, bal.Account, param.GetType());
				}
			return new Catalog.Models.Balance
			{
				Code = Code,
				Name = Balance.Name,
				Quantity = Balance.Quantity.ToString("N0"),
				Purchase = Balance.Purchase.ToString("N0"),
				Current = Current.ToString("N0"),
				Revenue = Balance.Revenue.ToString("C0"),
				Rate = Balance.Rate.ToString("P2")
			};
		}
		public override async Task<Tuple<dynamic, bool, int>> OnReceiveConclusion<T>(T param) where T : struct
		{
			if (param is Catalog.OpenAPI.Conclusion con && int.TryParse(con.CurrentPrice[0] is '-' ? con.CurrentPrice[1..] : con.CurrentPrice, out int current))
				try
				{
					var cash = 0;
					var remove = true;
					await Slim.WaitAsync();
					Current = current;

					switch (con.OrderState)
					{
						case conclusion:
							if (OrderNumber.Remove(con.OrderNumber))
								remove = false;

							break;

						case acceptance when con.UnsettledQuantity[0] is not '0':
							if (int.TryParse(con.OrderPrice, out int price) && price > 0)
								OrderNumber[con.OrderNumber] = price;

							break;

						case confirmation when con.OrderClassification.EndsWith(cancellantion) || con.OrderClassification.EndsWith(correction):
							if (con.OrderClassification.EndsWith(cancellantion) && OrderNumber.TryGetValue(con.OriginalOrderNumber, out dynamic order_price))
								cash = (order_price < Current ? order_price : 0) * (int.TryParse(con.OrderQuantity, out int volume) ? volume : 0);

							remove = OrderNumber.Remove(con.OriginalOrderNumber);
							break;
					}
					return new Tuple<dynamic, bool, int>(current, remove, cash);
				}
				catch (Exception ex)
				{
					Base.SendMessage(con.Name, ex.StackTrace, param.GetType());
				}
				finally
				{
					if (Slim.Release() > 0)
						Base.SendMessage(con.Name, con.Account, param.GetType());
				}
			return null;
		}
		public override async Task AnalyzeTheConclusionAsync(string[] param)
		{
			if (int.TryParse(param[^1], out int volume))
				try
				{
					await Slim.WaitAsync();

					if (Strategics is Catalog.SatisfyConditionsAccordingToTrends sc && Short is not null && Long is not null && Trend is not null)
						Send?.Invoke(this, new SendConsecutive(new Catalog.Strategics.Charts
						{
							Date = param[0],
							Price = param[1],
							Volume = volume
						}));
				}
				catch (Exception ex)
				{
					Base.SendMessage(Code, ex.StackTrace, GetType());
				}
				finally
				{
					if (Slim.Release() > 0)
						Base.SendMessage(Code, param.Length, GetType());
				}
			if (Balance is Balance bal && int.TryParse(param[1][0] is '-' ? param[1][1..] : param[1], out int current))
			{
				Current = current;
				bal.Revenue = (current - bal.Purchase) * bal.Quantity;
				bal.Rate = current / (double)bal.Purchase - 1;
			}
		}
		public override async Task AnalyzeTheQuotesAsync(string[] param)
		{
			try
			{
				await Quote.WaitAsync();
				Send?.Invoke(this, new SendConsecutive(param));
			}
			catch (Exception ex)
			{
				Base.SendMessage(Code, ex.StackTrace, GetType());
			}
			finally
			{
				if (Quote.Release() > 0)
					Base.SendMessage(Code, param.Length, GetType());
			}
		}
		public override (IEnumerable<Collect>, uint, uint, string) SortTheRecordedInformation => base.SortTheRecordedInformation;
		public override bool Collector
		{
			get; set;
		}
		public override bool Wait
		{
			get; set;
		}
		public override string Code
		{
			get; set;
		}
		public override Queue<Collect> Collection
		{
			get; set;
		}
		public override Dictionary<string, dynamic> OrderNumber
		{
			get; set;
		}
		public override dynamic Current
		{
			get; set;
		}
		public override dynamic Offer
		{
			get; set;
		}
		public override double Capital
		{
			get; protected internal set;
		}
		public override Balance Balance
		{
			get; set;
		}
		public override Interface.IStrategics Strategics
		{
			get; set;
		}
		public override dynamic Bid
		{
			get; set;
		}
		public override Stack<double> Short
		{
			protected internal get; set;
		}
		public override Stack<double> Long
		{
			protected internal get; set;
		}
		public override Stack<double> Trend
		{
			protected internal get; set;
		}
		protected internal override Tuple<int, int, int> Line
		{
			get; set;
		}
		protected internal override SemaphoreSlim Quote => new SemaphoreSlim(1, 1);
		protected internal override SemaphoreSlim Slim => new SemaphoreSlim(1, 1);
		protected internal override DateTime NextOrderTime
		{
			get; set;
		}
		protected internal override string DateChange
		{
			get; set;
		}
		protected internal override bool GetCheckOnDate(string date) => open_market.Equals(DateChange);
		protected internal override bool GetCheckOnDeadline(string time)
		{
			DateChange = open_market;
			NextOrderTime = DateTime.Now;

			return true;
		}
		const string order = "Order|";
	}
}